namespace CapturaDePolizas_2026_NET8.Business;

public static class NumeroALetrasHelper
{
    private static readonly string[] Unidades =
    {
        "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE",
        "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE",
        "VEINTE", "VEINTIUN", "VEINTIDOS", "VEINTITRES", "VEINTICUATRO",
        "VEINTICINCO", "VEINTISEIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
    };

    private static readonly string[] Decenas =
    {
        "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA",
        "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
    };

    private static readonly string[] Centenas =
    {
        "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS",
        "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
    };

    public static string ConvertirAMonedaLetra(decimal cantidad)
    {
        decimal valorAbsoluto = Math.Abs(Math.Round(cantidad, 2, MidpointRounding.AwayFromZero));
        long parteEntera = (long)Math.Truncate(valorAbsoluto);
        int centavos = (int)Math.Round((valorAbsoluto - parteEntera) * 100m);

        string letrasEntero = ConvertirEntero(parteEntera).Trim();
        string moneda;

        if (parteEntera == 1)
        {
            moneda = "PESO";
        }
        else if (parteEntera >= 1_000_000 && parteEntera % 1_000_000 == 0)
        {
            moneda = "DE PESOS";
        }
        else
        {
            moneda = "PESOS";
        }

        return $"({letrasEntero} {moneda} {centavos:00}/100 M.N.)";
    }

    private static string ConvertirEntero(long numero)
    {
        if (numero == 0)
            return "CERO";

        if (numero >= 1_000_000)
        {
            long millones = numero / 1_000_000;
            long resto = numero % 1_000_000;

            string prefijoMillones = millones == 1
                ? "UN MILLON"
                : $"{ConvertirEntero(millones)} MILLONES";

            return resto > 0
                ? $"{prefijoMillones} {ConvertirEntero(resto)}"
                : prefijoMillones;
        }

        if (numero >= 1_000)
        {
            long miles = numero / 1_000;
            long resto = numero % 1_000;

            string prefijoMiles = $"{ConvertirEntero(miles)} MIL";

            return resto > 0
                ? $"{prefijoMiles} {ConvertirEntero(resto)}"
                : prefijoMiles;
        }

        if (numero == 100)
            return "CIEN";

        if (numero > 100)
        {
            long centena = numero / 100;
            long resto = numero % 100;

            return resto > 0
                ? $"{Centenas[centena]} {ConvertirEntero(resto)}"
                : Centenas[centena];
        }

        if (numero < 30)
            return Unidades[numero];

        long decena = numero / 10;
        long unidad = numero % 10;

        return unidad > 0
            ? $"{Decenas[decena]} Y {Unidades[unidad]}"
            : Decenas[decena];
    }
}
