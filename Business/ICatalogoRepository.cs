using CapturaDePolizas_2026_NET8.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Repositories
{
    public interface ICatalogoRepository
    {
        bool ValidarCuenta(string cuentaId, out string nombreCuenta);
        bool ValidarSubcuenta(string subcuentaId, string cuentaPadreId, out string nombreSubcuenta);
        int ObtenerSiguienteFolio();
        string ObtenerSiguienteNumeroCheque();
        bool GuardarPoliza(PolizaModel model);
        DataTable ObtenerTablaCuentas();
        DataTable ObtenerTablaSubcuentas();
        DataTable ObtenerCuentasBusqueda();
        DataTable ObtenerSubcuentasBusqueda(int cuentaId);
        DataTable ObtenerTablaPolizas();
        DataTable ObtenerMovimientosPoliza(int polizaId);
        DataTable ObtenerTablaVacia();
        DataTable ObtenerEstadosFinancieros(bool incluirCuentasOrden);
    }
}
