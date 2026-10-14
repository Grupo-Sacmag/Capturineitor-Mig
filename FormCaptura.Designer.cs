namespace CapturaDePolizas_2026_NET8
{
    partial class FormCaptura
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
            btnAnterior = new Button();
            btnSiguiente = new Button();
            chkVarios = new CheckBox();
            menuStrip = new MenuStrip();
            mnuCaptura = new ToolStripMenuItem();
            mnuCapturaPoliza = new ToolStripMenuItem();
            mnuCapturaCheque = new ToolStripMenuItem();
            mnuVer = new ToolStripMenuItem();
            mnuVerCuentas = new ToolStripMenuItem();
            mnuVerSubcuentas = new ToolStripMenuItem();
            mnuVerPolizas = new ToolStripMenuItem();
            mnuVerCheques = new ToolStripMenuItem();
            mnuVerEstadosFinancieros = new ToolStripMenuItem();
            pnlCabeceraComun = new Panel();
            lblFecha = new Label();
            lblPoliza = new Label();
            txtPolizaNo = new TextBox();
            btnGuardar = new Button();
            btnEstadoSumas = new Button();
            pnlCamposCheque = new Panel();
            lblBeneficiario = new Label();
            txtBeneficiario = new TextBox();
            lblMonto = new Label();
            txtMonto = new TextBox();
            lblConceptoCheque = new Label();
            txtConceptoCheque = new TextBox();
            lblNumeroCheque = new Label();
            txtNumeroCheque = new TextBox();
            lblRFC = new Label();
            txtRFC = new TextBox();
            lblFolios = new Label();
            txtFolios = new TextBox();
            btnImprimir = new Button();
            pnlCamposPoliza = new Panel();
            lblConceptoPoliza = new Label();
            txtConceptoPoliza = new TextBox();
            dgvMovimientos = new DataGridView();
            pnlTotales = new Panel();
            lblResumenSuma = new Label();
            tsAuxiliares = new ToolStripMenuItem();
            menuStrip.SuspendLayout();
            pnlCabeceraComun.SuspendLayout();
            pnlCamposCheque.SuspendLayout();
            pnlCamposPoliza.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).BeginInit();
            pnlTotales.SuspendLayout();
            SuspendLayout();
            // 
            // btnAnterior
            // 
            btnAnterior.BackColor = SystemColors.ButtonHighlight;
            btnAnterior.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAnterior.Location = new Point(956, 4);
            btnAnterior.Margin = new Padding(4);
            btnAnterior.Name = "btnAnterior";
            btnAnterior.Size = new Size(38, 38);
            btnAnterior.TabIndex = 6;
            btnAnterior.Text = "⮪";
            btnAnterior.UseVisualStyleBackColor = false;
            btnAnterior.Visible = false;
            // 
            // btnSiguiente
            // 
            btnSiguiente.BackColor = SystemColors.ButtonHighlight;
            btnSiguiente.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSiguiente.Location = new Point(1002, 4);
            btnSiguiente.Margin = new Padding(4);
            btnSiguiente.Name = "btnSiguiente";
            btnSiguiente.Size = new Size(37, 38);
            btnSiguiente.TabIndex = 7;
            btnSiguiente.Text = "⮫";
            btnSiguiente.UseVisualStyleBackColor = false;
            btnSiguiente.Visible = false;
            // 
            // chkVarios
            // 
            chkVarios.AutoSize = true;
            chkVarios.Location = new Point(676, 71);
            chkVarios.Margin = new Padding(4);
            chkVarios.Name = "chkVarios";
            chkVarios.Size = new Size(57, 19);
            chkVarios.TabIndex = 12;
            chkVarios.Text = "Varios";
            chkVarios.UseVisualStyleBackColor = true;
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { mnuCaptura, tsAuxiliares, mnuVer });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(5, 2, 0, 2);
            menuStrip.Size = new Size(1050, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // mnuCaptura
            // 
            mnuCaptura.DropDownItems.AddRange(new ToolStripItem[] { mnuCapturaPoliza, mnuCapturaCheque });
            mnuCaptura.Name = "mnuCaptura";
            mnuCaptura.Size = new Size(61, 20);
            mnuCaptura.Text = "Captura";
            // 
            // mnuCapturaPoliza
            // 
            mnuCapturaPoliza.Name = "mnuCapturaPoliza";
            mnuCapturaPoliza.Size = new Size(180, 22);
            mnuCapturaPoliza.Text = "Póliza";
            mnuCapturaPoliza.Click += MnuCapturaPoliza_Click;
            // 
            // mnuCapturaCheque
            // 
            mnuCapturaCheque.Name = "mnuCapturaCheque";
            mnuCapturaCheque.Size = new Size(180, 22);
            mnuCapturaCheque.Text = "Cheque";
            mnuCapturaCheque.Click += MnuCapturaCheque_Click;
            // 
            // mnuVer
            // 
            mnuVer.DropDownItems.AddRange(new ToolStripItem[] { mnuVerCuentas, mnuVerSubcuentas, mnuVerPolizas, mnuVerCheques, mnuVerEstadosFinancieros });
            mnuVer.Name = "mnuVer";
            mnuVer.Size = new Size(35, 20);
            mnuVer.Text = "Ver";
            // 
            // mnuVerCuentas
            // 
            mnuVerCuentas.Name = "mnuVerCuentas";
            mnuVerCuentas.ShortcutKeys = Keys.Control | Keys.M;
            mnuVerCuentas.Size = new Size(217, 22);
            mnuVerCuentas.Text = "Cuentas";
            // 
            // mnuVerSubcuentas
            // 
            mnuVerSubcuentas.Name = "mnuVerSubcuentas";
            mnuVerSubcuentas.ShortcutKeys = Keys.Control | Keys.N;
            mnuVerSubcuentas.Size = new Size(217, 22);
            mnuVerSubcuentas.Text = "Subcuentas";
            // 
            // mnuVerPolizas
            // 
            mnuVerPolizas.Name = "mnuVerPolizas";
            mnuVerPolizas.ShortcutKeys = Keys.Control | Keys.D;
            mnuVerPolizas.Size = new Size(217, 22);
            mnuVerPolizas.Text = "Polizas";
            // 
            // mnuVerCheques
            // 
            mnuVerCheques.Name = "mnuVerCheques";
            mnuVerCheques.ShortcutKeys = Keys.Control | Keys.K;
            mnuVerCheques.Size = new Size(217, 22);
            mnuVerCheques.Text = "Cheques";
            // 
            // mnuVerEstadosFinancieros
            // 
            mnuVerEstadosFinancieros.Name = "mnuVerEstadosFinancieros";
            mnuVerEstadosFinancieros.ShortcutKeys = Keys.Control | Keys.E;
            mnuVerEstadosFinancieros.Size = new Size(217, 22);
            mnuVerEstadosFinancieros.Text = "Estados Financieros";
            // 
            // pnlCabeceraComun
            // 
            pnlCabeceraComun.BackColor = SystemColors.Control;
            pnlCabeceraComun.Controls.Add(lblFecha);
            pnlCabeceraComun.Controls.Add(lblPoliza);
            pnlCabeceraComun.Controls.Add(txtPolizaNo);
            pnlCabeceraComun.Controls.Add(btnGuardar);
            pnlCabeceraComun.Controls.Add(btnEstadoSumas);
            pnlCabeceraComun.Dock = DockStyle.Top;
            pnlCabeceraComun.Location = new Point(0, 24);
            pnlCabeceraComun.Margin = new Padding(4);
            pnlCabeceraComun.Name = "pnlCabeceraComun";
            pnlCabeceraComun.Size = new Size(1050, 69);
            pnlCabeceraComun.TabIndex = 1;
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = true;
            lblFecha.Location = new Point(11, 11);
            lblFecha.Margin = new Padding(4, 0, 4, 0);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(122, 15);
            lblFecha.TabIndex = 0;
            lblFecha.Text = "Día, de JUNIO de 2025";
            // 
            // lblPoliza
            // 
            lblPoliza.AutoSize = true;
            lblPoliza.Location = new Point(11, 40);
            lblPoliza.Margin = new Padding(4, 0, 4, 0);
            lblPoliza.Name = "lblPoliza";
            lblPoliza.Size = new Size(63, 15);
            lblPoliza.TabIndex = 1;
            lblPoliza.Text = "Póliza No.:";
            // 
            // txtPolizaNo
            // 
            txtPolizaNo.Location = new Point(98, 40);
            txtPolizaNo.Margin = new Padding(4);
            txtPolizaNo.Name = "txtPolizaNo";
            txtPolizaNo.Size = new Size(116, 23);
            txtPolizaNo.TabIndex = 2;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(326, 37);
            btnGuardar.Margin = new Padding(4);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(99, 29);
            btnGuardar.TabIndex = 4;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            // 
            // btnEstadoSumas
            // 
            btnEstadoSumas.BackColor = Color.LightPink;
            btnEstadoSumas.Enabled = false;
            btnEstadoSumas.FlatStyle = FlatStyle.Flat;
            btnEstadoSumas.ForeColor = Color.Red;
            btnEstadoSumas.Location = new Point(438, 37);
            btnEstadoSumas.Margin = new Padding(4);
            btnEstadoSumas.Name = "btnEstadoSumas";
            btnEstadoSumas.Size = new Size(164, 29);
            btnEstadoSumas.TabIndex = 5;
            btnEstadoSumas.Text = "Sumas incorrectas";
            btnEstadoSumas.UseVisualStyleBackColor = false;
            // 
            // pnlCamposCheque
            // 
            pnlCamposCheque.Controls.Add(lblBeneficiario);
            pnlCamposCheque.Controls.Add(txtBeneficiario);
            pnlCamposCheque.Controls.Add(lblMonto);
            pnlCamposCheque.Controls.Add(txtMonto);
            pnlCamposCheque.Controls.Add(lblConceptoCheque);
            pnlCamposCheque.Controls.Add(txtConceptoCheque);
            pnlCamposCheque.Controls.Add(lblNumeroCheque);
            pnlCamposCheque.Controls.Add(txtNumeroCheque);
            pnlCamposCheque.Controls.Add(lblRFC);
            pnlCamposCheque.Controls.Add(txtRFC);
            pnlCamposCheque.Controls.Add(lblFolios);
            pnlCamposCheque.Controls.Add(txtFolios);
            pnlCamposCheque.Controls.Add(chkVarios);
            pnlCamposCheque.Controls.Add(btnImprimir);
            pnlCamposCheque.Dock = DockStyle.Top;
            pnlCamposCheque.Location = new Point(0, 93);
            pnlCamposCheque.Margin = new Padding(4);
            pnlCamposCheque.Name = "pnlCamposCheque";
            pnlCamposCheque.Size = new Size(1050, 173);
            pnlCamposCheque.TabIndex = 2;
            pnlCamposCheque.Visible = false;
            // 
            // lblBeneficiario
            // 
            lblBeneficiario.AutoSize = true;
            lblBeneficiario.Location = new Point(11, 11);
            lblBeneficiario.Margin = new Padding(4, 0, 4, 0);
            lblBeneficiario.Name = "lblBeneficiario";
            lblBeneficiario.Size = new Size(72, 15);
            lblBeneficiario.TabIndex = 0;
            lblBeneficiario.Text = "Beneficiario:";
            // 
            // txtBeneficiario
            // 
            txtBeneficiario.Location = new Point(98, 11);
            txtBeneficiario.Margin = new Padding(4);
            txtBeneficiario.Name = "txtBeneficiario";
            txtBeneficiario.Size = new Size(232, 23);
            txtBeneficiario.TabIndex = 1;
            // 
            // lblMonto
            // 
            lblMonto.AutoSize = true;
            lblMonto.Location = new Point(11, 40);
            lblMonto.Margin = new Padding(4, 0, 4, 0);
            lblMonto.Name = "lblMonto";
            lblMonto.Size = new Size(46, 15);
            lblMonto.TabIndex = 2;
            lblMonto.Text = "Monto:";
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(98, 40);
            txtMonto.Margin = new Padding(4);
            txtMonto.Name = "txtMonto";
            txtMonto.Size = new Size(116, 23);
            txtMonto.TabIndex = 3;
            // 
            // lblConceptoCheque
            // 
            lblConceptoCheque.AutoSize = true;
            lblConceptoCheque.Location = new Point(350, 11);
            lblConceptoCheque.Margin = new Padding(4, 0, 4, 0);
            lblConceptoCheque.Name = "lblConceptoCheque";
            lblConceptoCheque.Size = new Size(62, 15);
            lblConceptoCheque.TabIndex = 4;
            lblConceptoCheque.Text = "Concepto:";
            // 
            // txtConceptoCheque
            // 
            txtConceptoCheque.Location = new Point(430, 11);
            txtConceptoCheque.Margin = new Padding(4);
            txtConceptoCheque.Name = "txtConceptoCheque";
            txtConceptoCheque.Size = new Size(232, 23);
            txtConceptoCheque.TabIndex = 5;
            // 
            // lblNumeroCheque
            // 
            lblNumeroCheque.AutoSize = true;
            lblNumeroCheque.Location = new Point(350, 40);
            lblNumeroCheque.Margin = new Padding(4, 0, 4, 0);
            lblNumeroCheque.Name = "lblNumeroCheque";
            lblNumeroCheque.Size = new Size(54, 15);
            lblNumeroCheque.TabIndex = 6;
            lblNumeroCheque.Text = "Número:";
            // 
            // txtNumeroCheque
            // 
            txtNumeroCheque.Location = new Point(430, 40);
            txtNumeroCheque.Margin = new Padding(4);
            txtNumeroCheque.Name = "txtNumeroCheque";
            txtNumeroCheque.Size = new Size(116, 23);
            txtNumeroCheque.TabIndex = 7;
            // 
            // lblRFC
            // 
            lblRFC.AutoSize = true;
            lblRFC.Location = new Point(11, 69);
            lblRFC.Margin = new Padding(4, 0, 4, 0);
            lblRFC.Name = "lblRFC";
            lblRFC.Size = new Size(40, 15);
            lblRFC.TabIndex = 8;
            lblRFC.Text = "R.F.C.:";
            // 
            // txtRFC
            // 
            txtRFC.Location = new Point(98, 69);
            txtRFC.Margin = new Padding(4);
            txtRFC.Name = "txtRFC";
            txtRFC.Size = new Size(116, 23);
            txtRFC.TabIndex = 9;
            // 
            // lblFolios
            // 
            lblFolios.AutoSize = true;
            lblFolios.Location = new Point(350, 69);
            lblFolios.Margin = new Padding(4, 0, 4, 0);
            lblFolios.Name = "lblFolios";
            lblFolios.Size = new Size(57, 15);
            lblFolios.TabIndex = 10;
            lblFolios.Text = "FOLIO(S):";
            // 
            // txtFolios
            // 
            txtFolios.Location = new Point(430, 69);
            txtFolios.Margin = new Padding(4);
            txtFolios.Name = "txtFolios";
            txtFolios.Size = new Size(232, 23);
            txtFolios.TabIndex = 11;
            // 
            // btnImprimir
            // 
            btnImprimir.Location = new Point(11, 104);
            btnImprimir.Margin = new Padding(4);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(88, 26);
            btnImprimir.TabIndex = 12;
            btnImprimir.Text = "Imprimir";
            btnImprimir.UseVisualStyleBackColor = true;
            // 
            // pnlCamposPoliza
            // 
            pnlCamposPoliza.Controls.Add(lblConceptoPoliza);
            pnlCamposPoliza.Controls.Add(txtConceptoPoliza);
            pnlCamposPoliza.Controls.Add(btnAnterior);
            pnlCamposPoliza.Controls.Add(btnSiguiente);
            pnlCamposPoliza.Dock = DockStyle.Top;
            pnlCamposPoliza.Location = new Point(0, 266);
            pnlCamposPoliza.Margin = new Padding(4);
            pnlCamposPoliza.Name = "pnlCamposPoliza";
            pnlCamposPoliza.Size = new Size(1050, 58);
            pnlCamposPoliza.TabIndex = 3;
            pnlCamposPoliza.Visible = false;
            // 
            // lblConceptoPoliza
            // 
            lblConceptoPoliza.AutoSize = true;
            lblConceptoPoliza.Location = new Point(11, 11);
            lblConceptoPoliza.Margin = new Padding(4, 0, 4, 0);
            lblConceptoPoliza.Name = "lblConceptoPoliza";
            lblConceptoPoliza.Size = new Size(62, 15);
            lblConceptoPoliza.TabIndex = 0;
            lblConceptoPoliza.Text = "Concepto:";
            // 
            // txtConceptoPoliza
            // 
            txtConceptoPoliza.Location = new Point(98, 11);
            txtConceptoPoliza.Margin = new Padding(4);
            txtConceptoPoliza.Name = "txtConceptoPoliza";
            txtConceptoPoliza.Size = new Size(350, 23);
            txtConceptoPoliza.TabIndex = 1;
            // 
            // dgvMovimientos
            // 
            dgvMovimientos.AllowUserToAddRows = false;
            dgvMovimientos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMovimientos.Dock = DockStyle.Fill;
            dgvMovimientos.Location = new Point(0, 324);
            dgvMovimientos.Margin = new Padding(4);
            dgvMovimientos.Name = "dgvMovimientos";
            dgvMovimientos.RowHeadersWidth = 51;
            dgvMovimientos.Size = new Size(1050, 155);
            dgvMovimientos.TabIndex = 6;
            // 
            // pnlTotales
            // 
            pnlTotales.BackColor = Color.LightGray;
            pnlTotales.Controls.Add(lblResumenSuma);
            pnlTotales.Dock = DockStyle.Bottom;
            pnlTotales.Location = new Point(0, 479);
            pnlTotales.Margin = new Padding(4);
            pnlTotales.Name = "pnlTotales";
            pnlTotales.Size = new Size(1050, 40);
            pnlTotales.TabIndex = 7;
            // 
            // lblResumenSuma
            // 
            lblResumenSuma.AutoSize = true;
            lblResumenSuma.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblResumenSuma.Location = new Point(11, 11);
            lblResumenSuma.Margin = new Padding(4, 0, 4, 0);
            lblResumenSuma.Name = "lblResumenSuma";
            lblResumenSuma.Size = new Size(377, 16);
            lblResumenSuma.TabIndex = 0;
            lblResumenSuma.Text = "Suma Debe: 0.00 | Suma Haber: 0.00 | Diferencia: 0.00";
            // 
            // tsAuxiliares
            // 
            tsAuxiliares.Name = "tsAuxiliares";
            tsAuxiliares.Size = new Size(69, 20);
            tsAuxiliares.Text = "Auxiliares";
            tsAuxiliares.Click += tsAuxiliares_Click;
            // 
            // FormCaptura
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1050, 519);
            Controls.Add(dgvMovimientos);
            Controls.Add(pnlTotales);
            Controls.Add(pnlCamposPoliza);
            Controls.Add(pnlCamposCheque);
            Controls.Add(pnlCabeceraComun);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Margin = new Padding(4);
            Name = "FormCaptura";
            Text = "SACMAG DE MEXICO SA CV - Captura de Pólizas";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            pnlCabeceraComun.ResumeLayout(false);
            pnlCabeceraComun.PerformLayout();
            pnlCamposCheque.ResumeLayout(false);
            pnlCamposCheque.PerformLayout();
            pnlCamposPoliza.ResumeLayout(false);
            pnlCamposPoliza.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).EndInit();
            pnlTotales.ResumeLayout(false);
            pnlTotales.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuCaptura;
        private System.Windows.Forms.ToolStripMenuItem mnuCapturaPoliza;
        private System.Windows.Forms.ToolStripMenuItem mnuCapturaCheque;
        private System.Windows.Forms.Panel pnlCabeceraComun;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lblPoliza;
        private System.Windows.Forms.TextBox txtPolizaNo;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnEstadoSumas;
        private System.Windows.Forms.Panel pnlTotales;
        private System.Windows.Forms.Label lblResumenSuma;
        private System.Windows.Forms.Panel pnlCamposCheque;
        private System.Windows.Forms.Label lblBeneficiario;
        private System.Windows.Forms.TextBox txtBeneficiario;
        private System.Windows.Forms.Label lblMonto;
        private System.Windows.Forms.TextBox txtMonto;
        private System.Windows.Forms.Label lblConceptoCheque;
        private System.Windows.Forms.TextBox txtConceptoCheque;
        private System.Windows.Forms.Label lblNumeroCheque;
        private System.Windows.Forms.TextBox txtNumeroCheque;
        private System.Windows.Forms.Label lblRFC;
        private System.Windows.Forms.TextBox txtRFC;
        private System.Windows.Forms.Label lblFolios;
        private System.Windows.Forms.TextBox txtFolios;
        private System.Windows.Forms.Button btnImprimir;
        private System.Windows.Forms.Panel pnlCamposPoliza;
        private System.Windows.Forms.Label lblConceptoPoliza;
        private System.Windows.Forms.TextBox txtConceptoPoliza;
        private System.Windows.Forms.DataGridView dgvMovimientos;
        private System.Windows.Forms.ToolStripMenuItem mnuVer;
        private System.Windows.Forms.ToolStripMenuItem mnuVerCuentas;
        private System.Windows.Forms.ToolStripMenuItem mnuVerSubcuentas;
        private System.Windows.Forms.ToolStripMenuItem mnuVerPolizas;
        private System.Windows.Forms.ToolStripMenuItem mnuVerCheques;
        private System.Windows.Forms.ToolStripMenuItem mnuVerEstadosFinancieros;
        private System.Windows.Forms.Button btnAnterior;
        private System.Windows.Forms.Button btnSiguiente;
        private System.Windows.Forms.CheckBox chkVarios;
        private ToolStripMenuItem tsAuxiliares;
    }
}