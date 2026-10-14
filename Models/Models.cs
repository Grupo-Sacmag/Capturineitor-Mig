namespace GaCostos.Models;

public sealed record CuentaMayor(string Cuenta, string Nombre, string Saldo, int RangoInferior, int RangoSuperior);
public sealed record CuentaAuxiliar(string Cuenta, string Nombre, string Saldo, int RangoInferior, int RangoSuperior, int Guia);
public sealed record MovimientoAuxiliar(string Fecha, short Poliza, string Concepto, decimal Importe, decimal SaldoAcumulado);
public sealed record AuxiliarResultado(decimal SaldoInicial, IReadOnlyList<MovimientoAuxiliar> Movimientos);
public sealed record RegistroOperacion(string Cuenta, string Descripcion, string Fecha, decimal Importe, string Identificador, string Real);