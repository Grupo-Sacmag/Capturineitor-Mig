using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CapturaDePolizas_2026_NET8
{
    partial class FormDetallePoliza
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
            menuStrip1 = new MenuStrip();
            mnuArchivo = new ToolStripMenuItem();
            mnuArchivoCerrar = new ToolStripMenuItem();
            mnuEdicion = new ToolStripMenuItem();
            mnuImprimir = new ToolStripMenuItem();
            pnlHeader = new Panel();
            lblYellowBar = new Label();
            dgvMovimientos = new DataGridView();
            menuStrip1.SuspendLayout();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuArchivo, mnuEdicion, mnuImprimir });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 2, 0, 2);
            menuStrip1.Size = new Size(1190, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // mnuArchivo
            // 
            mnuArchivo.DropDownItems.AddRange(new ToolStripItem[] { mnuArchivoCerrar });
            mnuArchivo.Name = "mnuArchivo";
            mnuArchivo.Size = new Size(60, 20);
            mnuArchivo.Text = "Archivo";
            // 
            // mnuArchivoCerrar
            // 
            mnuArchivoCerrar.Name = "mnuArchivoCerrar";
            mnuArchivoCerrar.Size = new Size(180, 22);
            mnuArchivoCerrar.Text = "Cerrar";
            // 
            // mnuEdicion
            // 
            mnuEdicion.Name = "mnuEdicion";
            mnuEdicion.Size = new Size(58, 20);
            mnuEdicion.Text = "Edicion";
            // 
            // mnuImprimir
            // 
            mnuImprimir.Name = "mnuImprimir";
            mnuImprimir.Size = new Size(65, 20);
            mnuImprimir.Text = "Imprimir";
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = SystemColors.Control;
            pnlHeader.Controls.Add(lblYellowBar);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 24);
            pnlHeader.Margin = new Padding(4, 3, 4, 3);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1190, 58);
            pnlHeader.TabIndex = 1;
            // 
            // lblYellowBar
            // 
            lblYellowBar.BackColor = Color.Yellow;
            lblYellowBar.BorderStyle = BorderStyle.FixedSingle;
            lblYellowBar.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            lblYellowBar.ForeColor = Color.Black;
            lblYellowBar.Location = new Point(12, 12);
            lblYellowBar.Margin = new Padding(4, 0, 4, 0);
            lblYellowBar.Name = "lblYellowBar";
            lblYellowBar.Size = new Size(1149, 34);
            lblYellowBar.TabIndex = 0;
            lblYellowBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvMovimientos
            // 
            dgvMovimientos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMovimientos.Dock = DockStyle.Fill;
            dgvMovimientos.Location = new Point(0, 82);
            dgvMovimientos.Margin = new Padding(4, 3, 4, 3);
            dgvMovimientos.Name = "dgvMovimientos";
            dgvMovimientos.Size = new Size(1190, 511);
            dgvMovimientos.TabIndex = 2;
            // 
            // FormDetallePoliza
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1190, 593);
            Controls.Add(dgvMovimientos);
            Controls.Add(pnlHeader);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormDetallePoliza";
            Text = "Detalle de Poliza";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuArchivo;
        private System.Windows.Forms.ToolStripMenuItem mnuArchivoCerrar;
        private System.Windows.Forms.ToolStripMenuItem mnuEdicion;
        private System.Windows.Forms.ToolStripMenuItem mnuImprimir;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblYellowBar;
        private System.Windows.Forms.DataGridView dgvMovimientos;
    }
}