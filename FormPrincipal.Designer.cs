namespace GaCostos
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer? components = null;

        private MenuStrip menuStripPrincipal = null!;
        private ToolStripMenuItem menuArchivo = null!;
        private ToolStripMenuItem menuActualizar = null!;
        private ToolStripMenuItem menuCambiarSubdirectorio = null!;
        private ToolStripMenuItem menuVerificarArchivos = null!;
        private ToolStripMenuItem menuEdicion = null!;
        private ToolStripMenuItem menuSeleccionarYCopiarTodo = null!;
        private ToolStripMenuItem menuVersion = null!;

        private DataGridView dgvPrincipal = null!;
        private Label lblRuta = null!;
        private Button btnRegresar = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrincipal));
            menuStripPrincipal = new MenuStrip();
            menuArchivo = new ToolStripMenuItem();
            menuActualizar = new ToolStripMenuItem();
            menuCambiarSubdirectorio = new ToolStripMenuItem();
            menuVerificarArchivos = new ToolStripMenuItem();
            menuEdicion = new ToolStripMenuItem();
            menuSeleccionarYCopiarTodo = new ToolStripMenuItem();
            menuDGVConEncabezados = new ToolStripMenuItem();
            menuDGVSinEncabezados = new ToolStripMenuItem();
            menuSeleccionFilaCompleta = new ToolStripMenuItem();
            menuVersion = new ToolStripMenuItem();
            dgvPrincipal = new DataGridView();
            colCuenta = new DataGridViewTextBoxColumn();
            colNombre = new DataGridViewTextBoxColumn();
            colSaldo = new DataGridViewTextBoxColumn();
            colRangoInferior = new DataGridViewTextBoxColumn();
            colRangoSuperior = new DataGridViewTextBoxColumn();
            colGuia = new DataGridViewTextBoxColumn();
            colFecha = new DataGridViewTextBoxColumn();
            colPoliza = new DataGridViewTextBoxColumn();
            colConcepto = new DataGridViewTextBoxColumn();
            colDebe = new DataGridViewTextBoxColumn();
            colHaber = new DataGridViewTextBoxColumn();
            lblRuta = new Label();
            btnRegresar = new Button();
            label1 = new Label();
            btnVolverACaptura = new Button();
            menuStripPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPrincipal).BeginInit();
            SuspendLayout();
            // 
            // menuStripPrincipal
            // 
            menuStripPrincipal.ImageScalingSize = new Size(20, 20);
            menuStripPrincipal.Items.AddRange(new ToolStripItem[] { menuArchivo, menuEdicion, menuVersion });
            menuStripPrincipal.Location = new Point(0, 0);
            menuStripPrincipal.Name = "menuStripPrincipal";
            menuStripPrincipal.Padding = new Padding(5, 2, 0, 2);
            menuStripPrincipal.Size = new Size(998, 24);
            menuStripPrincipal.TabIndex = 0;
            menuStripPrincipal.Text = "menuStripPrincipal";
            // 
            // menuArchivo
            // 
            menuArchivo.DropDownItems.AddRange(new ToolStripItem[] { menuActualizar, menuCambiarSubdirectorio, menuVerificarArchivos });
            menuArchivo.Name = "menuArchivo";
            menuArchivo.Size = new Size(60, 20);
            menuArchivo.Text = "Archivo";
            // 
            // menuActualizar
            // 
            menuActualizar.Name = "menuActualizar";
            menuActualizar.Size = new Size(192, 22);
            menuActualizar.Text = "Actualizar";
            // 
            // menuCambiarSubdirectorio
            // 
            menuCambiarSubdirectorio.Name = "menuCambiarSubdirectorio";
            menuCambiarSubdirectorio.Size = new Size(192, 22);
            menuCambiarSubdirectorio.Text = "Cambiar subdirectorio";
            // 
            // menuVerificarArchivos
            // 
            menuVerificarArchivos.Name = "menuVerificarArchivos";
            menuVerificarArchivos.Size = new Size(192, 22);
            menuVerificarArchivos.Text = "Verificar archivos";
            // 
            // menuEdicion
            // 
            menuEdicion.DropDownItems.AddRange(new ToolStripItem[] { menuSeleccionarYCopiarTodo, menuSeleccionFilaCompleta });
            menuEdicion.Name = "menuEdicion";
            menuEdicion.Size = new Size(58, 20);
            menuEdicion.Text = "Edición";
            // 
            // menuSeleccionarYCopiarTodo
            // 
            menuSeleccionarYCopiarTodo.DropDownItems.AddRange(new ToolStripItem[] { menuDGVConEncabezados, menuDGVSinEncabezados });
            menuSeleccionarYCopiarTodo.Name = "menuSeleccionarYCopiarTodo";
            menuSeleccionarYCopiarTodo.Size = new Size(214, 22);
            menuSeleccionarYCopiarTodo.Text = "Seleccionar y Copiar Todo";
            // 
            // menuDGVConEncabezados
            // 
            menuDGVConEncabezados.Name = "menuDGVConEncabezados";
            menuDGVConEncabezados.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
            menuDGVConEncabezados.Size = new Size(279, 22);
            menuDGVConEncabezados.Text = "Con Encabezados";
            menuDGVConEncabezados.Click += menuDGVConEncabezados_Click;
            // 
            // menuDGVSinEncabezados
            // 
            menuDGVSinEncabezados.Name = "menuDGVSinEncabezados";
            menuDGVSinEncabezados.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
            menuDGVSinEncabezados.Size = new Size(279, 22);
            menuDGVSinEncabezados.Text = "Sin Encabezados";
            menuDGVSinEncabezados.Click += menuDGVSinEncabezados_Click;
            // 
            // menuSeleccionFilaCompleta
            // 
            menuSeleccionFilaCompleta.Checked = true;
            menuSeleccionFilaCompleta.CheckOnClick = true;
            menuSeleccionFilaCompleta.CheckState = CheckState.Checked;
            menuSeleccionFilaCompleta.Name = "menuSeleccionFilaCompleta";
            menuSeleccionFilaCompleta.Size = new Size(214, 22);
            menuSeleccionFilaCompleta.Text = "Selección de Fila completa";
            menuSeleccionFilaCompleta.Click += menuSeleccionFilaCompleta_Click;
            // 
            // menuVersion
            // 
            menuVersion.Name = "menuVersion";
            menuVersion.Size = new Size(57, 20);
            menuVersion.Text = "Versión";
            menuVersion.Click += MenuVersion_Click;
            // 
            // dgvPrincipal
            // 
            dgvPrincipal.AllowUserToAddRows = false;
            dgvPrincipal.AllowUserToDeleteRows = false;
            dgvPrincipal.AllowUserToResizeColumns = false;
            dgvPrincipal.AllowUserToResizeRows = false;
            dgvPrincipal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPrincipal.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dgvPrincipal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPrincipal.Columns.AddRange(new DataGridViewColumn[] { colCuenta, colNombre, colSaldo, colRangoInferior, colRangoSuperior, colGuia, colFecha, colPoliza, colConcepto, colDebe, colHaber });
            dgvPrincipal.Location = new Point(10, 68);
            dgvPrincipal.Margin = new Padding(3, 2, 3, 2);
            dgvPrincipal.Name = "dgvPrincipal";
            dgvPrincipal.ReadOnly = true;
            dgvPrincipal.RowHeadersWidth = 51;
            dgvPrincipal.RowTemplate.Height = 29;
            dgvPrincipal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPrincipal.Size = new Size(976, 420);
            dgvPrincipal.TabIndex = 1;
            dgvPrincipal.CellFormatting += dgvPrincipal_CellFormatting;
            dgvPrincipal.DataBindingComplete += dgvPrincipal_DataBindingComplete;
            dgvPrincipal.KeyDown += dgvPrincipal_KeyDown_1;
            // 
            // colCuenta
            // 
            colCuenta.DataPropertyName = "Cuenta";
            colCuenta.HeaderText = "Cuenta";
            colCuenta.Name = "colCuenta";
            colCuenta.ReadOnly = true;
            // 
            // colNombre
            // 
            colNombre.DataPropertyName = "Nombre";
            colNombre.HeaderText = "Nombre";
            colNombre.Name = "colNombre";
            colNombre.ReadOnly = true;
            // 
            // colSaldo
            // 
            colSaldo.DataPropertyName = "Saldo";
            colSaldo.HeaderText = "Saldo";
            colSaldo.Name = "colSaldo";
            colSaldo.ReadOnly = true;
            // 
            // colRangoInferior
            // 
            colRangoInferior.DataPropertyName = "RangoInferior";
            colRangoInferior.HeaderText = "Rango Inferior";
            colRangoInferior.Name = "colRangoInferior";
            colRangoInferior.ReadOnly = true;
            // 
            // colRangoSuperior
            // 
            colRangoSuperior.DataPropertyName = "RangoSuperior";
            colRangoSuperior.HeaderText = "Rango Superior";
            colRangoSuperior.Name = "colRangoSuperior";
            colRangoSuperior.ReadOnly = true;
            // 
            // colGuia
            // 
            colGuia.DataPropertyName = "Guia";
            colGuia.HeaderText = "Guia";
            colGuia.Name = "colGuia";
            colGuia.ReadOnly = true;
            colGuia.Visible = false;
            // 
            // colFecha
            // 
            colFecha.DataPropertyName = "Fecha";
            colFecha.HeaderText = "Fecha";
            colFecha.Name = "colFecha";
            colFecha.ReadOnly = true;
            // 
            // colPoliza
            // 
            colPoliza.DataPropertyName = "Poliza";
            colPoliza.HeaderText = "Poliza";
            colPoliza.Name = "colPoliza";
            colPoliza.ReadOnly = true;
            // 
            // colConcepto
            // 
            colConcepto.DataPropertyName = "Concepto";
            colConcepto.HeaderText = "Concepto";
            colConcepto.Name = "colConcepto";
            colConcepto.ReadOnly = true;
            // 
            // colDebe
            // 
            colDebe.DataPropertyName = "Debe";
            colDebe.HeaderText = "Debe";
            colDebe.Name = "colDebe";
            colDebe.ReadOnly = true;
            // 
            // colHaber
            // 
            colHaber.DataPropertyName = "Haber";
            colHaber.HeaderText = "Haber";
            colHaber.Name = "colHaber";
            colHaber.ReadOnly = true;
            // 
            // lblRuta
            // 
            lblRuta.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblRuta.AutoEllipsis = true;
            lblRuta.Location = new Point(213, 39);
            lblRuta.Name = "lblRuta";
            lblRuta.Size = new Size(773, 18);
            lblRuta.TabIndex = 2;
            lblRuta.Text = "Ruta de datos no configurada.";
            // 
            // btnRegresar
            // 
            btnRegresar.Location = new Point(898, 32);
            btnRegresar.Margin = new Padding(3, 2, 3, 2);
            btnRegresar.Name = "btnRegresar";
            btnRegresar.Size = new Size(88, 29);
            btnRegresar.TabIndex = 3;
            btnRegresar.Text = "Regresar";
            btnRegresar.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoEllipsis = true;
            label1.Location = new Point(143, 39);
            label1.Name = "label1";
            label1.Size = new Size(64, 18);
            label1.TabIndex = 4;
            label1.Text = "Directorio:";
            // 
            // btnVolverACaptura
            // 
            btnVolverACaptura.Location = new Point(12, 38);
            btnVolverACaptura.Name = "btnVolverACaptura";
            btnVolverACaptura.Size = new Size(107, 23);
            btnVolverACaptura.TabIndex = 5;
            btnVolverACaptura.Text = "Volver a Captura";
            btnVolverACaptura.UseVisualStyleBackColor = true;
            btnVolverACaptura.Click += btnVolverACaptura_Click;
            // 
            // FormPrincipal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(998, 495);
            Controls.Add(btnVolverACaptura);
            Controls.Add(label1);
            Controls.Add(btnRegresar);
            Controls.Add(lblRuta);
            Controls.Add(dgvPrincipal);
            Controls.Add(menuStripPrincipal);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripPrincipal;
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(790, 430);
            Name = "FormPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Costos 2026";
            FormClosing += FormPrincipal_FormClosing;
            menuStripPrincipal.ResumeLayout(false);
            menuStripPrincipal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPrincipal).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridViewTextBoxColumn colCuenta;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colSaldo;
        private DataGridViewTextBoxColumn colRangoInferior;
        private DataGridViewTextBoxColumn colRangoSuperior;
        private DataGridViewTextBoxColumn colGuia;
        private DataGridViewTextBoxColumn colFecha;
        private DataGridViewTextBoxColumn colPoliza;
        private DataGridViewTextBoxColumn colConcepto;
        private DataGridViewTextBoxColumn colDebe;
        private DataGridViewTextBoxColumn colHaber;
        private Label label1;
        private ToolStripMenuItem menuSeleccionFilaCompleta;
        private ToolStripMenuItem menuDGVConEncabezados;
        private ToolStripMenuItem menuDGVSinEncabezados;
        private Button btnVolverACaptura;
    }
}
