using System.Data;
using System.Globalization;

namespace CapturaDePolizas_2026_NET8;

public partial class FormDetallePoliza : Form
{
    private readonly int _folio;
    private readonly string _concepto;
    private readonly string _rfc;
    private readonly string _foliosFiscales;
    private readonly DateTime _fecha;
    private readonly DataTable _movimientos;

    public FormDetallePoliza(int folio, string concepto, string rfc, string foliosFiscales, DateTime fecha, DataTable movimientos)
    {
        ArgumentNullException.ThrowIfNull(movimientos);

        InitializeComponent();

        _folio = folio;
        _concepto = concepto;
        _rfc = rfc;
        _foliosFiscales = foliosFiscales;
        _fecha = fecha;
        _movimientos = movimientos;

        InicializarFormulario();
    }

    #region Inicialización

    private void InicializarFormulario()
    {
        ConfigurarFormulario();
        ConfigurarMenu();
        ConfigurarGrid();
        EnlazarEventos();

        CargarDetalle();
    }

    private void ConfigurarFormulario()
    {
        Text = CrearTituloFormulario();
        StartPosition = FormStartPosition.CenterParent;
        KeyPreview = true;
    }

    private void ConfigurarMenu()
    {
        mnuArchivoCerrar.Click += MnuArchivoCerrar_Click;
    }

    private void ConfigurarGrid()
    {
        dgvMovimientos.AllowUserToAddRows = false;
        dgvMovimientos.AllowUserToDeleteRows = false;
        dgvMovimientos.ReadOnly = true;
        dgvMovimientos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMovimientos.MultiSelect = false;
        dgvMovimientos.RowHeadersVisible = false;

        dgvMovimientos.BackgroundColor = Color.White;
        dgvMovimientos.BorderStyle = BorderStyle.None;
        dgvMovimientos.GridColor = Color.FromArgb(224, 224, 224);

        dgvMovimientos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        dgvMovimientos.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvMovimientos.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

        dgvMovimientos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        dgvMovimientos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        dgvMovimientos.EnableHeadersVisualStyles = false;
    }

    private void EnlazarEventos()
    {
        KeyDown += FormDetallePoliza_KeyDown;
    }

    #endregion

    #region Eventos

    private void MnuArchivoCerrar_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void FormDetallePoliza_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }

    #endregion

    #region Carga de detalle

    private void CargarDetalle()
    {
        ConfigurarEncabezado();
        InicializarColumnasGrid();
        CargarMovimientos();
    }

    private void ConfigurarEncabezado()
    {
        string rfcTexto = string.IsNullOrWhiteSpace(_rfc) ? "EN BLANCO" : _rfc.ToUpper();

        string folioFiscalTexto = string.IsNullOrWhiteSpace(_foliosFiscales) ? "EN BLANCO" : _foliosFiscales.ToUpper();

        lblYellowBar.Text = $"POLIZA:   {_folio} {_concepto.ToUpper()} RFC:{rfcTexto} FOLIO:{folioFiscalTexto}";
    }

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

        ConfigurarColumnasMovimiento();
        DeshabilitarOrdenamientoColumnas();
    }

    private void ConfigurarColumnasMovimiento()
    {
        ConfigurarColumna("Cuenta", 100, DataGridViewContentAlignment.MiddleLeft);
        ConfigurarColumna("SubCta", 80, DataGridViewContentAlignment.MiddleLeft);
        ConfigurarColumna("Parcial", 120, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumna("Debe", 120, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumna("Haber", 120, DataGridViewContentAlignment.MiddleRight);
        ConfigurarColumna("Redaccion", 250, DataGridViewContentAlignment.MiddleLeft);

        if (dgvMovimientos.Columns.Contains("Nombre"))
        {
            dgvMovimientos.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
    }

    private void ConfigurarColumna(string columna, int ancho, DataGridViewContentAlignment alineacion)
    {
        if (!dgvMovimientos.Columns.Contains(columna))
            return;

        dgvMovimientos.Columns[columna].Width = ancho;
        dgvMovimientos.Columns[columna].DefaultCellStyle.Alignment = alineacion;
    }

    private void DeshabilitarOrdenamientoColumnas()
    {
        foreach (DataGridViewColumn columna in dgvMovimientos.Columns)
        {
            columna.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    private void CargarMovimientos()
    {
        decimal totalDebe = 0m;
        decimal totalHaber = 0m;

        foreach (DataRow row in _movimientos.Rows)
        {
            AgregarFilaCuenta(row, ref totalDebe, ref totalHaber);
            AgregarFilaSubcuenta(row);
        }

        AgregarFilaSumasIguales(totalDebe, totalHaber);
    }

    private void AgregarFilaCuenta(DataRow row, ref decimal totalDebe, ref decimal totalHaber)
    {
        int index = dgvMovimientos.Rows.Add();

        dgvMovimientos["Cuenta", index].Value = ObtenerValor(row, "cuenta_id");
        dgvMovimientos["Nombre", index].Value = ObtenerTexto(row, "nombre_cuenta").ToUpper();

        decimal debe = ObtenerDecimal(row, "debe");
        decimal haber = ObtenerDecimal(row, "haber");

        if (debe > 0)
        {
            dgvMovimientos["Debe", index].Value = FormatearImporte(debe);
            totalDebe += debe;
        }

        if (haber > 0)
        {
            dgvMovimientos["Haber", index].Value = FormatearImporte(-haber);
            totalHaber += haber;
        }
    }

    private void AgregarFilaSubcuenta(DataRow row)
    {
        int index = dgvMovimientos.Rows.Add();

        object? subcuentaId = ObtenerValor(row, "subcuenta_id");

        if (subcuentaId is not null && subcuentaId != DBNull.Value)
        {
            dgvMovimientos["SubCta", index].Value = subcuentaId;
        }

        dgvMovimientos["Nombre", index].Value = ObtenerTexto(row, "nombre_subcuenta").ToUpper();

        decimal parcial = ObtenerDecimal(row, "parcial");

        if (parcial != 0)
        {
            dgvMovimientos["Parcial", index].Value = FormatearImporte(parcial);
        }

        dgvMovimientos["Redaccion", index].Value = ObtenerTexto(row, "redaccion").ToUpper();
    }

    private void AgregarFilaSumasIguales(decimal totalDebe, decimal totalHaber)
    {
        int index = dgvMovimientos.Rows.Add();

        dgvMovimientos["Nombre", index].Value = "SUMAS IGUALES";
        dgvMovimientos["Debe", index].Value = FormatearImporte(totalDebe);
        dgvMovimientos["Haber", index].Value = FormatearImporte(-totalHaber);

        dgvMovimientos.Rows[index].DefaultCellStyle.Font = new Font(dgvMovimientos.DefaultCellStyle.Font, FontStyle.Bold);
    }

    #endregion

    #region Helpers

    private string CrearTituloFormulario()
    {
        string mesNombre = ObtenerNombreMes(_fecha.Month);

        return $"SACMAG DE MEXICO SA CV Polizas de {mesNombre} de {_fecha.Year}";
    }

    private static string ObtenerNombreMes(int mes)
    {
        string[] meses =
        {
            string.Empty,
            "ENERO",
            "FEBRERO",
            "MARZO",
            "ABRIL",
            "MAYO",
            "JUNIO",
            "JULIO",
            "AGOSTO",
            "SEPTIEMBRE",
            "OCTUBRE",
            "NOVIEMBRE",
            "DICIEMBRE"
        };

        if (mes < 1 || mes > 12)
            return string.Empty;

        return meses[mes];
    }

    private static object? ObtenerValor(DataRow row, string columna)
    {
        if (!row.Table.Columns.Contains(columna))
            return null;

        return row[columna];
    }

    private static string ObtenerTexto(DataRow row, string columna)
    {
        if (!row.Table.Columns.Contains(columna))
            return string.Empty;

        if (row[columna] == DBNull.Value)
            return string.Empty;

        return row[columna]?.ToString() ?? string.Empty;
    }

    private static decimal ObtenerDecimal(DataRow row, string columna)
    {
        if (!row.Table.Columns.Contains(columna))
            return 0m;

        if (row[columna] == DBNull.Value)
            return 0m;

        return Convert.ToDecimal(row[columna]);
    }

    private static string FormatearImporte(decimal importe)
    {
        return importe.ToString("N2", CultureInfo.CurrentCulture);
    }

    #endregion
}