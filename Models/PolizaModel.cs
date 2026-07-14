using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Models
{
    public class PolizaModel
    {
        public DateTime Fecha { get; set; }
        public int Folio { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public bool EsCostos { get; set; }
        public byte TipoCaptura { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public string Beneficiario { get; set; } = string.Empty;
        public decimal MontoCheque { get; set; }
        public string NumeroCheque { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string FoliosFiscales { get; set; } = string.Empty;
        public bool RequiereImpresion { get; set; }
        public List<MovimientoModel> Movimientos { get; set; } = new();
    }
}
