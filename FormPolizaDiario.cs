using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GaCostos.Models;
using GaCostos.Services;
using GaCostos.Utils;

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
            _contabilidadService = contabilidadService;

            InitializeComponent();
            InicializarFormulario();
        }

        public bool CargarPoliza(string rutaDatos, string fechaMovimiento, int numeroPoliza)
        {
            try
            {
                dgvPoliza.Rows.Clear();

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
                MessageBox.Show(this, $"No fue posible cargar la póliza.{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        private void InicializarFormulario()
        {
            ConfigurarGrid();

            btnCerrar.Click += BtnCerrar_Click;
            dgvPoliza.KeyDown += DgvPoliza_KeyDown;
        }

        private void ConfigurarGrid()
        {
            dgvPoliza.Columns.Clear();

            AgregarColumna("Cuenta", "Cuenta", 100);
            AgregarColumna("SubCuenta", "SubCta", 100);
            AgregarColumna("Nombre", "Nombre", 260);
            AgregarColumna("Parcial", "Parcial", 120);
            AgregarColumna("Debe", "Debe", 120);
            AgregarColumna("Haber", "Haber", 120);
            AgregarColumna("Redaccion", "Redacción", 280);

            dgvPoliza.EnableHeadersVisualStyles = false;
            dgvPoliza.ColumnHeadersDefaultCellStyle.BackColor = Color.Gold;
            dgvPoliza.ColumnHeadersDefaultCellStyle.Font = new Font(dgvPoliza.Font, FontStyle.Bold);

            AlinearDerecha("Parcial");
            AlinearDerecha("Debe");
            AlinearDerecha("Haber");

            dgvPoliza.Columns["Nombre"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvPoliza.Columns["Redaccion"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void PintarPoliza(IReadOnlyList<RegistroOperacion> registros)
        {
            RegistroOperacion encabezado = registros[0];

            string titulo = $"Póliza: {encabezado.Cuenta}  {encabezado.Descripcion}";

            Text = titulo;
            lblTitulo.Text = titulo;

            AgregarFilaEncabezado(encabezado);

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

            AgregarFilaTotales(totalDebe, totalHaber);
        }

        private void AgregarFilaEncabezado(RegistroOperacion encabezado)
        {
            int indiceFila = dgvPoliza.Rows.Add(encabezado.Cuenta, string.Empty, encabezado.Descripcion, string.Empty, string.Empty, string.Empty, string.Empty);

            DataGridViewRow fila = dgvPoliza.Rows[indiceFila];

            fila.DefaultCellStyle.BackColor = Color.Gold;
            fila.DefaultCellStyle.Font = new Font(dgvPoliza.Font, FontStyle.Bold);
        }

        private void AgregarFilaCuentaMayor(RegistroOperacion registro, ref decimal totalDebe, ref decimal totalHaber)
        {
            decimal importe = registro.Importe;
            string debe = importe > 0 ? FormatHelper.FormatearImporte(importe) : string.Empty;
            string haber = importe < 0 ? FormatHelper.FormatearImporte(Math.Abs(importe)) : string.Empty;

            if (importe > 0)
                totalDebe += importe;

            if (importe < 0)
                totalHaber += Math.Abs(importe);

            int indiceFila = dgvPoliza.Rows.Add(registro.Cuenta, string.Empty, registro.Descripcion, string.Empty, debe, haber, string.Empty);
            DataGridViewRow fila = dgvPoliza.Rows[indiceFila];
            fila.DefaultCellStyle.Font = new Font(dgvPoliza.Font, FontStyle.Bold);
        }

        private void AgregarFilaSubCuenta(RegistroOperacion registro)
        {
            decimal importe = registro.Importe;
            string parcial = importe != 0 ? FormatHelper.FormatearImporte(importe) : string.Empty;
            dgvPoliza.Rows.Add(string.Empty, registro.Cuenta, registro.Descripcion, parcial, string.Empty, string.Empty, registro.Descripcion);
        }

        private void AgregarFilaTotales(decimal totalDebe, decimal totalHaber)
        {
            int indiceFila = dgvPoliza.Rows.Add(string.Empty, string.Empty, "TOTALES", string.Empty, FormatHelper.FormatearImporte(totalDebe),
                FormatHelper.FormatearImporte(totalHaber), string.Empty);

            DataGridViewRow fila = dgvPoliza.Rows[indiceFila];

            fila.DefaultCellStyle.BackColor = Color.Gainsboro;
            fila.DefaultCellStyle.Font = new Font(dgvPoliza.Font, FontStyle.Bold);
        }

        private void AgregarColumna(string name, string headerText, int width)
        {
            dgvPoliza.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true
            });
        }

        private void AlinearDerecha(string nombreColumna)
        {
            if (!dgvPoliza.Columns.Contains(nombreColumna))
                return;

            dgvPoliza.Columns[nombreColumna]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;
        }

        private void CopiarSeleccion()
        {
            if (dgvPoliza.GetCellCount(DataGridViewElementStates.Selected) == 0)
                return;

            DataObject? data = dgvPoliza.GetClipboardContent();

            if (data is not null)
                Clipboard.SetDataObject(data);
        }

        private void BtnCerrar_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void DgvPoliza_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                e.Handled = true;
                CopiarSeleccion();
            }
        }
    }
}
