using System.Data;

namespace CapturaDePolizas_2026_NET8;

public partial class FormVerGrid : Form
{
    private readonly string _titulo;
    private readonly DataTable _datos;

    public FormVerGrid(string titulo, DataTable datos)
    {
        ArgumentNullException.ThrowIfNull(datos);

        InitializeComponent();

        _titulo = titulo;
        _datos = datos;

        InicializarFormulario();
    }

    #region Inicialización

    private void InicializarFormulario()
    {
        ConfigurarFormulario();
        ConfigurarGrid();
        CargarDatos();
    }

    private void ConfigurarFormulario()
    {
        Text = _titulo;
        StartPosition = FormStartPosition.CenterParent;
    }

    private void ConfigurarGrid()
    {
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;

        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgv.Dock = DockStyle.Fill;

        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.None;
        dgv.GridColor = Color.FromArgb(224, 224, 224);

        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        dgv.EnableHeadersVisualStyles = false;
    }

    private void CargarDatos()
    {
        dgv.DataSource = _datos;
    }

    #endregion
}