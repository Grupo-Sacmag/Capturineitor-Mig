using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapturaDePolizas_2026_NET8
{
    public partial class FormBuscarCatalogo : Form
    {
        private readonly DataTable _datosOriginales;
        private readonly DataView _vistaFiltrada;
        public string? SelectedId { get; private set; }

        public FormBuscarCatalogo(string titulo, DataTable datos)
        {
            ArgumentNullException.ThrowIfNull(datos);
            InitializeComponent();

            Text = titulo;

            _datosOriginales = datos;
            _vistaFiltrada = new DataView(_datosOriginales);

            InicializarFormulario();
        }

        #region Inicialización

        private void InicializarFormulario()
        {
            KeyPreview = true;

            ConfigurarTextBox();
            ConfigurarGrid();
            EnlazarEventos();

            dgvResultados.DataSource = _vistaFiltrada;
        }

        private void ConfigurarTextBox()
        {
            txtBuscar.CharacterCasing = CharacterCasing.Upper;
        }

        private void ConfigurarGrid()
        {
            dgvResultados.AllowUserToAddRows = false;
            dgvResultados.AllowUserToDeleteRows = false;
            dgvResultados.ReadOnly = true;
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResultados.MultiSelect = false;
            dgvResultados.RowHeadersVisible = false;
            dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResultados.BackgroundColor = Color.White;
            dgvResultados.BorderStyle = BorderStyle.None;
            dgvResultados.GridColor = Color.FromArgb(224, 224, 224);

            dgvResultados.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvResultados.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvResultados.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgvResultados.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvResultados.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgvResultados.EnableHeadersVisualStyles = false;

            OcultarColumnasNoNecesarias();
        }

        private void OcultarColumnasNoNecesarias()
        {
            if (dgvResultados.Columns.Contains("Importe"))
            {
                dgvResultados.Columns["Importe"].Visible = false;
            }
        }

        private void EnlazarEventos()
        {
            Shown += FormBuscarCatalogo_Shown;
            KeyDown += FormBuscarCatalogo_KeyDown;

            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            txtBuscar.KeyDown += TxtBuscar_KeyDown;

            dgvResultados.KeyDown += DgvResultados_KeyDown;
            dgvResultados.CellDoubleClick += DgvResultados_CellDoubleClick;
        }

        #endregion

        #region Eventos

        private void FormBuscarCatalogo_Shown(object? sender, EventArgs e)
        {
            txtBuscar.Focus();
        }

        private void FormBuscarCatalogo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Cancelar();
            }
        }

        private void TxtBuscar_TextChanged(object? sender, EventArgs e)
        {
            AplicarFiltroBusqueda();
        }

        private void TxtBuscar_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                EnfocarPrimeraFila();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ConfirmarSeleccion();
            }
        }

        private void DgvResultados_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ConfirmarSeleccion();
            }
        }

        private void DgvResultados_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ConfirmarSeleccion();
            }
        }

        #endregion

        #region Lógica de búsqueda

        private void AplicarFiltroBusqueda()
        {
            string texto = SanitizarTextoFiltro(txtBuscar.Text);

            if (string.IsNullOrWhiteSpace(texto))
            {
                _vistaFiltrada.RowFilter = string.Empty;
                return;
            }

            _vistaFiltrada.RowFilter = CrearFiltroBusqueda(texto);
        }

        private static string CrearFiltroBusqueda(string texto)
        {
            bool esNumero = int.TryParse(texto, out _);

            if (esNumero)
            {
                return $"CONVERT([Número], 'System.String') LIKE '%{texto}%' OR [Nombre] LIKE '%{texto}%'";
            }

            return $"[Nombre] LIKE '%{texto}%'";
        }

        private static string SanitizarTextoFiltro(string texto)
        {
            return texto.Replace("'", "''").Trim();
        }

        #endregion

        #region Selección

        private void EnfocarPrimeraFila()
        {
            if (dgvResultados.Rows.Count == 0)
                return;

            dgvResultados.Focus();
            dgvResultados.CurrentCell = dgvResultados.Rows[0].Cells[0];
        }

        private void ConfirmarSeleccion()
        {
            if (dgvResultados.CurrentRow is null)
                return;

            if (!dgvResultados.Columns.Contains("Número"))
                return;

            SelectedId = dgvResultados.CurrentRow.Cells["Número"].Value?.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancelar()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion
    }
}
