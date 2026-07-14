using CapturaDePolizas_2026_NET8.Models;
using CapturaDePolizas_2026_NET8.Repositories;
using GaCostos;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CapturaDePolizas_2026_NET8;

public partial class FormCaptura : Form
{
    private enum TipoCaptura
    {
        Poliza,
        Cheque
    }

    private readonly ICatalogoRepository _catalogoRepository;

    private DataGridViewCell? _activeCell;

    private readonly Stack<string[,]> _undoStack = new();
    private readonly Stack<string[,]> _redoStack = new();

    private string[,]? _lastState;
    private bool _isUndoing;

    public FormCaptura()
    {
        InitializeComponent();

        _catalogoRepository = new CatalogoRepository();

        InicializarFormulario();
    }

    #region Inicialización

    private void InicializarFormulario()
    {
        ConfigurarFormulario();
        ConfigurarCamposTexto();
        ConfigurarGrid();
        EnlazarEventos();

        Load += FormCaptura_Load;
        Shown += FormCaptura_Shown;
    }

    private void ConfigurarFormulario()
    {
        KeyPreview = true;
    }

    private void ConfigurarCamposTexto()
    {
        txtConceptoPoliza.CharacterCasing = CharacterCasing.Upper;
        txtConceptoCheque.CharacterCasing = CharacterCasing.Upper;
        txtBeneficiario.CharacterCasing = CharacterCasing.Upper;
        txtRFC.CharacterCasing = CharacterCasing.Upper;
        txtFolios.CharacterCasing = CharacterCasing.Upper;
        txtNumeroCheque.CharacterCasing = CharacterCasing.Upper;

        txtPolizaNo.ReadOnly = true;
    }

    private void ConfigurarGrid()
    {
        dgvMovimientos.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvMovimientos.MultiSelect = false;

        InicializarColumnasGrid();
    }

    private void EnlazarEventos()
    {
        dgvMovimientos.EditingControlShowing += DgvMovimientos_EditingControlShowing;
        dgvMovimientos.KeyDown += DgvMovimientos_KeyDown;
        dgvMovimientos.SelectionChanged += DgvMovimientos_SelectionChanged;
        dgvMovimientos.CellDoubleClick += DgvMovimientos_CellDoubleClick;

        txtConceptoPoliza.TextChanged += TxtConceptoPoliza_TextChanged;
        txtConceptoCheque.TextChanged += TxtConceptoCheque_TextChanged;
        txtFolios.TextChanged += TxtFolios_TextChanged;

        txtConceptoPoliza.KeyDown += TxtConceptoPoliza_KeyDown;
        txtBeneficiario.KeyDown += TxtBeneficiario_KeyDown;
        txtConceptoCheque.KeyDown += TxtConceptoCheque_KeyDown;
        txtMonto.KeyDown += TxtMonto_KeyDown;
        txtRFC.KeyDown += TxtRFC_KeyDown;
        txtFolios.KeyDown += TxtFolios_KeyDown;

        btnGuardar.Click += BtnGuardar_Click;
        btnAnterior.Click += BtnAnterior_Click;
        btnSiguiente.Click += BtnSiguiente_Click;

        chkVarios.CheckedChanged += ChkVarios_CheckedChanged;

        mnuCapturaPoliza.Click += MnuCapturaPoliza_Click;
        mnuCapturaCheque.Click += MnuCapturaCheque_Click;

        mnuVerCuentas.Click += MnuVerCuentas_Click;
        mnuVerSubcuentas.Click += MnuVerSubcuentas_Click;
        mnuVerPolizas.Click += MnuVerPolizas_Click;
        mnuVerCheques.Click += MnuVerCheques_Click;
        mnuVerEstadosFinancieros.Click += MnuVerEstadosFinancieros_Click;
    }

    private void FormCaptura_Load(object? sender, EventArgs e)
    {
        ConfigurarPantalla(TipoCaptura.Poliza);

        CrearFilasIniciales();

        dgvMovimientos.CellValueChanged += DgvMovimientos_CellValueChanged;

        _lastState = SnapshotGridState();

        if (dgvMovimientos.Rows.Count > 0)
        {
            ReenfocarCelda(0, 0);
        }
    }

    private void FormCaptura_Shown(object? sender, EventArgs e)
    {
        txtConceptoPoliza.Focus();
    }

    #endregion

    #region Configuración de pantalla

    private void ConfigurarPantalla(TipoCaptura tipo)
    {
        pnlCamposCheque.Visible = tipo == TipoCaptura.Cheque;
        pnlCamposPoliza.Visible = tipo == TipoCaptura.Poliza;

        CargarSiguienteFolio();

        if (dgvMovimientos.Columns.Contains("Redaccion"))
        {
            dgvMovimientos.Columns["Redaccion"].Visible = tipo == TipoCaptura.Poliza;
        }

        if (dgvMovimientos.Columns.Contains("FolioFiscal"))
        {
            dgvMovimientos.Columns["FolioFiscal"].Visible = tipo == TipoCaptura.Cheque;
        }

        if (tipo == TipoCaptura.Poliza)
        {
            Text = "SACMAG DE MEXICO SA CV - Captura de Pólizas";
            txtConceptoPoliza.Focus();
        }
        else
        {
            Text = "SACMAG DE MEXICO SA CV - CAPTURA DE CHEQUES";
            CargarSiguienteNumeroCheque();
            txtBeneficiario.Focus();
        }

        ActualizarEstadoBotonGuardar();
    }

    private void CargarSiguienteFolio()
    {
        try
        {
            txtPolizaNo.Text = _catalogoRepository.ObtenerSiguienteFolio().ToString();
        }
        catch (Exception ex)
        {
            MostrarError("Error al obtener el siguiente número de póliza", ex);
            txtPolizaNo.Text = "1";
        }
    }

    private void CargarSiguienteNumeroCheque()
    {
        try
        {
            txtNumeroCheque.Text = _catalogoRepository.ObtenerSiguienteNumeroCheque();
        }
        catch (Exception ex)
        {
            MostrarError("Error al obtener el siguiente número de cheque", ex);
            txtNumeroCheque.Text = "1";
        }
    }

    private void IntentarCambiarPantalla(TipoCaptura nuevoTipo)
    {
        TipoCaptura tipoActual = ObtenerTipoCapturaActual();

        if (!FormularioTieneDatos())
        {
            RefrescarCaptura();
            ConfigurarPantalla(nuevoTipo);
            return;
        }

        DialogResult result = MessageBox.Show(
            "¿Desea guardar los cambios de la captura actual antes de cambiar de pantalla?",
            "Confirmar Guardado",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            if (GuardarCaptura(tipoActual))
            {
                RefrescarCaptura();
                ConfigurarPantalla(nuevoTipo);
            }

            return;
        }

        if (result == DialogResult.No)
        {
            RefrescarCaptura();
            ConfigurarPantalla(nuevoTipo);
        }
    }

    private TipoCaptura ObtenerTipoCapturaActual()
    {
        return pnlCamposPoliza.Visible
            ? TipoCaptura.Poliza
            : TipoCaptura.Cheque;
    }

    #endregion

    #region Grid

    private void InicializarColumnasGrid()
    {
        dgvMovimientos.Columns.Clear();

        dgvMovimientos.Columns.Add("Cuenta", "Cuenta");
        dgvMovimientos.Columns.Add("SubCta", "SubCta");
        dgvMovimientos.Columns.Add("Nombre", "Nombre");
        dgvMovimientos.Columns.Add("Parcial", "Parcial");
        dgvMovimientos.Columns.Add("Debe", "Debe");
        dgvMovimientos.Columns.Add("Haber", "Haber");
        dgvMovimientos.Columns.Add("Redaccion", "Redacción");
        dgvMovimientos.Columns.Add("FolioFiscal", "Folio fiscal");

        dgvMovimientos.Columns["Nombre"].ReadOnly = true;
        dgvMovimientos.Columns["Debe"].ReadOnly = true;
        dgvMovimientos.Columns["Haber"].ReadOnly = true;
        dgvMovimientos.Columns["Redaccion"].ReadOnly = true;
        dgvMovimientos.Columns["FolioFiscal"].ReadOnly = true;

        foreach (DataGridViewColumn columna in dgvMovimientos.Columns)
        {
            columna.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        dgvMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

        ConfigurarColumnaGrid("Cuenta", 100, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumnaGrid("SubCta", 80, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumnaGrid("Parcial", 120, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumnaGrid("Debe", 120, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumnaGrid("Haber", 120, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumnaGrid("Redaccion", 180, DataGridViewContentAlignment.MiddleLeft);
        ConfigurarColumnaGrid("FolioFiscal", 150, DataGridViewContentAlignment.MiddleLeft);

        dgvMovimientos.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgvMovimientos.Columns["Nombre"].MinimumWidth = 180;
    }

    private void ConfigurarColumnaGrid(
        string columna,
        int ancho,
        DataGridViewContentAlignment alineacion)
    {
        dgvMovimientos.Columns[columna].Width = ancho;
        dgvMovimientos.Columns[columna].DefaultCellStyle.Alignment = alineacion;
    }

    private void CrearFilasIniciales()
    {
        dgvMovimientos.Rows.Clear();

        for (int i = 0; i < 15; i++)
        {
            AgregarFilaGrid();
        }
    }

    private int AgregarFilaGrid()
    {
        int index = dgvMovimientos.Rows.Add();

        if (dgvMovimientos.Columns.Contains("Redaccion"))
        {
            dgvMovimientos["Redaccion", index].Value = string.Empty;
        }

        _lastState = SnapshotGridState();

        return index;
    }

    private void RefrescarCaptura()
    {
        _activeCell = null;

        CrearFilasIniciales();

        txtConceptoPoliza.Clear();
        txtConceptoCheque.Clear();
        txtBeneficiario.Clear();
        txtMonto.Clear();
        txtNumeroCheque.Clear();
        txtRFC.Clear();
        txtFolios.Clear();

        chkVarios.Checked = false;

        _undoStack.Clear();
        _redoStack.Clear();

        ActualizarBotonesUndoRedo();

        _lastState = SnapshotGridState();

        if (dgvMovimientos.Rows.Count > 0)
        {
            ReenfocarCelda(0, 0);
        }
    }

    private bool FormularioTieneDatos()
    {
        foreach (DataGridViewRow row in dgvMovimientos.Rows)
        {
            string cuenta = row.Cells["Cuenta"].Value?.ToString() ?? string.Empty;
            string subcuenta = row.Cells["SubCta"].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cuenta) || !string.IsNullOrWhiteSpace(subcuenta))
                return true;
        }

        return false;
    }

    #endregion

    #region Eventos de menú

    private void MnuCapturaPoliza_Click(object? sender, EventArgs e)
    {
        IntentarCambiarPantalla(TipoCaptura.Poliza);
    }

    private void MnuCapturaCheque_Click(object? sender, EventArgs e)
    {
        IntentarCambiarPantalla(TipoCaptura.Cheque);
    }

    private void MnuVerCuentas_Click(object? sender, EventArgs e)
    {
        try
        {
            DataTable datos = _catalogoRepository.ObtenerTablaCuentas();

            using FormVerCuentas form = new(datos);
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir catálogo de cuentas", ex);
        }
    }

    private void MnuVerSubcuentas_Click(object? sender, EventArgs e)
    {
        try
        {
            DataTable datos = _catalogoRepository.ObtenerTablaSubcuentas();

            using FormVerSubcuentas form = new(datos, _catalogoRepository);
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir catálogo de subcuentas", ex);
        }
    }

    private void MnuVerPolizas_Click(object? sender, EventArgs e)
    {
        try
        {
            DataTable datos = _catalogoRepository.ObtenerTablaPolizas();

            using FormVerPolizas form = new(
                datos,
                iniciarComoCheque: false,
                _catalogoRepository
            );

            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir pólizas", ex);
        }
    }

    private void MnuVerCheques_Click(object? sender, EventArgs e)
    {
        try
        {
            DataTable datos = _catalogoRepository.ObtenerTablaPolizas();

            using FormVerPolizas form = new(
                datos,
                iniciarComoCheque: true,
                _catalogoRepository
            );

            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir cheques", ex);
        }
    }

    private void MnuVerEstadosFinancieros_Click(object? sender, EventArgs e)
    {
        try
        {
            DataTable datos = _catalogoRepository.ObtenerTablaVacia();

            using FormVerGrid form = new("Estados Financieros", datos);
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir estados financieros", ex);
        }
    }

    #endregion

    #region Eventos de captura

    private void TxtConceptoPoliza_TextChanged(object? sender, EventArgs e)
    {
        SincronizarConcepto(txtConceptoPoliza.Text);
    }

    private void TxtConceptoCheque_TextChanged(object? sender, EventArgs e)
    {
        SincronizarConcepto(txtConceptoCheque.Text);
    }

    private void TxtFolios_TextChanged(object? sender, EventArgs e)
    {
        if (!chkVarios.Checked)
        {
            SincronizarFolioFiscal(txtFolios.Text);
        }
    }

    private void ChkVarios_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkVarios.Checked)
        {
            txtFolios.Enabled = false;
            txtFolios.Clear();

            if (dgvMovimientos.Columns.Contains("FolioFiscal"))
            {
                dgvMovimientos.Columns["FolioFiscal"].ReadOnly = false;
            }

            return;
        }

        txtFolios.Enabled = true;

        if (dgvMovimientos.Columns.Contains("FolioFiscal"))
        {
            SincronizarFolioFiscal(txtFolios.Text);
            dgvMovimientos.Columns["FolioFiscal"].ReadOnly = true;
        }
    }

    #endregion

    #region Eventos del grid

    private void DgvMovimientos_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_isUndoing)
            return;

        if (e.RowIndex < 0)
            return;

        GuardarEstadoParaUndo();

        if (e.ColumnIndex == 0 || e.ColumnIndex == 1)
        {
            ActualizarColumnasDependientes(e.RowIndex);
        }

        _lastState = SnapshotGridState();

        ActualizarBotonesUndoRedo();
        ActualizarEstadoBotonGuardar();
    }

    private void DgvMovimientos_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            DeshacerAccion();
            return;
        }

        if (e.Control && e.KeyCode == Keys.Y)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            RehacerAccion();
            return;
        }

        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            EjecutarCoreografiaTeclado();
        }
    }

    private void DgvMovimientos_EditingControlShowing(
        object? sender,
        DataGridViewEditingControlShowingEventArgs e)
    {
        if (e.Control is not TextBox textBox)
            return;

        textBox.KeyPress -= ValidarNumeroEntero;
        textBox.KeyPress -= ValidarNumeroDecimal;
        textBox.KeyDown -= EditingControl_KeyDown;

        int colIndex = dgvMovimientos.CurrentCell.ColumnIndex;

        if (colIndex == 0 || colIndex == 1)
        {
            textBox.KeyPress += ValidarNumeroEntero;
        }
        else if (colIndex == 3)
        {
            textBox.KeyPress += ValidarNumeroDecimal;
        }

        textBox.KeyDown += EditingControl_KeyDown;
    }

    private void EditingControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            EjecutarCoreografiaTeclado();
        }
    }

    private void DgvMovimientos_SelectionChanged(object? sender, EventArgs e)
    {
        if (_activeCell is null)
            return;

        if (dgvMovimientos.CurrentCell == _activeCell)
            return;

        dgvMovimientos.SelectionChanged -= DgvMovimientos_SelectionChanged;

        try
        {
            dgvMovimientos.CurrentCell = _activeCell;
            dgvMovimientos.BeginEdit(true);
        }
        finally
        {
            dgvMovimientos.SelectionChanged += DgvMovimientos_SelectionChanged;
        }
    }

    private void DgvMovimientos_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        if (e.ColumnIndex == 0 && e.RowIndex % 2 == 0)
        {
            AbrirBusquedaCuenta(e.RowIndex);
            return;
        }

        if (e.ColumnIndex == 1 && e.RowIndex % 2 != 0)
        {
            AbrirBusquedaSubcuenta(e.RowIndex);
            return;
        }

        if (dgvMovimientos.Columns.Contains("FolioFiscal")
            && e.ColumnIndex == dgvMovimientos.Columns["FolioFiscal"].Index
            && pnlCamposCheque.Visible
            && chkVarios.Checked)
        {
            CargarFolioFiscalDesdeXml(e.RowIndex);
        }
    }

    #endregion

    #region Navegación con Enter

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Z))
        {
            DeshacerAccion();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Y))
        {
            RehacerAccion();
            return true;
        }

        if (keyData == Keys.Enter && (dgvMovimientos.Focused || dgvMovimientos.ContainsFocus))
        {
            EjecutarCoreografiaTeclado();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void EjecutarCoreografiaTeclado()
    {
        if (dgvMovimientos.CurrentCell is null)
            return;

        if (dgvMovimientos.IsCurrentCellInEditMode)
        {
            dgvMovimientos.EndEdit();
        }

        int row = dgvMovimientos.CurrentCell.RowIndex;
        int col = dgvMovimientos.CurrentCell.ColumnIndex;

        switch (col)
        {
            case 0:
                ProcesarCeldaCuenta(row);
                break;

            case 1:
                ProcesarCeldaSubcuenta(row);
                break;

            case 3:
                ProcesarCeldaParcial(row);
                break;

            case 7:
                ProcesarCeldaFolioFiscal(row);
                break;
        }
    }

    private void ProcesarCeldaCuenta(int row)
    {
        string cuentaTexto = dgvMovimientos["Cuenta", row].Value?.ToString() ?? string.Empty;

        if (!int.TryParse(cuentaTexto, out int cuentaId))
        {
            MessageBox.Show(
                "POR FAVOR DIGITE UN NÚMERO DE CUENTA VÁLIDO.",
                "VALIDACIÓN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            ReenfocarCelda(row, 0);
            return;
        }

        try
        {
            if (_catalogoRepository.ValidarCuenta(cuentaId.ToString(), out string nombreCuenta))
            {
                dgvMovimientos["Nombre", row].Value = nombreCuenta.ToUpper();

                if (row == dgvMovimientos.Rows.Count - 1)
                {
                    AgregarFilaGrid();
                }

                ReenfocarCelda(row + 1, 1);
                return;
            }

            MessageBox.Show(
                $"LA CUENTA {cuentaId} NO EXISTE EN EL CATÁLOGO.",
                "VALIDACIÓN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            ReenfocarCelda(row, 0);
        }
        catch (Exception ex)
        {
            MostrarError("Error al validar cuenta", ex);
            ReenfocarCelda(row, 0);
        }
    }

    private void ProcesarCeldaSubcuenta(int row)
    {
        if (row == 0)
        {
            MessageBox.Show(
                "NO SE PUEDE CAPTURAR UNA SUBCUENTA EN EL PRIMER RENGLÓN SIN UNA CUENTA PADRE ARRIBA.",
                "ERROR DE FLUJO",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            ReenfocarCelda(row, 0);
            return;
        }

        string subcuentaTexto = dgvMovimientos["SubCta", row].Value?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(subcuentaTexto))
        {
            ProcesarSubcuentaVacia(row);
            return;
        }

        if (!int.TryParse(subcuentaTexto, out int subcuentaId))
        {
            MessageBox.Show(
                "POR FAVOR DIGITE UNA SUBCUENTA VÁLIDA.",
                "VALIDACIÓN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            ReenfocarCelda(row, 1);
            return;
        }

        int cuentaPadreId = ObtenerCuentaPadreId(row);

        if (cuentaPadreId == 0)
        {
            MessageBox.Show(
                "NO SE ENCONTRÓ UNA CUENTA PADRE VÁLIDA EN LAS FILAS SUPERIORES.",
                "ERROR DE RELACIÓN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            ReenfocarCelda(row, 0);
            return;
        }

        try
        {
            if (_catalogoRepository.ValidarSubcuenta(
                subcuentaId.ToString(),
                cuentaPadreId.ToString(),
                out string nombreSubcuenta))
            {
                dgvMovimientos["Nombre", row].Value = nombreSubcuenta.ToUpper();
                ReenfocarCelda(row, 3);
                return;
            }

            MessageBox.Show(
                $"LA SUBCUENTA {subcuentaId} NO EXISTE O NO PERTENECE A LA CUENTA PADRE {cuentaPadreId}.",
                "VALIDACIÓN RELACIONAL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            ReenfocarCelda(row, 1);
        }
        catch (Exception ex)
        {
            MostrarError("Error al validar subcuenta", ex);
            ReenfocarCelda(row, 1);
        }
    }

    private void ProcesarSubcuentaVacia(int row)
    {
        string subArriba = dgvMovimientos["SubCta", row - 1].Value?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(subArriba))
        {
            MessageBox.Show(
                "DEBES SELECCIONAR UNA SUBCUENTA.",
                "VALIDACIÓN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            ReenfocarCelda(row, 1);
            return;
        }

        ReenfocarCelda(row, 0);
    }

    private void ProcesarCeldaParcial(int row)
    {
        string parcialTexto = dgvMovimientos["Parcial", row].Value?.ToString() ?? string.Empty;

        if (!TryParseDecimal(parcialTexto, out decimal parcial) || parcial == 0)
        {
            MessageBox.Show(
                "SE DEBE DE COLOCAR UN MONTO.",
                "VALIDACIÓN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            ReenfocarCelda(row, 3);
            return;
        }

        ProcesarMontoParcial(row);

        if (row == dgvMovimientos.Rows.Count - 1)
        {
            AgregarFilaGrid();
        }

        if (dgvMovimientos.Columns["FolioFiscal"].Visible)
        {
            ReenfocarCelda(row, 7);
        }
        else
        {
            ReenfocarCelda(row + 1, 1);
        }
    }

    private void ProcesarCeldaFolioFiscal(int row)
    {
        if (row == dgvMovimientos.Rows.Count - 1)
        {
            AgregarFilaGrid();
        }

        ReenfocarCelda(row + 1, 1);
    }

    private void ReenfocarCelda(int fila, int columna)
    {
        if (fila < 0 || fila >= dgvMovimientos.Rows.Count)
            return;

        if (columna < 0 || columna >= dgvMovimientos.Columns.Count)
            return;

        _activeCell = dgvMovimientos.Rows[fila].Cells[columna];

        ActiveControl = dgvMovimientos;
        dgvMovimientos.Focus();
        dgvMovimientos.CurrentCell = _activeCell;
        dgvMovimientos.BeginEdit(true);

        dgvMovimientos.EditingControl?.Focus();
    }

    #endregion

    #region Cálculo Debe/Haber

    private void ProcesarMontoParcial(int rowIndex)
    {
        int parentCuentaRow = ObtenerFilaCuentaPadre(rowIndex);

        if (parentCuentaRow == -1)
            return;

        SincronizarRedaccionBloque(parentCuentaRow);

        string parcialTexto = dgvMovimientos["Parcial", rowIndex].Value?.ToString() ?? string.Empty;

        if (TryParseDecimal(parcialTexto, out decimal monto))
        {
            dgvMovimientos["Parcial", rowIndex].Value = monto.ToString("N2", CultureInfo.CurrentCulture);
            RecalcularDebeHaberCuenta(parentCuentaRow, monto, rowIndex);
        }
        else
        {
            RecalcularDebeHaberCuenta(parentCuentaRow, 0, rowIndex);
        }

        ActualizarEstadoBotonGuardar();
    }

    private void RecalcularDebeHaberCuenta(int parentCuentaRow, decimal newMonto, int editedRowIndex)
    {
        decimal sumaDebe = 0m;
        decimal sumaHaber = 0m;
        bool tieneSubcuentas = false;

        for (int r = parentCuentaRow + 1; r < dgvMovimientos.Rows.Count; r++)
        {
            string cuenta = dgvMovimientos["Cuenta", r].Value?.ToString() ?? string.Empty;
            string subcuenta = dgvMovimientos["SubCta", r].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cuenta))
                break;

            if (string.IsNullOrWhiteSpace(subcuenta))
                continue;

            tieneSubcuentas = true;

            decimal parcial = r == editedRowIndex
                ? newMonto
                : ObtenerParcialFila(r);

            if (parcial > 0)
            {
                sumaDebe += parcial;
            }
            else if (parcial < 0)
            {
                sumaHaber += Math.Abs(parcial);
            }

            dgvMovimientos["Debe", r].Value = string.Empty;
            dgvMovimientos["Haber", r].Value = string.Empty;
        }

        if (!tieneSubcuentas)
            return;

        dgvMovimientos["Debe", parentCuentaRow].Value =
            sumaDebe == 0 && sumaHaber == 0
                ? string.Empty
                : sumaDebe.ToString("N2", CultureInfo.CurrentCulture);

        dgvMovimientos["Haber", parentCuentaRow].Value =
            sumaDebe == 0 && sumaHaber == 0
                ? string.Empty
                : sumaHaber.ToString("N2", CultureInfo.CurrentCulture);
    }

    private decimal ObtenerParcialFila(int row)
    {
        string texto = dgvMovimientos["Parcial", row].Value?.ToString() ?? string.Empty;

        return TryParseDecimal(texto, out decimal parcial)
            ? parcial
            : 0m;
    }

    private int ObtenerFilaCuentaPadre(int rowIndex)
    {
        for (int r = rowIndex; r >= 0; r--)
        {
            string cuenta = dgvMovimientos["Cuenta", r].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cuenta))
                return r;
        }

        return -1;
    }

    private int ObtenerCuentaPadreId(int rowIndex)
    {
        for (int r = rowIndex - 1; r >= 0; r--)
        {
            string cuenta = dgvMovimientos["Cuenta", r].Value?.ToString() ?? string.Empty;

            if (int.TryParse(cuenta, out int cuentaId))
                return cuentaId;
        }

        return 0;
    }

    #endregion

    #region Sincronización

    private void ActualizarColumnasDependientes(int rowIndex)
    {
        string cuenta = dgvMovimientos["Cuenta", rowIndex].Value?.ToString() ?? string.Empty;
        string subcuenta = dgvMovimientos["SubCta", rowIndex].Value?.ToString() ?? string.Empty;

        bool tieneDatos = !string.IsNullOrWhiteSpace(cuenta) || !string.IsNullOrWhiteSpace(subcuenta);

        if (dgvMovimientos.Columns.Contains("Redaccion"))
        {
            dgvMovimientos["Redaccion", rowIndex].Value = tieneDatos
                ? ObtenerConceptoActivo()
                : string.Empty;
        }

        if (dgvMovimientos.Columns.Contains("FolioFiscal") && !chkVarios.Checked)
        {
            dgvMovimientos["FolioFiscal", rowIndex].Value = tieneDatos
                ? txtFolios.Text.ToUpper()
                : string.Empty;
        }
    }

    private void SincronizarConcepto(string concepto)
    {
        if (!dgvMovimientos.Columns.Contains("Redaccion"))
            return;

        foreach (DataGridViewRow row in dgvMovimientos.Rows)
        {
            string cuenta = row.Cells["Cuenta"].Value?.ToString() ?? string.Empty;
            string subcuenta = row.Cells["SubCta"].Value?.ToString() ?? string.Empty;

            row.Cells["Redaccion"].Value =
                !string.IsNullOrWhiteSpace(cuenta) || !string.IsNullOrWhiteSpace(subcuenta)
                    ? concepto.ToUpper()
                    : string.Empty;
        }
    }

    private void SincronizarRedaccionBloque(int parentCuentaRow)
    {
        if (!dgvMovimientos.Columns.Contains("Redaccion"))
            return;

        string concepto = ObtenerConceptoActivo().ToUpper();

        dgvMovimientos["Redaccion", parentCuentaRow].Value = concepto;

        for (int r = parentCuentaRow + 1; r < dgvMovimientos.Rows.Count; r++)
        {
            string cuenta = dgvMovimientos["Cuenta", r].Value?.ToString() ?? string.Empty;
            string subcuenta = dgvMovimientos["SubCta", r].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cuenta))
                break;

            if (!string.IsNullOrWhiteSpace(subcuenta))
            {
                dgvMovimientos["Redaccion", r].Value = concepto;
            }
        }
    }

    private void SincronizarFolioFiscal(string folio)
    {
        if (!dgvMovimientos.Columns.Contains("FolioFiscal"))
            return;

        _isUndoing = true;

        try
        {
            foreach (DataGridViewRow row in dgvMovimientos.Rows)
            {
                string cuenta = row.Cells["Cuenta"].Value?.ToString() ?? string.Empty;
                string subcuenta = row.Cells["SubCta"].Value?.ToString() ?? string.Empty;

                row.Cells["FolioFiscal"].Value =
                    !string.IsNullOrWhiteSpace(cuenta) || !string.IsNullOrWhiteSpace(subcuenta)
                        ? folio.ToUpper()
                        : string.Empty;
            }
        }
        finally
        {
            _isUndoing = false;
        }

        _lastState = SnapshotGridState();
    }

    private string ObtenerConceptoActivo()
    {
        return pnlCamposPoliza.Visible
            ? txtConceptoPoliza.Text
            : txtConceptoCheque.Text;
    }

    #endregion

    #region Guardado

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        TipoCaptura tipoActual = ObtenerTipoCapturaActual();

        if (GuardarCaptura(tipoActual))
        {
            RefrescarCaptura();
            ConfigurarPantalla(tipoActual);
        }
    }

    private bool GuardarCaptura(TipoCaptura tipo)
    {
        if (!ValidarFolio(out int folio))
            return false;

        CalcularTotales(out decimal totalDebe, out decimal totalHaber, out bool tieneMovimientos);

        if (!ValidarCapturaAntesDeGuardar(totalDebe, totalHaber, tieneMovimientos))
            return false;

        PolizaModel model = CrearPolizaModel(tipo, folio, totalDebe, totalHaber);

        AgregarMovimientosAlModelo(model, tipo);

        try
        {
            bool guardado = _catalogoRepository.GuardarPoliza(model);

            if (guardado)
            {
                MessageBox.Show(
                    "Captura guardada exitosamente en la base de datos.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            return guardado;
        }
        catch (Exception ex)
        {
            MostrarError("Error al guardar la póliza y sus movimientos", ex);
            return false;
        }
    }

    private bool ValidarFolio(out int folio)
    {
        if (int.TryParse(txtPolizaNo.Text, out folio))
            return true;

        MessageBox.Show(
            "Número de póliza inválido.",
            "Error de Validación",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );

        return false;
    }

    private bool ValidarCapturaAntesDeGuardar(
        decimal totalDebe,
        decimal totalHaber,
        bool tieneMovimientos)
    {
        if (!tieneMovimientos)
        {
            MessageBox.Show(
                "No se puede guardar una póliza/cheque sin movimientos.",
                "Error de Validación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return false;
        }

        if (totalDebe != totalHaber || totalDebe <= 0)
        {
            MessageBox.Show(
                "No se puede guardar la póliza/cheque: las sumas del Debe y el Haber deben estar balanceadas y ser mayores a cero.",
                "Error de Balance",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return false;
        }

        return true;
    }

    private PolizaModel CrearPolizaModel(
        TipoCaptura tipo,
        int folio,
        decimal totalDebe,
        decimal totalHaber)
    {
        var model = new PolizaModel
        {
            Fecha = DateTime.Today,
            Folio = folio,
            EsCostos = false,
            TipoCaptura = tipo == TipoCaptura.Cheque ? (byte)0 : (byte)1,
            TotalDebe = totalDebe,
            TotalHaber = totalHaber,
            RequiereImpresion = false
        };

        if (tipo == TipoCaptura.Poliza)
        {
            model.Concepto = txtConceptoPoliza.Text.ToUpper();
        }
        else
        {
            model.Concepto = txtConceptoCheque.Text.ToUpper();
            model.Beneficiario = txtBeneficiario.Text.ToUpper();
            model.MontoCheque = ObtenerMontoCheque();
            model.NumeroCheque = txtNumeroCheque.Text.ToUpper();
            model.RFC = txtRFC.Text.ToUpper();
            model.FoliosFiscales = txtFolios.Text.ToUpper();
        }

        return model;
    }

    private void AgregarMovimientosAlModelo(PolizaModel model, TipoCaptura tipo)
    {
        int currentCuentaId = 0;

        for (int i = 0; i < dgvMovimientos.Rows.Count; i++)
        {
            DataGridViewRow row = dgvMovimientos.Rows[i];

            string cuentaTexto = row.Cells["Cuenta"].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cuentaTexto) && int.TryParse(cuentaTexto, out int cuentaId))
            {
                currentCuentaId = cuentaId;

                if (!TieneSubcuentaAbajo(i))
                {
                    MovimientoModel movimiento = CrearMovimientoCuentaDirecta(row, cuentaId, tipo);
                    model.Movimientos.Add(movimiento);
                }

                continue;
            }

            string subcuentaTexto = row.Cells["SubCta"].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(subcuentaTexto) && int.TryParse(subcuentaTexto, out int subcuentaId))
            {
                MovimientoModel movimiento = CrearMovimientoSubcuenta(row, currentCuentaId, subcuentaId, tipo);
                model.Movimientos.Add(movimiento);
            }
        }
    }

    private MovimientoModel CrearMovimientoCuentaDirecta(
        DataGridViewRow row,
        int cuentaId,
        TipoCaptura tipo)
    {
        decimal debe = ObtenerDecimalCelda(row, "Debe");
        decimal haber = ObtenerDecimalCelda(row, "Haber");
        string redaccion = ObtenerTextoCelda(row, "Redaccion");

        var movimiento = new MovimientoModel
        {
            CuentaId = cuentaId,
            Debe = debe,
            Haber = haber,
            Redaccion = redaccion.ToUpper(),
            PerteneceConcepto = redaccion.Equals(ObtenerConceptoActivo(), StringComparison.OrdinalIgnoreCase)
        };

        if (tipo == TipoCaptura.Cheque)
        {
            movimiento.Beneficiario = txtBeneficiario.Text.ToUpper();
        }

        return movimiento;
    }

    private MovimientoModel CrearMovimientoSubcuenta(
        DataGridViewRow row,
        int cuentaId,
        int subcuentaId,
        TipoCaptura tipo)
    {
        decimal parcial = ObtenerDecimalCelda(row, "Parcial");

        decimal debe = parcial > 0 ? parcial : 0m;
        decimal haber = parcial < 0 ? Math.Abs(parcial) : 0m;

        string redaccion = ObtenerTextoCelda(row, "Redaccion");

        var movimiento = new MovimientoModel
        {
            CuentaId = cuentaId,
            SubcuentaId = subcuentaId,
            Parcial = parcial,
            Debe = debe,
            Haber = haber,
            Redaccion = redaccion.ToUpper(),
            PerteneceConcepto = redaccion.Equals(ObtenerConceptoActivo(), StringComparison.OrdinalIgnoreCase),
            FolioFiscal = ObtenerTextoCelda(row, "FolioFiscal")
        };

        if (tipo == TipoCaptura.Cheque)
        {
            movimiento.Beneficiario = txtBeneficiario.Text.ToUpper();
        }

        return movimiento;
    }

    private bool TieneSubcuentaAbajo(int rowIndex)
    {
        if (rowIndex + 1 >= dgvMovimientos.Rows.Count)
            return false;

        DataGridViewRow nextRow = dgvMovimientos.Rows[rowIndex + 1];

        string nextCuenta = nextRow.Cells["Cuenta"].Value?.ToString() ?? string.Empty;
        string subcuenta = nextRow.Cells["SubCta"].Value?.ToString() ?? string.Empty;

        return string.IsNullOrWhiteSpace(nextCuenta) && !string.IsNullOrWhiteSpace(subcuenta);
    }

    #endregion

    #region Estado de sumas

    private void ActualizarEstadoBotonGuardar()
    {
        CalcularTotales(out decimal totalDebe, out decimal totalHaber, out bool tieneMovimientos);

        decimal diferencia = totalDebe - totalHaber;

        if (lblResumenSuma is not null)
        {
            lblResumenSuma.Text =
                $"Suma Debe: {totalDebe:N2} | Suma Haber: {totalHaber:N2} | Diferencia: {diferencia:N2}";
        }

        bool balanceCorrecto = totalDebe == totalHaber && totalDebe > 0;

        if (btnGuardar is not null)
        {
            btnGuardar.Enabled = tieneMovimientos && balanceCorrecto;
        }

        if (btnEstadoSumas is not null)
        {
            if (balanceCorrecto)
            {
                btnEstadoSumas.Text = "Sumas correctas";
                btnEstadoSumas.ForeColor = Color.Green;
                btnEstadoSumas.BackColor = Color.LightGreen;
            }
            else
            {
                btnEstadoSumas.Text = "Sumas incorrectas";
                btnEstadoSumas.ForeColor = Color.Red;
                btnEstadoSumas.BackColor = Color.LightPink;
            }
        }
    }

    private void CalcularTotales(
        out decimal totalDebe,
        out decimal totalHaber,
        out bool tieneMovimientos)
    {
        totalDebe = 0m;
        totalHaber = 0m;
        tieneMovimientos = false;

        foreach (DataGridViewRow row in dgvMovimientos.Rows)
        {
            string cuenta = row.Cells["Cuenta"].Value?.ToString() ?? string.Empty;
            string subcuenta = row.Cells["SubCta"].Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cuenta) || !string.IsNullOrWhiteSpace(subcuenta))
            {
                tieneMovimientos = true;
            }

            totalDebe += ObtenerDecimalCelda(row, "Debe");
            totalHaber += ObtenerDecimalCelda(row, "Haber");
        }
    }

    #endregion

    #region Buscadores

    private void AbrirBusquedaCuenta(int rowIndex)
    {
        try
        {
            DataTable datos = _catalogoRepository.ObtenerCuentasBusqueda();

            using FormBuscarCatalogo form = new("Buscar Cuenta", datos);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                dgvMovimientos["Cuenta", rowIndex].Value = form.SelectedId;
                EjecutarCoreografiaTeclado();
            }
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir búsqueda de cuentas", ex);
        }
    }

    private void AbrirBusquedaSubcuenta(int rowIndex)
    {
        if (rowIndex == 0)
            return;

        string cuentaPadre = dgvMovimientos["Cuenta", rowIndex - 1].Value?.ToString() ?? string.Empty;

        if (!int.TryParse(cuentaPadre, out int cuentaPadreId))
        {
            MessageBox.Show(
                "No se encontró una Cuenta Padre válida en la fila superior.",
                "Error de Relación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            ReenfocarCelda(rowIndex - 1, 0);
            return;
        }

        try
        {
            DataTable datos = _catalogoRepository.ObtenerSubcuentasBusqueda(cuentaPadreId);

            using FormBuscarCatalogo form = new(
                $"Buscar Subcuenta para Cuenta {cuentaPadreId}",
                datos
            );

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                dgvMovimientos["SubCta", rowIndex].Value = form.SelectedId;
                EjecutarCoreografiaTeclado();
            }
        }
        catch (Exception ex)
        {
            MostrarError("Error al abrir búsqueda de subcuentas", ex);
        }
    }

    #endregion

    #region Folio fiscal XML

    private void CargarFolioFiscalDesdeXml(int rowIndex)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "Archivos XML (*.xml)|*.xml",
            Title = "SELECCIONAR FACTURA XML (CFDI)"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            string xmlContent = File.ReadAllText(dialog.FileName);
            string uuid = ExtraerUuidDeXml(xmlContent);

            if (string.IsNullOrWhiteSpace(uuid))
            {
                MessageBox.Show(
                    "NO SE ENCONTRÓ EL FOLIO FISCAL (UUID) EN EL ARCHIVO XML.",
                    "ADVERTENCIA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            GuardarEstadoParaUndo();

            dgvMovimientos["FolioFiscal", rowIndex].Value = uuid.ToUpper();

            _lastState = SnapshotGridState();

            ActualizarBotonesUndoRedo();

            MessageBox.Show(
                $"FOLIO FISCAL EXTRAÍDO CON ÉXITO:\n{uuid.ToUpper()}",
                "ÉXITO",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            MostrarError("Error al procesar el archivo XML", ex);
        }
    }

    private static string ExtraerUuidDeXml(string xmlContent)
    {
        Match match = Regex.Match(
            xmlContent,
            @"UUID\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase
        );

        return match.Success
            ? match.Groups[1].Value.Trim()
            : string.Empty;
    }

    #endregion

    #region Undo / Redo

    private void BtnAnterior_Click(object? sender, EventArgs e)
    {
        DeshacerAccion();
    }

    private void BtnSiguiente_Click(object? sender, EventArgs e)
    {
        RehacerAccion();
    }

    private void GuardarEstadoParaUndo()
    {
        if (_lastState is null)
            return;

        _undoStack.Push(_lastState);
        _redoStack.Clear();
    }

    private string[,] SnapshotGridState()
    {
        int rowCount = dgvMovimientos.Rows.Count;
        int colCount = dgvMovimientos.Columns.Count;

        string[,] state = new string[rowCount, colCount];

        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                state[r, c] = dgvMovimientos[c, r].Value?.ToString() ?? string.Empty;
            }
        }

        return state;
    }

    private void RestaurarGridState(string[,] state)
    {
        _isUndoing = true;
        dgvMovimientos.CellValueChanged -= DgvMovimientos_CellValueChanged;

        try
        {
            _activeCell = null;
            dgvMovimientos.Rows.Clear();

            int rowCount = state.GetLength(0);
            int colCount = state.GetLength(1);

            for (int r = 0; r < rowCount; r++)
            {
                dgvMovimientos.Rows.Add();

                for (int c = 0; c < colCount; c++)
                {
                    dgvMovimientos[c, r].Value = state[r, c];
                }
            }

            ActualizarEstadoBotonGuardar();
        }
        finally
        {
            dgvMovimientos.CellValueChanged += DgvMovimientos_CellValueChanged;
            _isUndoing = false;
        }
    }

    private void DeshacerAccion()
    {
        if (_undoStack.Count == 0 || _lastState is null)
            return;

        string[,] targetState = _undoStack.Pop();

        _redoStack.Push(_lastState);

        RestaurarGridState(targetState);

        _lastState = targetState;

        ActualizarBotonesUndoRedo();
    }

    private void RehacerAccion()
    {
        if (_redoStack.Count == 0 || _lastState is null)
            return;

        string[,] targetState = _redoStack.Pop();

        _undoStack.Push(_lastState);

        RestaurarGridState(targetState);

        _lastState = targetState;

        ActualizarBotonesUndoRedo();
    }

    private void ActualizarBotonesUndoRedo()
    {
        btnAnterior.Visible = _undoStack.Count > 0;
        btnSiguiente.Visible = _redoStack.Count > 0;
    }

    #endregion

    #region Eventos KeyDown de controles

    private void TxtConceptoPoliza_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            ReenfocarCelda(0, 0);
        }
    }

    private void TxtBeneficiario_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            txtConceptoCheque.Focus();
        }
    }

    private void TxtConceptoCheque_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            txtMonto.Focus();
        }
    }

    private void TxtMonto_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            txtRFC.Focus();
        }
    }

    private void TxtRFC_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            txtFolios.Focus();
        }
    }

    private void TxtFolios_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            ReenfocarCelda(0, 0);
        }
    }

    #endregion

    #region Validaciones de entrada

    private void ValidarNumeroEntero(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsDigit(e.KeyChar)
            && e.KeyChar != (char)Keys.Back
            && e.KeyChar != (char)Keys.Delete)
        {
            e.Handled = true;
        }
    }

    private void ValidarNumeroDecimal(object? sender, KeyPressEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (char.IsDigit(e.KeyChar)
            || e.KeyChar == (char)Keys.Back
            || e.KeyChar == (char)Keys.Delete)
        {
            e.Handled = false;
            return;
        }

        if (e.KeyChar == '.' && !textBox.Text.Contains('.'))
        {
            e.Handled = false;
            return;
        }

        if (e.KeyChar == '-' && textBox.SelectionStart == 0)
        {
            e.Handled = false;
            return;
        }

        e.Handled = true;
    }

    #endregion

    #region Helpers

    private static bool TryParseDecimal(string texto, out decimal valor)
    {
        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out valor) || decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }

    private decimal ObtenerDecimalCelda(DataGridViewRow row, string columna)
    {
        string texto = ObtenerTextoCelda(row, columna);

        return TryParseDecimal(texto, out decimal valor) ? valor : 0m;
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

    private decimal ObtenerMontoCheque()
    {
        return TryParseDecimal(txtMonto.Text, out decimal monto) ? monto : 0m;
    }

    private void MostrarError(string mensaje, Exception ex)
    {
        MessageBox.Show($"{mensaje}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
        );
    }

    #endregion

    private void tsAuxiliares_Click(object sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show("Se abrirá el menú de AUXILIARES, todo cambio aplicado se perderá si no se ha guardado.\n\n¿Desea continuar?", "Confirmación",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        Hide();

        try
        {
            using FormAuxiliar auxiliares = new FormAuxiliar();
            auxiliares.ShowDialog();
        }
        finally
        {
            // Se muestra la misma instancia de FormCaptura.
            Show();
            Activate();
        }
    }
}