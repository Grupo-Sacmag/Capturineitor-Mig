using System.Data;

namespace CapturaDePolizas_2026_NET8
{
    partial class FormVerCuentas
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
            this.dgvCuentas = new DataGridView();

            this.pnlFiltros.SuspendLayout();
            this.gbBusqueda.SuspendLayout();
            this.gbMonto.SuspendLayout();
            this.gbOrden.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCuentas)).BeginInit();
            this.SuspendLayout();
            //
            // pnlFiltros
            //
            this.pnlFiltros.BackColor = Color.FromArgb(245, 245, 247);
            this.pnlFiltros.Controls.Add(this.gbBusqueda);
            this.pnlFiltros.Controls.Add(this.gbOrden);
            this.pnlFiltros.Controls.Add(this.btnRestablecer);
            this.pnlFiltros.Dock = DockStyle.Top;
            this.pnlFiltros.Location = new Point(0, 0);
            this.pnlFiltros.Name = "pnlFiltros";
            this.pnlFiltros.Size = new Size(850, 80);
            //
            // gbBusqueda
            //
            this.gbBusqueda.Controls.Add(this.lblBuscar);
            this.gbBusqueda.Controls.Add(this.txtBuscarTexto);
            this.gbBusqueda.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbBusqueda.Location = new Point(10, 5);
            this.gbBusqueda.Name = "gbBusqueda";
            this.gbBusqueda.Size = new Size(200, 68);
            this.gbBusqueda.TabIndex = 0;
            this.gbBusqueda.TabStop = false;
            this.gbBusqueda.Text = "Búsqueda";
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new Point(10, 20);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new Size(95, 15);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "Número/Nombre";
            //
            // txtBuscarTexto
            //
            this.txtBuscarTexto.Location = new Point(10, 38);
            this.txtBuscarTexto.Name = "txtBuscarTexto";
            this.txtBuscarTexto.Size = new Size(180, 23);
            this.txtBuscarTexto.TabIndex = 1;
            //
            // gbMonto
            //
            this.gbMonto.Controls.Add(this.lblMin);
            this.gbMonto.Controls.Add(this.txtMontoMin);
            this.gbMonto.Controls.Add(this.lblMax);
            this.gbMonto.Controls.Add(this.txtMontoMax);
            this.gbMonto.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbMonto.Location = new Point(220, 5);
            this.gbMonto.Name = "gbMonto";
            this.gbMonto.Size = new Size(230, 68);
            this.gbMonto.TabIndex = 1;
            this.gbMonto.TabStop = false;
            this.gbMonto.Text = "Rango de Monto Bruto";
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
            this.txtMontoMin.Size = new Size(95, 23);
            this.txtMontoMin.TabIndex = 1;
            //
            // lblMax
            //
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new Point(120, 20);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new Size(33, 15);
            this.lblMax.TabIndex = 2;
            this.lblMax.Text = "Máx:";
            //
            // txtMontoMax
            //
            this.txtMontoMax.Location = new Point(120, 38);
            this.txtMontoMax.Name = "txtMontoMax";
            this.txtMontoMax.Size = new Size(95, 23);
            this.txtMontoMax.TabIndex = 3;
            //
            // gbOrden
            //
            this.gbOrden.Controls.Add(this.cmbColumna);
            this.gbOrden.Controls.Add(this.rbAsc);
            this.gbOrden.Controls.Add(this.rbDesc);
            this.gbOrden.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbOrden.Location = new Point(220, 5);
            this.gbOrden.Name = "gbOrden";
            this.gbOrden.Size = new Size(245, 68);
            this.gbOrden.TabIndex = 2;
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
            this.btnRestablecer.Location = new Point(480, 20);
            this.btnRestablecer.Name = "btnRestablecer";
            this.btnRestablecer.Size = new Size(110, 40);
            this.btnRestablecer.TabIndex = 3;
            this.btnRestablecer.Text = "Restablecer";
            this.btnRestablecer.UseVisualStyleBackColor = true;
            //
            // dgvCuentas
            //
            this.dgvCuentas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCuentas.Dock = DockStyle.Fill;
            this.dgvCuentas.Location = new Point(0, 80);
            this.dgvCuentas.Name = "dgvCuentas";
            this.dgvCuentas.Size = new Size(850, 400);
            this.dgvCuentas.TabIndex = 4;
            //
            // FormVerCuentas
            //
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(850, 480);
            this.Controls.Add(this.dgvCuentas);
            this.Controls.Add(this.pnlFiltros);
            this.Name = "FormVerCuentas";
            this.pnlFiltros.ResumeLayout(false);
            this.gbBusqueda.ResumeLayout(false);
            this.gbBusqueda.PerformLayout();
            this.gbMonto.ResumeLayout(false);
            this.gbMonto.PerformLayout();
            this.gbOrden.ResumeLayout(false);
            this.gbOrden.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCuentas)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlFiltros;
        private System.Windows.Forms.GroupBox gbBusqueda;
        private System.Windows.Forms.TextBox txtBuscarTexto;
        private System.Windows.Forms.Label lblBuscar;

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
        private System.Windows.Forms.DataGridView dgvCuentas;
                
        private DataTable dtOriginal;
        private DataView dvFiltrado;
    }
}