using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Configuration
{
    public static class ConnectionConfig
    {
        public static string DefaultConnection ="Server=.\\MSSQLSERVER01;Database=sacmag_empresa_1;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;";
    }
}
