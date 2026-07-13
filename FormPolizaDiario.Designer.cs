namespace GaCostos
{
    partial class FormPolizaDiario
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel pnlEncabezado = null!;
        private Label lblTitulo = null!;
        private Button btnCerrar = null!;
        private DataGridView dgvPoliza = null!;


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
            pnlEncabezado = new Panel();
            lblTitulo = new Label();
            btnCerrar = new Button();
            dgvPoliza = new DataGridView();
            pnlEncabezado.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPoliza).BeginInit();
            SuspendLayout();
            // 
            // pnlEncabezado
            // 
            pnlEncabezado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlEncabezado.Controls.Add(lblTitulo);
            pnlEncabezado.Controls.Add(btnCerrar);
            pnlEncabezado.Location = new Point(0, 0);
            pnlEncabezado.Margin = new Padding(3, 2, 3, 2);
            pnlEncabezado.Name = "pnlEncabezado";
            pnlEncabezado.Size = new Size(1050, 44);
            pnlEncabezado.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitulo.AutoEllipsis = true;
            lblTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitulo.Location = new Point(10, 13);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(917, 19);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Póliza de diario";
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Location = new Point(943, 9);
            btnCerrar.Margin = new Padding(3, 2, 3, 2);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(96, 26);
            btnCerrar.TabIndex = 1;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = true;
            // 
            // dgvPoliza
            // 
            dgvPoliza.AllowUserToAddRows = false;
            dgvPoliza.AllowUserToDeleteRows = false;
            dgvPoliza.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPoliza.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dgvPoliza.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPoliza.Location = new Point(10, 48);
            dgvPoliza.Margin = new Padding(3, 2, 3, 2);
            dgvPoliza.MultiSelect = false;
            dgvPoliza.Name = "dgvPoliza";
            dgvPoliza.ReadOnly = true;
            dgvPoliza.RowHeadersWidth = 51;
            dgvPoliza.RowTemplate.Height = 29;
            dgvPoliza.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPoliza.Size = new Size(1029, 453);
            dgvPoliza.TabIndex = 1;
            // 
            // FormPolizaDiario
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1050, 510);
            Controls.Add(dgvPoliza);
            Controls.Add(pnlEncabezado);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(877, 460);
            Name = "FormPolizaDiario";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Póliza de diario";
            pnlEncabezado.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPoliza).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}