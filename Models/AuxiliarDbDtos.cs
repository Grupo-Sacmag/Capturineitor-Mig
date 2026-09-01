using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Models
{
    public sealed record CuentaAuxiliarDbDto(int Id, string Nombre, decimal Saldo, int NumeroSubcuentas);
    public sealed record SubcuentaAuxiliarDbDto(int Id, int CuentaId, string Nombre, decimal Saldo);
    public sealed record MovimientoAuxiliarDbDto(int MovimientoId, int PolizaId, DateTime Fecha, int Folio, string Concepto, decimal Importe, decimal SaldoAcumulado);
    public sealed record ResultadoAuxiliarDbDto(decimal SaldoInicial, IReadOnlyList<MovimientoAuxiliarDbDto> Movimientos);
    public sealed record MovimientoPolizaDbDto(int MovimientoId, int CuentaId, int? SubcuentaId, string NombreCuenta, string NombreSubcuenta, decimal Parcial, decimal Debe, decimal Haber,
        string Redaccion);

    public sealed record PolizaAuxiliarDbDto(int Id, DateTime Fecha, int Folio, string Concepto, decimal TotalDebe, decimal TotalHaber, IReadOnlyList<MovimientoPolizaDbDto> Movimientos);
}
