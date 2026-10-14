using System.Drawing;
using System.Windows.Forms;

namespace CapturaDePolizas_2026_NET8
{
    partial class FormReporteBalance
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlTop;
        private Label lblTitulo;
        private Panel pnlGridContainer;
        private DataGridView dgv;

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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            pnlTop = new Panel();
            lblTitulo = new Label();
            lblEmpresa = new Label();
            pnlGridContainer = new Panel();
            dgv = new DataGridView();
            menuStrip1 = new MenuStrip();
            tsImprimir = new ToolStripMenuItem();
            PrintDoc = new System.Drawing.Printing.PrintDocument();
            pnlTop.SuspendLayout();
            pnlGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(240, 240, 240);
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblEmpresa);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 24);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(10);
            pnlTop.Size = new Size(1100, 80);
            pnlTop.TabIndex = 1;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Font = new Font("Times New Roman", 14F, FontStyle.Bold);
            lblTitulo.Location = new Point(10, 40);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(1080, 30);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Estado de posición Financiera al 31 de diciembre de 2021";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblEmpresa
            // 
            lblEmpresa.Dock = DockStyle.Top;
            lblEmpresa.Font = new Font("Times New Roman", 14F, FontStyle.Bold);
            lblEmpresa.Location = new Point(10, 10);
            lblEmpresa.Name = "lblEmpresa";
            lblEmpresa.Size = new Size(1080, 30);
            lblEmpresa.TabIndex = 1;
            lblEmpresa.Text = "SACMAG DE MEXICO SA DE CV";
            lblEmpresa.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlGridContainer
            // 
            pnlGridContainer.BackColor = Color.White;
            pnlGridContainer.Controls.Add(dgv);
            pnlGridContainer.Dock = DockStyle.Fill;
            pnlGridContainer.Location = new Point(0, 104);
            pnlGridContainer.Name = "pnlGridContainer";
            pnlGridContainer.Padding = new Padding(35, 0, 35, 10);
            pnlGridContainer.Size = new Size(1100, 646);
            pnlGridContainer.TabIndex = 3;
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv.ColumnHeadersVisible = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Arial", 7.5F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgv.DefaultCellStyle = dataGridViewCellStyle1;
            dgv.Dock = DockStyle.Fill;
            dgv.Location = new Point(35, 0);
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 22;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.Size = new Size(1030, 636);
            dgv.TabIndex = 0;
            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.CellPainting += Dgv_CellPainting;
            dgv.RowPostPaint += Dgv_RowPostPaint;
            dgv.DataBindingComplete += Dgv_DataBindingComplete;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { tsImprimir });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1100, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // tsImprimir
            // 
            tsImprimir.Name = "tsImprimir";
            tsImprimir.Size = new Size(65, 20);
            tsImprimir.Text = "Imprimir";
            tsImprimir.Click += tsImprimir_Click;
            // 
            // PrintDoc
            // 
            PrintDoc.PrintPage += PrintDoc_PrintPage;
            // 
            // FormReporteBalance
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 750);
            Controls.Add(pnlGridContainer);
            Controls.Add(pnlTop);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormReporteBalance";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cg: Estados financieros";
            WindowState = FormWindowState.Normal;
            pnlTop.ResumeLayout(false);
            pnlGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblEmpresa;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem tsImprimir;
        private System.Drawing.Printing.PrintDocument PrintDoc;
    }
}