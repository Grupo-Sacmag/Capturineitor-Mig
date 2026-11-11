
using CapturaDePolizas_2026_NET8.Models;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CapturaDePolizas_2026_NET8.Services;

public sealed class GestorSesionesLocales
{
    public const int MaximoSesiones = 5;
    public const string Borrador = "borrador";
    public const string EnProceso = "en_proceso";
    public const string Contabilizada = "contabilizada";

    private static readonly SemaphoreSlim Exclusividad = new(1, 1);

    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _raiz = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_storage");
    private string CarpetaSesiones => Path.Combine(_raiz, "Sessions");
    private string ArchivoSesiones => Path.Combine(CarpetaSesiones, "sessions.json");
    private string CarpetaRespaldos => Path.Combine(_raiz, "Backups");
    private string ArchivoMeta => Path.Combine(CarpetaRespaldos, "last_backup.meta");

    public static string CalcularHuella<T>(T objeto)
    {
        string json = JsonSerializer.Serialize(objeto, Opciones);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }

    private async Task<ArchivoSesionesLocales> LeerSinBloqueoAsync()
    {
        if (!File.Exists(ArchivoSesiones))
            return new ArchivoSesionesLocales();

        string contenido = await File.ReadAllTextAsync(ArchivoSesiones).ConfigureAwait(false);

        try
        {
            ArchivoSesionesLocales archivo = JsonSerializer.Deserialize<ArchivoSesionesLocales>(contenido, Opciones) ?? throw new InvalidDataException("Archivo de sesiones inválido.");

            if (archivo.Slots is null || archivo.Slots.Count > MaximoSesiones || archivo.Slots.Any(s => s is null || s.Id < 1 || s.Id > MaximoSesiones) || archivo.Slots.Select(s => s.Id).Distinct().Count() != archivo.Slots.Count)
            {
                throw new InvalidDataException("El contenido de sesiones no es válido.");
            }

            return archivo;
        }
        catch (JsonException)
        {
            throw new InvalidDataException("El archivo de sesiones no se pudo interpretar.");
        }
    }

    private async Task EscribirSinBloqueoAsync(ArchivoSesionesLocales archivo)
    {
        Directory.CreateDirectory(CarpetaSesiones);

        string temporal = Path.Combine(CarpetaSesiones, $"sessions.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllTextAsync(temporal, JsonSerializer.Serialize(archivo, Opciones), Encoding.UTF8).ConfigureAwait(false);
            File.Move(temporal, ArchivoSesiones, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporal))
                File.Delete(temporal);
        }
    }

    public async Task<List<SesionLocal>> ListarAsync()
    {
        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            return (await LeerSinBloqueoAsync().ConfigureAwait(false)).Slots.OrderBy(s => s.Id).ToList();
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public async Task<SesionLocal?> ObtenerAsync(int id)
    {
        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            return (await LeerSinBloqueoAsync().ConfigureAwait(false)).Slots.FirstOrDefault(s => s.Id == id);
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public async Task<SesionLocal> GuardarAsync(int id, string nombre, CapturaSesionLocal captura, PolizaModel poliza, bool permitirSobreescritura)
    {
        if (id < 1 || id > MaximoSesiones)
            throw new InvalidOperationException("Solo hay cinco espacios de trabajo.");

        nombre = (nombre ?? string.Empty).Trim();

        if (nombre.Length < 1 || nombre.Length > 100)
            throw new InvalidOperationException("El nombre debe tener entre 1 y 100 caracteres.");

        ArgumentNullException.ThrowIfNull(captura);
        ArgumentNullException.ThrowIfNull(poliza);

        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            ArchivoSesionesLocales archivo = await LeerSinBloqueoAsync().ConfigureAwait(false);
            SesionLocal? existente = archivo.Slots.FirstOrDefault(s => s.Id == id);

            if (existente is not null && !permitirSobreescritura)
                throw new InvalidOperationException("El espacio ya está ocupado.");

            if (existente?.Estado == EnProceso)
                throw new InvalidOperationException("Esta sesión requiere verificar su contabilización antes de modificarse.");

            if (existente is null && archivo.Slots.Count >= MaximoSesiones)
                throw new InvalidOperationException("Las cinco sesiones están ocupadas.");

            var nueva = new SesionLocal
            {
                Id = id,
                Nombre = nombre,
                FechaActualizacion = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                Estado = Borrador,
                Captura = captura,
                Poliza = poliza,
                Huella = CalcularHuella(captura),
                HuellaModelo = CalcularHuella(poliza)
            };

            if (existente is not null)
                archivo.Slots.Remove(existente);

            archivo.Slots.Add(nueva);
            archivo.Slots = archivo.Slots.OrderBy(s => s.Id).ToList();

            await EscribirSinBloqueoAsync(archivo).ConfigureAwait(false);

            return nueva;
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public async Task RenombrarAsync(int id, string nuevoNombre)
    {
        nuevoNombre = (nuevoNombre ?? string.Empty).Trim();

        if (nuevoNombre.Length is < 1 or > 100)
            throw new InvalidOperationException("El nombre debe tener entre 1 y 100 caracteres.");

        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            ArchivoSesionesLocales archivo = await LeerSinBloqueoAsync().ConfigureAwait(false);
            SesionLocal actual = archivo.Slots.FirstOrDefault(s => s.Id == id) ?? throw new InvalidOperationException("La sesión seleccionada ya no existe.");

            if (actual.Estado == EnProceso)
                throw new InvalidOperationException("La sesión está pendiente de revisión.");

            actual.Nombre = nuevoNombre;
            actual.FechaActualizacion = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            await EscribirSinBloqueoAsync(archivo).ConfigureAwait(false);
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public async Task EliminarAsync(int id)
    {
        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            ArchivoSesionesLocales archivo = await LeerSinBloqueoAsync().ConfigureAwait(false);
            SesionLocal actual = archivo.Slots.FirstOrDefault(s => s.Id == id) ?? throw new InvalidOperationException("La sesión seleccionada ya no existe.");

            if (actual.Estado == EnProceso)
                throw new InvalidOperationException("La contabilización de esta sesión debe verificarse antes de eliminarla.");

            archivo.Slots.Remove(actual);

            await EscribirSinBloqueoAsync(archivo).ConfigureAwait(false);
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public async Task<SesionLocal> ValidarGuardadoAsync(int id, string huellaCaptura, string huellaModelo)
    {
        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            SesionLocal s = (await LeerSinBloqueoAsync().ConfigureAwait(false)).Slots.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("La sesión ya no está guardada.");

            if (s.Estado != Borrador)
                throw new InvalidOperationException("La sesión no está disponible para contabilizar.");

            if (s.Huella != huellaCaptura || s.Huella != CalcularHuella(s.Captura) || s.HuellaModelo != huellaModelo || s.HuellaModelo != CalcularHuella(s.Poliza))
            {
                throw new InvalidOperationException("La captura cambió después de guardarse. Guárdala localmente otra vez.");
            }

            return s;
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public Task MarcarEnProcesoAsync(int id, string huella) => CambiarEstadoAsync(id, huella, Borrador, EnProceso);
    public Task MarcarContabilizadaAsync(int id, string huella) => CambiarEstadoAsync(id, huella, EnProceso, Contabilizada);
    public Task RecuperarBorradorRevisadoAsync(int id, string huella) => CambiarEstadoAsync(id, huella, EnProceso, Borrador);

    private async Task CambiarEstadoAsync(int id, string huella, string estadoAnterior, string nuevoEstado)
    {
        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            ArchivoSesionesLocales archivo = await LeerSinBloqueoAsync().ConfigureAwait(false);
            SesionLocal s = archivo.Slots.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("No se encontró la sesión local.");

            if (s.Huella != huella || s.Estado != estadoAnterior)
                throw new InvalidOperationException("El estado local de la sesión cambió.");

            s.Estado = nuevoEstado;

            await EscribirSinBloqueoAsync(archivo).ConfigureAwait(false);
        }
        finally
        {
            Exclusividad.Release();
        }
    }

    public async Task GenerarRespaldoSemanalSiCorrespondeAsync()
    {
        await Exclusividad.WaitAsync().ConfigureAwait(false);

        try
        {
            DateTime hoy = DateTime.Today;

            string semana = $"{ISOWeek.GetYear(hoy):0000}-{ISOWeek.GetWeekOfYear(hoy):00}";

            Directory.CreateDirectory(CarpetaRespaldos);

            if (File.Exists(ArchivoMeta) && (await File.ReadAllTextAsync(ArchivoMeta).ConfigureAwait(false)).Trim() == semana)
            {
                return;
            }

            byte[] contenido = File.Exists(ArchivoSesiones) ? await File.ReadAllBytesAsync(ArchivoSesiones).ConfigureAwait(false) : Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ArchivoSesionesLocales(), Opciones));
            string destino = Path.Combine(CarpetaRespaldos, $"backup_{hoy:yyyy_MM_dd}.json");

            if (!File.Exists(destino))
            {
                string temporal = Path.Combine(CarpetaRespaldos, $"backup.{Guid.NewGuid():N}.tmp");

                try
                {
                    await File.WriteAllBytesAsync(temporal, contenido).ConfigureAwait(false);
                    File.Move(temporal, destino);
                }
                finally
                {
                    if (File.Exists(temporal))
                        File.Delete(temporal);
                }
            }

            string metaTemporal = Path.Combine(CarpetaRespaldos, $"meta.{Guid.NewGuid():N}.tmp");

            try
            {
                await File.WriteAllTextAsync(metaTemporal, semana, Encoding.UTF8).ConfigureAwait(false);
                File.Move(metaTemporal, ArchivoMeta, overwrite: true);
            }
            finally
            {
                if (File.Exists(metaTemporal))
                    File.Delete(metaTemporal);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"No se pudo generar el respaldo semanal: {ex.GetType().Name}");
        }
        finally
        {
            Exclusividad.Release();
        }
    }
}