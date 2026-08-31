using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Entities
{
    public class Cuenta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoBruto { get; set; }
        public int NumSubcuentas { get; set; }
        public ICollection<Subcuenta> Subcuentas { get; set; } = new List<Subcuenta>();
    }
}
