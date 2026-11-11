using System.Text.Json;

namespace CapturaDePolizas_2026_NET8.Models;

public class ChequeTemplateConfig
{
    public string NombreBanco { get; set; } = "Banamex (Estándar)";
    public string FontName { get; set; } = "Arial";
    public float FontSize { get; set; } = 10f;

    // Ajuste fino global de impresora (en milímetros)
    public float OffsetX { get; set; } = 0f;
    public float OffsetY { get; set; } = 0f;

    // Parte superior: Cheque (en milímetros)
    public float FechaX { get; set; } = 135f;
    public float FechaY { get; set; } = 14f;

    public float BeneficiarioX { get; set; } = 18f;
    public float BeneficiarioY { get; set; } = 24f;

    public float ImporteNumeroX { get; set; } = 145f;
    public float ImporteNumeroY { get; set; } = 24f;

    public float ImporteLetraX { get; set; } = 18f;
    public float ImporteLetraY { get; set; } = 35f;

    // Parte inferior: Póliza Contable (en milímetros)
    public float ConceptoX { get; set; } = 20f;
    public float ConceptoY { get; set; } = 92f;

    public float InicioAplicacionY { get; set; } = 112f;
    public float InterlineadoAplicacionMm { get; set; } = 4.8f;
    public float SumasIgualesY { get; set; } = 168f;

    // Columnas de la tabla contable (en milímetros, posición X)
    public float CuentaX { get; set; } = 15f;
    public float SubcuentaX { get; set; } = 32f;
    public float NombreX { get; set; } = 50f;
    public float ParcialX { get; set; } = 132f;
    public float DebeX { get; set; } = 162f;
    public float HaberX { get; set; } = 192f;

    public ChequeTemplateConfig Clonar()
    {
        return (ChequeTemplateConfig)MemberwiseClone();
    }
}

public class ChequePrintSettings
{
    public string PlantillaActiva { get; set; } = "Banamex (Estándar)";
    public List<ChequeTemplateConfig> Plantillas { get; set; } = new();

    private static string ObtenerRutaArchivo()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GaCostos"
        );
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "cheque_config.json");
    }

    public static ChequePrintSettings Cargar()
    {
        string path = ObtenerRutaArchivo();

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<ChequePrintSettings>(json);
                if (settings is not null && settings.Plantillas.Count > 0)
                {
                    return settings;
                }
            }
            catch
            {
                // Si el archivo está corrupto, se restauran las plantillas por defecto
            }
        }

        var porDefecto = CrearPorDefecto();
        porDefecto.Guardar();
        return porDefecto;
    }

    public void Guardar()
    {
        string path = ObtenerRutaArchivo();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(path, json);
    }

    public ChequeTemplateConfig ObtenerPlantillaActiva()
    {
        var plantilla = Plantillas.FirstOrDefault(p =>
            p.NombreBanco.Equals(PlantillaActiva, StringComparison.OrdinalIgnoreCase));

        if (plantilla is not null)
            return plantilla;

        if (Plantillas.Count > 0)
            return Plantillas[0];

        var banamex = new ChequeTemplateConfig();
        Plantillas.Add(banamex);
        return banamex;
    }

    public static ChequePrintSettings CrearPorDefecto()
    {
        return new ChequePrintSettings
        {
            PlantillaActiva = "Banamex (Estándar)",
            Plantillas = new List<ChequeTemplateConfig>
            {
                new ChequeTemplateConfig
                {
                    NombreBanco = "Banamex (Estándar)",
                    FechaX = 135f,
                    FechaY = 14f,
                    BeneficiarioX = 18f,
                    BeneficiarioY = 24f,
                    ImporteNumeroX = 145f,
                    ImporteNumeroY = 24f,
                    ImporteLetraX = 18f,
                    ImporteLetraY = 35f,
                    ConceptoX = 20f,
                    ConceptoY = 92f,
                    InicioAplicacionY = 112f,
                    InterlineadoAplicacionMm = 4.8f,
                    SumasIgualesY = 168f,
                    CuentaX = 15f,
                    SubcuentaX = 32f,
                    NombreX = 50f,
                    ParcialX = 132f,
                    DebeX = 162f,
                    HaberX = 192f
                },
                new ChequeTemplateConfig
                {
                    NombreBanco = "BBVA",
                    FechaX = 132f,
                    FechaY = 15f,
                    BeneficiarioX = 18f,
                    BeneficiarioY = 25f,
                    ImporteNumeroX = 145f,
                    ImporteNumeroY = 25f,
                    ImporteLetraX = 18f,
                    ImporteLetraY = 36f,
                    ConceptoX = 20f,
                    ConceptoY = 92f,
                    InicioAplicacionY = 112f,
                    InterlineadoAplicacionMm = 4.8f,
                    SumasIgualesY = 168f,
                    CuentaX = 15f,
                    SubcuentaX = 32f,
                    NombreX = 50f,
                    ParcialX = 132f,
                    DebeX = 162f,
                    HaberX = 192f
                },
                new ChequeTemplateConfig
                {
                    NombreBanco = "Banorte",
                    FechaX = 134f,
                    FechaY = 14f,
                    BeneficiarioX = 18f,
                    BeneficiarioY = 24f,
                    ImporteNumeroX = 144f,
                    ImporteNumeroY = 24f,
                    ImporteLetraX = 18f,
                    ImporteLetraY = 35f,
                    ConceptoX = 20f,
                    ConceptoY = 92f,
                    InicioAplicacionY = 112f,
                    InterlineadoAplicacionMm = 4.8f,
                    SumasIgualesY = 168f,
                    CuentaX = 15f,
                    SubcuentaX = 32f,
                    NombreX = 50f,
                    ParcialX = 132f,
                    DebeX = 162f,
                    HaberX = 192f
                },
                new ChequeTemplateConfig
                {
                    NombreBanco = "Santander",
                    FechaX = 135f,
                    FechaY = 14f,
                    BeneficiarioX = 18f,
                    BeneficiarioY = 24f,
                    ImporteNumeroX = 145f,
                    ImporteNumeroY = 24f,
                    ImporteLetraX = 18f,
                    ImporteLetraY = 35f,
                    ConceptoX = 20f,
                    ConceptoY = 92f,
                    InicioAplicacionY = 112f,
                    InterlineadoAplicacionMm = 4.8f,
                    SumasIgualesY = 168f,
                    CuentaX = 15f,
                    SubcuentaX = 32f,
                    NombreX = 50f,
                    ParcialX = 132f,
                    DebeX = 162f,
                    HaberX = 192f
                }
            }
        };
    }
}
