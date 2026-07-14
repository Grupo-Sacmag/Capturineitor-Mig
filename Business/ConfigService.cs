namespace GaCostos.Services;

public sealed class ConfigService
{
    private const string ConfigFileName = "config.ini";

    private readonly string _appDirectory;
    private readonly string _configPath;

    public ConfigService()
    {
        _appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GaCostos");
        _configPath = Path.Combine(_appDirectory, ConfigFileName);
    }

    public string? LeerRutaDatos()
    {
        if (!File.Exists(_configPath))
            return null;

        string ruta = File.ReadAllText(_configPath).Trim();

        return string.IsNullOrWhiteSpace(ruta) ? null : ruta;
    }

    public void GuardarRutaDatos(string rutaDatos)
    {
        if (string.IsNullOrWhiteSpace(rutaDatos))
            throw new ArgumentException("La ruta de datos no puede estar vacía.", nameof(rutaDatos));

        Directory.CreateDirectory(_appDirectory);
        File.WriteAllText(_configPath, rutaDatos.Trim());
    }
}