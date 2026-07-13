using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapturaDePolizas_2026_NET8
{
    public partial class FormVerCuentas : Form
    {
        private readonly DataTable _datosOriginales;
        private readonly DataView _vistaFiltrada;

        private bool _eventosDeshabilitados;

        public FormVerCuentas(DataTable datos)
        {
            ArgumentNullException.ThrowIfNull(datos);

            InitializeComponent();

            _datosOriginales = datos;
            _vistaFiltrada = new DataView(_datosOriginales);

            InicializarFormulario();
        }

        #region Inicialización

        private void InicializarFormulario()
        {
            ConfigurarFormulario();
            ConfigurarControles();
            ConfigurarGrid();
            CargarComboOrdenamiento();
            EnlazarEventos();

            dgvCuentas.DataSource = _vistaFiltrada;

            ConfigurarColumnasGrid();
            AplicarFiltros();
        }

        private void ConfigurarFormulario()
        {
            Text = "Catálogo de Cuentas";
            StartPosition = FormStartPosition.CenterParent;
        }

        private void ConfigurarControles()
        {
            txtBuscarTexto.CharacterCasing = CharacterCasing.Upper;

            rbAsc.Checked = true;
        }

        private void ConfigurarGrid()
        {
            dgvCuentas.AllowUserToAddRows = false;
            dgvCuentas.AllowUserToDeleteRows = false;
            dgvCuentas.ReadOnly = true;
            dgvCuentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCuentas.MultiSelect = false;
            dgvCuentas.RowHeadersVisible = true;

            dgvCuentas.BackgroundColor = Color.White;
            dgvCuentas.BorderStyle = BorderStyle.None;
            dgvCuentas.GridColor = Color.FromArgb(224, 224, 224);

            dgvCuentas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvCuentas.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCuentas.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgvCuentas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvCuentas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgvCuentas.EnableHeadersVisualStyles = false;
        }

        private void CargarComboOrdenamiento()
        {
            cmbColumna.Items.Clear();

            cmbColumna.Items.Add("Número");
            cmbColumna.Items.Add("Nombre");
            cmbColumna.Items.Add("Subcuentas");

            cmbColumna.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbColumna.SelectedIndex = 0;
        }

        private void EnlazarEventos()
        {
            txtBuscarTexto.TextChanged += Filtros_Changed;
            txtMontoMin.TextChanged += Filtros_Changed;
            txtMontoMax.TextChanged += Filtros_Changed;

            rbAsc.CheckedChanged += Ordenamiento_Changed;
            rbDesc.CheckedChanged += Ordenamiento_Changed;
            cmbColumna.SelectedIndexChanged += Filtros_Changed;

            btnRestablecer.Click += BtnRestablecer_Click;
        }

        #endregion

        #region Eventos

        private void Filtros_Changed(object? sender, EventArgs e)
        {
            if (_eventosDeshabilitados)
                return;

            AplicarFiltros();
        }

        private void Ordenamiento_Changed(object? sender, EventArgs e)
        {
            if (_eventosDeshabilitados)
                return;

            if (sender == rbAsc && !rbAsc.Checked)
                return;

            if (sender == rbDesc && !rbDesc.Checked)
                return;

            AplicarFiltros();
        }

        private void BtnRestablecer_Click(object? sender, EventArgs e)
        {
            RestablecerFiltros();
        }

        #endregion

        #region Filtros y ordenamiento

        private void AplicarFiltros()
        {
            List<string> filtros = new();

            AgregarFiltroBusqueda(filtros);
            AgregarFiltroMontoMinimo(filtros);
            AgregarFiltroMontoMaximo(filtros);

            _vistaFiltrada.RowFilter = filtros.Count > 0 ? string.Join(" AND ", filtros) : string.Empty;

            AplicarOrdenamiento();
        }

        private void AgregarFiltroBusqueda(List<string> filtros)
        {
            string texto = SanitizarTextoFiltro(txtBuscarTexto.Text);

            if (string.IsNullOrWhiteSpace(texto))
                return;

            filtros.Add($"(CONVERT([Número], 'System.String') LIKE '%{texto}%' OR [Nombre] LIKE '%{texto}%')");
        }

        private void AgregarFiltroMontoMinimo(List<string> filtros)
        {
            if (!decimal.TryParse(txtMontoMin.Text, out decimal montoMinimo))
                return;

            filtros.Add($"[Monto Bruto] >= {montoMinimo.ToString(CultureInfo.InvariantCulture)}");
        }

        private void AgregarFiltroMontoMaximo(List<string> filtros)
        {
            if (!decimal.TryParse(txtMontoMax.Text, out decimal montoMaximo))
                return;

            filtros.Add($"[Monto Bruto] <= {montoMaximo.ToString(CultureInfo.InvariantCulture)}");
        }

        private void AplicarOrdenamiento()
        {
            string? columna = cmbColumna.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(columna))
                return;

            string direccion = rbAsc.Checked ? "ASC" : "DESC";

            _vistaFiltrada.Sort = $"[{columna}] {direccion}";
        }

        private static string SanitizarTextoFiltro(string texto)
        {
            return texto.Replace("'", "''").Trim();
        }

        #endregion

        #region Grid

        private void ConfigurarColumnasGrid()
        {
            DeshabilitarOrdenamientoManual();
            ConfigurarColumnaNumero();
            ConfigurarColumnaMontoBruto();
            ConfigurarColumnaSubcuentas();
            ConfigurarColumnaNombre();
        }

        private void DeshabilitarOrdenamientoManual()
        {
            foreach (DataGridViewColumn columna in dgvCuentas.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void ConfigurarColumnaNumero()
        {
            if (!dgvCuentas.Columns.Contains("Número"))
                return;

            dgvCuentas.Columns["Número"].Width = 80;
        }

        private void ConfigurarColumnaMontoBruto()
        {
            if (!dgvCuentas.Columns.Contains("Monto Bruto"))
                return;

            dgvCuentas.Columns["Monto Bruto"].Visible = false;
        }

        private void ConfigurarColumnaSubcuentas()
        {
            if (!dgvCuentas.Columns.Contains("Subcuentas"))
                return;

            dgvCuentas.Columns["Subcuentas"].Width = 100;
            dgvCuentas.Columns["Subcuentas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void ConfigurarColumnaNombre()
        {
            if (!dgvCuentas.Columns.Contains("Nombre"))
                return;

            dgvCuentas.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        #endregion

        #region Restablecer

        private void RestablecerFiltros()
        {
            _eventosDeshabilitados = true;

            try
            {
                txtBuscarTexto.Clear();
                txtMontoMin.Clear();
                txtMontoMax.Clear();

                rbAsc.Checked = true;
                rbDesc.Checked = false;

                if (cmbColumna.Items.Count > 0)
                {
                    cmbColumna.SelectedIndex = 0;
                }
            }
            finally
            {
                _eventosDeshabilitados = false;
            }

            AplicarFiltros();
        }

        #endregion
    }
}
