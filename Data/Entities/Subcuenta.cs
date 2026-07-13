using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Entities
{
    public class Subcuenta
    {
        public int Id { get; set; }
        public int CuentaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal ImporteNeto { get; set; }
        public bool Activa { get; set; }
        public Cuenta Cuenta { get; set; } = null!;
    }
}
