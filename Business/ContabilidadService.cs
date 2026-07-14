using CapturaDePolizas_2026_NET8.Business;
using GaCostos.Models;

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
}