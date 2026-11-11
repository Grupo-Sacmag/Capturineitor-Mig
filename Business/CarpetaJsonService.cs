using System.Globalization;
using System.Text;
using System.Text.Json;
using CapturaDePolizas_2026_NET8.Business;
using CapturaDePolizas_2026_NET8.Context;
using GaCostos.Models;
using Microsoft.EntityFrameworkCore;

namespace GaCostos.Services;

/// <summary>
/// Convierte la carpeta heredada (solo lectura) a un JSON de trabajo y, al final, lo guarda en la base de datos.
/// </summary>
public sealed class CarpetaJsonService
{
    private const int LongitudRegistroCatalogo = 64;
    private const int LongitudRegistroOperacion = 64;
    private const int LongitudRegistroFiscal = 38;

    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TrabajoJson ImportarCarpeta(string rutaDatos)
    {
        try
        {
            LegacyFileService legacy = new();

            TrabajoJson trabajo = new() { Origen = rutaDatos, Generado = DateTime.Now };

            // Mapa Guia (posición 1-based en cataux) -> (CuentaMayorId, SubcuentaId, NombreSubcuenta)
            Dictionary<int, (int CuentaMayorId, int SubcuentaId, string Nombre)> mapaGuiaCataux = new();

            foreach (CuentaMayor mayor in legacy.LeerCatalogoMayor(rutaDatos))
            {
                CuentaJson cuenta = new()
                {
                    Cuenta = mayor.Cuenta,
                    Nombre = mayor.Nombre,
                    Saldo = FormatHelper.ParseDecimalFlexible(mayor.Saldo)
                };

                int.TryParse(mayor.Cuenta.Trim(), out int cuentaMayorId);

                if (mayor.RangoInferior > 0 && mayor.RangoSuperior >= mayor.RangoInferior)
                {
                    foreach (CuentaAuxiliar aux in legacy.LeerCatalogoAuxiliar(rutaDatos, mayor.RangoInferior, mayor.RangoSuperior))
                    {
                        SubcuentaJson sub = new()
                        {
                            Cuenta = aux.Cuenta,
                            Nombre = aux.Nombre,
                            Saldo = FormatHelper.ParseDecimalFlexible(aux.Saldo)
                        };

                        if (int.TryParse(aux.Cuenta.Trim(), out int subId) && aux.Guia > 0)
                        {
                            mapaGuiaCataux[aux.Guia] = (cuentaMayorId, subId, aux.Nombre);
                        }

                        AuxiliarResultado resultado = legacy.LeerAuxiliar(rutaDatos, aux.Guia, 0);

                        foreach (MovimientoAuxiliar mov in resultado.Movimientos)
                        {
                            sub.Movimientos.Add(new MovimientoJson
                            {
                                Fecha = mov.Fecha,
                                Poliza = mov.Poliza,
                                Concepto = mov.Concepto,
                                Importe = mov.Importe
                            });
                        }

                        cuenta.Subcuentas.Add(sub);
                    }
                }

                trabajo.Cuentas.Add(cuenta);
            }

            ImportarPolizasDesdeOperaciones(rutaDatos, legacy, trabajo, mapaGuiaCataux);

            return trabajo;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ImportarCarpeta: " + ex.Message, ex);
        }
    }

    private static void ImportarPolizasDesdeOperaciones(
        string rutaDatos,
        LegacyFileService legacy,
        TrabajoJson trabajo,
        Dictionary<int, (int CuentaMayorId, int SubcuentaId, string Nombre)> mapaGuiaCataux)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(1252);

        (string numeroArchivo, int anioDatos) = LeerDatosCabecera(rutaDatos, legacy, encoding);
        if (string.IsNullOrWhiteSpace(numeroArchivo))
            return;

        int anioDefault = anioDatos > 1900 ? anioDatos : DetectarAnioDesdeAuxiliares(trabajo);

        // Mapa de (mes, folio) -> fecha exacta desde los movimientos de auxiliares
        Dictionary<(int Mes, int Folio), DateTime> fechasPorPoliza = new();
        foreach (var mov in trabajo.Cuentas.SelectMany(c => c.Subcuentas).SelectMany(s => s.Movimientos))
        {
            if (TryParseFechaAuxiliar(mov.Fecha, out DateTime dt))
            {
                fechasPorPoliza.TryAdd((dt.Month, mov.Poliza), dt);
            }
        }

        string catauxPath = Path.Combine(rutaDatos, "cataux");
        using FileStream? fsCataux = File.Exists(catauxPath)
            ? new FileStream(catauxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            : null;

        string fiscalPath = Path.Combine(rutaDatos, "fiscal");
        using FileStream? fsFiscal = File.Exists(fiscalPath)
            ? new FileStream(fiscalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            : null;

        int nextPolizaId = 1;
        int nextMovId = 1;

        for (int m = 1; m <= 13; m++)
        {
            string mesArchivo = m.ToString("D2");
            string filePath = ResolverArchivoOperaciones(rutaDatos, numeroArchivo, mesArchivo);

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                continue;

            int mesCalendario = m == 13 ? 12 : m;

            List<RegistroOperacionRaw> operaciones = LeerOperacionesArchivo(filePath, encoding);
            if (operaciones.Count == 0)
                continue;

            PolizaJson? polizaActual = null;
            int cuentaMayorActual = 0;

            for (int idx = 0; idx < operaciones.Count; idx++)
            {
                RegistroOperacionRaw op = operaciones[idx];
                int rPosicionVb6 = idx + 1;

                switch (op.Identificador)
                {
                    case "A":
                        {
                            if (polizaActual is not null && (polizaActual.Movimientos.Count > 0 || polizaActual.Folio > 0))
                            {
                                FinalizarPolizaImportada(polizaActual);
                                trabajo.Polizas.Add(polizaActual);
                            }

                            int.TryParse(op.Cuenta, out int folio);
                            int.TryParse(op.Fecha, out int dia);

                            DateTime fechaPoliza;
                            if (!fechasPorPoliza.TryGetValue((mesCalendario, folio), out fechaPoliza))
                            {
                                int diasMes = DateTime.DaysInMonth(anioDefault, mesCalendario);
                                int diaValido = dia >= 1 && dia <= diasMes ? dia : diasMes;
                                fechaPoliza = new DateTime(anioDefault, mesCalendario, diaValido);
                            }

                            polizaActual = new PolizaJson
                            {
                                Id = nextPolizaId++,
                                Folio = folio,
                                Fecha = fechaPoliza,
                                Concepto = op.Descripcion.ToUpper(),
                                EsCostos = false,
                                TipoCaptura = 1,
                                CreadoEn = fechaPoliza
                            };
                            cuentaMayorActual = 0;
                            break;
                        }

                    case "D":
                        {
                            // Según CAPTURA1.frm y Poliza1.frm:
                            // oper.impte (16 bytes) = RFC
                            // oper.CTA (6 bytes) + oper.descr (30 bytes) = Folio Fiscal de cabecera (36 chars)
                            if (polizaActual is null)
                                break;

                            polizaActual.RFC = op.ImporteTexto.ToUpper();
                            polizaActual.FoliosFiscales = (op.Cuenta + op.Descripcion).Trim().ToUpper();
                            break;
                        }

                    case "B":
                        {
                            if (polizaActual is null)
                                break;

                            int.TryParse(op.Cuenta, out cuentaMayorActual);
                            decimal debe = op.Importe > 0 ? op.Importe : 0m;
                            decimal haber = op.Importe < 0 ? Math.Abs(op.Importe) : 0m;

                            polizaActual.Movimientos.Add(new MovimientoPolizaJson
                            {
                                Id = nextMovId++,
                                CuentaId = cuentaMayorActual,
                                SubcuentaId = null,
                                Parcial = 0m,
                                Debe = debe,
                                Haber = haber,
                                Redaccion = op.Descripcion.ToUpper(),
                                CreadoEn = polizaActual.Fecha
                            });
                            break;
                        }

                    case "C":
                        {
                            // Según CAPTURA1.frm y Poliza1.frm:
                            // oper.CTA = Posición (Guia) en CATAUX (Get 4, Val(oper.CTA), CATAUX)
                            // oper.real = Número de subcuenta (CATAUX.C1)
                            // oper.impte = Parcial
                            // oper.descr = Redacción
                            // fiscal (pos r) = Folio fiscal de la línea
                            if (polizaActual is null)
                                break;

                            int subId = ResolverSubcuentaId(op, mapaGuiaCataux, fsCataux, encoding, out string nombreSubcuentaCataux);
                            string folioFiscalLinea = LeerFolioFiscal(fsFiscal, rPosicionVb6, encoding);
                            string redaccion = !string.IsNullOrWhiteSpace(op.Descripcion)
                                ? op.Descripcion.ToUpper()
                                : nombreSubcuentaCataux.ToUpper();

                            polizaActual.Movimientos.Add(new MovimientoPolizaJson
                            {
                                Id = nextMovId++,
                                CuentaId = cuentaMayorActual,
                                SubcuentaId = subId > 0 ? subId : null,
                                FolioFiscal = folioFiscalLinea.ToUpper(),
                                Parcial = op.Importe,
                                Debe = 0m,
                                Haber = 0m,
                                Redaccion = redaccion,
                                CreadoEn = polizaActual.Fecha
                            });
                            break;
                        }
                }
            }

            if (polizaActual is not null && (polizaActual.Movimientos.Count > 0 || polizaActual.Folio > 0))
            {
                FinalizarPolizaImportada(polizaActual);
                trabajo.Polizas.Add(polizaActual);
            }
        }
    }

    private static int ResolverSubcuentaId(
        RegistroOperacionRaw op,
        Dictionary<int, (int CuentaMayorId, int SubcuentaId, string Nombre)> mapaGuiaCataux,
        FileStream? fsCataux,
        Encoding encoding,
        out string nombreSubcuenta)
    {
        nombreSubcuenta = string.Empty;

        if (int.TryParse(op.Cuenta, out int guia) && guia > 0)
        {
            if (mapaGuiaCataux.TryGetValue(guia, out var info))
            {
                nombreSubcuenta = info.Nombre;
                if (info.SubcuentaId > 0)
                    return info.SubcuentaId;
            }

            if (fsCataux is not null)
            {
                long offset = (long)(guia - 1) * LongitudRegistroCatalogo;
                if (offset + LongitudRegistroCatalogo <= fsCataux.Length)
                {
                    fsCataux.Seek(offset, SeekOrigin.Begin);
                    byte[] buf = new byte[LongitudRegistroCatalogo];
                    if (fsCataux.Read(buf, 0, LongitudRegistroCatalogo) == LongitudRegistroCatalogo)
                    {
                        string reg = encoding.GetString(buf);
                        string c1 = reg.Substring(0, 6).Replace("\0", string.Empty).Trim();
                        string c2 = reg.Substring(6, 32).Replace("\0", string.Empty).Trim();
                        nombreSubcuenta = c2;
                        if (int.TryParse(c1, out int c1Id) && c1Id > 0)
                            return c1Id;
                    }
                }
            }
        }

        if (int.TryParse(op.Real, out int realSubId) && realSubId > 0)
            return realSubId;

        return 0;
    }

    private static string LeerFolioFiscal(FileStream? fsFiscal, int posicion1Based, Encoding encoding)
    {
        if (fsFiscal is null || posicion1Based < 1)
            return string.Empty;

        long offset = (long)(posicion1Based - 1) * LongitudRegistroFiscal;
        if (offset + 36 > fsFiscal.Length)
            return string.Empty;

        fsFiscal.Seek(offset, SeekOrigin.Begin);
        byte[] buf = new byte[36];
        int leidos = fsFiscal.Read(buf, 0, 36);
        if (leidos <= 0)
            return string.Empty;

        return encoding.GetString(buf, 0, leidos).Replace("\0", string.Empty).Trim();
    }

    private static (string NumeroArchivo, int Anio) LeerDatosCabecera(string rutaDatos, LegacyFileService legacy, Encoding encoding)
    {
        string filePath = Path.Combine(rutaDatos, "DATOS");
        if (!File.Exists(filePath))
            return (string.Empty, 0);

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        byte[] buffer = new byte[231];
        int leidos = fs.Read(buffer, 0, buffer.Length);

        string numeroArchivo = legacy.LeerNumeroArchivoDesdeDatos(rutaDatos).Replace("\0", string.Empty).Trim();
        int anio = 0;

        if (leidos >= 189)
        {
            string anioTexto = encoding.GetString(buffer, 184, 5).Replace("\0", string.Empty).Trim();
            int.TryParse(anioTexto, out anio);
        }

        return (numeroArchivo, anio);
    }

    private static void FinalizarPolizaImportada(PolizaJson poliza)
    {
        poliza.TotalDebe = poliza.Movimientos.Sum(m => m.Debe);
        poliza.TotalHaber = poliza.Movimientos.Sum(m => m.Haber);
    }

    private sealed record RegistroOperacionRaw(
        string Cuenta,
        string Descripcion,
        string Fecha,
        string ImporteTexto,
        decimal Importe,
        string Identificador,
        string Real);

    private static List<RegistroOperacionRaw> LeerOperacionesArchivo(string filePath, Encoding encoding)
    {
        List<RegistroOperacionRaw> operaciones = [];

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using BinaryReader reader = new(fs, encoding);

        while (fs.Position + LongitudRegistroOperacion <= fs.Length)
        {
            byte[] buffer = reader.ReadBytes(LongitudRegistroOperacion);
            string impteTexto = encoding.GetString(buffer, 38, 16).Replace("\0", string.Empty).Trim();

            operaciones.Add(new RegistroOperacionRaw(
                Cuenta: encoding.GetString(buffer, 0, 6).Replace("\0", string.Empty).Trim(),
                Descripcion: encoding.GetString(buffer, 6, 30).Replace("\0", string.Empty).Trim(),
                Fecha: encoding.GetString(buffer, 36, 2).Replace("\0", string.Empty).Trim(),
                ImporteTexto: impteTexto,
                Importe: FormatHelper.ParseDecimalFlexible(impteTexto),
                Identificador: encoding.GetString(buffer, 54, 1).Replace("\0", string.Empty).Trim(),
                Real: encoding.GetString(buffer, 55, 9).Replace("\0", string.Empty).Trim()));
        }

        return operaciones;
    }

    private static string ResolverArchivoOperaciones(string rutaDatos, string numeroArchivo, string mesArchivo)
    {
        string archivoExacto = Path.Combine(rutaDatos, numeroArchivo + mesArchivo);
        if (File.Exists(archivoExacto))
            return archivoExacto;

        try
        {
            var archivosDelMes = Directory.EnumerateFiles(rutaDatos)
                .Where(p => Path.GetFileName(p).EndsWith(mesArchivo, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (archivosDelMes.Count == 0)
                return string.Empty;

            var conPrefijo = archivosDelMes
                .Where(p => Path.GetFileName(p).StartsWith(numeroArchivo, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => Path.GetFileName(p).Length)
                .ThenBy(p => Path.GetFileName(p))
                .ToList();

            if (conPrefijo.Count > 0)
                return conPrefijo[0];

            return archivosDelMes.Count == 1 ? archivosDelMes[0] : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static int DetectarAnioDesdeAuxiliares(TrabajoJson trabajo)
    {
        foreach (var mov in trabajo.Cuentas.SelectMany(c => c.Subcuentas).SelectMany(s => s.Movimientos))
        {
            if (TryParseFechaAuxiliar(mov.Fecha, out DateTime dt))
                return dt.Year;
        }

        return DateTime.Today.Year;
    }

    private static bool TryParseFechaAuxiliar(string fecha, out DateTime resultado)
    {
        return DateTime.TryParseExact(
            fecha?.Trim(),
            ["dd/MM/yy", "dd/MM/yyyy"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out resultado);
    }

    public void GuardarJson(TrabajoJson trabajo, string rutaJson)
    {
        try
        {
            File.WriteAllText(rutaJson, JsonSerializer.Serialize(trabajo, Opciones));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en GuardarJson: " + ex.Message, ex);
        }
    }

    public TrabajoJson CargarJson(string rutaJson)
    {
        try
        {
            TrabajoJson? trabajo = JsonSerializer.Deserialize<TrabajoJson>(File.ReadAllText(rutaJson), Opciones);

            return trabajo ?? throw new InvalidOperationException("El archivo JSON está vacío o no tiene el formato esperado.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en CargarJson: " + ex.Message, ex);
        }
    }


    private static void ValidarTrabajoLocalParaSql(TrabajoJson trabajo, string? rutaJson)
    {
        ArgumentNullException.ThrowIfNull(trabajo);

        // El archivo debe existir físicamente y haberse guardado.
        if (string.IsNullOrWhiteSpace(rutaJson) || !File.Exists(rutaJson))
        {
            throw new InvalidOperationException("Antes de enviar datos a SQL Server, debes guardar el JSON localmente.");
        }

        // Recuperar desde disco el estado realmente guardado.
        TrabajoJson almacenado = JsonSerializer.Deserialize<TrabajoJson>(File.ReadAllText(rutaJson), Opciones) ?? throw new InvalidOperationException("El JSON local no contiene datos válidos.");

        // No permitir enviar cambios que solo existan en memoria.
        string contenidoLocal = JsonSerializer.Serialize(almacenado, Opciones);
        string contenidoActual = JsonSerializer.Serialize(trabajo, Opciones);

        if (!string.Equals(contenidoLocal, contenidoActual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Existen cambios que aún no se han guardado en el JSON local.");
        }

        if (trabajo.Polizas is null)
            throw new InvalidOperationException("El JSON no contiene una colección válida de pólizas.");

        foreach (PolizaJson poliza in trabajo.Polizas)
        {
            if (poliza is null || poliza.Folio <= 0 || poliza.Movimientos is null || poliza.Movimientos.Count == 0)
            {
                throw new InvalidOperationException("El JSON contiene una póliza sin movimientos válidos.");
            }

            decimal debeCalculado = 0m;
            decimal haberCalculado = 0m;

            foreach (MovimientoPolizaJson movimiento in poliza.Movimientos)
            {
                if (movimiento is null || movimiento.CuentaId <= 0 || movimiento.SubcuentaId is <= 0 || movimiento.Debe < 0m || movimiento.Haber < 0m || (movimiento.Debe > 0m &&
                    movimiento.Haber > 0m) || decimal.Round(movimiento.Debe, 2) != movimiento.Debe || decimal.Round(movimiento.Haber, 2) != movimiento.Haber || decimal.Round(movimiento.Parcial, 2) !=
                    movimiento.Parcial)
                {
                    throw new InvalidOperationException($"La póliza {poliza.Folio} tiene movimientos inválidos o importes con más de dos decimales.");
                }

                debeCalculado += movimiento.Debe;
                haberCalculado += movimiento.Haber;
            }

            if (poliza.TotalDebe <= 0m || poliza.TotalDebe != poliza.TotalHaber || debeCalculado != haberCalculado || poliza.TotalDebe != debeCalculado || poliza.TotalHaber != haberCalculado ||
                decimal.Round(poliza.TotalDebe, 2) != poliza.TotalDebe || decimal.Round(poliza.TotalHaber, 2) != poliza.TotalHaber)
            {
                throw new InvalidOperationException($"La póliza {poliza.Folio} no está correctamente cuadrada. No se enviarán datos a SQL Server.");
            }
        }
    }

    /// <summary>
    /// Agrega o actualiza (por Id) cuentas, subcuentas y pólizas en una sola transacción (SaveChanges). No borra nada.
    /// </summary>
    public ResultadoGuardadoBd GuardarEnBd(TrabajoJson trabajo, string? rutaJson)
    {

        try
        {
            ValidarTrabajoLocalParaSql(trabajo, rutaJson);

            ResultadoGuardadoBd resultado = new();

            using var db = new EmpresaDbContext();
            using var transaccion = db.Database.BeginTransaction();

            var cuentasDb = db.Cuentas.ToDictionary(c => c.Id);
            var subcuentasDb = db.Subcuentas.ToDictionary(s => (s.CuentaId, s.Id));

            foreach (CuentaJson cuentaJson in trabajo.Cuentas)
            {
                if (!int.TryParse(cuentaJson.Cuenta.Trim(), out int cuentaId))
                {
                    resultado.Omitidas++;
                    continue;
                }

                if (cuentasDb.TryGetValue(cuentaId, out var cuenta))
                {
                    cuenta.Nombre = cuentaJson.Nombre;
                    cuenta.MontoBruto = cuentaJson.Saldo;
                    resultado.CuentasActualizadas++;
                }
                else
                {
                    cuenta = new CapturaDePolizas_2026_NET8.Entities.Cuenta
                    {
                        Id = cuentaId,
                        Nombre = cuentaJson.Nombre,
                        MontoBruto = cuentaJson.Saldo
                    };
                    db.Cuentas.Add(cuenta);
                    cuentasDb[cuentaId] = cuenta;
                    resultado.CuentasNuevas++;
                }

                int totalSubcuentas = 0;

                foreach (SubcuentaJson subJson in cuentaJson.Subcuentas)
                {
                    if (!int.TryParse(subJson.Cuenta.Trim(), out int subId))
                    {
                        resultado.Omitidas++;
                        continue;
                    }

                    totalSubcuentas++;

                    if (subcuentasDb.TryGetValue((cuentaId, subId), out var sub))
                    {
                        sub.Nombre = subJson.Nombre;
                        sub.ImporteNeto = subJson.Saldo;
                        resultado.SubcuentasActualizadas++;
                    }
                    else
                    {
                        sub = new CapturaDePolizas_2026_NET8.Entities.Subcuenta
                        {
                            Id = subId,
                            CuentaId = cuentaId,
                            Nombre = subJson.Nombre,
                            ImporteNeto = subJson.Saldo,
                            Activa = true
                        };
                        db.Subcuentas.Add(sub);
                        subcuentasDb[(cuentaId, subId)] = sub;
                        resultado.SubcuentasNuevas++;
                    }
                }

                cuenta.NumSubcuentas = totalSubcuentas;
            }

            // Guardar pólizas del JSON que aún no existan en la BD (por Folio + Fecha)
            var foliosDb = db.Polizas.Select(p => new { p.Folio, Fecha = p.Fecha.Date }).ToHashSet();

            foreach (PolizaJson polizaJson in trabajo.Polizas)
            {
                if (foliosDb.Contains(new { polizaJson.Folio, Fecha = polizaJson.Fecha.Date }))
                    continue;

                var polizaEntity = new CapturaDePolizas_2026_NET8.Entities.Poliza
                {
                    Fecha = polizaJson.Fecha,
                    Folio = polizaJson.Folio,
                    Concepto = polizaJson.Concepto.ToUpper(),
                    EsCostos = polizaJson.EsCostos,
                    TipoCaptura = polizaJson.TipoCaptura,
                    TotalDebe = polizaJson.TotalDebe,
                    TotalHaber = polizaJson.TotalHaber,
                    Beneficiario = polizaJson.Beneficiario.ToUpper(),
                    MontoCheque = polizaJson.MontoCheque,
                    NumeroCheque = polizaJson.NumeroCheque.ToUpper(),
                    RFC = polizaJson.RFC.ToUpper(),
                    FoliosFiscales = polizaJson.FoliosFiscales.ToUpper(),
                    RequiereImpresion = polizaJson.RequiereImpresion,
                    CreadoEn = polizaJson.CreadoEn == default ? DateTime.Now : polizaJson.CreadoEn
                };

                foreach (MovimientoPolizaJson movJson in polizaJson.Movimientos)
                {
                    polizaEntity.Movimientos.Add(new CapturaDePolizas_2026_NET8.Entities.Movimiento
                    {
                        CuentaId = movJson.CuentaId,
                        SubcuentaId = movJson.SubcuentaId,
                        FolioFiscal = movJson.FolioFiscal.ToUpper(),
                        Parcial = movJson.Parcial,
                        Debe = movJson.Debe,
                        Haber = movJson.Haber,
                        Redaccion = movJson.Redaccion.ToUpper(),
                        Beneficiario = movJson.Beneficiario.ToUpper(),
                        PerteneceConcepto = movJson.PerteneceConcepto,
                        CreadoEn = movJson.CreadoEn == default ? DateTime.Now : movJson.CreadoEn
                    });
                }

                db.Polizas.Add(polizaEntity);
                foliosDb.Add(new { polizaJson.Folio, Fecha = polizaJson.Fecha.Date });
                resultado.PolizasNuevas++;
            }


            db.SaveChanges();
            transaccion.Commit();

            return resultado;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en GuardarEnBd: " + ex.Message, ex);
        }
    }
}
