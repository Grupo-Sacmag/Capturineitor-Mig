namespace GaCostos
{
    partial class FormPolizaDiario
    {
        private System.ComponentModel.IContainer? components = null;

        private MenuStrip menuStripPrincipal = null!;
        private ToolStripMenuItem menuEdicion = null!;
        private ToolStripMenuItem menuCopiarSeleccion = null!;
        private ToolStripMenuItem menuSeleccionarTodo = null!;
        private ToolStripMenuItem menuImprimir = null!;

        private Panel pnlEncabezado = null!;
        private Label lblTitulo = null!;

        private Panel pnlContenido = null!;
        private DataGridView dgvPoliza = null!;

        private TableLayoutPanel tlpSumas = null!;
        private Label lblSumasIguales = null!;
        private Label lblTotalParcial = null!;
        private Label lblTotalDebe = null!;
        private Label lblTotalHaber = null!;
        private Label lblEspacioRedaccion = null!;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPolizaDiario));
            menuStripPrincipal = new MenuStrip();
            menuEdicion = new ToolStripMenuItem();
            menuCopiarSeleccion = new ToolStripMenuItem();
            menuSeleccionarTodo = new ToolStripMenuItem();
            menuImprimir = new ToolStripMenuItem();
            tsVistaPrevia = new ToolStripMenuItem();
            tsImprimirDoc = new ToolStripMenuItem();
            pnlEncabezado = new Panel();
            lblTitulo = new Label();
            pnlContenido = new Panel();
            dgvPoliza = new DataGridView();
            tlpSumas = new TableLayoutPanel();
            lblSumasIguales = new Label();
            lblTotalParcial = new Label();
            lblTotalDebe = new Label();
            lblTotalHaber = new Label();
            lblEspacioRedaccion = new Label();
            menuStripPrincipal.SuspendLayout();
            pnlEncabezado.SuspendLayout();
            pnlContenido.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPoliza).BeginInit();
            tlpSumas.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripPrincipal
            // 
            menuStripPrincipal.ImageScalingSize = new Size(20, 20);
            menuStripPrincipal.Items.AddRange(new ToolStripItem[] { menuEdicion, menuImprimir });
            menuStripPrincipal.Location = new Point(0, 0);
            menuStripPrincipal.Name = "menuStripPrincipal";
            menuStripPrincipal.Padding = new Padding(5, 2, 0, 2);
            menuStripPrincipal.Size = new Size(1050, 24);
            menuStripPrincipal.TabIndex = 0;
            // 
            // menuEdicion
            // 
            menuEdicion.DropDownItems.AddRange(new ToolStripItem[] { menuCopiarSeleccion, menuSeleccionarTodo });
            menuEdicion.Name = "menuEdicion";
            menuEdicion.Size = new Size(58, 20);
            menuEdicion.Text = "Edición";
            // 
            // menuCopiarSeleccion
            // 
            menuCopiarSeleccion.Name = "menuCopiarSeleccion";
            menuCopiarSeleccion.ShortcutKeys = Keys.Control | Keys.C;
            menuCopiarSeleccion.Size = new Size(204, 22);
            menuCopiarSeleccion.Text = "Copiar selección";
            // 
            // menuSeleccionarTodo
            // 
            menuSeleccionarTodo.Name = "menuSeleccionarTodo";
            menuSeleccionarTodo.ShortcutKeys = Keys.Control | Keys.A;
            menuSeleccionarTodo.Size = new Size(204, 22);
            menuSeleccionarTodo.Text = "Seleccionar todo";
            // 
            // menuImprimir
            // 
            menuImprimir.DropDownItems.AddRange(new ToolStripItem[] { tsVistaPrevia, tsImprimirDoc });
            menuImprimir.Name = "menuImprimir";
            menuImprimir.Size = new Size(65, 20);
            menuImprimir.Text = "Imprimir";
            // 
            // tsVistaPrevia
            // 
            tsVistaPrevia.Name = "tsVistaPrevia";
            tsVistaPrevia.Size = new Size(186, 22);
            tsVistaPrevia.Text = "Vista Previa";
            tsVistaPrevia.Click += tsVistaPrevia_Click;
            // 
            // tsImprimirDoc
            // 
            tsImprimirDoc.Name = "tsImprimirDoc";
            tsImprimirDoc.Size = new Size(186, 22);
            tsImprimirDoc.Text = "Imprimir Documento";
            tsImprimirDoc.Click += tsImprimirDoc_Click;
            // 
            // pnlEncabezado
            // 
            pnlEncabezado.BackColor = Color.Yellow;
            pnlEncabezado.Controls.Add(lblTitulo);
            pnlEncabezado.Dock = DockStyle.Top;
            pnlEncabezado.Location = new Point(0, 24);
            pnlEncabezado.Name = "pnlEncabezado";
            pnlEncabezado.Padding = new Padding(10, 0, 10, 0);
            pnlEncabezado.Size = new Size(1050, 34);
            pnlEncabezado.TabIndex = 1;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoEllipsis = true;
            lblTitulo.BackColor = Color.Yellow;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            lblTitulo.Location = new Point(10, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(1030, 34);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Póliza:";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlContenido
            // 
            pnlContenido.Controls.Add(dgvPoliza);
            pnlContenido.Controls.Add(tlpSumas);
            pnlContenido.Dock = DockStyle.Fill;
            pnlContenido.Location = new Point(0, 58);
            pnlContenido.Name = "pnlContenido";
            pnlContenido.Padding = new Padding(8);
            pnlContenido.Size = new Size(1050, 452);
            pnlContenido.TabIndex = 2;
            // 
            // dgvPoliza
            // 
            dgvPoliza.AllowUserToAddRows = false;
            dgvPoliza.AllowUserToDeleteRows = false;
            dgvPoliza.AllowUserToResizeColumns = false;
            dgvPoliza.AllowUserToResizeRows = false;
            dgvPoliza.BackgroundColor = Color.White;
            dgvPoliza.BorderStyle = BorderStyle.Fixed3D;
            dgvPoliza.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dgvPoliza.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvPoliza.ColumnHeadersHeight = 22;
            dgvPoliza.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvPoliza.Dock = DockStyle.Fill;
            dgvPoliza.EnableHeadersVisualStyles = false;
            dgvPoliza.GridColor = Color.Silver;
            dgvPoliza.Location = new Point(8, 8);
            dgvPoliza.Name = "dgvPoliza";
            dgvPoliza.ReadOnly = true;
            dgvPoliza.RowHeadersVisible = false;
            dgvPoliza.RowTemplate.Height = 20;
            dgvPoliza.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvPoliza.Size = new Size(1034, 412);
            dgvPoliza.TabIndex = 0;
            // 
            // tlpSumas
            // 
            tlpSumas.BackColor = Color.Gainsboro;
            tlpSumas.ColumnCount = 7;
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpSumas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpSumas.Controls.Add(lblSumasIguales, 0, 0);
            tlpSumas.Controls.Add(lblTotalParcial, 3, 0);
            tlpSumas.Controls.Add(lblTotalDebe, 4, 0);
            tlpSumas.Controls.Add(lblTotalHaber, 5, 0);
            tlpSumas.Controls.Add(lblEspacioRedaccion, 6, 0);
            tlpSumas.Dock = DockStyle.Bottom;
            tlpSumas.Location = new Point(8, 420);
            tlpSumas.Margin = new Padding(0);
            tlpSumas.Name = "tlpSumas";
            tlpSumas.RowCount = 1;
            tlpSumas.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpSumas.Size = new Size(1034, 24);
            tlpSumas.TabIndex = 1;
            // 
            // lblSumasIguales
            // 
            lblSumasIguales.BackColor = Color.Gainsboro;
            lblSumasIguales.BorderStyle = BorderStyle.FixedSingle;
            tlpSumas.SetColumnSpan(lblSumasIguales, 3);
            lblSumasIguales.Dock = DockStyle.Fill;
            lblSumasIguales.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            lblSumasIguales.Location = new Point(0, 0);
            lblSumasIguales.Margin = new Padding(0);
            lblSumasIguales.Name = "lblSumasIguales";
            lblSumasIguales.Padding = new Padding(0, 0, 6, 0);
            lblSumasIguales.Size = new Size(390, 24);
            lblSumasIguales.TabIndex = 0;
            lblSumasIguales.Text = "SUMAS IGUALES";
            lblSumasIguales.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalParcial
            // 
            lblTotalParcial.Location = new Point(393, 0);
            lblTotalParcial.Name = "lblTotalParcial";
            lblTotalParcial.Size = new Size(100, 23);
            lblTotalParcial.TabIndex = 1;
            // 
            // lblTotalDebe
            // 
            lblTotalDebe.Location = new Point(503, 0);
            lblTotalDebe.Name = "lblTotalDebe";
            lblTotalDebe.Size = new Size(100, 23);
            lblTotalDebe.TabIndex = 2;
            // 
            // lblTotalHaber
            // 
            lblTotalHaber.Location = new Point(613, 0);
            lblTotalHaber.Name = "lblTotalHaber";
            lblTotalHaber.Size = new Size(100, 23);
            lblTotalHaber.TabIndex = 3;
            // 
            // lblEspacioRedaccion
            // 
            lblEspacioRedaccion.BackColor = Color.Gainsboro;
            lblEspacioRedaccion.BorderStyle = BorderStyle.FixedSingle;
            lblEspacioRedaccion.Dock = DockStyle.Fill;
            lblEspacioRedaccion.Location = new Point(720, 0);
            lblEspacioRedaccion.Margin = new Padding(0);
            lblEspacioRedaccion.Name = "lblEspacioRedaccion";
            lblEspacioRedaccion.Size = new Size(314, 24);
            lblEspacioRedaccion.TabIndex = 4;
            // 
            // FormPolizaDiario
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1050, 510);
            Controls.Add(pnlContenido);
            Controls.Add(pnlEncabezado);
            Controls.Add(menuStripPrincipal);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStripPrincipal;
            MinimumSize = new Size(900, 460);
            Name = "FormPolizaDiario";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Póliza:";
            menuStripPrincipal.ResumeLayout(false);
            menuStripPrincipal.PerformLayout();
            pnlEncabezado.ResumeLayout(false);
            pnlContenido.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPoliza).EndInit();
            tlpSumas.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private static void ConfigurarLabelTotal(Label label, string nombre)
        {
            label.BackColor = Color.Gainsboro;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.Dock = DockStyle.Fill;
            label.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            label.Margin = new Padding(0);
            label.Name = nombre;
            label.Padding = new Padding(0, 0, 4, 0);
            label.Text = string.Empty;
            label.TextAlign = ContentAlignment.MiddleRight;
        }

        #endregion

        private Button btnCerrar;
        private ToolStripMenuItem tsVistaPrevia;
        private ToolStripMenuItem tsImprimirDoc;
    }
}