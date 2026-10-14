using CapturaDePolizas_2026_NET8.Repositories;
using System.Data;
using System.Globalization;

namespace CapturaDePolizas_2026_NET8;

public partial class FormVerSubcuentas : Form
{
    private readonly DataTable _datosOriginales;
    private readonly DataView _vistaFiltrada;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly int? _cuentaIdInicial;

    private bool _eventosDeshabilitados;

    public FormVerSubcuentas(DataTable datos, ICatalogoRepository catalogoRepository, int? cuentaIdInicial = null)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ArgumentNullException.ThrowIfNull(catalogoRepository);

        InitializeComponent();

        _datosOriginales = datos;
        _vistaFiltrada = new DataView(_datosOriginales);
        _catalogoRepository = catalogoRepository;
        _cuentaIdInicial = cuentaIdInicial;

        InicializarFormulario();
    }

    #region Inicialización

    private void InicializarFormulario()
    {
        ConfigurarFormulario();
        ConfigurarControles();
        ConfigurarGrid();
        CargarComboOrdenamiento();
        CargarComboCuentas();
        EnlazarEventos();

        dgvSubcuentas.DataSource = _vistaFiltrada;

        ConfigurarColumnasGrid();
        AplicarFiltros();

        Shown += (s, e) =>
        {
            if (dgvSubcuentas.Rows.Count > 0)
            {
                dgvSubcuentas.Focus();
            }
        };
    }

    private void ConfigurarFormulario()
    {
        Text = "Catálogo de Subcuentas";
        StartPosition = FormStartPosition.CenterParent;
    }

    private void ConfigurarControles()
    {
        txtBuscarTexto.CharacterCasing = CharacterCasing.Upper;

        cmbCuentaPadre.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbColumna.DropDownStyle = ComboBoxStyle.DropDownList;

        rbAsc.Checked = true;
    }

    private void ConfigurarGrid()
    {
        dgvSubcuentas.AllowUserToAddRows = false;
        dgvSubcuentas.AllowUserToDeleteRows = false;
        dgvSubcuentas.ReadOnly = true;
        dgvSubcuentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSubcuentas.MultiSelect = false;
        dgvSubcuentas.RowHeadersVisible = true;

        dgvSubcuentas.BackgroundColor = Color.White;
        dgvSubcuentas.BorderStyle = BorderStyle.None;
        dgvSubcuentas.GridColor = Color.FromArgb(224, 224, 224);

        dgvSubcuentas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        dgvSubcuentas.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvSubcuentas.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

        dgvSubcuentas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        dgvSubcuentas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        dgvSubcuentas.EnableHeadersVisualStyles = false;
    }

    private void CargarComboOrdenamiento()
    {
        cmbColumna.Items.Clear();

        cmbColumna.Items.Add("Cuenta");
        cmbColumna.Items.Add("Subcuenta");
        cmbColumna.Items.Add("Nombre");
        cmbColumna.Items.Add("Debe");
        cmbColumna.Items.Add("Haber");
        cmbColumna.Items.Add("Importe Neto");

        cmbColumna.SelectedIndex = 0;
    }

    private void CargarComboCuentas()
    {
        _eventosDeshabilitados = true;

        try
        {
            cmbCuentaPadre.Items.Clear();
            cmbCuentaPadre.Items.Add("(Todas)");

            DataTable cuentas = _catalogoRepository.ObtenerCuentasBusqueda();

            int indiceSeleccionar = 0;
            int indice = 1;

            foreach (DataRow row in cuentas.Rows)
            {
                string numero = row["Número"]?.ToString() ?? string.Empty;
                string nombre = row["Nombre"]?.ToString() ?? string.Empty;

                cmbCuentaPadre.Items.Add($"{numero} - {nombre}");

                if (_cuentaIdInicial.HasValue && int.TryParse(numero, out int cId) && cId == _cuentaIdInicial.Value)
                {
                    indiceSeleccionar = indice;
                }

                indice++;
            }

            cmbCuentaPadre.SelectedIndex = indiceSeleccionar;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar el catálogo de cuentas para filtrar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _eventosDeshabilitados = false;
        }
    }

    private void EnlazarEventos()
    {
        txtBuscarTexto.TextChanged += Filtros_Changed;
        cmbCuentaPadre.SelectedIndexChanged += Filtros_Changed;

        txtMontoMin.TextChanged += Filtros_Changed;
        txtMontoMax.TextChanged += Filtros_Changed;

        rbAsc.CheckedChanged += Ordenamiento_Changed;
        rbDesc.CheckedChanged += Ordenamiento_Changed;
        cmbColumna.SelectedIndexChanged += Filtros_Changed;

        btnRestablecer.Click += BtnRestablecer_Click;
        dgvSubcuentas.CellDoubleClick += DgvSubcuentas_CellDoubleClick;
        dgvSubcuentas.KeyDown += DgvSubcuentas_KeyDown;
    }

    private void DgvSubcuentas_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            AbrirPolizasDeSubcuentaSeleccionada();
        }
    }

    private void DgvSubcuentas_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            AbrirPolizasDeSubcuentaSeleccionada();
        }
    }

    private void AbrirPolizasDeSubcuentaSeleccionada()
    {
        if (dgvSubcuentas.CurrentRow?.DataBoundItem is not DataRowView drv)
            return;

        if (int.TryParse(drv["Cuenta"]?.ToString(), out int cuentaId) &&
            int.TryParse(drv["Subcuenta"]?.ToString(), out int subcuentaId))
        {
            try
            {
                DataTable datosPolizas = _catalogoRepository.ObtenerTablaPolizasPorSubcuenta(cuentaId, subcuentaId);

                using FormVerPolizas form = new(datosPolizas, iniciarComoCheque: null, _catalogoRepository);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir las pólizas de la subcuenta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Enter)
        {
            AbrirPolizasDeSubcuentaSeleccionada();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
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
        AgregarFiltroCuentaPadre(filtros);
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

        filtros.Add($"(CONVERT([Subcuenta], 'System.String') LIKE '%{texto}%' OR [Nombre] LIKE '%{texto}%')");
    }

    private void AgregarFiltroCuentaPadre(List<string> filtros)
    {
        if (cmbCuentaPadre.SelectedIndex <= 0)
            return;

        string? textoSeleccionado = cmbCuentaPadre.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(textoSeleccionado))
            return;

        int cuentaId = ExtraerCuentaIdDesdeCombo(textoSeleccionado);

        if (cuentaId <= 0)
            return;

        filtros.Add($"[Cuenta] = {cuentaId}");
    }

    private void AgregarFiltroMontoMinimo(List<string> filtros)
    {
        if (!decimal.TryParse(txtMontoMin.Text, out decimal montoMinimo))
            return;

        filtros.Add($"[Importe Neto] >= {montoMinimo.ToString(CultureInfo.InvariantCulture)}");
    }

    private void AgregarFiltroMontoMaximo(List<string> filtros)
    {
        if (!decimal.TryParse(txtMontoMax.Text, out decimal montoMaximo))
            return;

        filtros.Add($"[Importe Neto] <= {montoMaximo.ToString(CultureInfo.InvariantCulture)}");
    }

    private void AplicarOrdenamiento()
    {
        string? columna = cmbColumna.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(columna))
            return;

        string direccion = rbAsc.Checked ? "ASC" : "DESC";

        _vistaFiltrada.Sort = $"[{columna}] {direccion}";
    }

    private static int ExtraerCuentaIdDesdeCombo(string textoSeleccionado)
    {
        string[] partes = textoSeleccionado.Split('-', 2);

        if (partes.Length == 0)
            return 0;

        return int.TryParse(partes[0].Trim(), out int cuentaId) ? cuentaId : 0;
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
        ConfigurarColumnaCuenta();
        ConfigurarColumnaSubcuenta();
        ConfigurarColumnaNombre();
        ConfigurarColumnaDebe();
        ConfigurarColumnaHaber();
        ConfigurarColumnaImporteNeto();
    }

    private void DeshabilitarOrdenamientoManual()
    {
        foreach (DataGridViewColumn columna in dgvSubcuentas.Columns)
        {
            columna.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    private void ConfigurarColumnaCuenta()
    {
        if (!dgvSubcuentas.Columns.Contains("Cuenta"))
            return;

        dgvSubcuentas.Columns["Cuenta"].Width = 100;
    }

    private void ConfigurarColumnaSubcuenta()
    {
        if (!dgvSubcuentas.Columns.Contains("Subcuenta"))
            return;

        dgvSubcuentas.Columns["Subcuenta"].Width = 80;
    }

    private void ConfigurarColumnaDebe()
    {
        if (!dgvSubcuentas.Columns.Contains("Debe"))
            return;

        dgvSubcuentas.Columns["Debe"].Width = 110;
        dgvSubcuentas.Columns["Debe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvSubcuentas.Columns["Debe"].DefaultCellStyle.Format = "N2";
    }

    private void ConfigurarColumnaHaber()
    {
        if (!dgvSubcuentas.Columns.Contains("Haber"))
            return;

        dgvSubcuentas.Columns["Haber"].Width = 110;
        dgvSubcuentas.Columns["Haber"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvSubcuentas.Columns["Haber"].DefaultCellStyle.Format = "N2";
    }

    private void ConfigurarColumnaImporteNeto()
    {
        if (!dgvSubcuentas.Columns.Contains("Importe Neto"))
            return;

        dgvSubcuentas.Columns["Importe Neto"].HeaderText = "Total";
        dgvSubcuentas.Columns["Importe Neto"].Width = 120;
        dgvSubcuentas.Columns["Importe Neto"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvSubcuentas.Columns["Importe Neto"].DefaultCellStyle.Format = "N2";
        dgvSubcuentas.Columns["Importe Neto"].Visible = true;
    }

    private void ConfigurarColumnaNombre()
    {
        if (!dgvSubcuentas.Columns.Contains("Nombre"))
            return;

        dgvSubcuentas.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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

            if (cmbCuentaPadre.Items.Count > 0)
            {
                cmbCuentaPadre.SelectedIndex = 0;
            }

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