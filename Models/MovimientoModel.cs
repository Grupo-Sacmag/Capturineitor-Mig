using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Models
{
    public class MovimientoModel
    {
        public int CuentaId { get; set; }
        public int? SubcuentaId { get; set; }
        public string FolioFiscal { get; set; } = string.Empty;
        public decimal Parcial { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public string Redaccion { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public bool PerteneceConcepto { get; set; }
    }
}
