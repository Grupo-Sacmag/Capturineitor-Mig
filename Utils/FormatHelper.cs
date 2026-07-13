using System.Globalization;

namespace GaCostos.Utils;

public static class FormatHelper
{
    private static readonly CultureInfo CulturaMexico = new("es-MX");

    public static string FormatearSaldo(string saldoRaw)
    {
        if (decimal.TryParse(saldoRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal saldo))
        {
            return saldo.ToString("N2", CulturaMexico);
        }

        if (decimal.TryParse(saldoRaw, NumberStyles.Any, CulturaMexico, out saldo))
        {
            return saldo.ToString("N2", CulturaMexico);
        }

        return saldoRaw.Trim();
    }

    public static string FormatearImporte(decimal importe)
    {
        return importe.ToString("N2", CulturaMexico);
    }

    public static string ObtenerMesDesdeFecha(string fecha)
    {
        if (string.IsNullOrWhiteSpace(fecha))
            return "00";

        string[] partes = fecha.Trim().Split('/');

        if (partes.Length < 2)
            return "00";

        string mes = partes[1].Trim();

        return mes.PadLeft(2, '0');
    }

    public static decimal ParseDecimalFlexible(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return 0m;

        if (decimal.TryParse(valor.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultado))
        {
            return resultado;
        }

        if (decimal.TryParse(valor.Trim(), NumberStyles.Any, CulturaMexico, out resultado))
        {
            return resultado;
        }

        return 0m;
    }
}