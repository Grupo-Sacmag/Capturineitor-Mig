
using System.Text.Json.Serialization;

namespace CapturaDePolizas_2026_NET8.Models;
public sealed class ArchivoSesionesLocales
{
    [JsonPropertyName("slots")]
    public List<SesionLocal> Slots { get; set; } = new();
}

public sealed class SesionLocal
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("fecha_actualizacion")]
    public string FechaActualizacion { get; set; } = string.Empty;

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = "borrador";

    [JsonPropertyName("huella")]
    public string Huella { get; set; } = string.Empty;

    [JsonPropertyName("huella_modelo")]
    public string HuellaModelo { get; set; } = string.Empty;

    [JsonPropertyName("captura")]
    public CapturaSesionLocal Captura { get; set; } = new();

    [JsonPropertyName("poliza")]
    public PolizaModel Poliza { get; set; } = new();
}

public sealed class CapturaSesionLocal
{
    public byte TipoCaptura { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string ConceptoPoliza { get; set; } = string.Empty;
    public string ConceptoCheque { get; set; } = string.Empty;
    public string Beneficiario { get; set; } = string.Empty;
    public string MontoCheque { get; set; } = string.Empty;
    public string NumeroCheque { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string FoliosFiscales { get; set; } = string.Empty;
    public bool Varios { get; set; }
    public bool ChequeImpreso { get; set; }

    public List<Dictionary<string, string>> Filas { get; set; } = new();
}