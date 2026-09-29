using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapturaDePolizas_2026_NET8.Business;
using GaCostos.Models;
using GaCostos.Services;

namespace GaCostos
{
    public partial class FormPolizaDiario : Form
    {
        private readonly ContabilidadService _contabilidadService;

        public FormPolizaDiario() : this(new ContabilidadService())
        {
        }

        public FormPolizaDiario(ContabilidadService contabilidadService)
        {
            _contabilidadService = contabilidadService ?? throw new ArgumentNullException(nameof(contabilidadService));

            InitializeComponent();
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

            menuCopiarSeleccion.Click += MenuCopiarSeleccion_Click;
            menuSeleccionarTodo.Click += MenuSeleccionarTodo_Click;
            dgvPoliza.KeyDown += DgvPoliza_KeyDown;
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
    }
}
