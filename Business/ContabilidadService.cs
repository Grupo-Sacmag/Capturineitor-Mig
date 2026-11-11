using CapturaDePolizas_2026_NET8.Business;
using GaCostos.Models;
using Microsoft.EntityFrameworkCore;

namespace GaCostos.Services;

public sealed class ContabilidadService
{
    private readonly LegacyFileService _legacyFileService;

    public ContabilidadService()
    {
        _legacyFileService = new LegacyFileService();
    }

    public IReadOnlyList<CuentaMayor> ObtenerCatalogoMayor(string rutaDatos)
    {
        return _legacyFileService.LeerCatalogoMayor(rutaDatos);
    }

    public IReadOnlyList<CuentaAuxiliar> ObtenerCatalogoAuxiliar(string rutaDatos, int rangoInferior, int rangoSuperior)
    {
        return _legacyFileService.LeerCatalogoAuxiliar(rutaDatos, rangoInferior, rangoSuperior);
    }

    public AuxiliarResultado ObtenerMovimientosAuxiliar(string rutaDatos, int guia, int mesProceso = 0)
    {
        return _legacyFileService.LeerAuxiliar(rutaDatos, guia, mesProceso);
    }

    public IReadOnlyList<RegistroOperacion> ObtenerPolizaDiario(string rutaDatos, string fechaMovimiento, int numeroPoliza)
    {
        string numeroArchivo = _legacyFileService.LeerNumeroArchivoDesdeDatos(rutaDatos);

        if (string.IsNullOrWhiteSpace(numeroArchivo))
            return [];

        string mes = FormatHelper.ObtenerMesDesdeFecha(fechaMovimiento);

        return _legacyFileService.BuscarPoliza(rutaDatos, numeroArchivo, mes, numeroPoliza);
    }

    // ===== Origen: JSON de trabajo (posiciones base 1 como identificador de navegación) =====

    public IReadOnlyList<CuentaMayor> ObtenerCatalogoMayorJson(TrabajoJson json)
    {
        try
        {
            return json.Cuentas
                .Select((c, i) => new CuentaMayor(c.Cuenta, c.Nombre, FormatHelper.FormatearImporte(c.Saldo), i + 1, i + 1))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerCatalogoMayorJson: " + ex.Message, ex);
        }
    }

    public IReadOnlyList<CuentaAuxiliar> ObtenerCatalogoAuxiliarJson(TrabajoJson json, int posicionCuenta)
    {
        try
        {
            if (posicionCuenta < 1 || posicionCuenta > json.Cuentas.Count)
                return [];

            return json.Cuentas[posicionCuenta - 1].Subcuentas
                .Select((s, i) => new CuentaAuxiliar(s.Cuenta, s.Nombre, FormatHelper.FormatearImporte(s.Saldo), posicionCuenta, posicionCuenta, i + 1))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerCatalogoAuxiliarJson: " + ex.Message, ex);
        }
    }

    public AuxiliarResultado ObtenerMovimientosAuxiliarJson(TrabajoJson json, int posicionCuenta, int posicionSubcuenta)
    {
        try
        {
            if (posicionCuenta < 1 || posicionCuenta > json.Cuentas.Count)
                return new AuxiliarResultado(0m, []);

            List<SubcuentaJson> subs = json.Cuentas[posicionCuenta - 1].Subcuentas;

            if (posicionSubcuenta < 1 || posicionSubcuenta > subs.Count)
                return new AuxiliarResultado(0m, []);

            decimal acumulado = 0m;
            List<MovimientoAuxiliar> movimientos = [];

            foreach (MovimientoJson m in subs[posicionSubcuenta - 1].Movimientos)
            {
                acumulado += m.Importe;
                movimientos.Add(new MovimientoAuxiliar(m.Fecha, m.Poliza, m.Concepto, m.Importe, acumulado));
            }

            return new AuxiliarResultado(0m, movimientos);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerMovimientosAuxiliarJson: " + ex.Message, ex);
        }
    }

    public IReadOnlyList<RegistroOperacion> ObtenerPolizaDiarioJson(TrabajoJson json, int folio, string? fechaMovimiento = null)
    {
        try
        {
            string mesBuscado = FormatHelper.ObtenerMesDesdeFecha(fechaMovimiento ?? string.Empty);

            PolizaJson? poliza = null;
            if (int.TryParse(mesBuscado, out int mesNum) && mesNum >= 1 && mesNum <= 12)
            {
                poliza = json.Polizas.FirstOrDefault(p => p.Folio == folio && p.Fecha.Month == mesNum);
            }

            poliza ??= json.Polizas.FirstOrDefault(p => p.Folio == folio);

            if (poliza is null)
                return [];

            string fecha = poliza.Fecha.ToString("dd/MM/yy");
            List<RegistroOperacion> registros =
            [
                new RegistroOperacion(poliza.Folio.ToString(), poliza.Concepto, fecha, 0m, "A", string.Empty)
            ];

            foreach (var m in poliza.Movimientos.OrderBy(x => x.Id))
            {
                var cuenta = json.Cuentas.FirstOrDefault(c => int.TryParse(c.Cuenta.Trim(), out int cId) && cId == m.CuentaId);
                var sub = m.SubcuentaId.HasValue
                    ? cuenta?.Subcuentas.FirstOrDefault(s => int.TryParse(s.Cuenta.Trim(), out int sId) && sId == m.SubcuentaId.Value)
                    : null;

                decimal importe = m.Debe - m.Haber;

                if (importe != 0m)
                {
                    string descMayor = !string.IsNullOrWhiteSpace(cuenta?.Nombre) ? cuenta.Nombre.ToUpper() : m.Redaccion.ToUpper();
                    registros.Add(new RegistroOperacion(m.CuentaId.ToString(), descMayor, fecha, importe, "B", string.Empty));
                }

                if (m.SubcuentaId.HasValue)
                {
                    string descSub = !string.IsNullOrWhiteSpace(m.Redaccion)
                        ? m.Redaccion.ToUpper()
                        : (sub?.Nombre ?? string.Empty).ToUpper();

                    registros.Add(new RegistroOperacion(m.SubcuentaId.Value.ToString(), descSub, fecha, m.Parcial, "C", poliza.Folio.ToString()));
                }
            }

            return registros;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerPolizaDiarioJson: " + ex.Message, ex);
        }
    }

    // ===== Origen: Base de datos (SQL Server) =====

    /// <summary>Catálogo mayor desde la tabla cuentas. RangoInferior/RangoSuperior = Id de la cuenta (identificador de navegación).</summary>
    public IReadOnlyList<CuentaMayor> ObtenerCatalogoMayorBd()
    {
        try
        {
            using var db = new CapturaDePolizas_2026_NET8.Context.EmpresaDbContext();

            return db.Cuentas.AsNoTracking().OrderBy(c => c.Id).ToList()
                .Select(c => new CuentaMayor(
                    Cuenta: c.Id.ToString(),
                    Nombre: c.Nombre.ToUpper(),
                    Saldo: FormatHelper.FormatearImporte(c.MontoBruto),
                    RangoInferior: c.Id,
                    RangoSuperior: c.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerCatalogoMayorBd: " + ex.Message, ex);
        }
    }

    /// <summary>Catálogo auxiliar desde subcuentas de la cuenta indicada. Guia = Id de la subcuenta.</summary>
    public IReadOnlyList<CuentaAuxiliar> ObtenerCatalogoAuxiliarBd(int cuentaId)
    {
        try
        {
            using var db = new CapturaDePolizas_2026_NET8.Context.EmpresaDbContext();

            return db.Subcuentas.AsNoTracking().Where(s => s.CuentaId == cuentaId && s.Activa).OrderBy(s => s.Id).ToList()
                .Select(s => new CuentaAuxiliar(
                    Cuenta: s.Id.ToString(),
                    Nombre: s.Nombre.ToUpper(),
                    Saldo: FormatHelper.FormatearImporte(s.ImporteNeto),
                    RangoInferior: s.CuentaId,
                    RangoSuperior: s.CuentaId,
                    Guia: s.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerCatalogoAuxiliarBd: " + ex.Message, ex);
        }
    }

    /// <summary>Movimientos de una subcuenta. Importe = Debe - Haber; saldo acumulado por fecha y folio. Poliza = folio.</summary>
    public AuxiliarResultado ObtenerMovimientosAuxiliarBd(int cuentaId, int subcuentaId)
    {
        try
        {
            using var db = new CapturaDePolizas_2026_NET8.Context.EmpresaDbContext();

            var filas = (
                from m in db.Movimientos.AsNoTracking()
                join p in db.Polizas.AsNoTracking() on m.PolizaId equals p.Id
                where m.CuentaId == cuentaId && m.SubcuentaId == subcuentaId
                orderby p.Fecha, p.Folio, m.Id
                select new { p.Fecha, p.Folio, p.Concepto, m.Redaccion, m.Debe, m.Haber }
            ).ToList();

            List<MovimientoAuxiliar> movimientos = [];
            decimal acumulado = 0m;

            foreach (var fila in filas)
            {
                decimal importe = fila.Debe - fila.Haber;
                acumulado += importe;

                string concepto = string.IsNullOrWhiteSpace(fila.Redaccion) ? fila.Concepto : fila.Redaccion;

                movimientos.Add(new MovimientoAuxiliar(
                    Fecha: fila.Fecha.ToString("dd/MM/yy"),
                    Poliza: checked((short)fila.Folio),
                    Concepto: concepto,
                    Importe: importe,
                    SaldoAcumulado: acumulado));
            }

            return new AuxiliarResultado(0m, movimientos);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerMovimientosAuxiliarBd: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Póliza (por folio) en el mismo formato de RegistroOperacion que usa FormPolizaDiario:
    /// A = encabezado, B = cuenta mayor (importe = Debe - Haber), C = subcuenta (importe = Parcial).
    /// </summary>
    public IReadOnlyList<RegistroOperacion> ObtenerPolizaDiarioBd(int folio)
    {
        try
        {
            using var db = new CapturaDePolizas_2026_NET8.Context.EmpresaDbContext();

            var poliza = db.Polizas.AsNoTracking().FirstOrDefault(p => p.Folio == folio);

            if (poliza is null)
                return [];

            var movimientos = (
                from m in db.Movimientos.AsNoTracking()
                join c in db.Cuentas.AsNoTracking() on m.CuentaId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                join s in db.Subcuentas.AsNoTracking()
                    on new { CuentaId = m.CuentaId, Id = m.SubcuentaId ?? -1 }
                    equals new { CuentaId = s.CuentaId, Id = s.Id } into sj
                from s in sj.DefaultIfEmpty()
                where m.PolizaId == poliza.Id
                orderby m.Id
                select new
                {
                    m.CuentaId,
                    m.SubcuentaId,
                    NombreCuenta = c != null ? c.Nombre : string.Empty,
                    NombreSubcuenta = s != null ? s.Nombre : string.Empty,
                    m.Parcial,
                    m.Debe,
                    m.Haber
                }
            ).ToList();

            string fecha = poliza.Fecha.ToString("dd/MM/yy");
            List<RegistroOperacion> registros = [];

            registros.Add(new RegistroOperacion(poliza.Folio.ToString(), poliza.Concepto, fecha, 0m, "A", string.Empty));

            foreach (var m in movimientos)
            {
                decimal importe = m.Debe - m.Haber;

                if (importe != 0m)
                    registros.Add(new RegistroOperacion(m.CuentaId.ToString(), m.NombreCuenta.ToUpper(), fecha, importe, "B", string.Empty));

                if (m.SubcuentaId.HasValue)
                    registros.Add(new RegistroOperacion(m.SubcuentaId.Value.ToString(), m.NombreSubcuenta.ToUpper(), fecha, m.Parcial, "C", string.Empty));
            }

            return registros;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en ObtenerPolizaDiarioBd: " + ex.Message, ex);
        }
    }
}