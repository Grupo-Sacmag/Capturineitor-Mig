using CapturaDePolizas_2026_NET8.Repositories;
using System.Data;
using System.Globalization;

namespace CapturaDePolizas_2026_NET8;

public partial class FormVerPolizas : Form
{
    private readonly DataTable _datosOriginales;
    private readonly DataView _vistaFiltrada;
    private readonly ICatalogoRepository _catalogoRepository;

    private bool _eventosDeshabilitados;

    public FormVerPolizas(
        DataTable datos,
        bool iniciarComoCheque,
        ICatalogoRepository catalogoRepository)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ArgumentNullException.ThrowIfNull(catalogoRepository);

        InitializeComponent();

        _datosOriginales = datos;
        _vistaFiltrada = new DataView(_datosOriginales);
        _catalogoRepository = catalogoRepository;

        InicializarFormulario(iniciarComoCheque);
    }

    #region Inicialización

    private void InicializarFormulario(bool iniciarComoCheque)
    {
        ConfigurarFormulario();
        ConfigurarControles();
        ConfigurarGrid();
        CargarComboOrdenamiento();
        EnlazarEventos();

        dgvPolizas.DataSource = _vistaFiltrada;

        ConfigurarColumnasGrid();
        AplicarEstadoInicial(iniciarComoCheque);
        AplicarFiltros();
    }

    private void ConfigurarFormulario()
    {
        Text = "Registro de Pólizas y Cheques con Filtros";
        StartPosition = FormStartPosition.CenterParent;
    }

    private void ConfigurarControles()
    {
        txtBuscarTexto.CharacterCasing = CharacterCasing.Upper;

        cmbOrden.DropDownStyle = ComboBoxStyle.DropDownList;

        rbTodos.Checked = true;
        rbFechaUnica.Checked = true;

        dtpDesde.Value = DateTime.Today;
        dtpHasta.Value = DateTime.Today;
    }

    private void ConfigurarGrid()
    {
        dgvPolizas.AllowUserToAddRows = false;
        dgvPolizas.AllowUserToDeleteRows = false;
        dgvPolizas.ReadOnly = true;
        dgvPolizas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvPolizas.MultiSelect = false;
        dgvPolizas.RowHeadersVisible = false;

        dgvPolizas.BackgroundColor = Color.White;
        dgvPolizas.BorderStyle = BorderStyle.None;
        dgvPolizas.GridColor = Color.FromArgb(224, 224, 224);

        dgvPolizas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        dgvPolizas.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvPolizas.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

        dgvPolizas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        dgvPolizas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        dgvPolizas.EnableHeadersVisualStyles = false;
    }

    private void CargarComboOrdenamiento()
    {
        cmbOrden.Items.Clear();

        cmbOrden.Items.Add("Folio (Ascendente)");
        cmbOrden.Items.Add("Folio (Descendente)");
        cmbOrden.Items.Add("ID (Ascendente)");
        cmbOrden.Items.Add("ID (Descendente)");
        cmbOrden.Items.Add("Fecha (Ascendente)");
        cmbOrden.Items.Add("Fecha (Descendente)");

        cmbOrden.SelectedIndex = 0;
    }

    private void AplicarEstadoInicial(bool iniciarComoCheque)
    {
        _eventosDeshabilitados = true;

        try
        {
            if (iniciarComoCheque)
            {
                rbCheque.Checked = true;
            }
            else
            {
                rbPoliza.Checked = true;
            }

            ActualizarEstadoControlesFecha();
        }
        finally
        {
            _eventosDeshabilitados = false;
        }
    }

    private void EnlazarEventos()
    {
        rbTodos.CheckedChanged += Filtros_Changed;
        rbPoliza.CheckedChanged += Filtros_Changed;
        rbCheque.CheckedChanged += Filtros_Changed;

        chkFiltroFecha.CheckedChanged += ChkFiltroFecha_CheckedChanged;
        rbFechaUnica.CheckedChanged += TipoFecha_Changed;
        rbRangoFechas.CheckedChanged += TipoFecha_Changed;

        dtpDesde.ValueChanged += Filtros_Changed;
        dtpHasta.ValueChanged += Filtros_Changed;

        txtBuscarTexto.TextChanged += Filtros_Changed;
        cmbOrden.SelectedIndexChanged += Filtros_Changed;

        btnRestablecer.Click += BtnRestablecer_Click;

        dgvPolizas.KeyDown += DgvPolizas_KeyDown;
        dgvPolizas.CellDoubleClick += DgvPolizas_CellDoubleClick;
    }

    #endregion

    #region Eventos

    private void Filtros_Changed(object? sender, EventArgs e)
    {
        if (_eventosDeshabilitados)
            return;

        AplicarFiltros();
    }

    private void ChkFiltroFecha_CheckedChanged(object? sender, EventArgs e)
    {
        ActualizarEstadoControlesFecha();

        if (_eventosDeshabilitados)
            return;

        AplicarFiltros();
    }

    private void TipoFecha_Changed(object? sender, EventArgs e)
    {
        ActualizarEstadoControlesFecha();

        if (_eventosDeshabilitados)
            return;

        AplicarFiltros();
    }

    private void BtnRestablecer_Click(object? sender, EventArgs e)
    {
        RestablecerFiltros();
    }

    private void DgvPolizas_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;

        AbrirDetallePolizaSeleccionada();
    }

    private void DgvPolizas_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        AbrirDetallePolizaSeleccionada();
    }

    #endregion

    #region Filtros

    private void AplicarFiltros()
    {
        List<string> filtros = new();

        AgregarFiltroTipoCaptura(filtros);
        AgregarFiltroFecha(filtros);
        AgregarFiltroBusqueda(filtros);

        _vistaFiltrada.RowFilter = filtros.Count > 0 ? string.Join(" AND ", filtros) : string.Empty;

        AplicarOrdenamiento();
        CalcularTotalesFiltrados();
        ActualizarVisibilidadColumnas();
    }

    private void AgregarFiltroTipoCaptura(List<string> filtros)
    {
        if (rbPoliza.Checked)
        {
            filtros.Add("tipo_captura = 1");
        }
        else if (rbCheque.Checked)
        {
            filtros.Add("tipo_captura = 0");
        }
    }

    private void AgregarFiltroFecha(List<string> filtros)
    {
        if (!chkFiltroFecha.Checked)
            return;

        if (rbFechaUnica.Checked)
        {
            DateTime fecha = dtpDesde.Value.Date;

            filtros.Add($"fecha = #{fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}#");

            return;
        }

        DateTime desde = dtpDesde.Value.Date;
        DateTime hasta = dtpHasta.Value.Date;

        filtros.Add(
            $"fecha >= #{desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}# " +
            $"AND fecha <= #{hasta.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}#"
        );
    }

    private void AgregarFiltroBusqueda(List<string> filtros)
    {
        string texto = SanitizarTextoFiltro(txtBuscarTexto.Text);

        if (string.IsNullOrWhiteSpace(texto))
            return;

        filtros.Add(
            $"(ISNULL(concepto, '') LIKE '%{texto}%' " +
            $"OR ISNULL(beneficiario, '') LIKE '%{texto}%' " +
            $"OR ISNULL(rfc, '') LIKE '%{texto}%' " +
            $"OR ISNULL(numero_cheque, '') LIKE '%{texto}%')"
        );
    }

    private void AplicarOrdenamiento()
    {
        if (cmbOrden.SelectedIndex < 0)
            return;

        _vistaFiltrada.Sort = cmbOrden.SelectedIndex switch
        {
            0 => "folio ASC",
            1 => "folio DESC",
            2 => "id ASC",
            3 => "id DESC",
            4 => "fecha ASC",
            5 => "fecha DESC",
            _ => "folio ASC"
        };
    }

    private static string SanitizarTextoFiltro(string texto)
    {
        return texto.Replace("'", "''").Trim();
    }

    #endregion

    #region Fechas

    private void ActualizarEstadoControlesFecha()
    {
        bool filtroHabilitado = chkFiltroFecha.Checked;

        rbFechaUnica.Enabled = filtroHabilitado;
        rbRangoFechas.Enabled = filtroHabilitado;

        dtpDesde.Enabled = filtroHabilitado;
        lblDesde.Enabled = filtroHabilitado;

        bool rangoHabilitado = filtroHabilitado && rbRangoFechas.Checked;

        dtpHasta.Enabled = rangoHabilitado;
        lblHasta.Enabled = rangoHabilitado;
    }

    #endregion

    #region Totales

    private void CalcularTotalesFiltrados()
    {
        decimal totalDebe = 0;
        decimal totalHaber = 0;

        foreach (DataRowView rowView in _vistaFiltrada)
        {
            DataRow row = rowView.Row;

            totalDebe += ObtenerDecimal(row, "total_debe");
            totalHaber += ObtenerDecimal(row, "total_haber");
        }

        decimal diferencia = Math.Abs(totalDebe - totalHaber);

        lblSumDebe.Text = $"Total Debe: {totalDebe.ToString("C2", CultureInfo.CurrentCulture)}";
        lblSumHaber.Text = $"Total Haber: {totalHaber.ToString("C2", CultureInfo.CurrentCulture)}";
        lblDiferencia.Text = $"Diferencia: {diferencia.ToString("C2", CultureInfo.CurrentCulture)}";

        lblDiferencia.ForeColor = diferencia > 0.01m ? Color.DarkRed : Color.DarkGreen;
    }

    private static decimal ObtenerDecimal(DataRow row, string columna)
    {
        if (!row.Table.Columns.Contains(columna))
            return 0m;

        if (row[columna] == DBNull.Value)
            return 0m;

        return Convert.ToDecimal(row[columna]);
    }

    #endregion

    #region Grid

    private void ConfigurarColumnasGrid()
    {
        OcultarColumnasInternas();
        RenombrarColumnas();
        ConfigurarOrdenamientoColumnas();
        ConfigurarTamanosColumnas();
    }

    private void OcultarColumnasInternas()
    {
        string[] columnasOcultas =
        {
            "id",
            "requiere_impresion",
            "es_costos",
            "tipo_captura",
            "folios_fiscales"
        };

        foreach (string columna in columnasOcultas)
        {
            if (dgvPolizas.Columns.Contains(columna))
            {
                dgvPolizas.Columns[columna].Visible = false;
            }
        }
    }

    private void RenombrarColumnas()
    {
        Dictionary<string, string> titulos = new()
        {
            { "id", "ID" },
            { "fecha", "Fecha" },
            { "folio", "Folio" },
            { "concepto", "Concepto" },
            { "total_debe", "Total Debe" },
            { "total_haber", "Total Haber" },
            { "beneficiario", "Beneficiario" },
            { "monto_cheque", "Monto Cheque" },
            { "numero_cheque", "Número Cheque" },
            { "rfc", "RFC" },
            { "creado_en", "Creado En" }
        };

        foreach ((string columna, string titulo) in titulos)
        {
            if (dgvPolizas.Columns.Contains(columna))
            {
                dgvPolizas.Columns[columna].HeaderText = titulo;
            }
        }
    }

    private void ConfigurarOrdenamientoColumnas()
    {
        foreach (DataGridViewColumn columna in dgvPolizas.Columns)
        {
            columna.SortMode = EsColumnaOrdenable(columna.Name) ? DataGridViewColumnSortMode.Automatic : DataGridViewColumnSortMode.NotSortable;
        }
    }

    private static bool EsColumnaOrdenable(string columna)
    {
        return columna is "id" or "folio" or "fecha" or "total_debe" or "total_haber";
    }

    private void ConfigurarTamanosColumnas()
    {
        ConfigurarAncho("id", 40);
        ConfigurarAncho("fecha", 90);
        ConfigurarAncho("folio", 60);
        ConfigurarAncho("beneficiario", 180);
        ConfigurarAncho("numero_cheque", 100);
        ConfigurarAncho("rfc", 100);
        ConfigurarAncho("creado_en", 140);

        ConfigurarColumnaMoneda("total_debe", 100);
        ConfigurarColumnaMoneda("total_haber", 100);
        ConfigurarColumnaMoneda("monto_cheque", 100);

        if (dgvPolizas.Columns.Contains("concepto"))
        {
            dgvPolizas.Columns["concepto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
    }

    private void ConfigurarAncho(string columna, int ancho)
    {
        if (!dgvPolizas.Columns.Contains(columna))
            return;

        dgvPolizas.Columns[columna].Width = ancho;
    }

    private void ConfigurarColumnaMoneda(string columna, int ancho)
    {
        if (!dgvPolizas.Columns.Contains(columna))
            return;

        dgvPolizas.Columns[columna].Width = ancho;
        dgvPolizas.Columns[columna].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvPolizas.Columns[columna].DefaultCellStyle.Format = "N2";
    }

    private void ActualizarVisibilidadColumnas()
    {
        bool mostrandoPolizas = rbPoliza.Checked;

        SetColumnaVisible("beneficiario", !mostrandoPolizas);
        SetColumnaVisible("numero_cheque", !mostrandoPolizas);
        SetColumnaVisible("rfc", !mostrandoPolizas);
        SetColumnaVisible("monto_cheque", !mostrandoPolizas);
    }

    private void SetColumnaVisible(string columna, bool visible)
    {
        if (!dgvPolizas.Columns.Contains(columna))
            return;

        dgvPolizas.Columns[columna].Visible = visible;
    }

    #endregion

    #region Detalle

    private void AbrirDetallePolizaSeleccionada()
    {
        if (dgvPolizas.CurrentRow is null)
            return;

        DataGridViewRow row = dgvPolizas.CurrentRow;

        int polizaId = ObtenerEnteroCelda(row, "id");

        if (polizaId <= 0)
            return;

        int folio = ObtenerEnteroCelda(row, "folio");
        string concepto = ObtenerTextoCelda(row, "concepto");
        string rfc = ObtenerTextoCelda(row, "rfc");
        string foliosFiscales = ObtenerTextoCelda(row, "folios_fiscales");
        DateTime fecha = ObtenerFechaCelda(row, "fecha");

        try
        {
            DataTable movimientos = _catalogoRepository.ObtenerMovimientosPoliza(polizaId);

            using FormDetallePoliza form = new(
                folio,
                concepto,
                rfc,
                foliosFiscales,
                fecha,
                movimientos
            );

            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al abrir el detalle de la póliza: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
            );
        }
    }

    private static int ObtenerEnteroCelda(DataGridViewRow row, string columna)
    {
        if (!row.DataGridView.Columns.Contains(columna))
            return 0;

        object? valor = row.Cells[columna].Value;

        if (valor is null || valor == DBNull.Value)
            return 0;

        return Convert.ToInt32(valor);
    }

    private static string ObtenerTextoCelda(DataGridViewRow row, string columna)
    {
        if (!row.DataGridView.Columns.Contains(columna))
            return string.Empty;

        object? valor = row.Cells[columna].Value;

        if (valor is null || valor == DBNull.Value)
            return string.Empty;

        return valor.ToString() ?? string.Empty;
    }

    private static DateTime ObtenerFechaCelda(DataGridViewRow row, string columna)
    {
        if (!row.DataGridView.Columns.Contains(columna))
            return DateTime.Today;

        object? valor = row.Cells[columna].Value;

        if (valor is null || valor == DBNull.Value)
            return DateTime.Today;

        return Convert.ToDateTime(valor);
    }

    #endregion

    #region Restablecer

    private void RestablecerFiltros()
    {
        _eventosDeshabilitados = true;

        try
        {
            rbTodos.Checked = true;

            chkFiltroFecha.Checked = false;
            rbFechaUnica.Checked = true;

            dtpDesde.Value = DateTime.Today;
            dtpHasta.Value = DateTime.Today;

            txtBuscarTexto.Clear();

            if (cmbOrden.Items.Count > 0)
            {
                cmbOrden.SelectedIndex = 0;
            }

            ActualizarEstadoControlesFecha();
        }
        finally
        {
            _eventosDeshabilitados = false;
        }

        AplicarFiltros();
    }

    #endregion
}