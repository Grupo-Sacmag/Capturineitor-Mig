using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CapturaDePolizas_2026_NET8
{
    public class FormReporteBalance : Form
    {
        private DataTable _datos;
        private DataGridView dgv;

        public FormReporteBalance(DataTable datos)
        {
            _datos = datos;
            
            this.Text = "Cg: Estados financieros";
            this.Size = new Size(1100, 750);
            this.WindowState = FormWindowState.Maximized;

            // Panel superior
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 80;
            pnlTop.BackColor = Color.FromArgb(240, 240, 240);
            pnlTop.Padding = new Padding(10);

            Label lblTitulo = new Label();
            lblTitulo.Text = "Estado de posición Financiera al 31 de diciembre de 2021"; 
            lblTitulo.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Height = 30;

            Label lblEmpresa = new Label();
            lblEmpresa.Text = "SACMAG DE MEXICO SA DE CV";
            lblEmpresa.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblEmpresa.TextAlign = ContentAlignment.MiddleCenter;
            lblEmpresa.Dock = DockStyle.Top;
            lblEmpresa.Height = 30;

            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblEmpresa);

            // DataGridView
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DefaultCellStyle.SelectionBackColor = Color.White;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Font = new Font("Arial", 7.5F);
            dgv.RowTemplate.Height = 15;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.CellFormatting += Dgv_CellFormatting;

            this.Controls.Add(dgv);
            this.Controls.Add(pnlTop);

            dgv.DataBindingComplete += (s, ev) =>
            {
                if (dgv.Columns.Count >= 4)
                {
                    dgv.Columns[0].FillWeight = 65;
                    dgv.Columns[1].FillWeight = 35;
                    dgv.Columns[2].FillWeight = 65;
                    dgv.Columns[3].FillWeight = 35;
                    
                    dgv.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgv.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            };
            
            dgv.DataSource = _datos;
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null)
            {
                string text = e.Value.ToString() ?? "";
                
                if (text == "A C T I V O" || text == "P A S I V O" || text == "HABER" || text == "SOCIAL")
                {
                    e.CellStyle.ForeColor = Color.Blue;
                    e.CellStyle.Font = new Font(e.CellStyle.Font ?? dgv.Font, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else if (text.Equals("Circulante", StringComparison.OrdinalIgnoreCase) || 
                         text.Equals("Fijo", StringComparison.OrdinalIgnoreCase) || 
                         text.Equals("Diferido", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.Blue;
                    e.CellStyle.Font = new Font(e.CellStyle.Font ?? dgv.Font, FontStyle.Italic | FontStyle.Underline | FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else if (text.StartsWith("Suma "))
                {
                    e.CellStyle.ForeColor = Color.Blue;
                    e.CellStyle.Font = new Font(e.CellStyle.Font ?? dgv.Font, FontStyle.Bold);
                }
                else if (text == "CUENTAS DE ORDEN")
                {
                    e.CellStyle.ForeColor = Color.Blue;
                    e.CellStyle.Font = new Font(e.CellStyle.Font ?? dgv.Font, FontStyle.Bold);
                    
                    if (e.ColumnIndex == 0)
                    {
                        e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
            }
        }
    }
}
