using System.Drawing.Printing;
using System.Globalization;
using CapturaDePolizas_2026_NET8.Models;

namespace CapturaDePolizas_2026_NET8.Business;

public class ChequeRenglonImpresion
{
    public string Cuenta { get; set; } = string.Empty;
    public string Subcuenta { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Parcial { get; set; } = string.Empty;
    public string Debe { get; set; } = string.Empty;
    public string Haber { get; set; } = string.Empty;
}

public class ChequePrintData
{
    public string FechaTexto { get; set; } = string.Empty;
    public string Beneficiario { get; set; } = string.Empty;
    public string ImporteNumeroTexto { get; set; } = string.Empty;
    public string ImporteLetraTexto { get; set; } = string.Empty;
    public string ConceptoTexto { get; set; } = string.Empty;
    public string NumeroCheque { get; set; } = string.Empty;
    public List<ChequeRenglonImpresion> Renglones { get; set; } = new();
    public string SumaDebeTexto { get; set; } = string.Empty;
    public string SumaHaberTexto { get; set; } = string.Empty;

    public static ChequePrintData CrearDatosPrueba()
    {
        return new ChequePrintData
        {
            FechaTexto = "CDMX, a " + DateTime.Today.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX")).ToUpper(),
            Beneficiario = "A FAVOR DE BENEFICIARIO DE PRUEBA S.A. DE C.V.",
            ImporteNumeroTexto = "000,000.00",
            ImporteLetraTexto = "(CERO PESOS 00/100 M.N.)",
            ConceptoTexto = "PAGO DE FACTURA (PRUEBA DE ALINEACION)",
            NumeroCheque = "0001",
            Renglones = new List<ChequeRenglonImpresion>
            {
                new()
                {
                    Cuenta = "1102",
                    Subcuenta = "",
                    Nombre = "BANCOS NACIONALES",
                    Parcial = "",
                    Debe = "",
                    Haber = "000,000.00"
                },
                new()
                {
                    Cuenta = "",
                    Subcuenta = "0001",
                    Nombre = "BANAMEX CTA. CHEQUES",
                    Parcial = "-000,000.00",
                    Debe = "",
                    Haber = ""
                }
            },
            SumaDebeTexto = "000,000.00",
            SumaHaberTexto = "000,000.00"
        };
    }
}

public static class ChequePrintService
{
    public static bool ImprimirConDialogo(
        IWin32Window owner,
        ChequePrintData datos,
        ChequeTemplateConfig plantilla)
    {
        using PrintDocument printDoc = CrearPrintDocument(datos, plantilla);
        using PrintDialog dialog = new()
        {
            Document = printDoc,
            UseEXDialog = true,
            AllowSomePages = false,
            AllowSelection = false
        };

        if (dialog.ShowDialog(owner) != DialogResult.OK)
            return false;

        printDoc.Print();
        return true;
    }

    public static void MostrarVistaPrevia(
        IWin32Window owner,
        ChequePrintData datos,
        ChequeTemplateConfig plantilla)
    {
        using PrintDocument printDoc = CrearPrintDocument(datos, plantilla);
        using PrintPreviewDialog preview = new()
        {
            Document = printDoc,
            Width = 900,
            Height = 700,
            StartPosition = FormStartPosition.CenterParent,
            Text = $"Vista Previa de Cheque - {plantilla.NombreBanco}"
        };

        preview.ShowDialog(owner);
    }

    private static PrintDocument CrearPrintDocument(
        ChequePrintData datos,
        ChequeTemplateConfig plantilla)
    {
        PrintDocument doc = new()
        {
            DocumentName = string.IsNullOrWhiteSpace(datos.NumeroCheque)
                ? "Impresión de Cheque"
                : $"Cheque #{datos.NumeroCheque}",
            OriginAtMargins = false
        };

        doc.DefaultPageSettings.Landscape = false;
        doc.PrintPage += (_, e) => DibujarPaginaCheque(e, datos, plantilla);

        return doc;
    }

    private static void DibujarPaginaCheque(
        PrintPageEventArgs e,
        ChequePrintData datos,
        ChequeTemplateConfig plantilla)
    {
        if (e.Graphics is null)
            return;

        Graphics g = e.Graphics;
        g.PageUnit = GraphicsUnit.Millimeter;

        string fontName = string.IsNullOrWhiteSpace(plantilla.FontName) ? "Arial" : plantilla.FontName;
        float fontSize = plantilla.FontSize <= 4f ? 10f : plantilla.FontSize;

        using Font fontRegular = new(fontName, fontSize, FontStyle.Regular);
        using Font fontBold = new(fontName, fontSize, FontStyle.Bold);
        Brush brush = Brushes.Black;

        float ox = plantilla.OffsetX;
        float oy = plantilla.OffsetY;

        // 1. Fecha
        g.DrawString(
            datos.FechaTexto,
            fontRegular,
            brush,
            plantilla.FechaX + ox,
            plantilla.FechaY + oy
        );

        // 2. Beneficiario
        g.DrawString(
            datos.Beneficiario,
            fontBold,
            brush,
            plantilla.BeneficiarioX + ox,
            plantilla.BeneficiarioY + oy
        );

        // 3. Importe en Número
        g.DrawString(
            datos.ImporteNumeroTexto,
            fontBold,
            brush,
            plantilla.ImporteNumeroX + ox,
            plantilla.ImporteNumeroY + oy
        );

        // 4. Importe con Letra
        g.DrawString(
            datos.ImporteLetraTexto,
            fontRegular,
            brush,
            plantilla.ImporteLetraX + ox,
            plantilla.ImporteLetraY + oy
        );

        // 5. Concepto (e identificador de número de cheque si existe)
        string textoConcepto = string.IsNullOrWhiteSpace(datos.NumeroCheque)
            ? datos.ConceptoTexto
            : $"{datos.ConceptoTexto}     # {datos.NumeroCheque}";

        g.DrawString(
            textoConcepto,
            fontRegular,
            brush,
            plantilla.ConceptoX + ox,
            plantilla.ConceptoY + oy
        );

        // 6. Aplicación Contable (máximo 11 renglones como en VB6)
        if (datos.Renglones.Count > 11)
        {
            g.DrawString(
                "VER APLICACIÓN ANEXA.",
                fontBold,
                brush,
                plantilla.NombreX + ox,
                plantilla.InicioAplicacionY + oy
            );
        }
        else
        {
            using StringFormat formatoDerecha = new()
            {
                Alignment = StringAlignment.Far
            };

            float yActual = plantilla.InicioAplicacionY + oy;
            float saltoLinea = plantilla.InterlineadoAplicacionMm <= 1f ? 4.8f : plantilla.InterlineadoAplicacionMm;

            foreach (ChequeRenglonImpresion renglon in datos.Renglones)
            {
                if (!string.IsNullOrWhiteSpace(renglon.Cuenta))
                {
                    g.DrawString(renglon.Cuenta, fontRegular, brush, plantilla.CuentaX + ox, yActual);
                }
                else if (!string.IsNullOrWhiteSpace(renglon.Subcuenta))
                {
                    g.DrawString(renglon.Subcuenta, fontRegular, brush, plantilla.SubcuentaX + ox, yActual);
                }

                if (!string.IsNullOrWhiteSpace(renglon.Nombre))
                {
                    g.DrawString(renglon.Nombre, fontRegular, brush, plantilla.NombreX + ox, yActual);
                }

                if (!string.IsNullOrWhiteSpace(renglon.Parcial))
                {
                    g.DrawString(renglon.Parcial, fontRegular, brush, plantilla.ParcialX + ox, yActual, formatoDerecha);
                }

                if (!string.IsNullOrWhiteSpace(renglon.Debe))
                {
                    g.DrawString(renglon.Debe, fontRegular, brush, plantilla.DebeX + ox, yActual, formatoDerecha);
                }

                if (!string.IsNullOrWhiteSpace(renglon.Haber))
                {
                    g.DrawString(renglon.Haber, fontRegular, brush, plantilla.HaberX + ox, yActual, formatoDerecha);
                }

                yActual += saltoLinea;
            }

            // 7. Sumas Iguales (cuando son <= 11 renglones)
            float ySumas = plantilla.SumasIgualesY + oy;

            if (!string.IsNullOrWhiteSpace(datos.SumaDebeTexto))
            {
                g.DrawString(datos.SumaDebeTexto, fontBold, brush, plantilla.DebeX + ox, ySumas, formatoDerecha);
            }

            if (!string.IsNullOrWhiteSpace(datos.SumaHaberTexto))
            {
                g.DrawString(datos.SumaHaberTexto, fontBold, brush, plantilla.HaberX + ox, ySumas, formatoDerecha);
            }
        }

        e.HasMorePages = false;
    }
}
