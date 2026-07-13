using System.Data;

namespace CapturaDePolizas_2026_NET8
{
    partial class FormVerSubcuentas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlFiltros = new Panel();
            this.gbBusqueda = new GroupBox();
            this.lblBuscar = new Label();
            this.txtBuscarTexto = new TextBox();

            this.gbCuentaPadre = new GroupBox();
            this.cmbCuentaPadre = new ComboBox();

            this.gbMonto = new GroupBox();
            this.lblMin = new Label();
            this.txtMontoMin = new TextBox();
            this.lblMax = new Label();
            this.txtMontoMax = new TextBox();

            this.gbOrden = new GroupBox();
            this.cmbColumna = new ComboBox();
            this.rbAsc = new RadioButton();
            this.rbDesc = new RadioButton();

            this.btnRestablecer = new Button();
            this.dgvSubcuentas = new DataGridView();

            this.pnlFiltros.SuspendLayout();
            this.gbBusqueda.SuspendLayout();
            this.gbCuentaPadre.SuspendLayout();
            this.gbMonto.SuspendLayout();
            this.gbOrden.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubcuentas)).BeginInit();
            this.SuspendLayout();
            //
            // pnlFiltros
            //
            this.pnlFiltros.BackColor = Color.FromArgb(245, 245, 247);
            this.pnlFiltros.Controls.Add(this.gbBusqueda);
            this.pnlFiltros.Controls.Add(this.gbCuentaPadre);
            this.pnlFiltros.Controls.Add(this.gbOrden);
            this.pnlFiltros.Controls.Add(this.btnRestablecer);
            this.pnlFiltros.Dock = DockStyle.Top;
            this.pnlFiltros.Location = new Point(0, 0);
            this.pnlFiltros.Name = "pnlFiltros";
            this.pnlFiltros.Size = new Size(1000, 80);
            //
            // gbBusqueda
            //
            this.gbBusqueda.Controls.Add(this.lblBuscar);
            this.gbBusqueda.Controls.Add(this.txtBuscarTexto);
            this.gbBusqueda.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbBusqueda.Location = new Point(10, 5);
            this.gbBusqueda.Name = "gbBusqueda";
            this.gbBusqueda.Size = new Size(180, 68);
            this.gbBusqueda.TabIndex = 0;
            this.gbBusqueda.TabStop = false;
            this.gbBusqueda.Text = "Búsqueda";
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new Point(10, 20);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new Size(102, 15);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "Subcta. o Nombre";
            //
            // txtBuscarTexto
            //
            this.txtBuscarTexto.Location = new Point(10, 38);
            this.txtBuscarTexto.Name = "txtBuscarTexto";
            this.txtBuscarTexto.Size = new Size(160, 23);
            this.txtBuscarTexto.TabIndex = 1;
            //
            // gbCuentaPadre
            //
            this.gbCuentaPadre.Controls.Add(this.cmbCuentaPadre);
            this.gbCuentaPadre.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbCuentaPadre.Location = new Point(200, 5);
            this.gbCuentaPadre.Name = "gbCuentaPadre";
            this.gbCuentaPadre.Size = new Size(180, 68);
            this.gbCuentaPadre.TabIndex = 1;
            this.gbCuentaPadre.TabStop = false;
            this.gbCuentaPadre.Text = "Filtrar por Cuenta";
            //
            // cmbCuentaPadre
            //
            this.cmbCuentaPadre.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCuentaPadre.FormattingEnabled = true;
            this.cmbCuentaPadre.Location = new Point(10, 32);
            this.cmbCuentaPadre.Name = "cmbCuentaPadre";
            this.cmbCuentaPadre.Size = new Size(160, 23);
            this.cmbCuentaPadre.TabIndex = 0;
            //
            // gbMonto
            //
            this.gbMonto.Controls.Add(this.lblMin);
            this.gbMonto.Controls.Add(this.txtMontoMin);
            this.gbMonto.Controls.Add(this.lblMax);
            this.gbMonto.Controls.Add(this.txtMontoMax);
            this.gbMonto.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbMonto.Location = new Point(390, 5);
            this.gbMonto.Name = "gbMonto";
            this.gbMonto.Size = new Size(210, 68);
            this.gbMonto.TabIndex = 2;
            this.gbMonto.TabStop = false;
            this.gbMonto.Text = "Rango de Importe Neto";
            //
            // lblMin
            //
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new Point(10, 20);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new Size(31, 15);
            this.lblMin.TabIndex = 0;
            this.lblMin.Text = "Mín:";
            //
            // txtMontoMin
            //
            this.txtMontoMin.Location = new Point(10, 38);
            this.txtMontoMin.Name = "txtMontoMin";
            this.txtMontoMin.Size = new Size(85, 23);
            this.txtMontoMin.TabIndex = 1;
            //
            // lblMax
            //
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new Point(110, 20);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new Size(33, 15);
            this.lblMax.TabIndex = 2;
            this.lblMax.Text = "Máx:";
            //
            // txtMontoMax
            //
            this.txtMontoMax.Location = new Point(110, 38);
            this.txtMontoMax.Name = "txtMontoMax";
            this.txtMontoMax.Size = new Size(85, 23);
            this.txtMontoMax.TabIndex = 3;
            //
            // gbOrden
            //
            this.gbOrden.Controls.Add(this.cmbColumna);
            this.gbOrden.Controls.Add(this.rbAsc);
            this.gbOrden.Controls.Add(this.rbDesc);
            this.gbOrden.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbOrden.Location = new Point(390, 5);
            this.gbOrden.Name = "gbOrden";
            this.gbOrden.Size = new Size(245, 68);
            this.gbOrden.TabIndex = 3;
            this.gbOrden.TabStop = false;
            this.gbOrden.Text = "Ordenación";
            //
            // cmbColumna
            //
            this.cmbColumna.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbColumna.FormattingEnabled = true;
            this.cmbColumna.Location = new Point(10, 28);
            this.cmbColumna.Name = "cmbColumna";
            this.cmbColumna.Size = new Size(110, 23);
            this.cmbColumna.TabIndex = 0;
            //
            // rbAsc
            //
            this.rbAsc.AutoSize = true;
            this.rbAsc.Checked = true;
            this.rbAsc.Location = new Point(130, 18);
            this.rbAsc.Name = "rbAsc";
            this.rbAsc.Size = new Size(87, 19);
            this.rbAsc.TabIndex = 1;
            this.rbAsc.TabStop = true;
            this.rbAsc.Text = "Ascendente";
            this.rbAsc.UseVisualStyleBackColor = true;
            //
            // rbDesc
            //
            this.rbDesc.AutoSize = true;
            this.rbDesc.Location = new Point(130, 41);
            this.rbDesc.Name = "rbDesc";
            this.rbDesc.Size = new Size(93, 19);
            this.rbDesc.TabIndex = 2;
            this.rbDesc.Text = "Descendente";
            this.rbDesc.UseVisualStyleBackColor = true;
            //
            // btnRestablecer
            //
            this.btnRestablecer.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            this.btnRestablecer.Location = new Point(650, 20);
            this.btnRestablecer.Name = "btnRestablecer";
            this.btnRestablecer.Size = new Size(110, 40);
            this.btnRestablecer.TabIndex = 4;
            this.btnRestablecer.Text = "Restablecer";
            this.btnRestablecer.UseVisualStyleBackColor = true;
            //
            // dgvSubcuentas
            //
            this.dgvSubcuentas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSubcuentas.Dock = DockStyle.Fill;
            this.dgvSubcuentas.Location = new Point(0, 80);
            this.dgvSubcuentas.Name = "dgvSubcuentas";
            this.dgvSubcuentas.Size = new Size(1000, 400);
            this.dgvSubcuentas.TabIndex = 5;
            //
            // FormVerSubcuentas
            //
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 480);
            this.Controls.Add(this.dgvSubcuentas);
            this.Controls.Add(this.pnlFiltros);
            this.Name = "FormVerSubcuentas";
            this.pnlFiltros.ResumeLayout(false);
            this.gbBusqueda.ResumeLayout(false);
            this.gbBusqueda.PerformLayout();
            this.gbCuentaPadre.ResumeLayout(false);
            this.gbMonto.ResumeLayout(false);
            this.gbMonto.PerformLayout();
            this.gbOrden.ResumeLayout(false);
            this.gbOrden.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubcuentas)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlFiltros;
        private System.Windows.Forms.GroupBox gbBusqueda;
        private System.Windows.Forms.TextBox txtBuscarTexto;
        private System.Windows.Forms.Label lblBuscar;

        private System.Windows.Forms.GroupBox gbCuentaPadre;
        private System.Windows.Forms.ComboBox cmbCuentaPadre;

        private System.Windows.Forms.GroupBox gbMonto;
        private System.Windows.Forms.Label lblMin;
        private System.Windows.Forms.TextBox txtMontoMin;
        private System.Windows.Forms.Label lblMax;
        private System.Windows.Forms.TextBox txtMontoMax;

        private System.Windows.Forms.GroupBox gbOrden;
        private System.Windows.Forms.ComboBox cmbColumna;
        private System.Windows.Forms.RadioButton rbAsc;
        private System.Windows.Forms.RadioButton rbDesc;

        private System.Windows.Forms.Button btnRestablecer;
        private System.Windows.Forms.DataGridView dgvSubcuentas;

        private DataTable dtOriginal;
        private DataView dvFiltrado;
        private bool deshabilitarEventos = false;
        //private readonly CatalogoRepository _catalogoRepo = new CatalogoRepository();
    }
}