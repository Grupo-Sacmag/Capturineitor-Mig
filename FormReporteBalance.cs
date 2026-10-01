using System;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using GaCostos;
using GaCostos.Services;

namespace CapturaDePolizas_2026_NET8
{
    public partial class FormReporteBalance : Form
    {
        private readonly DataTable _datos;

        // Fuentes cacheadas por estilo (evita crear una Font por cada celda)
        private readonly Dictionary<FontStyle, Font> _fuentes = new();

        // ---- Parámetros de impresión (unidades = 1/100 de pulgada) ----
        private const float AnchoNatural = 1000f;   // ancho lógico del reporte
        private const float AltoFila = 16f;
        private const float AltoLinea = 26f;        // alto de cada línea del encabezado
        private const float AltoEncabezado = AltoLinea * 2 + 10f;
        private const float EscalaMinima = 0.6f;    // mínimo antes de paginar
        private const int MargenImpresion = 50;     // 0.5"
        private PrintDocument? _printDoc;
        private int _filaActual;


        public FormReporteBalance(DataTable datos)
        {
            _datos = datos;

            InitializeComponent();

            dgv.DataSource = _datos;
        }

        // =====================================================
        //  ESTILOS (compartidos por pantalla e impresión)
        // =====================================================

        private readonly record struct EstiloReporte(Color Color, FontStyle Estilo, DataGridViewContentAlignment? Alineacion);

        // =====================================================
        //  ACCIÓN DE IMPRESIÓN
        // =====================================================
        private void tsImprimir_Click(object sender, EventArgs e) => ImprimirReporte();

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;

            var estilo = ObtenerEstilo(e.Value.ToString() ?? "", e.ColumnIndex);
            if (estilo is null) return;

            e.CellStyle.ForeColor = estilo.Value.Color;
            e.CellStyle.Font = ObtenerFuente(estilo.Value.Estilo);

            if (estilo.Value.Alineacion is { } alineacion)
                e.CellStyle.Alignment = alineacion;
        }

        private void Dgv_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgv.Columns.Count >= 4)
            {
                dgv.Columns[0].FillWeight = 65;
                dgv.Columns[1].FillWeight = 35;
                dgv.Columns[2].FillWeight = 65;
                dgv.Columns[3].FillWeight = 35;

                dgv.Columns[1].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;

                dgv.Columns[3].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;

                // Si las columnas de importes son numéricas (decimal) y no texto, este formato se respeta también al imprimir (usa FormattedValue).
                dgv.Columns[1].DefaultCellStyle.Format = "N2";
                dgv.Columns[3].DefaultCellStyle.Format = "N2";
            }
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics!;
            var area = e.MarginBounds;

            var cols = dgv.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            float sumaPesos = cols.Sum(c => c.FillWeight);
            float[] anchos = cols.Select(c => AnchoNatural * c.FillWeight / sumaPesos).ToArray();

            int totalFilas = dgv.Rows.Count;
            float alturaTotal = AltoEncabezado + totalFilas * AltoFila;

            // Auto-fit: primero el ancho (nunca se cortan columnas), luego el alto
            float escala = Math.Min(1f, area.Width / AnchoNatural);
            float escalaAlto = area.Height / alturaTotal;
            if (escalaAlto < escala)
                escala = Math.Max(escalaAlto, Math.Min(escala, EscalaMinima));

            // Si aun así no cabe, se pagina
            float altoUtil = area.Height / escala;
            int filasPorPagina = Math.Max(1, (int)((altoUtil - AltoEncabezado) / AltoFila));

            var estadoGraficos = g.Save();
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.TranslateTransform(area.Left + (area.Width - AnchoNatural * escala) / 2f, area.Top);
            g.ScaleTransform(escala, escala);

            // ---- Encabezado institucional (mismo orden y fuente que en pantalla) ----
            using (var centrado = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                g.DrawString(lblEmpresa.Text, lblEmpresa.Font, Brushes.Black,
                    new RectangleF(0, 0, AnchoNatural, AltoLinea), centrado);

                g.DrawString(lblTitulo.Text, lblTitulo.Font, Brushes.Black,
                    new RectangleF(0, AltoLinea, AnchoNatural, AltoLinea), centrado);
            }

            // ---- Filas ----
            int fin = Math.Min(_filaActual + filasPorPagina, totalFilas);
            float y = AltoEncabezado;

            for (int i = _filaActual; i < fin; i++, y += AltoFila)
            {
                float x = 0;

                for (int j = 0; j < cols.Count; j++)
                {
                    var col = cols[j];
                    var celda = dgv.Rows[i].Cells[col.Index];

                    string texto = celda.FormattedValue?.ToString() ?? "";
                    if (texto.Length > 0)
                    {
                        // Valores base de la columna (igual que en pantalla)
                        var alineacion = col.DefaultCellStyle.Alignment;
                        if (alineacion == DataGridViewContentAlignment.NotSet)
                            alineacion = DataGridViewContentAlignment.MiddleLeft;

                        Color color = Color.Black;
                        FontStyle estiloFuente = FontStyle.Regular;

                        // Mismas reglas que CellFormatting
                        var estilo = ObtenerEstilo(celda.Value?.ToString() ?? "", col.Index);
                        if (estilo is not null)
                        {
                            color = estilo.Value.Color;
                            estiloFuente = estilo.Value.Estilo;
                            if (estilo.Value.Alineacion is { } alin)
                                alineacion = alin;
                        }

                        using var pincel = new SolidBrush(color);
                        using var formato = CrearFormato(alineacion);

                        g.DrawString(texto, ObtenerFuente(estiloFuente), pincel,
                            new RectangleF(x + 2, y, anchos[j] - 4, AltoFila), formato);
                    }

                    x += anchos[j];
                }
            }

            g.Restore(estadoGraficos);

            _filaActual = fin;
            e.HasMorePages = fin < totalFilas;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.P))
            {
                ImprimirReporte();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static EstiloReporte? ObtenerEstilo(string text, int columnIndex)
        {
            if (text == "A C T I V O" ||
                text == "P A S I V O" ||
                text == "HABER" ||
                text == "SOCIAL")
            {
                return new EstiloReporte(
                    Color.Blue,
                    FontStyle.Bold,
                    DataGridViewContentAlignment.MiddleCenter);
            }

            if (text.Equals("Circulante", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("Fijo", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("Diferido", StringComparison.OrdinalIgnoreCase))
            {
                return new EstiloReporte(
                    Color.Blue,
                    FontStyle.Italic | FontStyle.Underline | FontStyle.Bold,
                    DataGridViewContentAlignment.MiddleCenter);
            }

            if (text.StartsWith("Suma ", StringComparison.Ordinal))
            {
                return new EstiloReporte(Color.Blue, FontStyle.Bold, null);
            }

            if (text == "CUENTAS DE ORDEN")
            {
                return new EstiloReporte(
                    Color.Blue,
                    FontStyle.Bold,
                    columnIndex == 0 ? DataGridViewContentAlignment.MiddleRight : null);
            }

            return null;
        }

        private Font ObtenerFuente(FontStyle estilo)
        {
            if (!_fuentes.TryGetValue(estilo, out var fuente))
            {
                var baseFont = dgv.DefaultCellStyle.Font ?? dgv.Font;
                fuente = new Font(baseFont, estilo);
                _fuentes[estilo] = fuente;
            }
            return fuente;
        }

        private void ImprimirReporte()
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show(this, "No hay información para imprimir.", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _printDoc?.Dispose();
                _printDoc = new PrintDocument { DocumentName = "Estado de Posición Financiera" };
                _printDoc.BeginPrint += (_, _) => _filaActual = 0;
                _printDoc.PrintPage += PrintDoc_PrintPage;

                var ps = _printDoc.DefaultPageSettings;
                ps.Margins = new Margins(MargenImpresion, MargenImpresion, MargenImpresion, MargenImpresion);

                // Forzar tamaño Carta si la impresora lo soporta
                foreach (PaperSize tam in _printDoc.PrinterSettings.PaperSizes)
                {
                    if (tam.Kind == PaperKind.Letter)
                    {
                        ps.PaperSize = tam;
                        break;
                    }
                }

                // Orientación: la que permita la mayor escala (empata -> horizontal)
                float alto = AltoEncabezado + dgv.Rows.Count * AltoFila;
                float escalaHoriz = Math.Min(1f, Math.Min(1000f / AnchoNatural, 750f / alto));
                float escalaVert = Math.Min(1f, Math.Min(750f / AnchoNatural, 1000f / alto));
                ps.Landscape = escalaHoriz >= escalaVert;

                using var preview = new PrintPreviewDialog
                {
                    Document = _printDoc,
                    UseAntiAlias = true,
                    WindowState = FormWindowState.Maximized,
                    Text = "Vista previa - Estado de Posición Financiera"
                };
                preview.ShowDialog(this);
            }
            catch (InvalidPrinterException)
            {
                MessageBox.Show(this, "No se encontró una impresora instalada. Instale una impresora o habilite 'Microsoft Print to PDF'.", "Imprimir", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo generar la impresión:\n" + ex.Message, "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        

        private static StringFormat CrearFormato(DataGridViewContentAlignment alineacion)
        {
            var horizontal = alineacion switch
            {
                DataGridViewContentAlignment.TopRight or
                DataGridViewContentAlignment.MiddleRight or
                DataGridViewContentAlignment.BottomRight => StringAlignment.Far,

                DataGridViewContentAlignment.TopCenter or
                DataGridViewContentAlignment.MiddleCenter or
                DataGridViewContentAlignment.BottomCenter => StringAlignment.Center,

                _ => StringAlignment.Near
            };

            return new StringFormat
            {
                Alignment = horizontal,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.EllipsisCharacter
            };
        }

        // =====================================================
        //  ICONO Y LIMPIEZA
        // =====================================================

        private static Bitmap CrearIconoImprimir()
        {
            var bmp = new Bitmap(24, 24);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var lapiz = new Pen(Color.FromArgb(60, 60, 60), 1.5f);
            using var cuerpo = new SolidBrush(Color.FromArgb(110, 110, 110));

            g.FillRectangle(Brushes.White, 6, 2, 12, 7);      // hoja que entra
            g.DrawRectangle(lapiz, 6, 2, 12, 7);

            g.FillRectangle(cuerpo, 2, 9, 20, 9);             // cuerpo de la impresora
            g.DrawRectangle(lapiz, 2, 9, 20, 9);

            g.FillRectangle(Brushes.White, 6, 14, 12, 8);     // hoja que sale
            g.DrawRectangle(lapiz, 6, 14, 12, 8);
            g.DrawLine(Pens.Gray, 8, 17, 16, 17);
            g.DrawLine(Pens.Gray, 8, 19, 16, 19);

            return bmp;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (var f in _fuentes.Values) f.Dispose();
            _fuentes.Clear();

            tsImprimir.Image?.Dispose();
            _printDoc?.Dispose();

            base.OnFormClosed(e);
        }

        private void AbrirCuentaSeleccionada()
        {
            if (dgv.CurrentCell is null) return;

            bool ladoIzquierdo = dgv.CurrentCell.ColumnIndex <= 1;
            string colCuenta = ladoIzquierdo ? "CtaIzq" : "CtaDer";

            if (!_datos.Columns.Contains(colCuenta) || dgv.CurrentCell.OwningRow?.DataBoundItem is not DataRowView drv)
                return;

            string cuenta = Convert.ToString(drv.Row[colCuenta])?.Trim() ?? "";
            if (cuenta.Length == 0)
            {
                System.Media.SystemSounds.Beep.Play();   // título, suma o renglón vacío
                return;
            }

            // El texto del renglón ya viene como "Id Nombre"
            string texto = Convert.ToString(drv.Row[ladoIzquierdo ? 0 : 2])?.Trim() ?? cuenta;

            try
            {
                string? ruta = new ConfigService().LeerRutaDatos();

                if (string.IsNullOrWhiteSpace(ruta) || !Directory.Exists(ruta))
                {
                    MessageBox.Show(this, "La ruta de datos del Auxiliar no está configurada. Configúrela desde el menú AUXILIARES.", "Estados Financieros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var mayor = new ContabilidadService()
                    .ObtenerCatalogoMayor(ruta)
                    .FirstOrDefault(c => MismaCuenta(c.Cuenta, cuenta));

                if (mayor is null || mayor.RangoInferior <= 0 || mayor.RangoSuperior <= 0)
                {
                    MessageBox.Show(this, $"No se encontraron subcuentas para la cuenta {cuenta} en el catálogo de auxiliares.", "Estados Financieros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var auxiliar = new FormAuxiliar(mayor.RangoInferior, mayor.RangoSuperior, "Cuenta: " + texto);
                auxiliar.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el detalle de la cuenta:\n" + ex.Message, "Estados Financieros", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            dgv.Focus();
        }

        private static bool MismaCuenta(string a, string b)
        {
            a = a.Trim();
            b = b.Trim();

            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(a.TrimStart('0'), b.TrimStart('0'), StringComparison.OrdinalIgnoreCase);
        }
    }
}
