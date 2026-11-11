using System.Text.Json;
using System.Text.Json.Serialization;

namespace CapturaDePolizas_2026_NET8.Business;

public class RegistroProyectoJson
{
    [JsonPropertyName("mes")]
    public int Mes { get; set; }

    [JsonPropertyName("poliza")]
    public int Poliza { get; set; }

    [JsonPropertyName("proyecto")]
    public string Proyecto { get; set; } = string.Empty;
}

public static class ProyectoJsonService
{
    private static string ObtenerRutaJson()
    {
        return Path.Combine(AppContext.BaseDirectory, "proyectos.json");
    }

    public static void GuardarProyectos(int mes, int numeroPoliza, IEnumerable<string> proyectos)
    {
        List<string> limpios = proyectos
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim().ToUpper())
            .ToList();

        if (limpios.Count == 0)
            return;

        string ruta = ObtenerRutaJson();
        List<RegistroProyectoJson> lista = LeerTodos();

        foreach (string proy in limpios)
        {
            lista.Add(new RegistroProyectoJson
            {
                Mes = mes,
                Poliza = numeroPoliza,
                Proyecto = proy
            });
        }

        var opciones = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(lista, opciones);
        File.WriteAllText(ruta, json);
    }

    public static List<RegistroProyectoJson> LeerTodos()
    {
        string ruta = ObtenerRutaJson();

        if (!File.Exists(ruta))
            return new List<RegistroProyectoJson>();

        try
        {
            string contenido = File.ReadAllText(ruta);
            if (string.IsNullOrWhiteSpace(contenido))
                return new List<RegistroProyectoJson>();

            return JsonSerializer.Deserialize<List<RegistroProyectoJson>>(contenido)
                ?? new List<RegistroProyectoJson>();
        }
        catch
        {
            return new List<RegistroProyectoJson>();
        }
    }
}
