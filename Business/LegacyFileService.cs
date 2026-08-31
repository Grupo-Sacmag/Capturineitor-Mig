using System.Text;
using CapturaDePolizas_2026_NET8.Business;
using GaCostos.Models;

namespace GaCostos.Services;

public sealed class LegacyFileService
{
    private const int LongitudRegistroCatalogo = 64;
    private const int LongitudRegistroAuxiliar = 56;
    private const int LongitudRegistroOperacion = 64;

    private readonly Encoding _encoding;

    public LegacyFileService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encoding = Encoding.GetEncoding(1252);
    }

    public IReadOnlyList<CuentaMayor> LeerCatalogoMayor(string rutaDatos)
    {
        ValidarRutaDatos(rutaDatos);

        string filePath = Path.Combine(rutaDatos, "catmay");

        if (!File.Exists(filePath))
            return [];

        List<CuentaMayor> cuentas = [];

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        using BinaryReader reader = new(fs, _encoding);

        while (fs.Position + LongitudRegistroCatalogo <= fs.Length)
        {
            byte[] buffer = reader.ReadBytes(LongitudRegistroCatalogo);
            string registro = _encoding.GetString(buffer);

            string cuenta = ObtenerTexto(registro, 0, 6);

            if (string.IsNullOrWhiteSpace(cuenta))
                continue;

            cuentas.Add(new CuentaMayor(
                Cuenta: cuenta,
                Nombre: ObtenerTexto(registro, 6, 32),
                Saldo: FormatHelper.FormatearSaldo(ObtenerTexto(registro, 38, 16)),
                RangoInferior: ObtenerEntero(registro, 54, 5),
                RangoSuperior: ObtenerEntero(registro, 59, 5)));
        }

        return cuentas;
    }

    public IReadOnlyList<CuentaAuxiliar> LeerCatalogoAuxiliar(string rutaDatos, int rangoInferior, int rangoSuperior)
    {
        ValidarRutaDatos(rutaDatos);

        if (rangoInferior <= 0)
            return [];

        if (rangoSuperior < rangoInferior)
            return [];

        string filePath = Path.Combine(rutaDatos, "cataux");

        if (!File.Exists(filePath))
            return [];

        List<CuentaAuxiliar> cuentas = [];

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        using BinaryReader reader = new(fs, _encoding);

        for (int posicion = rangoInferior; posicion <= rangoSuperior; posicion++)
        {
            long offset = (long)(posicion - 1) * LongitudRegistroCatalogo;

            if (offset + LongitudRegistroCatalogo > fs.Length)
                break;

            fs.Seek(offset, SeekOrigin.Begin);

            byte[] buffer = reader.ReadBytes(LongitudRegistroCatalogo);
            string registro = _encoding.GetString(buffer);

            string cuenta = ObtenerTexto(registro, 0, 6);

            if (string.IsNullOrWhiteSpace(cuenta) || cuenta == "0")
                continue;

            cuentas.Add(new CuentaAuxiliar(
                Cuenta: cuenta,
                Nombre: ObtenerTexto(registro, 6, 32),
                Saldo: FormatHelper.FormatearSaldo(ObtenerTexto(registro, 38, 16)),
                RangoInferior: ObtenerEntero(registro, 54, 5),
                RangoSuperior: ObtenerEntero(registro, 59, 5),
                Guia: posicion));
        }

        return cuentas;
    }

    public AuxiliarResultado LeerAuxiliar(string rutaDatos, int guia, int mesProceso = 0)
    {
        ValidarRutaDatos(rutaDatos);

        if (guia <= 0)
            return new AuxiliarResultado(0m, []);

        string filePath = Path.Combine(rutaDatos, "AUXILIAR", $"AX{guia}");

        if (!File.Exists(filePath))
            return new AuxiliarResultado(0m, []);

        decimal saldoInicial = 0m;
        List<MovimientoAuxiliar> movimientosSinSaldo = [];

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        byte[] buffer = new byte[LongitudRegistroAuxiliar];

        while (fs.Read(buffer, 0, LongitudRegistroAuxiliar) == LongitudRegistroAuxiliar)
        {
            string fecha = _encoding.GetString(buffer, 0, 8).Trim().Replace("\0", "");

            if (fecha.Length < 8)
                continue;

            if (!int.TryParse(fecha.Substring(3, 2), out int mesRegistro))
                continue;

            short poliza = BitConverter.ToInt16(buffer, 8);

            string concepto = _encoding.GetString(buffer, 10, 30).Trim().Replace("\0", "");

            decimal importe = Convert.ToDecimal(BitConverter.ToDouble(buffer, 40));

            if (mesProceso > 0 && (mesRegistro < mesProceso || mesRegistro == 13))
            {
                saldoInicial += importe;
                continue;
            }

            if (mesProceso > 0 && mesRegistro != mesProceso)
                continue;

            movimientosSinSaldo.Add(new MovimientoAuxiliar(
                Fecha: fecha,
                Poliza: poliza,
                Concepto: concepto,
                Importe: importe,
                SaldoAcumulado: 0m));
        }

        List<MovimientoAuxiliar> movimientosOrdenados = movimientosSinSaldo.OrderBy(m => CrearLlaveOrdenFecha(m.Fecha, m.Poliza)).ToList();

        decimal acumulado = mesProceso > 0 ? saldoInicial : 0m;

        List<MovimientoAuxiliar> movimientosConSaldo = [];

        foreach (MovimientoAuxiliar movimiento in movimientosOrdenados)
        {
            acumulado += movimiento.Importe;

            movimientosConSaldo.Add(movimiento with
            {
                SaldoAcumulado = acumulado
            });
        }

        return new AuxiliarResultado(SaldoInicial: saldoInicial, Movimientos: movimientosConSaldo);
    }

    public string LeerNumeroArchivoDesdeDatos(string rutaDatos)
    {
        ValidarRutaDatos(rutaDatos);

        string filePath = Path.Combine(rutaDatos, "DATOS");

        if (!File.Exists(filePath))
            return string.Empty;

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        byte[] buffer = new byte[231];

        int bytesLeidos = fs.Read(buffer, 0, buffer.Length);

        if (bytesLeidos < 184)
            return string.Empty;

        string numeroArchivo = _encoding.GetString(buffer, 169, 15);

        return LimpiarTexto(numeroArchivo);
    }

    public IReadOnlyList<RegistroOperacion> BuscarPoliza(
    string rutaDatos,
    string numeroArchivo,
    string mes,
    int numeroPoliza)
    {
        ValidarRutaDatos(rutaDatos);

        string numeroArchivoLimpio = LimpiarTexto(numeroArchivo);

        if (string.IsNullOrWhiteSpace(numeroArchivoLimpio))
            return [];

        if (numeroPoliza <= 0)
            return [];

        string mesArchivo = NormalizarMesArchivo(mes);

        string filePath = ResolverArchivoOperaciones(
            rutaDatos,
            numeroArchivoLimpio,
            mesArchivo);

        if (string.IsNullOrWhiteSpace(filePath))
            return [];

        List<RegistroOperacion> operaciones = LeerOperaciones(filePath);

        int indiceObjetivo = BuscarIndiceOperacionObjetivo(
            operaciones,
            numeroPoliza);

        if (indiceObjetivo < 0)
            return [];

        int indiceInicio = BuscarInicioPoliza(
            operaciones,
            indiceObjetivo);

        if (indiceInicio < 0)
            return [];

        return ExtraerBloquePoliza(
            operaciones,
            indiceInicio);
    }

    private List<RegistroOperacion> LeerOperaciones(string filePath)
    {
        List<RegistroOperacion> operaciones = [];

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        using BinaryReader reader = new(fs, _encoding);

        while (fs.Position + LongitudRegistroOperacion <= fs.Length)
        {
            byte[] buffer = reader.ReadBytes(LongitudRegistroOperacion);

            operaciones.Add(new RegistroOperacion(
                Cuenta: _encoding.GetString(buffer, 0, 6).Trim(),
                Descripcion: _encoding.GetString(buffer, 6, 30).Trim(),
                Fecha: _encoding.GetString(buffer, 36, 2).Trim(),
                Importe: FormatHelper.ParseDecimalFlexible(_encoding.GetString(buffer, 38, 16).Trim()),
                Identificador: _encoding.GetString(buffer, 54, 1).Trim(),
                Real: _encoding.GetString(buffer, 55, 9).Trim()));
        }

        return operaciones;
    }

    private static int BuscarIndiceOperacionObjetivo(IReadOnlyList<RegistroOperacion> operaciones, int numeroPoliza)
    {
        for (int i = 0; i < operaciones.Count; i++)
        {
            RegistroOperacion operacion = operaciones[i];

            if (operacion.Identificador == "C" && int.TryParse(operacion.Real, out int real) && real == numeroPoliza)
            {
                return i;
            }
        }

        for (int i = 0; i < operaciones.Count; i++)
        {
            RegistroOperacion operacion = operaciones[i];

            if (operacion.Identificador == "A" && int.TryParse(operacion.Cuenta, out int cuenta) && cuenta == numeroPoliza)
            {
                return i;
            }
        }

        return -1;
    }

    private static int BuscarInicioPoliza(IReadOnlyList<RegistroOperacion> operaciones, int indiceObjetivo)
    {
        for (int i = indiceObjetivo; i >= 0; i--)
        {
            if (operaciones[i].Identificador == "A")
                return i;
        }

        return -1;
    }

    private static IReadOnlyList<RegistroOperacion> ExtraerBloquePoliza(IReadOnlyList<RegistroOperacion> operaciones, int indiceInicio)
    {
        List<RegistroOperacion> resultado = [];

        for (int i = indiceInicio; i < operaciones.Count; i++)
        {
            if (i > indiceInicio && operaciones[i].Identificador == "A")
                break;

            resultado.Add(operaciones[i]);
        }

        return resultado;
    }

    private static string CrearLlaveOrdenFecha(string fecha, int poliza)
    {
        if (fecha.Length < 8)
            return fecha;

        string dia = fecha.Substring(0, 2);
        string mes = fecha.Substring(3, 2);

        return $"{mes}{dia}{poliza:D6}";
    }

    private static string ObtenerTexto(string registro, int inicio, int longitud)
    {
        if (registro.Length <= inicio)
            return string.Empty;

        if (registro.Length < inicio + longitud)
            longitud = registro.Length - inicio;

        return registro.Substring(inicio, longitud).Trim().Replace("\0", "");
    }

    private static int ObtenerEntero(string registro, int inicio, int longitud)
    {
        string texto = ObtenerTexto(registro, inicio, longitud);

        return int.TryParse(texto, out int resultado) ? resultado : 0;
    }

    private static void ValidarRutaDatos(string rutaDatos)
    {
        if (string.IsNullOrWhiteSpace(rutaDatos))
            throw new ArgumentException("La ruta de datos no puede estar vacía.", nameof(rutaDatos));

        if (!Directory.Exists(rutaDatos))
            throw new DirectoryNotFoundException($"No existe la ruta de datos: {rutaDatos}");
    }

    private static string ResolverArchivoOperaciones(string rutaDatos, string numeroArchivo, string mesArchivo)
    {
        string numeroArchivoLimpio = LimpiarTexto(numeroArchivo);

        if (string.IsNullOrWhiteSpace(numeroArchivoLimpio))
            return string.Empty;

        string archivoExacto = Path.Combine(
            rutaDatos,
            numeroArchivoLimpio + mesArchivo);

        if (File.Exists(archivoExacto))
            return archivoExacto;

        try
        {
            List<string> archivosDelMes = Directory.EnumerateFiles(rutaDatos)
                .Where(path =>
                {
                    string nombre = Path.GetFileName(path);

                    return nombre.EndsWith(
                        mesArchivo,
                        StringComparison.OrdinalIgnoreCase);
                }).ToList();

            if (archivosDelMes.Count == 0)
                return string.Empty;

            List<string> archivosConPrefijo = archivosDelMes
                .Where(path =>
                {
                    string nombre = Path.GetFileName(path);

                    return nombre.StartsWith(
                        numeroArchivoLimpio,
                        StringComparison.OrdinalIgnoreCase);
                }).ToList();

            if (archivosConPrefijo.Count == 1)
                return archivosConPrefijo[0];

            if (archivosConPrefijo.Count > 1)
            {
                return archivosConPrefijo.OrderBy(path => Path.GetFileName(path).Length).ThenBy(path => Path.GetFileName(path)).First();
            }

            if (archivosDelMes.Count == 1)
                return archivosDelMes[0];

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizarMesArchivo(string mes)
    {
        string mesLimpio = LimpiarTexto(mes);

        if (mesLimpio == "00")
            return "13";

        if (string.IsNullOrWhiteSpace(mesLimpio))
            return "13";

        return mesLimpio.PadLeft(2, '0');
    }

    private static string LimpiarTexto(string valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;

        return valor
            .Replace("\0", string.Empty)
            .Trim();
    }
}