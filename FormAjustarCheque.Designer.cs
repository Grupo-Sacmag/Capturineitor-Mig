namespace CapturaDePolizas_2026_NET8;

partial class FormAjustarCheque
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        mnuArchivo = new ToolStripMenuItem();
        mnuArchivar = new ToolStripMenuItem();
        mnuVistaPrevia = new ToolStripMenuItem();
        mnuPruebaImpresion = new ToolStripMenuItem();
        mnuRestaurarDefecto = new ToolStripMenuItem();
        mnuSalida = new ToolStripMenuItem();
        pnlSuperior = new Panel();
        lblBanco = new Label();
        cmbPlantillaBanco = new ComboBox();
        btnNuevoBanco = new Button();
        lblOffsetX = new Label();
        numOffsetX = new NumericUpDown();
        lblOffsetY = new Label();
        numOffsetY = new NumericUpDown();
        lblFontSize = new Label();
        numFontSize = new NumericUpDown();
        dgvCoordenadas = new DataGridView();
        pnlInferior = new Panel();
        btnGuardar = new Button();
        btnVistaPrevia = new Button();
        btnPruebaImpresion = new Button();
        btnCerrar = new Button();

        menuStrip.SuspendLayout();
        pnlSuperior.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numOffsetX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numOffsetY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numFontSize).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvCoordenadas).BeginInit();
        pnlInferior.SuspendLayout();
        SuspendLayout();

        // menuStrip
        menuStrip.Items.AddRange(new ToolStripItem[] { mnuArchivo });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(640, 24);

        // mnuArchivo
        mnuArchivo.DropDownItems.AddRange(new ToolStripItem[]
        {
            mnuArchivar,
            mnuVistaPrevia,
            mnuPruebaImpresion,
            mnuRestaurarDefecto,
            new ToolStripSeparator(),
            mnuSalida
        });
        mnuArchivo.Name = "mnuArchivo";
        mnuArchivo.Text = "&Archivo";

        mnuArchivar.Name = "mnuArchivar";
        mnuArchivar.Text = "&Archivar (Guardar)";

        mnuVistaPrevia.Name = "mnuVistaPrevia";
        mnuVistaPrevia.Text = "&Vista Previa";

        mnuPruebaImpresion.Name = "mnuPruebaImpresion";
        mnuPruebaImpresion.Text = "&Prueba Impresión";

        mnuRestaurarDefecto.Name = "mnuRestaurarDefecto";
        mnuRestaurarDefecto.Text = "&Restaurar valores Banamex";

        mnuSalida.Name = "mnuSalida";
        mnuSalida.Text = "&Salida";

        // pnlSuperior
        pnlSuperior.Dock = DockStyle.Top;
        pnlSuperior.Height = 82;
        pnlSuperior.Padding = new Padding(10);
        pnlSuperior.Controls.Add(lblBanco);
        pnlSuperior.Controls.Add(cmbPlantillaBanco);
        pnlSuperior.Controls.Add(btnNuevoBanco);
        pnlSuperior.Controls.Add(lblOffsetX);
        pnlSuperior.Controls.Add(numOffsetX);
        pnlSuperior.Controls.Add(lblOffsetY);
        pnlSuperior.Controls.Add(numOffsetY);
        pnlSuperior.Controls.Add(lblFontSize);
        pnlSuperior.Controls.Add(numFontSize);

        lblBanco.AutoSize = true;
        lblBanco.Location = new Point(12, 14);
        lblBanco.Text = "Banco / Formato:";

        cmbPlantillaBanco.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPlantillaBanco.Location = new Point(118, 10);
        cmbPlantillaBanco.Size = new Size(210, 23);

        btnNuevoBanco.Location = new Point(336, 9);
        btnNuevoBanco.Size = new Size(105, 25);
        btnNuevoBanco.Text = "+ Nuevo Banco";
        btnNuevoBanco.UseVisualStyleBackColor = true;

        lblOffsetX.AutoSize = true;
        lblOffsetX.Location = new Point(12, 48);
        lblOffsetX.Text = "Ajuste Global X (mm):";

        numOffsetX.DecimalPlaces = 1;
        numOffsetX.Increment = 0.5m;
        numOffsetX.Minimum = -50m;
        numOffsetX.Maximum = 50m;
        numOffsetX.Location = new Point(140, 45);
        numOffsetX.Size = new Size(68, 23);

        lblOffsetY.AutoSize = true;
        lblOffsetY.Location = new Point(222, 48);
        lblOffsetY.Text = "Ajuste Global Y (mm):";

        numOffsetY.DecimalPlaces = 1;
        numOffsetY.Increment = 0.5m;
        numOffsetY.Minimum = -50m;
        numOffsetY.Maximum = 50m;
        numOffsetY.Location = new Point(348, 45);
        numOffsetY.Size = new Size(68, 23);

        lblFontSize.AutoSize = true;
        lblFontSize.Location = new Point(430, 48);
        lblFontSize.Text = "Letra (pt):";

        numFontSize.DecimalPlaces = 1;
        numFontSize.Increment = 0.5m;
        numFontSize.Minimum = 6m;
        numFontSize.Maximum = 16m;
        numFontSize.Value = 10m;
        numFontSize.Location = new Point(495, 45);
        numFontSize.Size = new Size(65, 23);

        // dgvCoordenadas
        dgvCoordenadas.AllowUserToAddRows = false;
        dgvCoordenadas.AllowUserToDeleteRows = false;
        dgvCoordenadas.Dock = DockStyle.Fill;
        dgvCoordenadas.RowHeadersVisible = false;
        dgvCoordenadas.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dgvCoordenadas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // pnlInferior
        pnlInferior.Dock = DockStyle.Bottom;
        pnlInferior.Height = 48;
        pnlInferior.Controls.Add(btnGuardar);
        pnlInferior.Controls.Add(btnVistaPrevia);
        pnlInferior.Controls.Add(btnPruebaImpresion);
        pnlInferior.Controls.Add(btnCerrar);

        btnGuardar.Location = new Point(12, 10);
        btnGuardar.Size = new Size(130, 28);
        btnGuardar.Text = "Guardar Ajustes";
        btnGuardar.UseVisualStyleBackColor = true;

        btnVistaPrevia.Location = new Point(152, 10);
        btnVistaPrevia.Size = new Size(130, 28);
        btnVistaPrevia.Text = "Vista Previa";
        btnVistaPrevia.UseVisualStyleBackColor = true;

        btnPruebaImpresion.Location = new Point(292, 10);
        btnPruebaImpresion.Size = new Size(140, 28);
        btnPruebaImpresion.Text = "Prueba Impresión";
        btnPruebaImpresion.UseVisualStyleBackColor = true;

        btnCerrar.Location = new Point(510, 10);
        btnCerrar.Size = new Size(110, 28);
        btnCerrar.Text = "Cerrar";
        btnCerrar.UseVisualStyleBackColor = true;

        // FormAjustarCheque
        ClientSize = new Size(640, 520);
        Controls.Add(dgvCoordenadas);
        Controls.Add(pnlInferior);
        Controls.Add(pnlSuperior);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Ajustes para la impresión de cheques (mm)";

        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        pnlSuperior.ResumeLayout(false);
        pnlSuperior.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numOffsetX).EndInit();
        ((System.ComponentModel.ISupportInitialize)numOffsetY).EndInit();
        ((System.ComponentModel.ISupportInitialize)numFontSize).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvCoordenadas).EndInit();
        pnlInferior.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private MenuStrip menuStrip = null!;
    private ToolStripMenuItem mnuArchivo = null!;
    private ToolStripMenuItem mnuArchivar = null!;
    private ToolStripMenuItem mnuVistaPrevia = null!;
    private ToolStripMenuItem mnuPruebaImpresion = null!;
    private ToolStripMenuItem mnuRestaurarDefecto = null!;
    private ToolStripMenuItem mnuSalida = null!;
    private Panel pnlSuperior = null!;
    private Label lblBanco = null!;
    private ComboBox cmbPlantillaBanco = null!;
    private Button btnNuevoBanco = null!;
    private Label lblOffsetX = null!;
    private NumericUpDown numOffsetX = null!;
    private Label lblOffsetY = null!;
    private NumericUpDown numOffsetY = null!;
    private Label lblFontSize = null!;
    private NumericUpDown numFontSize = null!;
    private DataGridView dgvCoordenadas = null!;
    private Panel pnlInferior = null!;
    private Button btnGuardar = null!;
    private Button btnVistaPrevia = null!;
    private Button btnPruebaImpresion = null!;
    private Button btnCerrar = null!;
}
