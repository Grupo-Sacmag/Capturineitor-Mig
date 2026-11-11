using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CapturaDePolizas_2026_NET8.Models;
using GaCostos.Models;
using GaCostos.Services;

namespace CapturaDePolizas_2026_NET8.Repositories
{
    public class JsonCatalogoRepository : ICatalogoRepository
    {
        private readonly CarpetaJsonService _jsonService = new();

        private static TrabajoJson ObtenerTrabajo()
        {
            if (SesionTrabajoJson.Actual is null)
            {
                SesionTrabajoJson.Actual = new TrabajoJson
                {
                    Origen = "Memoria",
                    Generado = DateTime.Now
                };
            }

            return SesionTrabajoJson.Actual;
        }

        private void PersistirSiTieneRuta(TrabajoJson trabajo)
        {
            SesionTrabajoJson.Modificado = true;

            if (!string.IsNullOrWhiteSpace(SesionTrabajoJson.RutaArchivo))
            {
                _jsonService.GuardarJson(trabajo, SesionTrabajoJson.RutaArchivo);
                SesionTrabajoJson.Modificado = false;
            }
        }

        public bool ValidarCuenta(string cuentaId, out string nombreCuenta)
        {
            nombreCuenta = string.Empty;

            try
            {
                if (!int.TryParse(cuentaId?.Trim(), out int id))
                    return false;

                var trabajo = ObtenerTrabajo();
                var cuenta = trabajo.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == id);

                if (cuenta is null)
                    return false;

                nombreCuenta = cuenta.Nombre.ToUpper();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ValidarCuenta (JSON): " + ex.Message, ex);
            }
        }

        public bool ValidarSubcuenta(string subcuentaId, string cuentaPadreId, out string nombreSubcuenta)
        {
            nombreSubcuenta = string.Empty;

            try
            {
                if (!int.TryParse(subcuentaId?.Trim(), out int subId))
                    return false;

                if (!int.TryParse(cuentaPadreId?.Trim(), out int cuentaId))
                    return false;

                var trabajo = ObtenerTrabajo();
                var cuenta = trabajo.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == cuentaId);

                if (cuenta is null)
                    return false;

                var subcuenta = cuenta.Subcuentas.FirstOrDefault(s => s.Activa && int.TryParse(s.Cuenta.Trim(), out int sId) && sId == subId);

                if (subcuenta is null)
                    return false;

                nombreSubcuenta = subcuenta.Nombre.ToUpper();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ValidarSubcuenta (JSON): " + ex.Message, ex);
            }
        }

        public int ObtenerSiguienteFolio()
        {
            try
            {
                var trabajo = ObtenerTrabajo();

                int maxPolizas = trabajo.Polizas.Count > 0 ? trabajo.Polizas.Max(p => p.Folio) : 0;
                int maxMovs = trabajo.Cuentas
                    .SelectMany(c => c.Subcuentas)
                    .SelectMany(s => s.Movimientos)
                    .Select(m => (int)m.Poliza)
                    .DefaultIfEmpty(0)
                    .Max();

                return Math.Max(maxPolizas, maxMovs) + 1;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerSiguienteFolio (JSON): " + ex.Message, ex);
            }
        }

        public string ObtenerSiguienteNumeroCheque()
        {
            try
            {
                var trabajo = ObtenerTrabajo();
                int maxVal = 0;

                foreach (var poliza in trabajo.Polizas.Where(p => p.TipoCaptura == 0 && !string.IsNullOrWhiteSpace(p.NumeroCheque)))
                {
                    if (int.TryParse(poliza.NumeroCheque.Trim(), out int numero) && numero > maxVal)
                    {
                        maxVal = numero;
                    }
                }

                return (maxVal + 1).ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerSiguienteNumeroCheque (JSON): " + ex.Message, ex);
            }
        }

        public bool GuardarPoliza(PolizaModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var trabajo = ObtenerTrabajo();

                int nextPolizaId = (trabajo.Polizas.Count > 0 ? trabajo.Polizas.Max(p => p.Id) : 0) + 1;
                int nextMovId = (trabajo.Polizas.SelectMany(p => p.Movimientos).Select(m => m.Id).DefaultIfEmpty(0).Max()) + 1;

                var polizaJson = new PolizaJson
                {
                    Id = nextPolizaId,
                    Fecha = model.Fecha,
                    Folio = model.Folio,
                    Concepto = model.Concepto.ToUpper(),
                    EsCostos = model.EsCostos,
                    TipoCaptura = model.TipoCaptura,
                    TotalDebe = model.TotalDebe,
                    TotalHaber = model.TotalHaber,
                    Beneficiario = model.Beneficiario.ToUpper(),
                    MontoCheque = model.MontoCheque,
                    NumeroCheque = model.NumeroCheque.ToUpper(),
                    RFC = model.RFC.ToUpper(),
                    FoliosFiscales = model.FoliosFiscales.ToUpper(),
                    RequiereImpresion = model.RequiereImpresion,
                    CreadoEn = DateTime.Now
                };

                foreach (MovimientoModel movModel in model.Movimientos)
                {
                    var movPoliza = new MovimientoPolizaJson
                    {
                        Id = nextMovId++,
                        CuentaId = movModel.CuentaId,
                        SubcuentaId = movModel.SubcuentaId,
                        FolioFiscal = movModel.FolioFiscal.ToUpper(),
                        Parcial = movModel.Parcial,
                        Debe = movModel.Debe,
                        Haber = movModel.Haber,
                        Redaccion = movModel.Redaccion.ToUpper(),
                        Beneficiario = movModel.Beneficiario.ToUpper(),
                        PerteneceConcepto = movModel.PerteneceConcepto,
                        CreadoEn = DateTime.Now
                    };

                    polizaJson.Movimientos.Add(movPoliza);

                    // Actualizar saldos y movimientos en el árbol de cuentas/subcuentas del JSON
                    var cuenta = trabajo.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == movModel.CuentaId);
                    if (cuenta is not null)
                    {
                        decimal deltaMayor = movModel.Debe - movModel.Haber;
                        cuenta.Saldo += deltaMayor;

                        if (movModel.SubcuentaId.HasValue)
                        {
                            var sub = cuenta.Subcuentas.FirstOrDefault(s => int.TryParse(s.Cuenta.Trim(), out int sId) && sId == movModel.SubcuentaId.Value);
                            if (sub is not null)
                            {
                                decimal importeAux = deltaMayor != 0m ? deltaMayor : movModel.Parcial;
                                sub.Saldo += importeAux;

                                short folioShort = model.Folio > short.MaxValue ? short.MaxValue : (short)model.Folio;
                                string conceptoMov = !string.IsNullOrWhiteSpace(movModel.Redaccion) ? movModel.Redaccion.ToUpper() : model.Concepto.ToUpper();

                                sub.Movimientos.Add(new MovimientoJson
                                {
                                    Fecha = model.Fecha.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
                                    Poliza = folioShort,
                                    Concepto = conceptoMov,
                                    Importe = importeAux
                                });
                            }
                        }
                    }
                }

                trabajo.Polizas.Add(polizaJson);
                PersistirSiTieneRuta(trabajo);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en GuardarPoliza (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerTablaCuentas()
        {
            try
            {
                DataTable tabla = new();
                tabla.Columns.Add("Número", typeof(int));
                tabla.Columns.Add("Nombre", typeof(string));
                tabla.Columns.Add("Monto Bruto", typeof(decimal));
                tabla.Columns.Add("Subcuentas", typeof(int));

                var trabajo = ObtenerTrabajo();

                foreach (var c in trabajo.Cuentas
                    .Select(c => (Valido: int.TryParse(c.Cuenta.Trim(), out int id), Id: id, Cuenta: c))
                    .Where(x => x.Valido)
                    .OrderBy(x => x.Id))
                {
                    tabla.Rows.Add(
                        c.Id,
                        c.Cuenta.Nombre.ToUpper(),
                        c.Cuenta.Saldo,
                        c.Cuenta.Subcuentas.Count(s => s.Activa)
                    );
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerTablaCuentas (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerTablaSubcuentas()
        {
            try
            {
                DataTable tabla = new();
                tabla.Columns.Add("Cuenta", typeof(int));
                tabla.Columns.Add("Subcuenta", typeof(int));
                tabla.Columns.Add("Nombre", typeof(string));
                tabla.Columns.Add("Debe", typeof(decimal));
                tabla.Columns.Add("Haber", typeof(decimal));
                tabla.Columns.Add("Importe Neto", typeof(decimal));

                var trabajo = ObtenerTrabajo();

                var acumuladosPolizas = trabajo.Polizas
                    .SelectMany(p => p.Movimientos)
                    .Where(m => m.SubcuentaId.HasValue)
                    .GroupBy(m => (m.CuentaId, SubId: m.SubcuentaId!.Value))
                    .ToDictionary(
                        g => g.Key,
                        g => (Debe: g.Sum(x => x.Debe), Haber: g.Sum(x => x.Haber))
                    );

                foreach (var c in trabajo.Cuentas
                    .Select(c => (Valido: int.TryParse(c.Cuenta.Trim(), out int id), Id: id, Cuenta: c))
                    .Where(x => x.Valido)
                    .OrderBy(x => x.Id))
                {
                    foreach (var s in c.Cuenta.Subcuentas
                        .Where(s => s.Activa)
                        .Select(s => (Valido: int.TryParse(s.Cuenta.Trim(), out int sId), Id: sId, Sub: s))
                        .Where(x => x.Valido)
                        .OrderBy(x => x.Id))
                    {
                        decimal totalDebe = s.Sub.Movimientos.Where(m => m.Importe > 0).Sum(m => m.Importe);
                        decimal totalHaber = s.Sub.Movimientos.Where(m => m.Importe < 0).Sum(m => Math.Abs(m.Importe));

                        if (totalDebe == 0m && totalHaber == 0m && acumuladosPolizas.TryGetValue((c.Id, s.Id), out var ac))
                        {
                            totalDebe = ac.Debe;
                            totalHaber = ac.Haber;
                        }

                        tabla.Rows.Add(
                            c.Id,
                            s.Id,
                            s.Sub.Nombre.ToUpper(),
                            totalDebe,
                            totalHaber,
                            s.Sub.Saldo
                        );
                    }
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerTablaSubcuentas (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerCuentasBusqueda()
        {
            try
            {
                DataTable tabla = new();
                tabla.Columns.Add("Número", typeof(int));
                tabla.Columns.Add("Nombre", typeof(string));
                tabla.Columns.Add("Importe", typeof(decimal));

                var trabajo = ObtenerTrabajo();

                foreach (var c in trabajo.Cuentas
                    .Select(c => (Valido: int.TryParse(c.Cuenta.Trim(), out int id), Id: id, Cuenta: c))
                    .Where(x => x.Valido)
                    .OrderBy(x => x.Id))
                {
                    tabla.Rows.Add(c.Id, c.Cuenta.Nombre.ToUpper(), c.Cuenta.Saldo);
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerCuentasBusqueda (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerSubcuentasBusqueda(int cuentaId)
        {
            try
            {
                DataTable tabla = new();
                tabla.Columns.Add("Número", typeof(int));
                tabla.Columns.Add("Nombre", typeof(string));
                tabla.Columns.Add("Importe", typeof(decimal));

                var trabajo = ObtenerTrabajo();
                var cuenta = trabajo.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == cuentaId);

                if (cuenta is null)
                    return tabla;

                foreach (var s in cuenta.Subcuentas
                    .Where(s => s.Activa)
                    .Select(s => (Valido: int.TryParse(s.Cuenta.Trim(), out int sId), Id: sId, Sub: s))
                    .Where(x => x.Valido)
                    .OrderBy(x => x.Id))
                {
                    tabla.Rows.Add(s.Id, s.Sub.Nombre.ToUpper(), s.Sub.Saldo);
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerSubcuentasBusqueda (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerTablaPolizas()
        {
            try
            {
                DataTable tabla = CrearTablaPolizas();
                var trabajo = ObtenerTrabajo();

                foreach (var poliza in trabajo.Polizas.OrderBy(p => p.Folio))
                {
                    AgregarFilaPoliza(tabla, poliza);
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerTablaPolizas (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerTablaPolizasPorSubcuenta(int cuentaId, int subcuentaId)
        {
            try
            {
                DataTable tabla = CrearTablaPolizas();
                var trabajo = ObtenerTrabajo();

                var polizasCapturadas = trabajo.Polizas
                    .Where(p => p.Movimientos.Any(m => m.CuentaId == cuentaId && m.SubcuentaId == subcuentaId))
                    .OrderByDescending(p => p.Fecha)
                    .ThenByDescending(p => p.Folio)
                    .ToList();

                foreach (var poliza in polizasCapturadas)
                {
                    AgregarFilaPoliza(tabla, poliza);
                }

                // Incluir también movimientos heredados del auxiliar que no estén en Polizas
                var foliosExistentes = polizasCapturadas.Select(p => p.Folio).ToHashSet();
                var cuenta = trabajo.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == cuentaId);
                var sub = cuenta?.Subcuentas.FirstOrDefault(s => int.TryParse(s.Cuenta.Trim(), out int sId) && sId == subcuentaId);

                if (sub is not null)
                {
                    int idSintetico = -1;
                    foreach (var mov in sub.Movimientos.Where(m => !foliosExistentes.Contains(m.Poliza)))
                    {
                        DateTime fecha = ParsearFechaCorta(mov.Fecha);
                        decimal debe = mov.Importe > 0 ? mov.Importe : 0m;
                        decimal haber = mov.Importe < 0 ? Math.Abs(mov.Importe) : 0m;

                        tabla.Rows.Add(
                            idSintetico--,
                            fecha,
                            (int)mov.Poliza,
                            mov.Concepto.ToUpper(),
                            false,
                            (byte)1,
                            debe,
                            haber,
                            string.Empty,
                            0m,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            false,
                            fecha
                        );
                    }
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerTablaPolizasPorSubcuenta (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerMovimientosPoliza(int polizaId)
        {
            try
            {
                DataTable tabla = new();
                tabla.Columns.Add("cuenta_id", typeof(int));
                tabla.Columns.Add("subcuenta_id", typeof(int));
                tabla.Columns.Add("nombre_cuenta", typeof(string));
                tabla.Columns.Add("nombre_subcuenta", typeof(string));
                tabla.Columns.Add("parcial", typeof(decimal));
                tabla.Columns.Add("debe", typeof(decimal));
                tabla.Columns.Add("haber", typeof(decimal));
                tabla.Columns.Add("redaccion", typeof(string));
                tabla.Columns.Add("folio_fiscal", typeof(string));

                var trabajo = ObtenerTrabajo();
                var poliza = trabajo.Polizas.FirstOrDefault(p => p.Id == polizaId);

                if (poliza is null)
                    return tabla;

                foreach (var mov in poliza.Movimientos.OrderBy(m => m.Id))
                {
                    var cuenta = trabajo.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == mov.CuentaId);
                    var sub = mov.SubcuentaId.HasValue
                        ? cuenta?.Subcuentas.FirstOrDefault(s => int.TryParse(s.Cuenta.Trim(), out int sId) && sId == mov.SubcuentaId.Value)
                        : null;

                    tabla.Rows.Add(
                        mov.CuentaId,
                        mov.SubcuentaId.HasValue ? mov.SubcuentaId.Value : DBNull.Value,
                        (cuenta?.Nombre ?? string.Empty).ToUpper(),
                        (sub?.Nombre ?? string.Empty).ToUpper(),
                        mov.Parcial,
                        mov.Debe,
                        mov.Haber,
                        mov.Redaccion.ToUpper(),
                        mov.FolioFiscal.ToUpper()
                    );
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerMovimientosPoliza (JSON): " + ex.Message, ex);
            }
        }

        public DataTable ObtenerTablaVacia()
        {
            DataTable tabla = new();
            tabla.Columns.Add("Info", typeof(string));
            tabla.Rows.Add("Sin información disponible / Módulo en desarrollo");
            return tabla;
        }

        public DataTable ObtenerEstadosFinancieros(bool incluirCuentasOrden)
        {
            try
            {
                DataTable tabla = new();
                tabla.Columns.Add("Activo", typeof(string));
                tabla.Columns.Add("Monto Activo", typeof(string));
                tabla.Columns.Add("Pasivo y Capital", typeof(string));
                tabla.Columns.Add("Monto Pasivo", typeof(string));
                tabla.Columns.Add("CtaIzq", typeof(string));
                tabla.Columns.Add("CtaDer", typeof(string));

                var trabajo = ObtenerTrabajo();

                var cuentas = trabajo.Cuentas
                    .Select(c =>
                    {
                        int.TryParse(c.Cuenta.Trim(), out int id);
                        (string tipo, string clasif) = ResolverTipoYClasificacion(id, c.TipoCuenta, c.Clasificacion);
                        return (Id: id, Nombre: c.Nombre, MontoBruto: c.Saldo, TipoCuenta: tipo, Clasificacion: clasif);
                    })
                    .Where(c => c.Id > 0 && c.MontoBruto != 0)
                    .OrderBy(c => c.Id)
                    .ToList();

                var activos = cuentas.Where(c => c.TipoCuenta == "ACTIVO").ToList();
                var pasivos = cuentas.Where(c => c.TipoCuenta == "PASIVO").ToList();
                var capital = cuentas.Where(c => c.TipoCuenta == "CAPITAL").ToList();
                var orden = cuentas.Where(c => c.TipoCuenta == "CUENTAS DE ORDEN" || c.TipoCuenta == "ORDEN").ToList();

                string[] clasificaciones = ["CIRCULANTE", "FIJO", "DIFERIDO"];

                var lineasActivo = new List<(string Concepto, string Monto, string Cuenta)>
                {
                    ("A C T I V O", "", "")
                };
                lineasActivo.AddRange(GenerarLineasLado(activos, clasificaciones, out decimal totalActivo));
                lineasActivo.Add(("Suma Activo", totalActivo.ToString("N2"), ""));

                var lineasPasivoCapital = new List<(string Concepto, string Monto, string Cuenta)>
                {
                    ("P A S I V O", "", "")
                };
                lineasPasivoCapital.AddRange(GenerarLineasLado(pasivos, clasificaciones, out decimal totalPasivo));
                lineasPasivoCapital.Add(("Suma Pasivo", totalPasivo.ToString("N2"), ""));
                lineasPasivoCapital.Add(("", "", ""));
                lineasPasivoCapital.Add(("HABER", "", ""));
                lineasPasivoCapital.Add(("SOCIAL", "", ""));

                decimal totalCapital = 0;
                foreach (var cap in capital)
                {
                    lineasPasivoCapital.Add(($"{cap.Id} {cap.Nombre}", cap.MontoBruto.ToString("N2"), $"{cap.Id}"));
                    totalCapital += cap.MontoBruto;
                }
                lineasPasivoCapital.Add(("Suma Pasivo y Haber Soc", (totalPasivo + totalCapital).ToString("N2"), ""));

                int maxLines = Math.Max(lineasActivo.Count, lineasPasivoCapital.Count);
                for (int i = 0; i < maxLines; i++)
                {
                    var izq = i < lineasActivo.Count ? lineasActivo[i] : (Concepto: "", Monto: "", Cuenta: "");
                    var der = i < lineasPasivoCapital.Count ? lineasPasivoCapital[i] : (Concepto: "", Monto: "", Cuenta: "");
                    tabla.Rows.Add(izq.Concepto, izq.Monto, der.Concepto, der.Monto, izq.Cuenta, der.Cuenta);
                }

                if (incluirCuentasOrden)
                {
                    tabla.Rows.Add("", "", "", "", "", "");
                    tabla.Rows.Add("CUENTAS DE ORDEN", "", "", "", "", "");
                    foreach (var ord in orden)
                    {
                        tabla.Rows.Add($"{ord.Id} {ord.Nombre}", ord.MontoBruto.ToString("N2"), "", "", $"{ord.Id}", "");
                    }
                }

                return tabla;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en ObtenerEstadosFinancieros (JSON): " + ex.Message, ex);
            }
        }

        private static List<(string Concepto, string Monto, string Cuenta)> GenerarLineasLado(
            List<(int Id, string Nombre, decimal MontoBruto, string TipoCuenta, string Clasificacion)> cuentas,
            string[] ordenClasificaciones,
            out decimal granTotal)
        {
            var lineas = new List<(string Concepto, string Monto, string Cuenta)>();
            granTotal = 0;

            foreach (var clasif in ordenClasificaciones)
            {
                var cuentasClasif = cuentas.Where(c => c.Clasificacion == clasif).ToList();
                if (cuentasClasif.Count > 0)
                {
                    lineas.Add((CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clasif.ToLower()), "", ""));
                    decimal subtotal = 0;
                    foreach (var c in cuentasClasif)
                    {
                        lineas.Add(($"{c.Id} {c.Nombre}", c.MontoBruto.ToString("N2"), $"{c.Id}"));
                        subtotal += c.MontoBruto;
                    }
                    granTotal += subtotal;
                }
            }

            return lineas;
        }

        private static (string Tipo, string Clasificacion) ResolverTipoYClasificacion(int id, string? tipoCuenta, string? clasificacion)
        {
            string tipo = (tipoCuenta ?? string.Empty).Trim().ToUpper();
            string clasif = (clasificacion ?? string.Empty).Trim().ToUpper();

            if (!string.IsNullOrEmpty(tipo))
                return (tipo, string.IsNullOrEmpty(clasif) ? "CIRCULANTE" : clasif);

            int centena = id / 100;
            int millar = id / 1000;

            return millar switch
            {
                1 => ("ACTIVO", centena switch { 11 => "CIRCULANTE", 12 => "FIJO", 13 => "DIFERIDO", _ => "CIRCULANTE" }),
                2 => ("PASIVO", centena switch { 21 => "CIRCULANTE", 22 => "FIJO", 23 => "DIFERIDO", _ => "CIRCULANTE" }),
                3 => ("CAPITAL", ""),
                _ => ("CUENTAS DE ORDEN", "")
            };
        }

        private static DateTime ParsearFechaCorta(string fechaTexto)
        {
            if (DateTime.TryParseExact(fechaTexto?.Trim(), ["dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt;

            return DateTime.Today;
        }

        private static void AgregarFilaPoliza(DataTable tabla, PolizaJson poliza)
        {
            tabla.Rows.Add(
                poliza.Id,
                poliza.Fecha,
                poliza.Folio,
                poliza.Concepto.ToUpper(),
                poliza.EsCostos,
                poliza.TipoCaptura,
                poliza.TotalDebe,
                poliza.TotalHaber,
                poliza.Beneficiario.ToUpper(),
                poliza.MontoCheque,
                poliza.NumeroCheque.ToUpper(),
                poliza.RFC.ToUpper(),
                poliza.FoliosFiscales.ToUpper(),
                poliza.RequiereImpresion,
                poliza.CreadoEn
            );
        }

        private static DataTable CrearTablaPolizas()
        {
            DataTable tabla = new();
            tabla.Columns.Add("id", typeof(int));
            tabla.Columns.Add("fecha", typeof(DateTime));
            tabla.Columns.Add("folio", typeof(int));
            tabla.Columns.Add("concepto", typeof(string));
            tabla.Columns.Add("es_costos", typeof(bool));
            tabla.Columns.Add("tipo_captura", typeof(byte));
            tabla.Columns.Add("total_debe", typeof(decimal));
            tabla.Columns.Add("total_haber", typeof(decimal));
            tabla.Columns.Add("beneficiario", typeof(string));
            tabla.Columns.Add("monto_cheque", typeof(decimal));
            tabla.Columns.Add("numero_cheque", typeof(string));
            tabla.Columns.Add("rfc", typeof(string));
            tabla.Columns.Add("folios_fiscales", typeof(string));
            tabla.Columns.Add("requiere_impresion", typeof(bool));
            tabla.Columns.Add("creado_en", typeof(DateTime));
            return tabla;
        }
    }
}

