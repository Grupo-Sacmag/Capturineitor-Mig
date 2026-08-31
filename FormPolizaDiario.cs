using CapturaDePolizas_2026_NET8.Business;
using CapturaDePolizas_2026_NET8.Models;
using GaCostos.Models;
using GaCostos.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GaCostos
{
    public partial class FormPolizaDiario : Form
    {
        private readonly ContabilidadService _contabilidadService;
        private readonly PrintDocument _documentoImpresion = new();

        private List<DataGridViewColumn> _columnasImpresion = [];
        private List<DataGridViewRow> _filasImpresion = [];

        private int _indiceFilaImpresion;
        private bool _totalesImpresos;
        private bool _imprimirTotalesExternos;

        private const float AltoTituloImpresion = 30f;
        private const float AltoEncabezadoImpresion = 24f;
        private const float AltoFilaMinimoImpresion = 20f;
        private const float AltoTotalesImpresion = 24f;

        public FormPolizaDiario() : this(new ContabilidadService())
        {
        }

        public FormPolizaDiario(ContabilidadService contabilidadService)
        {
            _contabilidadService = contabilidadService ?? throw new ArgumentNullException(nameof(contabilidadService));
            InitializeComponent();

            // El contenedor del formulario se encargará de liberar el PrintDocument al cerrar la ventana.
            components?.Add(_documentoImpresion);
            InicializarFormulario();
        }

        public bool CargarPoliza(string rutaDatos, string fechaMovimiento, int numeroPoliza)
        {
            try
            {
                LimpiarFormulario();

                IReadOnlyList<RegistroOperacion> registros = _contabilidadService.ObtenerPolizaDiario(rutaDatos, fechaMovimiento, numeroPoliza);

                if (registros.Count == 0)
                {
                    MessageBox.Show(this, "No se encontró la póliza en el archivo de operaciones.", "Sin resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                PintarPoliza(registros);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"No fue posible cargar la póliza." + $"{Environment.NewLine}{Environment.NewLine}" + ex.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void InicializarFormulario()
        {
            ConfigurarGrid();
            ConfigurarImpresion();

            menuCopiarSeleccion.Click += MenuCopiarSeleccion_Click;
            menuSeleccionarTodo.Click += MenuSeleccionarTodo_Click;
            dgvPoliza.KeyDown += DgvPoliza_KeyDown;
        }

        private void ConfigurarImpresion()
        {
            _documentoImpresion.DefaultPageSettings.Landscape = true;

            _documentoImpresion.DefaultPageSettings.Margins = new Margins(
                    left: 40,
                    right: 40,
                    top: 40,
                    bottom: 40);

            _documentoImpresion.BeginPrint += DocumentoImpresion_BeginPrint;
            _documentoImpresion.PrintPage += DocumentoImpresion_PrintPage;
        }

        private void ConfigurarGrid()
        {
            dgvPoliza.Columns.Clear();

            AgregarColumnaTexto(
                name: "Cuenta",
                headerText: "Cuenta",
                width: 90);

            AgregarColumnaTexto(
                name: "SubCuenta",
                headerText: "SubCta",
                width: 80);

            AgregarColumnaTexto(
                name: "Nombre",
                headerText: "Nombre",
                width: 220);

            AgregarColumnaImporte(
                name: "Parcial",
                headerText: "Parcial",
                width: 110);

            AgregarColumnaImporte(
                name: "Debe",
                headerText: "Debe",
                width: 110);

            AgregarColumnaImporte(
                name: "Haber",
                headerText: "Haber",
                width: 110);

            AgregarColumnaTexto(
                name: "Redaccion",
                headerText: "Redacción",
                width: 250);

            dgvPoliza.Columns["Redaccion"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvPoliza.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            dgvPoliza.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvPoliza.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            dgvPoliza.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPoliza.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            dgvPoliza.DefaultCellStyle.BackColor = Color.White;
            dgvPoliza.DefaultCellStyle.ForeColor = Color.Black;
            dgvPoliza.DefaultCellStyle.SelectionBackColor = Color.Yellow;
            dgvPoliza.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        private void LimpiarFormulario()
        {
            dgvPoliza.Rows.Clear();

            Text = "Póliza:";
            lblTitulo.Text = "Póliza:";

            lblTotalParcial.Text = string.Empty;
            lblTotalDebe.Text = string.Empty;
            lblTotalHaber.Text = string.Empty;

            lblSumasIguales.Text = "SUMAS IGUALES";
            lblSumasIguales.BackColor = Color.Gainsboro;
            lblSumasIguales.ForeColor = Color.Black;
        }

        private void PintarPoliza(
            IReadOnlyList<RegistroOperacion> registros)
        {
            RegistroOperacion encabezado = registros.FirstOrDefault(registro => registro.Identificador == "A") ?? registros[0];

            string numeroPoliza = encabezado.Cuenta.Trim();
            string nombrePoliza = encabezado.Descripcion.Trim();
            string titulo = string.IsNullOrWhiteSpace(nombrePoliza) ? $"Póliza: {numeroPoliza}" : $"Póliza: {numeroPoliza} {nombrePoliza}";

            Text = titulo;
            lblTitulo.Text = titulo;

            decimal totalDebe = 0m;
            decimal totalHaber = 0m;

            foreach (RegistroOperacion registro in registros)
            {
                switch (registro.Identificador)
                {
                    case "A":
                    case "D":
                        break;
                    case "B":
                        AgregarFilaCuentaMayor(registro, ref totalDebe, ref totalHaber);
                        break;
                    case "C":
                        AgregarFilaSubCuenta(registro);
                        break;
                }
            }

            ActualizarSumas(totalDebe, totalHaber);
            dgvPoliza.ClearSelection();
        }

        private void AgregarFilaCuentaMayor(RegistroOperacion registro, ref decimal totalDebe, ref decimal totalHaber)
        {
            decimal importe = registro.Importe;
            decimal? debe = importe > 0 ? importe : null;
            decimal? haber = importe < 0 ? Math.Abs(importe) : null;

            if (debe.HasValue)
                totalDebe += debe.Value;

            if (haber.HasValue)
                totalHaber += haber.Value;

            int indiceFila = dgvPoliza.Rows.Add(registro.Cuenta, string.Empty, registro.Descripcion, null, debe, haber, string.Empty);

            DataGridViewRow fila = dgvPoliza.Rows[indiceFila];

            fila.DefaultCellStyle.Font = new Font(dgvPoliza.Font, FontStyle.Bold);
            fila.DefaultCellStyle.BackColor = Color.WhiteSmoke;
        }

        private void AgregarFilaSubCuenta(RegistroOperacion registro)
        {
            decimal? parcial = registro.Importe != 0m ? registro.Importe : null;
            int indiceFila = dgvPoliza.Rows.Add(string.Empty, registro.Cuenta, registro.Descripcion, parcial, null, null, registro.Descripcion);

            DataGridViewRow fila = dgvPoliza.Rows[indiceFila];

            fila.Cells["Nombre"].Style.Padding = new Padding(8, 0, 0, 0);
        }

        private void ActualizarSumas(decimal totalDebe, decimal totalHaber)
        {
            lblTotalParcial.Text = string.Empty;
            lblTotalDebe.Text = FormatHelper.FormatearImporte(totalDebe);
            lblTotalHaber.Text = FormatHelper.FormatearImporte(totalHaber);

            decimal debeRedondeado = decimal.Round(totalDebe, 2);
            decimal haberRedondeado = decimal.Round(totalHaber, 2);
            bool sumasIguales = debeRedondeado == haberRedondeado;

            lblSumasIguales.Text = "SUMAS IGUALES";
            lblSumasIguales.BackColor = sumasIguales ? Color.Gainsboro : Color.MistyRose;
            lblSumasIguales.ForeColor = sumasIguales ? Color.Black : Color.DarkRed;
        }

        private void AgregarColumnaTexto(string name, string headerText, int width)
        {
            dgvPoliza.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = name,
                    HeaderText = headerText,
                    Width = width,
                    MinimumWidth = 30,
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
        }

        private void AgregarColumnaImporte(string name, string headerText, int width)
        {
            DataGridViewTextBoxColumn columna = new()
            {
                Name = name,
                HeaderText = headerText,
                Width = width,
                MinimumWidth = 70,
                ReadOnly = true,
                ValueType = typeof(decimal),
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            columna.DefaultCellStyle.Format = "N2";
            columna.DefaultCellStyle.NullValue = string.Empty;

            dgvPoliza.Columns.Add(columna);
        }

        private void CopiarSeleccion()
        {
            if (dgvPoliza.GetCellCount(DataGridViewElementStates.Selected) == 0)
            {
                return;
            }

            DataObject? datos = dgvPoliza.GetClipboardContent();

            if (datos is not null)
            {
                Clipboard.SetDataObject(datos, true);
            }
        }

        private void SeleccionarTodo()
        {
            if (dgvPoliza.Rows.Count == 0)
                return;

            dgvPoliza.SelectAll();
        }

        private void MenuCopiarSeleccion_Click(object? sender, EventArgs e)
        {
            CopiarSeleccion();
        }

        private void MenuSeleccionarTodo_Click(object? sender, EventArgs e)
        {
            SeleccionarTodo();
        }

        private void DgvPoliza_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                Close();
                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                CopiarSeleccion();
                return;
            }

            if (e.Control && e.KeyCode == Keys.A)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                SeleccionarTodo();
            }
        }

        private void tsVistaPrevia_Click(object sender, EventArgs e)
        {
            MostrarVistaPrevia();
        }

        private void tsImprimirDoc_Click(object sender, EventArgs e)
        {
            ImprimirDocumento();
        }

        private void MostrarVistaPrevia()
        {
            if (!ValidarContenidoParaImprimir())
                return;

            PrepararDocumentoImpresion();

            try
            {
                using PrintPreviewDialog vistaPrevia = new()
                {
                    Document = _documentoImpresion,
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 1200,
                    Height = 800,
                    UseAntiAlias = true
                };

                vistaPrevia.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MostrarErrorImpresion(
                    "No fue posible generar la vista previa.",
                    ex);
            }
        }

        private void ImprimirDocumento()
        {
            if (!ValidarContenidoParaImprimir())
                return;

            PrepararDocumentoImpresion();

            using PrintDialog dialogoImpresion = new()
            {
                Document = _documentoImpresion,
                AllowCurrentPage = false,
                AllowSelection = false,
                AllowSomePages = false,
                UseEXDialog = true
            };

            if (dialogoImpresion.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                UseWaitCursor = true;
                _documentoImpresion.Print();
            }
            catch (InvalidPrinterException ex)
            {
                MostrarErrorImpresion("La impresora seleccionada no está disponible.", ex);
            }
            catch (Exception ex)
            {
                MostrarErrorImpresion("No fue posible imprimir la póliza.", ex);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private bool ValidarContenidoParaImprimir()
        {
            bool tieneFilas = dgvPoliza.Rows.Cast<DataGridViewRow>().Any(fila => fila.Visible && !fila.IsNewRow);

            if (tieneFilas)
                return true;

            MessageBox.Show(this, "No hay información de la póliza para imprimir.", "Sin información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void PrepararDocumentoImpresion()
        {
            string titulo = string.IsNullOrWhiteSpace(lblTitulo.Text) ? "Póliza" : lblTitulo.Text.Trim();
            _documentoImpresion.DocumentName =
                titulo.Length > 100
                    ? titulo[..100]
                    : titulo;
        }

        private void DocumentoImpresion_BeginPrint(object? sender, PrintEventArgs e)
        {
            _indiceFilaImpresion = 0;
            _totalesImpresos = false;
            _columnasImpresion = dgvPoliza.Columns.Cast<DataGridViewColumn>().Where(columna => columna.Visible).OrderBy(columna => columna.DisplayIndex).ToList();

            _filasImpresion = dgvPoliza.Rows.Cast<DataGridViewRow>().Where(fila => fila.Visible && !fila.IsNewRow).ToList();

            // Si TOTALES o SUMAS IGUALES ya está dentro del grid, no se imprime otra fila externa.
            _imprimirTotalesExternos = !ContieneFilaTotalesEnGrid();
        }

        private void DocumentoImpresion_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (_columnasImpresion.Count == 0)
            {
                e.HasMorePages = false;
                return;
            }

            Graphics graphics = e.Graphics;

            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            RectangleF areaImpresion = new(
                e.MarginBounds.Left,
                e.MarginBounds.Top,
                e.MarginBounds.Width,
                e.MarginBounds.Height);

            using Pen lapizBorde = new(
                Color.Gray,
                0.5f);

            float posicionY = areaImpresion.Top;

            DibujarTitulo(
                graphics,
                areaImpresion.Left,
                posicionY,
                areaImpresion.Width,
                lapizBorde);

            posicionY += AltoTituloImpresion + 5f;

            float[] anchosColumnas = CalcularAnchosColumnas(areaImpresion.Width);

            DibujarEncabezados(
                graphics,
                areaImpresion.Left,
                posicionY,
                anchosColumnas,
                lapizBorde);

            posicionY += AltoEncabezadoImpresion;

            bool imprimioFilaEnPagina = false;

            while (_indiceFilaImpresion < _filasImpresion.Count)
            {
                DataGridViewRow fila = _filasImpresion[_indiceFilaImpresion];

                float altoFila = CalcularAltoFila(graphics, fila, anchosColumnas);
                bool esUltimaFila = _indiceFilaImpresion == _filasImpresion.Count - 1;
                float espacioTotales = esUltimaFila && _imprimirTotalesExternos && !_totalesImpresos ? AltoTotalesImpresion : 0f;
                bool cabeEnPagina = posicionY + altoFila + espacioTotales <= areaImpresion.Bottom;

                if (!cabeEnPagina && imprimioFilaEnPagina)
                {
                    e.HasMorePages = true;
                    return;
                }

                // Protección para una fila excepcionalmente alta.
                if (posicionY + altoFila > areaImpresion.Bottom)
                {
                    altoFila = Math.Max(AltoFilaMinimoImpresion, areaImpresion.Bottom - posicionY);
                }

                DibujarFila(graphics, fila, areaImpresion.Left, posicionY, altoFila, anchosColumnas, lapizBorde);

                posicionY += altoFila;
                _indiceFilaImpresion++;
                imprimioFilaEnPagina = true;
            }

            if (_imprimirTotalesExternos && !_totalesImpresos)
            {
                if (posicionY + AltoTotalesImpresion >
                    areaImpresion.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                DibujarFilaTotales(graphics, areaImpresion.Left, posicionY, anchosColumnas, lapizBorde);
                _totalesImpresos = true;
            }
            e.HasMorePages = false;
        }

        private float[] CalcularAnchosColumnas(float anchoDisponible)
        {
            float anchoTotalGrid = _columnasImpresion.Sum(columna => Math.Max(1, columna.Width));
            float[] anchos = new float[_columnasImpresion.Count];
            float anchoAcumulado = 0f;

            for (int i = 0; i < _columnasImpresion.Count; i++)
            {
                if (i == _columnasImpresion.Count - 1)
                {
                    anchos[i] = anchoDisponible - anchoAcumulado;
                    break;
                }

                float proporcion = _columnasImpresion[i].Width / anchoTotalGrid;
                anchos[i] = anchoDisponible * proporcion;
                anchoAcumulado += anchos[i];
            }

            return anchos;
        }

        private void DibujarTitulo(Graphics graphics, float posicionX, float posicionY, float ancho, Pen lapizBorde)
        {
            RectangleF rectangulo = new(posicionX, posicionY, ancho, AltoTituloImpresion);
            using SolidBrush fondo = new(Color.Yellow);
            using Font fuente = new( "Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point);

            graphics.FillRectangle(fondo, rectangulo);
            graphics.DrawRectangle(lapizBorde, rectangulo.X, rectangulo.Y, rectangulo.Width, rectangulo.Height);
            
            DibujarTextoCelda(graphics, rectangulo, lblTitulo.Text, fuente, Color.Black, DataGridViewContentAlignment.MiddleLeft, wrap: false);
        }

        private void DibujarEncabezados(Graphics graphics, float posicionX, float posicionY, float[] anchosColumnas, Pen lapizBorde)
        {
            float x = posicionX;

            for (int i = 0; i < _columnasImpresion.Count; i++)
            {
                DataGridViewColumn columna = _columnasImpresion[i];
                RectangleF rectangulo = new(x, posicionY, anchosColumnas[i], AltoEncabezadoImpresion);
                DataGridViewCellStyle estilo = columna.HeaderCell.InheritedStyle;
                Color fondo = ObtenerColorFondo(estilo.BackColor, Color.Gainsboro);
                Color colorTexto = ObtenerColorTexto(estilo.ForeColor, Color.Black);
                using SolidBrush brochaFondo = new(fondo);
                using Font fuente = CrearFuenteImpresion(estilo.Font ?? dgvPoliza.Font, forzarNegrita: true);
                graphics.FillRectangle(brochaFondo, rectangulo);
                graphics.DrawRectangle(lapizBorde, rectangulo.X, rectangulo.Y, rectangulo.Width, rectangulo.Height);
                DibujarTextoCelda(graphics, rectangulo, columna.HeaderText, fuente, colorTexto, estilo.Alignment == DataGridViewContentAlignment.NotSet ? 
                    DataGridViewContentAlignment.MiddleCenter : estilo.Alignment, wrap: false);
                x += anchosColumnas[i];
            }
        }

        private void DibujarFila(Graphics graphics, DataGridViewRow fila, float posicionX, float posicionY, float altoFila, float[] anchosColumnas, Pen lapizBorde)
        {
            float x = posicionX;

            for (int i = 0; i < _columnasImpresion.Count; i++)
            {
                DataGridViewColumn columna = _columnasImpresion[i];
                DataGridViewCell celda = fila.Cells[columna.Index];
                DataGridViewCellStyle estilo = celda.InheritedStyle;
                RectangleF rectangulo = new(x, posicionY, anchosColumnas[i], altoFila);
                Color fondo = ObtenerColorFondo(estilo.BackColor, Color.White);                
                Color colorTexto = ObtenerColorTexto(estilo.ForeColor, Color.Black);
                string texto = celda.FormattedValue?.ToString() ?? string.Empty;
                bool wrap = estilo.WrapMode == DataGridViewTriState.True;
                using SolidBrush brochaFondo = new(fondo);
                using Font fuente = CrearFuenteImpresion(estilo.Font ?? dgvPoliza.Font);
                graphics.FillRectangle(brochaFondo, rectangulo);
                graphics.DrawRectangle(lapizBorde, rectangulo.X, rectangulo.Y, rectangulo.Width, rectangulo.Height);
                DibujarTextoCelda(graphics, rectangulo, texto, fuente, colorTexto, estilo.Alignment == DataGridViewContentAlignment.NotSet ? DataGridViewContentAlignment.MiddleLeft
                    : estilo.Alignment, wrap);
                x += anchosColumnas[i];
            }
        }

        private float CalcularAltoFila(Graphics graphics, DataGridViewRow fila, float[] anchosColumnas)
        {
            float alto = Math.Max(AltoFilaMinimoImpresion, fila.Height);

            for (int i = 0; i < _columnasImpresion.Count; i++)
            {
                DataGridViewColumn columna = _columnasImpresion[i];
                DataGridViewCell celda = fila.Cells[columna.Index];
                DataGridViewCellStyle estilo = celda.InheritedStyle;

                if (estilo.WrapMode != DataGridViewTriState.True)
                    continue;

                string texto = celda.FormattedValue?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(texto))
                    continue;

                using Font fuente = CrearFuenteImpresion(estilo.Font ?? dgvPoliza.Font);
                using StringFormat formato = CrearFormatoTexto(estilo.Alignment, wrap: true);
                float anchoTexto = Math.Max(10f, anchosColumnas[i] - 6f);

                SizeF areaMaxima = new(anchoTexto, float.MaxValue);
                SizeF medida = graphics.MeasureString(texto, fuente, areaMaxima, formato);

                alto = Math.Max(alto, medida.Height + 4f);
            }

            return alto;
        }

        private void DibujarFilaTotales(Graphics graphics, float posicionX, float posicionY,  float[] anchosColumnas, Pen lapizBorde)
        {
            int indiceParcial = _columnasImpresion.FindIndex(columna => columna.Name.Equals("Parcial", StringComparison.OrdinalIgnoreCase));

            if (indiceParcial < 0)
            {
                indiceParcial = Math.Max(0, _columnasImpresion.Count - 3);
            }

            float anchoEtiqueta = 0f;

            for (int i = 0; i < indiceParcial; i++)
            {
                anchoEtiqueta += anchosColumnas[i];
            }

            using Font fuente = new("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point);

            if (anchoEtiqueta > 0f)
            {
                RectangleF rectanguloEtiqueta = new(posicionX, posicionY, anchoEtiqueta, AltoTotalesImpresion);
                DibujarCeldaTotal(graphics, rectanguloEtiqueta, ObtenerTextoEtiquetaTotales(), fuente, DataGridViewContentAlignment.MiddleRight, lapizBorde);
            }

            float x = posicionX + anchoEtiqueta;

            for (int i = indiceParcial; i < _columnasImpresion.Count; i++)
            {
                DataGridViewColumn columna = _columnasImpresion[i];

                string texto = columna.Name switch
                {
                    "Parcial" => ObtenerTextoTotal(
                        "lblTotalParcial",
                        "Parcial"),

                    "Debe" => ObtenerTextoTotal(
                        "lblTotalDebe",
                        "Debe"),

                    "Haber" => ObtenerTextoTotal(
                        "lblTotalHaber",
                        "Haber"),

                    _ => string.Empty
                };

                RectangleF rectangulo = new(x, posicionY, anchosColumnas[i], AltoTotalesImpresion);
                DibujarCeldaTotal(graphics, rectangulo, texto, fuente, DataGridViewContentAlignment.MiddleRight, lapizBorde);
                x += anchosColumnas[i];
            }
        }

        private void DibujarCeldaTotal(Graphics graphics, RectangleF rectangulo, string texto, Font fuente, DataGridViewContentAlignment alineacion, Pen lapizBorde)
        {
            using SolidBrush fondo = new(Color.Gainsboro);
            graphics.FillRectangle(fondo, rectangulo);
            graphics.DrawRectangle(lapizBorde, rectangulo.X, rectangulo.Y, rectangulo.Width, rectangulo.Height);
            DibujarTextoCelda(graphics, rectangulo, texto, fuente, Color.Black, alineacion, wrap: false);
        }

        private bool ContieneFilaTotalesEnGrid()
        {
            return _filasImpresion.Any(EsFilaTotal);
        }

        private static bool EsFilaTotal(DataGridViewRow fila)
        {
            return fila.Cells.Cast<DataGridViewCell>().Any(celda =>
                {
                    string texto = celda.FormattedValue?.ToString()?.Trim() ?? string.Empty;
                    return texto.Equals("TOTALES", StringComparison.OrdinalIgnoreCase) || texto.Equals("SUMAS IGUALES",StringComparison.OrdinalIgnoreCase);
                });
        }

        private string ObtenerTextoEtiquetaTotales()
        {
            Control? control = Controls.Find("lblSumasIguales", searchAllChildren: true).FirstOrDefault();

            if (control is Label label)
                return label.Text;

            return "SUMAS IGUALES";
        }

        private string ObtenerTextoTotal(string nombreLabel, string nombreColumna)
        {
            Control? control = Controls.Find(nombreLabel, searchAllChildren: true).FirstOrDefault();

            if (control is Label label)
            {
                // Incluso si está vacío, se respeta exactamente lo que actualmente muestra la interfaz.
                return label.Text;
            }

            return CalcularTotalColumna(nombreColumna);
        }

        private string CalcularTotalColumna(string nombreColumna)
        {
            if (!dgvPoliza.Columns.Contains(nombreColumna))
            {
                return string.Empty;
            }

            decimal total = 0m;
            bool encontroImporte = false;

            foreach (DataGridViewRow fila in _filasImpresion)
            {
                if (EsFilaTotal(fila))
                    continue;

                DataGridViewCell celda = fila.Cells[nombreColumna];

                if (!IntentarObtenerDecimal(celda, out decimal importe))
                {
                    continue;
                }

                total += importe;
                encontroImporte = true;
            }

            if (!encontroImporte)
                return string.Empty;

            return total.ToString("N2", CultureInfo.GetCultureInfo("es-MX"));
        }

        private static bool IntentarObtenerDecimal(DataGridViewCell celda, out decimal importe)
        {
            importe = 0m;

            if (celda.Value is decimal decimalValue)
            {
                importe = decimalValue;
                return true;
            }

            if (celda.Value is int intValue)
            {
                importe = intValue;
                return true;
            }

            string texto = celda.FormattedValue?.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texto))
                return false;

            CultureInfo culturaMexico = CultureInfo.GetCultureInfo("es-MX");

            if (decimal.TryParse(texto, NumberStyles.Any, culturaMexico, out importe))
            {
                return true;
            }

            return decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out importe);
        }

        private static void DibujarTextoCelda(Graphics graphics, RectangleF rectangulo, string texto, Font fuente, Color colorTexto, DataGridViewContentAlignment alineacion, bool wrap)
        {
            RectangleF areaTexto = new(rectangulo.X + 3f, rectangulo.Y + 1f, Math.Max(0f, rectangulo.Width - 6f), Math.Max(0f, rectangulo.Height - 2f));

            using SolidBrush brocha = new(colorTexto);
            using StringFormat formato = CrearFormatoTexto(alineacion, wrap);
            graphics.DrawString(texto, fuente, brocha, areaTexto, formato);
        }

        private static StringFormat CrearFormatoTexto(DataGridViewContentAlignment alineacion, bool wrap)
        {
            StringFormat formato = new()
            {
                Trimming = StringTrimming.EllipsisCharacter
            };

            formato.Alignment = alineacion switch
            {
                DataGridViewContentAlignment.TopCenter 
                or DataGridViewContentAlignment.MiddleCenter
                or DataGridViewContentAlignment.BottomCenter
                => StringAlignment.Center,

                DataGridViewContentAlignment.TopRight
                or DataGridViewContentAlignment.MiddleRight
                or DataGridViewContentAlignment.BottomRight
                => StringAlignment.Far,

                _ => StringAlignment.Near
            };

            formato.LineAlignment = alineacion switch
            {
                DataGridViewContentAlignment.TopLeft
                or DataGridViewContentAlignment.TopCenter
                or DataGridViewContentAlignment.TopRight
                => StringAlignment.Near,

                DataGridViewContentAlignment.BottomLeft
                or DataGridViewContentAlignment.BottomCenter
                or DataGridViewContentAlignment.BottomRight
                => StringAlignment.Far,

                _ => StringAlignment.Center
            };

            if (!wrap)
            {
                formato.FormatFlags |= StringFormatFlags.NoWrap;
            }

            return formato;
        }

        private static Font CrearFuenteImpresion(Font fuenteOriginal, bool forzarNegrita = false)
        {
            FontStyle estilo = fuenteOriginal.Style;

            if (forzarNegrita)
                estilo |= FontStyle.Bold;

            return new Font(fuenteOriginal.FontFamily, fuenteOriginal.Size, estilo, GraphicsUnit.Point);
        }

        private static Color ObtenerColorFondo(Color color, Color predeterminado)
        {
            if (color.IsEmpty || color.A == 0)
            {
                return predeterminado;
            }

            return color;
        }

        private static Color ObtenerColorTexto(Color color, Color predeterminado)
        {
            if (color.IsEmpty || color.A == 0)
            {
                return predeterminado;
            }

            return color;
        }

        private void MostrarErrorImpresion(string mensaje, Exception ex)
        {
            MessageBox.Show(this, $"{mensaje}" + $"{Environment.NewLine}{Environment.NewLine}" + ex.Message, "Error de impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool CargarPolizaNueva(PolizaAuxiliarDbDto poliza)
        {
            ArgumentNullException.ThrowIfNull(poliza);

            try
            {
                LimpiarFormulario();
                PintarPolizaNueva(poliza);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No fue posible cargar la póliza desde la base de datos." + Environment.NewLine + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void PintarPolizaNueva(PolizaAuxiliarDbDto poliza)
        {
            string titulo = string.IsNullOrWhiteSpace(poliza.Concepto) ? $"Póliza: {poliza.Folio}" : $"Póliza: {poliza.Folio} {poliza.Concepto}";

            Text = titulo;
            lblTitulo.Text = titulo;

            int indice = 0;

            while (indice < poliza.Movimientos.Count)
            {
                MovimientoPolizaDbDto movimiento = poliza.Movimientos[indice];

                // Movimiento directamente contra una cuenta.
                if (!movimiento.SubcuentaId.HasValue)
                {
                    AgregarFilaCuentaNueva(
                        cuentaId: movimiento.CuentaId,
                        nombre: movimiento.NombreCuenta,
                        debe: movimiento.Debe,
                        haber: movimiento.Haber,
                        redaccion: movimiento.Redaccion);

                    indice++;
                    continue;
                }

                // Bloque consecutivo de subcuentas de la misma cuenta.
                int cuentaId = movimiento.CuentaId;
                string nombreCuenta = movimiento.NombreCuenta;
                int finBloque = indice;
                decimal debeCuenta = 0m;
                decimal haberCuenta = 0m;

                while (finBloque < poliza.Movimientos.Count)
                {
                    MovimientoPolizaDbDto actual = poliza.Movimientos[finBloque];

                    if (!actual.SubcuentaId.HasValue || actual.CuentaId != cuentaId)
                    {
                        break;
                    }

                    debeCuenta += actual.Debe;
                    haberCuenta += actual.Haber;
                    finBloque++;
                }

                AgregarFilaCuentaNueva(
                    cuentaId,
                    nombreCuenta,
                    debeCuenta,
                    haberCuenta,
                    string.Empty);

                for (int i = indice; i < finBloque; i++)
                {
                    AgregarFilaSubcuentaNueva(poliza.Movimientos[i]);
                }

                indice = finBloque;
            }

            ActualizarSumas(poliza.TotalDebe, poliza.TotalHaber);
            dgvPoliza.ClearSelection();
        }

        private void AgregarFilaCuentaNueva(int cuentaId, string nombre, decimal debe, decimal haber, string redaccion)
        {
            decimal? valorDebe = debe != 0m ? debe : null;
            decimal? valorHaber = haber != 0m ? haber : null;
            int indiceFila = dgvPoliza.Rows.Add(cuentaId.ToString(), string.Empty, nombre, null, valorDebe, valorHaber, redaccion);

            DataGridViewRow fila = dgvPoliza.Rows[indiceFila];

            fila.DefaultCellStyle.Font = new Font(dgvPoliza.Font, FontStyle.Bold);
            fila.DefaultCellStyle.BackColor = Color.WhiteSmoke;
        }

        private void AgregarFilaSubcuentaNueva(MovimientoPolizaDbDto movimiento)
        {
            decimal? parcial = movimiento.Parcial != 0m ? movimiento.Parcial : null;
            int indiceFila = dgvPoliza.Rows.Add(string.Empty, movimiento.SubcuentaId?.ToString() ?? string.Empty, movimiento.NombreSubcuenta, parcial, null, null, movimiento.Redaccion);
            dgvPoliza.Rows[indiceFila].Cells["Nombre"].Style.Padding = new Padding(8, 0, 0, 0);
        }
    }
}
