using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CapturaDePolizas_2026_NET8
{
    partial class FormVerPolizas
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
            this.gbTipo = new GroupBox();
            this.rbTodos = new RadioButton();
            this.rbPoliza = new RadioButton();
            this.rbCheque = new RadioButton();

            this.gbFechas = new GroupBox();
            this.chkFiltroFecha = new CheckBox();
            this.rbFechaUnica = new RadioButton();
            this.rbRangoFechas = new RadioButton();
            this.lblDesde = new Label();
            this.dtpDesde = new DateTimePicker();
            this.lblHasta = new Label();
            this.dtpHasta = new DateTimePicker();

            this.gbBusqueda = new GroupBox();
            this.lblBuscar = new Label();
            this.txtBuscarTexto = new TextBox();

            this.gbOrden = new GroupBox();
            this.lblOrden = new Label();
            this.cmbOrden = new ComboBox();

            this.pnlTotales = new Panel();
            this.lblSumDebe = new Label();
            this.lblSumHaber = new Label();
            this.lblDiferencia = new Label();

            this.btnRestablecer = new Button();
            this.dgvPolizas = new DataGridView();

            this.pnlFiltros.SuspendLayout();
            this.gbTipo.SuspendLayout();
            this.gbFechas.SuspendLayout();
            this.gbBusqueda.SuspendLayout();
            this.gbOrden.SuspendLayout();
            this.pnlTotales.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPolizas)).BeginInit();
            this.SuspendLayout();
            //
            // pnlFiltros
            //
            this.pnlFiltros.BackColor = Color.FromArgb(245, 245, 247);
            this.pnlFiltros.Controls.Add(this.gbTipo);
            this.pnlFiltros.Controls.Add(this.gbFechas);
            this.pnlFiltros.Controls.Add(this.gbBusqueda);
            this.pnlFiltros.Controls.Add(this.gbOrden);
            this.pnlFiltros.Controls.Add(this.btnRestablecer);
            this.pnlFiltros.Dock = DockStyle.Top;
            this.pnlFiltros.Location = new Point(0, 0);
            this.pnlFiltros.Name = "pnlFiltros";
            this.pnlFiltros.Size = new Size(1100, 120);
            //
            // gbTipo
            //
            this.gbTipo.Controls.Add(this.rbTodos);
            this.gbTipo.Controls.Add(this.rbPoliza);
            this.gbTipo.Controls.Add(this.rbCheque);
            this.gbTipo.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbTipo.Location = new Point(10, 10);
            this.gbTipo.Name = "gbTipo";
            this.gbTipo.Size = new Size(160, 100);
            this.gbTipo.TabIndex = 0;
            this.gbTipo.TabStop = false;
            this.gbTipo.Text = "Tipo de Captura";
            //
            // rbTodos
            //
            this.rbTodos.AutoSize = true;
            this.rbTodos.Checked = true;
            this.rbTodos.Location = new Point(10, 20);
            this.rbTodos.Name = "rbTodos";
            this.rbTodos.Size = new Size(57, 19);
            this.rbTodos.Text = "Todos";
            this.rbTodos.UseVisualStyleBackColor = true;
            //
            // rbPoliza
            //
            this.rbPoliza.AutoSize = true;
            this.rbPoliza.Location = new Point(10, 45);
            this.rbPoliza.Name = "rbPoliza";
            this.rbPoliza.Size = new Size(84, 19);
            this.rbPoliza.Text = "Ver Pólizas";
            this.rbPoliza.UseVisualStyleBackColor = true;
            //
            // rbCheque
            //
            this.rbCheque.AutoSize = true;
            this.rbCheque.Location = new Point(10, 70);
            this.rbCheque.Name = "rbCheque";
            this.rbCheque.Size = new Size(93, 19);
            this.rbCheque.Text = "Ver Cheques";
            this.rbCheque.UseVisualStyleBackColor = true;
            //
            // gbFechas
            //
            this.gbFechas.Controls.Add(this.chkFiltroFecha);
            this.gbFechas.Controls.Add(this.rbFechaUnica);
            this.gbFechas.Controls.Add(this.rbRangoFechas);
            this.gbFechas.Controls.Add(this.lblDesde);
            this.gbFechas.Controls.Add(this.dtpDesde);
            this.gbFechas.Controls.Add(this.lblHasta);
            this.gbFechas.Controls.Add(this.dtpHasta);
            this.gbFechas.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbFechas.Location = new Point(180, 10);
            this.gbFechas.Name = "gbFechas";
            this.gbFechas.Size = new Size(380, 100);
            this.gbFechas.TabIndex = 1;
            this.gbFechas.TabStop = false;
            this.gbFechas.Text = "Filtrar por Fechas";
            //
            // chkFiltroFecha
            //
            this.chkFiltroFecha.AutoSize = true;
            this.chkFiltroFecha.Location = new Point(10, 20);
            this.chkFiltroFecha.Name = "chkFiltroFecha";
            this.chkFiltroFecha.Size = new Size(111, 19);
            this.chkFiltroFecha.Text = "Habilitar filtro";
            this.chkFiltroFecha.UseVisualStyleBackColor = true;
            //
            // rbFechaUnica
            //
            this.rbFechaUnica.AutoSize = true;
            this.rbFechaUnica.Checked = true;
            this.rbFechaUnica.Location = new Point(150, 20);
            this.rbFechaUnica.Name = "rbFechaUnica";
            this.rbFechaUnica.Size = new Size(89, 19);
            this.rbFechaUnica.Text = "Fecha única";
            this.rbFechaUnica.UseVisualStyleBackColor = true;
            //
            // rbRangoFechas
            //
            this.rbRangoFechas.AutoSize = true;
            this.rbRangoFechas.Location = new Point(250, 20);
            this.rbRangoFechas.Name = "rbRangoFechas";
            this.rbRangoFechas.Size = new Size(113, 19);
            this.rbRangoFechas.Text = "Rango de fechas";
            this.rbRangoFechas.UseVisualStyleBackColor = true;
            //
            // lblDesde
            //
            this.lblDesde.AutoSize = true;
            this.lblDesde.Location = new Point(10, 56);
            this.lblDesde.Name = "lblDesde";
            this.lblDesde.Size = new Size(42, 15);
            this.lblDesde.TabIndex = 3;
            this.lblDesde.Text = "Desde:";
            //
            // dtpDesde
            //
            this.dtpDesde.Format = DateTimePickerFormat.Short;
            this.dtpDesde.Location = new Point(58, 52);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new Size(115, 23);
            this.dtpDesde.TabIndex = 4;
            //
            // lblHasta
            //
            this.lblHasta.AutoSize = true;
            this.lblHasta.Location = new Point(190, 56);
            this.lblHasta.Name = "lblHasta";
            this.lblHasta.Size = new Size(40, 15);
            this.lblHasta.TabIndex = 5;
            this.lblHasta.Text = "Hasta:";
            //
            // dtpHasta
            //
            this.dtpHasta.Format = DateTimePickerFormat.Short;
            this.dtpHasta.Location = new Point(236, 52);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new Size(115, 23);
            this.dtpHasta.TabIndex = 6;
            //
            // gbBusqueda
            //
            this.gbBusqueda.Controls.Add(this.lblBuscar);
            this.gbBusqueda.Controls.Add(this.txtBuscarTexto);
            this.gbBusqueda.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbBusqueda.Location = new Point(570, 10);
            this.gbBusqueda.Name = "gbBusqueda";
            this.gbBusqueda.Size = new Size(230, 100);
            this.gbBusqueda.TabIndex = 2;
            this.gbBusqueda.TabStop = false;
            this.gbBusqueda.Text = "Búsqueda Rápida";
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new Point(10, 22);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new Size(189, 15);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "Buscar por Concepto / RFC / Benef.:";
            //
            // txtBuscarTexto
            //
            this.txtBuscarTexto.Location = new Point(10, 48);
            this.txtBuscarTexto.Name = "txtBuscarTexto";
            this.txtBuscarTexto.Size = new Size(210, 23);
            this.txtBuscarTexto.TabIndex = 1;
            //
            // gbOrden
            //
            this.gbOrden.Controls.Add(this.lblOrden);
            this.gbOrden.Controls.Add(this.cmbOrden);
            this.gbOrden.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.gbOrden.Location = new Point(810, 10);
            this.gbOrden.Name = "gbOrden";
            this.gbOrden.Size = new Size(170, 100);
            this.gbOrden.TabIndex = 3;
            this.gbOrden.TabStop = false;
            this.gbOrden.Text = "Ordenar Por";
            //
            // lblOrden
            //
            this.lblOrden.AutoSize = true;
            this.lblOrden.Location = new Point(10, 22);
            this.lblOrden.Name = "lblOrden";
            this.lblOrden.Size = new Size(107, 15);
            this.lblOrden.Text = "Selecciona criterio:";
            //
            // cmbOrden
            //
            this.cmbOrden.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbOrden.FormattingEnabled = true;
            this.cmbOrden.Items.AddRange(new object[] {
                "Folio (Ascendente)",
                "Folio (Descendente)",
                "ID (Ascendente)",
                "ID (Descendente)",
                "Fecha (Ascendente)",
                "Fecha (Descendente)"
            });
            this.cmbOrden.Location = new Point(10, 48);
            this.cmbOrden.Name = "cmbOrden";
            this.cmbOrden.Size = new Size(150, 23);
            this.cmbOrden.TabIndex = 1;
            this.cmbOrden.SelectedIndex = 0;
            //
            // pnlTotales
            //
            this.pnlTotales.BackColor = Color.FromArgb(235, 235, 240);
            this.pnlTotales.Controls.Add(this.lblSumDebe);
            this.pnlTotales.Controls.Add(this.lblSumHaber);
            this.pnlTotales.Controls.Add(this.lblDiferencia);
            this.pnlTotales.Dock = DockStyle.Bottom;
            this.pnlTotales.Location = new Point(0, 565);
            this.pnlTotales.Name = "pnlTotales";
            this.pnlTotales.Size = new Size(1100, 35);
            this.pnlTotales.TabIndex = 5;
            //
            // lblSumDebe
            //
            this.lblSumDebe.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            this.lblSumDebe.Location = new Point(10, 8);
            this.lblSumDebe.Name = "lblSumDebe";
            this.lblSumDebe.Size = new Size(240, 20);
            this.lblSumDebe.Text = "Total Debe: $0.00";
            //
            // lblSumHaber
            //
            this.lblSumHaber.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            this.lblSumHaber.Location = new Point(270, 8);
            this.lblSumHaber.Name = "lblSumHaber";
            this.lblSumHaber.Size = new Size(240, 20);
            this.lblSumHaber.Text = "Total Haber: $0.00";
            //
            // lblDiferencia
            //
            this.lblDiferencia.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            this.lblDiferencia.ForeColor = Color.DarkRed;
            this.lblDiferencia.Location = new Point(530, 8);
            this.lblDiferencia.Name = "lblDiferencia";
            this.lblDiferencia.Size = new Size(240, 20);
            this.lblDiferencia.Text = "Diferencia: $0.00";
            //
            // btnRestablecer
            //
            this.btnRestablecer.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            this.btnRestablecer.Location = new Point(990, 35);
            this.btnRestablecer.Name = "btnRestablecer";
            this.btnRestablecer.Size = new Size(100, 50);
            this.btnRestablecer.TabIndex = 4;
            this.btnRestablecer.Text = "Restablecer Filtros";
            this.btnRestablecer.UseVisualStyleBackColor = true;
            //
            // dgvPolizas
            //
            this.dgvPolizas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPolizas.Dock = DockStyle.Fill;
            this.dgvPolizas.Location = new Point(0, 120);
            this.dgvPolizas.Name = "dgvPolizas";
            this.dgvPolizas.Size = new Size(1100, 445);
            this.dgvPolizas.TabIndex = 6;
            //
            // FormVerPolizas
            //
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1100, 600);
            this.Controls.Add(this.dgvPolizas);
            this.Controls.Add(this.pnlTotales);
            this.Controls.Add(this.pnlFiltros);
            this.Name = "FormVerPolizas";
            this.pnlFiltros.ResumeLayout(false);
            this.gbTipo.ResumeLayout(false);
            this.gbTipo.PerformLayout();
            this.gbFechas.ResumeLayout(false);
            this.gbFechas.PerformLayout();
            this.gbBusqueda.ResumeLayout(false);
            this.gbBusqueda.PerformLayout();
            this.gbOrden.ResumeLayout(false);
            this.gbOrden.PerformLayout();
            this.pnlTotales.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPolizas)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlFiltros;
        private System.Windows.Forms.GroupBox gbTipo;
        private System.Windows.Forms.RadioButton rbTodos;
        private System.Windows.Forms.RadioButton rbPoliza;
        private System.Windows.Forms.RadioButton rbCheque;

        private System.Windows.Forms.GroupBox gbFechas;
        private System.Windows.Forms.CheckBox chkFiltroFecha;
        private System.Windows.Forms.RadioButton rbFechaUnica;
        private System.Windows.Forms.RadioButton rbRangoFechas;
        private System.Windows.Forms.Label lblDesde;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label lblHasta;
        private System.Windows.Forms.DateTimePicker dtpHasta;

        private System.Windows.Forms.GroupBox gbBusqueda;
        private System.Windows.Forms.TextBox txtBuscarTexto;
        private System.Windows.Forms.Label lblBuscar;

        private System.Windows.Forms.Button btnRestablecer;

        private System.Windows.Forms.DataGridView dgvPolizas;

        private System.Windows.Forms.GroupBox gbOrden;
        private System.Windows.Forms.Label lblOrden;
        private System.Windows.Forms.ComboBox cmbOrden;

        private System.Windows.Forms.Panel pnlTotales;
        private System.Windows.Forms.Label lblSumDebe;
        private System.Windows.Forms.Label lblSumHaber;
        private System.Windows.Forms.Label lblDiferencia;

        private DataTable dtOriginal;
        private DataView dvFiltrado;
        private bool deshabilitarEventos = false;
        //private Data.CatalogoRepository _catalogoRepo;

        
    }
}