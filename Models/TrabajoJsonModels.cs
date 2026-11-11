namespace GaCostos.Models;

public static class SesionTrabajoJson
{
    public static bool UsarJson { get; set; }
    public static TrabajoJson? Actual { get; set; }
    public static string? RutaArchivo { get; set; }
    public static bool Modificado { get; set; }

    public static void Activar(TrabajoJson trabajo, string rutaArchivo)
    {
        Actual = trabajo;
        RutaArchivo = rutaArchivo;
        UsarJson = true;
        Modificado = false;
    }

    public static void Desactivar()
    {
        UsarJson = false;
    }
}

public sealed class TrabajoJson
{
    public string Origen { get; set; } = string.Empty;
    public DateTime Generado { get; set; }
    public List<CuentaJson> Cuentas { get; set; } = [];
    public List<PolizaJson> Polizas { get; set; } = [];
}

public sealed class CuentaJson
{
    public string Cuenta { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string? TipoCuenta { get; set; }
    public string? Clasificacion { get; set; }
    public List<SubcuentaJson> Subcuentas { get; set; } = [];
}

public sealed class SubcuentaJson
{
    public string Cuenta { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public bool Activa { get; set; } = true;
    public List<MovimientoJson> Movimientos { get; set; } = [];
}

public sealed class MovimientoJson
{
    public string Fecha { get; set; } = string.Empty;
    public short Poliza { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Importe { get; set; }
}

public sealed class PolizaJson
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Folio { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public bool EsCostos { get; set; }
    public byte TipoCaptura { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public string Beneficiario { get; set; } = string.Empty;
    public decimal MontoCheque { get; set; }
    public string NumeroCheque { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string FoliosFiscales { get; set; } = string.Empty;
    public bool RequiereImpresion { get; set; }
    public DateTime CreadoEn { get; set; }
    public List<MovimientoPolizaJson> Movimientos { get; set; } = [];
}

public sealed class MovimientoPolizaJson
{
    public int Id { get; set; }
    public int CuentaId { get; set; }
    public int? SubcuentaId { get; set; }
    public string FolioFiscal { get; set; } = string.Empty;
    public decimal Parcial { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Redaccion { get; set; } = string.Empty;
    public string Beneficiario { get; set; } = string.Empty;
    public bool PerteneceConcepto { get; set; }
    public DateTime CreadoEn { get; set; }
}

public sealed class ResultadoGuardadoBd
{
    public int CuentasNuevas { get; set; }
    public int CuentasActualizadas { get; set; }
    public int SubcuentasNuevas { get; set; }
    public int SubcuentasActualizadas { get; set; }
    public int PolizasNuevas { get; set; }
    public int Omitidas { get; set; }
}
