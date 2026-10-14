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
using CapturaDePolizas_2026_NET8.Repositories;

namespace CapturaDePolizas_2026_NET8
{
    public partial class FormReporteBalance : Form
    {
        private readonly DataTable _datos;
        private readonly ICatalogoRepository? _catalogoRepository;

        // Fuentes cacheadas por estilo (evita crear una Font por cada celda)
        private readonly Dictionary<FontStyle, Font> _fuentes = new();

        // ---- Parámetros de impresión (unidades = 1/100 de pulgada) ----
        private const float AnchoNatural = 1000f;   // ancho lógico del reporte
        private const float AltoFila = 22f;
        private const float AltoLinea = 26f;        // alto de cada línea del encabezado
        private const float AltoEncabezado = AltoLinea * 2 + 10f;
        private const float EscalaMinima = 0.6f;    // mínimo antes de paginar
        private const int MargenImpresion = 50;     // 0.5"
        private PrintDocument? _printDoc;
        private int _filaActual;


        public FormReporteBalance(DataTable datos, ICatalogoRepository? catalogoRepository = null)
        {
            _datos = datos;
            _catalogoRepository = catalogoRepository;

            InitializeComponent();

            dgv.DataSource = _datos;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            dgv.CurrentCellChanged += (s, e) => dgv.Invalidate();
            Shown += (s, e) =>
            {
                if (dgv.Rows.Count > 0)
                {
                    dgv.Focus();
                }
            };
        }

        private void Dgv_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                AbrirCuentaSeleccionada();
            }
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

            var estilo = ObtenerEstilo(e.Value.ToString() ?? "", e.ColumnIndex, e.RowIndex);
            if (estilo is null) return;

            e.CellStyle.ForeColor = estilo.Value.Color;
            e.CellStyle.Font = ObtenerFuente(estilo.Value.Estilo);
            e.CellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            e.CellStyle.SelectionForeColor = Color.White;

            if (estilo.Value.Alineacion is { } alineacion)
                e.CellStyle.Alignment = alineacion;
        }

        private void Dgv_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && EsFilaCuentaDeOrden(e.RowIndex))
            {
                using var brush = new SolidBrush(Color.White);
                e.Graphics.FillRectangle(brush, e.CellBounds);
                e.Handled = true;
            }
        }

        private void Dgv_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex < 0 || !EsFilaCuentaDeOrden(e.RowIndex))
                return;

            var g = e.Graphics;
            var estado = g.Save();
            g.SetClip(e.RowBounds);

            float totalWidth = dgv.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).Sum(c => c.Width);
            float xCenter = totalWidth / 2f;

            string textoCol0 = Convert.ToString(dgv.Rows[e.RowIndex].Cells[0].Value)?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(textoCol0))
            {
                g.Restore(estado);
                return;
            }

            if (textoCol0 == "CUENTAS DE ORDEN")
            {
                using var formatoCentrado = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                using var pincelAzul = new SolidBrush(Color.Blue);
                g.DrawString("CUENTAS DE ORDEN", ObtenerFuente(FontStyle.Bold), pincelAzul,
                    new RectangleF(0, e.RowBounds.Y, totalWidth, e.RowBounds.Height), formatoCentrado);
                g.Restore(estado);
                return;
            }

            // Fila de cuenta de orden
            string desc = textoCol0;
            string monto = dgv.Rows[e.RowIndex].Cells[1].FormattedValue?.ToString()?.Trim() ?? "";

            bool isSelected = dgv.CurrentCell != null && dgv.CurrentCell.RowIndex == e.RowIndex;

            float anchoDesc = 330f;
            float anchoMonto = 140f;
            float separacion = 10f;
            float anchoTotalBloque = anchoDesc + separacion + anchoMonto;

            float xInicio = xCenter - (anchoTotalBloque / 2f);
            float xMonto = xInicio + anchoDesc + separacion;

            if (isSelected)
            {
                using var pincelSel = new SolidBrush(Color.FromArgb(0, 120, 215));
                g.FillRectangle(pincelSel, xInicio - 5, e.RowBounds.Y + 1, anchoTotalBloque + 10, e.RowBounds.Height - 2);
            }

            Brush pincelTexto = isSelected ? Brushes.White : Brushes.Black;

            using (var formatoDesc = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                g.DrawString(desc, ObtenerFuente(FontStyle.Regular), pincelTexto,
                    new RectangleF(xInicio, e.RowBounds.Y, anchoDesc, e.RowBounds.Height), formatoDesc);
            }

            if (!string.IsNullOrEmpty(monto))
            {
                using var formatoMonto = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                g.DrawString(monto, ObtenerFuente(FontStyle.Regular), pincelTexto,
                    new RectangleF(xMonto, e.RowBounds.Y, anchoMonto, e.RowBounds.Height), formatoMonto);
            }

            g.Restore(estado);
        }

        private void Dgv_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgv.Columns.Contains("CtaIzq"))
                dgv.Columns["CtaIzq"].Visible = false;

            if (dgv.Columns.Contains("CtaDer"))
                dgv.Columns["CtaDer"].Visible = false;

            if (dgv.Columns.Count >= 4)
            {
                dgv.Columns[0].FillWeight = 75;
                dgv.Columns[1].FillWeight = 25;
                dgv.Columns[2].FillWeight = 75;
                dgv.Columns[3].FillWeight = 25;

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
                if (EsFilaCuentaDeOrden(i))
                {
                    string textoCol0 = dgv.Rows[i].Cells[0].Value?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(textoCol0))
                    {
                        float xCenter = AnchoNatural / 2f;

                        if (textoCol0 == "CUENTAS DE ORDEN")
                        {
                            using var pincelAzul = new SolidBrush(Color.Blue);
                            using var formatoCentrado = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center,
                                FormatFlags = StringFormatFlags.NoWrap
                            };
                            g.DrawString("CUENTAS DE ORDEN", ObtenerFuente(FontStyle.Bold), pincelAzul,
                                new RectangleF(0, y, AnchoNatural, AltoFila), formatoCentrado);
                        }
                        else
                        {
                            string desc = textoCol0;
                            string monto = dgv.Rows[i].Cells[1].FormattedValue?.ToString()?.Trim() ?? "";

                            float anchoDesc = 330f;
                            float anchoMonto = 140f;
                            float separacion = 10f;
                            float anchoTotalBloque = anchoDesc + separacion + anchoMonto;

                            float xInicio = xCenter - (anchoTotalBloque / 2f);
                            float xMonto = xInicio + anchoDesc + separacion;

                            using var pincelNegro = new SolidBrush(Color.Black);

                            using (var formatoDesc = new StringFormat
                            {
                                Alignment = StringAlignment.Near,
                                LineAlignment = StringAlignment.Center,
                                FormatFlags = StringFormatFlags.NoWrap,
                                Trimming = StringTrimming.EllipsisCharacter
                            })
                            {
                                g.DrawString(desc, ObtenerFuente(FontStyle.Regular), pincelNegro,
                                    new RectangleF(xInicio, y, anchoDesc, AltoFila), formatoDesc);
                            }

                            if (!string.IsNullOrEmpty(monto))
                            {
                                using var formatoMonto = new StringFormat
                                {
                                    Alignment = StringAlignment.Far,
                                    LineAlignment = StringAlignment.Center,
                                    FormatFlags = StringFormatFlags.NoWrap
                                };
                                g.DrawString(monto, ObtenerFuente(FontStyle.Regular), pincelNegro,
                                    new RectangleF(xMonto, y, anchoMonto, AltoFila), formatoMonto);
                            }
                        }
                    }
                    continue;
                }

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
                        var estilo = ObtenerEstilo(celda.Value?.ToString() ?? "", col.Index, i);
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

            if (keyData == Keys.Enter && (dgv.Focused || dgv.ContainsFocus))
            {
                AbrirCuentaSeleccionada();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private EstiloReporte? ObtenerEstilo(string text, int columnIndex, int rowIndex = -1)
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
                    columnIndex == 0 ? DataGridViewContentAlignment.MiddleCenter : null);
            }

            if (rowIndex >= 0 && columnIndex == 0 && EsFilaCuentaDeOrden(rowIndex) && !string.IsNullOrWhiteSpace(text))
            {
                return new EstiloReporte(
                    Color.Black,
                    FontStyle.Regular,
                    DataGridViewContentAlignment.MiddleCenter);
            }

            return null;
        }

        private bool EsFilaCuentaDeOrden(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _datos.Rows.Count)
                return false;

            for (int i = 0; i <= rowIndex; i++)
            {
                if (Convert.ToString(_datos.Rows[i][0])?.Trim() == "CUENTAS DE ORDEN")
                {
                    return true;
                }
            }

            return false;
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

            bool esOrden = EsFilaCuentaDeOrden(dgv.CurrentCell.RowIndex);
            bool ladoIzquierdo = esOrden || dgv.CurrentCell.ColumnIndex <= 1;
            string colCuenta = ladoIzquierdo ? "CtaIzq" : "CtaDer";

            if (!_datos.Columns.Contains(colCuenta) || dgv.CurrentCell.OwningRow?.DataBoundItem is not DataRowView drv)
                return;

            string cuentaTexto = Convert.ToString(drv.Row[colCuenta])?.Trim() ?? "";
            if (string.IsNullOrEmpty(cuentaTexto) || !int.TryParse(cuentaTexto, out int cuentaId) || cuentaId <= 0)
            {
                System.Media.SystemSounds.Beep.Play();   // título, suma o renglón vacío
                return;
            }

            try
            {
                var repo = _catalogoRepository ?? new CatalogoRepository();
                DataTable datosSubcuentas = repo.ObtenerTablaSubcuentas();

                using var formSubcuentas = new FormVerSubcuentas(datosSubcuentas, repo, cuentaId);
                formSubcuentas.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el detalle de las subcuentas:\n" + ex.Message, "Estados Financieros", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
