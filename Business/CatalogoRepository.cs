using CapturaDePolizas_2026_NET8.Context;
using CapturaDePolizas_2026_NET8.Entities;
using CapturaDePolizas_2026_NET8.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Globalization;

namespace CapturaDePolizas_2026_NET8.Repositories
{
    public class CatalogoRepository : ICatalogoRepository
    {
        public bool ValidarCuenta(string cuentaId, out string nombreCuenta)
        {
            nombreCuenta = string.Empty;

            if (!int.TryParse(cuentaId, out int id))
                return false;

            using var db = new EmpresaDbContext();

            var cuenta = db.Cuentas.AsNoTracking().FirstOrDefault(c => c.Id == id);

            if (cuenta is null)
                return false;

            nombreCuenta = cuenta.Nombre.ToUpper();
            return true;
        }

        public bool ValidarSubcuenta(string subcuentaId, string cuentaPadreId, out string nombreSubcuenta)
        {
            nombreSubcuenta = string.Empty;

            if (!int.TryParse(subcuentaId, out int subId))
                return false;

            if (!int.TryParse(cuentaPadreId, out int cuentaId))
                return false;

            using var db = new EmpresaDbContext();

            var subcuenta = db.Subcuentas.AsNoTracking().FirstOrDefault(s => s.Id == subId && s.CuentaId == cuentaId);

            if (subcuenta is null)
                return false;

            nombreSubcuenta = subcuenta.Nombre.ToUpper();
            return true;
        }

        public int ObtenerSiguienteFolio()
        {
            using var db = new EmpresaDbContext();

            int maxFolio = db.Polizas.Select(p => (int?)p.Folio).Max() ?? 0;

            return maxFolio + 1;
        }

        public string ObtenerSiguienteNumeroCheque()
        {
            using var db = new EmpresaDbContext();

            var numerosCheque = db.Polizas.AsNoTracking().Where(p => p.TipoCaptura == 0 && p.NumeroCheque != null && p.NumeroCheque != string.Empty)
                .Select(p => p.NumeroCheque).ToList();

            int maxVal = 0;

            foreach (string numeroCheque in numerosCheque)
            {
                if (int.TryParse(numeroCheque, out int numero) && numero > maxVal)
                {
                    maxVal = numero;
                }
            }

            return (maxVal + 1).ToString();
        }

        public bool GuardarPoliza(PolizaModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            using var db = new EmpresaDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                var poliza = CrearPolizaEntity(model);

                db.Polizas.Add(poliza);
                db.SaveChanges();

                foreach (MovimientoModel movimientoModel in model.Movimientos)
                {
                    var movimiento = CrearMovimientoEntity(movimientoModel, poliza.Id);
                    db.Movimientos.Add(movimiento);
                }

                db.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public DataTable ObtenerTablaCuentas()
        {
            DataTable tabla = CrearTablaCuentas();

            using var db = new EmpresaDbContext();

            var cuentas = db.Cuentas.AsNoTracking().OrderBy(c => c.Id).ToList();

            foreach (Cuenta cuenta in cuentas)
            {
                tabla.Rows.Add(
                    cuenta.Id,
                    cuenta.Nombre.ToUpper(),
                    cuenta.MontoBruto,
                    cuenta.NumSubcuentas
                );
            }

            return tabla;
        }

        public DataTable ObtenerTablaSubcuentas()
        {
            DataTable tabla = CrearTablaSubcuentas();

            using var db = new EmpresaDbContext();

            var subcuentas = db.Subcuentas.AsNoTracking().Where(s => s.Activa).OrderBy(s => s.CuentaId).ThenBy(s => s.Id).ToList();

            foreach (Subcuenta subcuenta in subcuentas)
            {
                tabla.Rows.Add(
                    subcuenta.CuentaId,
                    subcuenta.Id,
                    subcuenta.Nombre.ToUpper(),
                    subcuenta.ImporteNeto
                );
            }

            return tabla;
        }

        public DataTable ObtenerCuentasBusqueda()
        {
            DataTable tabla = CrearTablaBusqueda();

            using var db = new EmpresaDbContext();

            var cuentas = db.Cuentas.AsNoTracking().OrderBy(c => c.Id).ToList();

            foreach (Cuenta cuenta in cuentas)
            {
                tabla.Rows.Add(
                    cuenta.Id,
                    cuenta.Nombre.ToUpper(),
                    cuenta.MontoBruto
                );
            }

            return tabla;
        }

        public DataTable ObtenerSubcuentasBusqueda(int cuentaId)
        {
            DataTable tabla = CrearTablaBusqueda();

            using var db = new EmpresaDbContext();

            var subcuentas = db.Subcuentas.AsNoTracking().Where(s => s.CuentaId == cuentaId && s.Activa).OrderBy(s => s.Id).ToList();

            foreach (Subcuenta subcuenta in subcuentas)
            {
                tabla.Rows.Add(
                    subcuenta.Id,
                    subcuenta.Nombre.ToUpper(),
                    subcuenta.ImporteNeto
                );
            }

            return tabla;
        }

        public DataTable ObtenerTablaPolizas()
        {
            DataTable tabla = CrearTablaPolizas();

            using var db = new EmpresaDbContext();

            var polizas = db.Polizas.AsNoTracking().OrderBy(p => p.Folio).ToList();

            foreach (Poliza poliza in polizas)
            {
                tabla.Rows.Add(
                    poliza.Id,
                    poliza.Fecha,
                    poliza.Folio,
                    poliza.Concepto.ToUpper(),
                    poliza.EsCostos,
                    poliza.TipoCaptura,
                    poliza.TotalDebe,
                    poliza.TotalHaber,
                    poliza.Beneficiario.ToUpper(),
                    poliza.MontoCheque,
                    poliza.NumeroCheque.ToUpper(),
                    poliza.RFC.ToUpper(),
                    poliza.FoliosFiscales.ToUpper(),
                    poliza.RequiereImpresion,
                    poliza.CreadoEn
                );
            }

            return tabla;
        }

        public DataTable ObtenerMovimientosPoliza(int polizaId)
        {
            DataTable tabla = CrearTablaMovimientosPoliza();

            using var db = new EmpresaDbContext();

            var movimientos = (
                from movimiento in db.Movimientos.AsNoTracking()
                join cuenta in db.Cuentas.AsNoTracking()
                    on movimiento.CuentaId equals cuenta.Id into cuentasJoin
                from cuenta in cuentasJoin.DefaultIfEmpty()

                join subcuenta in db.Subcuentas.AsNoTracking()
                    on new
                    {
                        CuentaId = movimiento.CuentaId,
                        Id = movimiento.SubcuentaId ?? -1
                    }
                    equals new
                    {
                        CuentaId = subcuenta.CuentaId,
                        Id = subcuenta.Id
                    }
                    into subcuentasJoin
                from subcuenta in subcuentasJoin.DefaultIfEmpty()

                where movimiento.PolizaId == polizaId
                orderby movimiento.Id

                select new
                {
                    movimiento.CuentaId,
                    movimiento.SubcuentaId,
                    NombreCuenta = cuenta != null ? cuenta.Nombre : string.Empty,
                    NombreSubcuenta = subcuenta != null ? subcuenta.Nombre : string.Empty,
                    movimiento.Parcial,
                    movimiento.Debe,
                    movimiento.Haber,
                    movimiento.Redaccion,
                    movimiento.FolioFiscal
                }
            ).ToList();

            foreach (var movimiento in movimientos)
            {
                tabla.Rows.Add(
                    movimiento.CuentaId,
                    movimiento.SubcuentaId.HasValue ? movimiento.SubcuentaId.Value : DBNull.Value,
                    movimiento.NombreCuenta.ToUpper(),
                    movimiento.NombreSubcuenta.ToUpper(),
                    movimiento.Parcial,
                    movimiento.Debe,
                    movimiento.Haber,
                    movimiento.Redaccion.ToUpper(),
                    movimiento.FolioFiscal.ToUpper()
                );
            }

            return tabla;
        }

        public DataTable ObtenerTablaVacia()
        {
            DataTable tabla = new();

            tabla.Columns.Add("Info", typeof(string));
            tabla.Rows.Add("Sin información disponible / Módulo en desarrollo");

            return tabla;
        }

        public DataTable ObtenerEstadosFinancieros(bool incluirCuentasOrden)
        {
            DataTable tabla = new();
            tabla.Columns.Add("Activo", typeof(string));
            tabla.Columns.Add("Monto Activo", typeof(string));
            tabla.Columns.Add("Pasivo y Capital", typeof(string));
            tabla.Columns.Add("Monto Pasivo", typeof(string));

            using var db = new EmpresaDbContext();
            var cuentas = db.Cuentas.AsNoTracking().ToList();

            var activos = cuentas.Where(c => c.TipoCuenta?.ToUpper() == "ACTIVO" && c.MontoBruto != 0).ToList();
            var pasivos = cuentas.Where(c => c.TipoCuenta?.ToUpper() == "PASIVO" && c.MontoBruto != 0).ToList();
            var capital = cuentas.Where(c => c.TipoCuenta?.ToUpper() == "CAPITAL" && c.MontoBruto != 0).ToList();
            var orden = cuentas.Where(c => (c.TipoCuenta?.ToUpper() == "CUENTAS DE ORDEN" || c.TipoCuenta?.ToUpper() == "ORDEN") && c.MontoBruto != 0).ToList();

            string[] clasificaciones = new[] { "CIRCULANTE", "FIJO", "DIFERIDO" };

            var lineasActivo = new List<(string Concepto, string Monto)>();
            lineasActivo.Add(("A C T I V O", ""));
            lineasActivo.AddRange(GenerarLineasLado(activos, clasificaciones, out decimal totalActivo));
            lineasActivo.Add(("Suma Activo", totalActivo.ToString("N2")));

            var lineasPasivoCapital = new List<(string Concepto, string Monto)>();
            lineasPasivoCapital.Add(("P A S I V O", ""));
            lineasPasivoCapital.AddRange(GenerarLineasLado(pasivos, clasificaciones, out decimal totalPasivo));
            lineasPasivoCapital.Add(("Suma Pasivo", totalPasivo.ToString("N2")));
            lineasPasivoCapital.Add(("", ""));
            lineasPasivoCapital.Add(("HABER", ""));
            lineasPasivoCapital.Add(("SOCIAL", ""));

            decimal totalCapital = 0;
            foreach (var cap in capital)
            {
                lineasPasivoCapital.Add((cap.Id + " " + cap.Nombre, cap.MontoBruto.ToString("N2")));
                totalCapital += cap.MontoBruto;
            }
            lineasPasivoCapital.Add(("Suma Pasivo y Haber Soc", (totalPasivo + totalCapital).ToString("N2")));

            int maxLines = Math.Max(lineasActivo.Count, lineasPasivoCapital.Count);
            for (int i = 0; i < maxLines; i++)
            {
                var izq = i < lineasActivo.Count ? lineasActivo[i] : (Concepto: "", Monto: "");
                var der = i < lineasPasivoCapital.Count ? lineasPasivoCapital[i] : (Concepto: "", Monto: "");
                tabla.Rows.Add(izq.Concepto, izq.Monto, der.Concepto, der.Monto);
            }

            if (incluirCuentasOrden)
            {
                tabla.Rows.Add("", "", "", "");
                tabla.Rows.Add("CUENTAS DE ORDEN", "", "", "");
                foreach (var ord in orden)
                {
                    tabla.Rows.Add(ord.Id + " " + ord.Nombre, ord.MontoBruto.ToString("N2"), "", "");
                }
            }

            return tabla;
        }

        private List<(string Concepto, string Monto)> GenerarLineasLado(List<Cuenta> cuentas, string[] ordenClasificaciones, out decimal granTotal)
        {
            var lineas = new List<(string Concepto, string Monto)>();
            granTotal = 0;

            foreach (var clasif in ordenClasificaciones)
            {
                var cuentasClasif = cuentas.Where(c => c.Clasificacion?.ToUpper() == clasif).ToList();
                if (cuentasClasif.Any())
                {
                    lineas.Add((CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clasif.ToLower()), ""));
                    decimal subtotal = 0;
                    foreach (var c in cuentasClasif)
                    {
                        lineas.Add((c.Id + " " + c.Nombre, c.MontoBruto.ToString("N2")));
                        subtotal += c.MontoBruto;
                    }
                    granTotal += subtotal;
                }
            }
            return lineas;
        }

        private static Poliza CrearPolizaEntity(PolizaModel model)
        {
            return new Poliza
            {
                Fecha = model.Fecha,
                Folio = model.Folio,
                Concepto = model.Concepto.ToUpper(),
                EsCostos = model.EsCostos,
                TipoCaptura = model.TipoCaptura,
                TotalDebe = model.TotalDebe,
                TotalHaber = model.TotalHaber,
                Beneficiario = model.Beneficiario.ToUpper(),
                MontoCheque = model.MontoCheque,
                NumeroCheque = model.NumeroCheque.ToUpper(),
                RFC = model.RFC.ToUpper(),
                FoliosFiscales = model.FoliosFiscales.ToUpper(),
                RequiereImpresion = model.RequiereImpresion,
                CreadoEn = DateTime.Now
            };
        }

        private static Movimiento CrearMovimientoEntity(MovimientoModel model, int polizaId)
        {
            return new Movimiento
            {
                PolizaId = polizaId,
                CuentaId = model.CuentaId,
                SubcuentaId = model.SubcuentaId,
                FolioFiscal = model.FolioFiscal.ToUpper(),
                Parcial = model.Parcial,
                Debe = model.Debe,
                Haber = model.Haber,
                Redaccion = model.Redaccion.ToUpper(),
                Beneficiario = model.Beneficiario.ToUpper(),
                PerteneceConcepto = model.PerteneceConcepto,
                CreadoEn = DateTime.Now
            };
        }

        private static DataTable CrearTablaCuentas()
        {
            DataTable tabla = new();

            tabla.Columns.Add("Número", typeof(int));
            tabla.Columns.Add("Nombre", typeof(string));
            tabla.Columns.Add("Monto Bruto", typeof(decimal));
            tabla.Columns.Add("Subcuentas", typeof(int));

            return tabla;
        }

        private static DataTable CrearTablaSubcuentas()
        {
            DataTable tabla = new();

            tabla.Columns.Add("Cuenta", typeof(int));
            tabla.Columns.Add("Subcuenta", typeof(int));
            tabla.Columns.Add("Nombre", typeof(string));
            tabla.Columns.Add("Importe Neto", typeof(decimal));

            return tabla;
        }

        private static DataTable CrearTablaBusqueda()
        {
            DataTable tabla = new();

            tabla.Columns.Add("Número", typeof(int));
            tabla.Columns.Add("Nombre", typeof(string));
            tabla.Columns.Add("Importe", typeof(decimal));

            return tabla;
        }

        private static DataTable CrearTablaPolizas()
        {
            DataTable tabla = new();

            tabla.Columns.Add("id", typeof(int));
            tabla.Columns.Add("fecha", typeof(DateTime));
            tabla.Columns.Add("folio", typeof(int));
            tabla.Columns.Add("concepto", typeof(string));
            tabla.Columns.Add("es_costos", typeof(bool));
            tabla.Columns.Add("tipo_captura", typeof(byte));
            tabla.Columns.Add("total_debe", typeof(decimal));
            tabla.Columns.Add("total_haber", typeof(decimal));
            tabla.Columns.Add("beneficiario", typeof(string));
            tabla.Columns.Add("monto_cheque", typeof(decimal));
            tabla.Columns.Add("numero_cheque", typeof(string));
            tabla.Columns.Add("rfc", typeof(string));
            tabla.Columns.Add("folios_fiscales", typeof(string));
            tabla.Columns.Add("requiere_impresion", typeof(bool));
            tabla.Columns.Add("creado_en", typeof(DateTime));

            return tabla;
        }

        private static DataTable CrearTablaMovimientosPoliza()
        {
            DataTable tabla = new();

            tabla.Columns.Add("cuenta_id", typeof(int));
            tabla.Columns.Add("subcuenta_id", typeof(int));
            tabla.Columns.Add("nombre_cuenta", typeof(string));
            tabla.Columns.Add("nombre_subcuenta", typeof(string));
            tabla.Columns.Add("parcial", typeof(decimal));
            tabla.Columns.Add("debe", typeof(decimal));
            tabla.Columns.Add("haber", typeof(decimal));
            tabla.Columns.Add("redaccion", typeof(string));
            tabla.Columns.Add("folio_fiscal", typeof(string));

            return tabla;
        }
    }
}