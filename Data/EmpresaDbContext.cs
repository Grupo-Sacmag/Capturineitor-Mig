using CapturaDePolizas_2026_NET8.Configuration;
using CapturaDePolizas_2026_NET8.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapturaDePolizas_2026_NET8.Context
{
    public class EmpresaDbContext : DbContext
    {
        private readonly string _connectionString;

        public EmpresaDbContext() : this(ConnectionConfig.DefaultConnection)
        {

        }

        public EmpresaDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Cuenta> Cuentas => Set<Cuenta>();
        public DbSet<Subcuenta> Subcuentas => Set<Subcuenta>();
        public DbSet<Poliza> Polizas => Set<Poliza>();
        public DbSet<Movimiento> Movimientos => Set<Movimiento>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigurarCuenta(modelBuilder);
            ConfigurarSubcuenta(modelBuilder);
            ConfigurarPoliza(modelBuilder);
            ConfigurarMovimiento(modelBuilder);
        }

        private static void ConfigurarCuenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cuenta>(entity =>
            {
                entity.ToTable("cuentas");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
                entity.Property(c => c.Nombre).HasColumnName("nombre");
                entity.Property(c => c.MontoBruto).HasColumnName("monto_bruto");
                entity.Property(c => c.NumSubcuentas).HasColumnName("num_subcuentas");
            });
        }

        private static void ConfigurarSubcuenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subcuenta>(entity =>
            {
                entity.ToTable("subcuentas");
                entity.HasKey(s => new { s.Id, s.CuentaId });
                entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
                entity.Property(s => s.CuentaId).HasColumnName("cuenta_id");
                entity.Property(s => s.Nombre).HasColumnName("nombre");
                entity.Property(s => s.ImporteNeto).HasColumnName("importe_neto");
                entity.Property(s => s.Activa).HasColumnName("activa");
                entity.HasOne(s => s.Cuenta).WithMany(c => c.Subcuentas).HasForeignKey(s => s.CuentaId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurarPoliza(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Poliza>(entity =>
            {
                entity.ToTable("polizas");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.Fecha).HasColumnName("fecha");
                entity.Property(p => p.Folio).HasColumnName("folio");
                entity.Property(p => p.Concepto).HasColumnName("concepto");
                entity.Property(p => p.EsCostos).HasColumnName("es_costos");
                entity.Property(p => p.TipoCaptura).HasColumnName("tipo_captura");
                entity.Property(p => p.TotalDebe).HasColumnName("total_debe");
                entity.Property(p => p.TotalHaber).HasColumnName("total_haber");
                entity.Property(p => p.Beneficiario).HasColumnName("beneficiario");
                entity.Property(p => p.MontoCheque).HasColumnName("monto_cheque");
                entity.Property(p => p.NumeroCheque).HasColumnName("numero_cheque");
                entity.Property(p => p.RFC).HasColumnName("rfc");
                entity.Property(p => p.FoliosFiscales).HasColumnName("folios_fiscales");
                entity.Property(p => p.RequiereImpresion).HasColumnName("requiere_impresion");
                entity.Property(p => p.CreadoEn).HasColumnName("creado_en");
            });
        }

        private static void ConfigurarMovimiento(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movimiento>(entity =>
            {
                entity.ToTable("movimientos");

                entity.HasKey(m => m.Id);

                entity.Property(m => m.Id).HasColumnName("id");
                entity.Property(m => m.PolizaId).HasColumnName("poliza_id");
                entity.Property(m => m.CuentaId).HasColumnName("cuenta_id");
                entity.Property(m => m.SubcuentaId).HasColumnName("subcuenta_id");
                entity.Property(m => m.FolioFiscal).HasColumnName("folio_fiscal");
                entity.Property(m => m.Parcial).HasColumnName("parcial");
                entity.Property(m => m.Debe).HasColumnName("debe");
                entity.Property(m => m.Haber).HasColumnName("haber");
                entity.Property(m => m.Redaccion).HasColumnName("redaccion");
                entity.Property(m => m.Beneficiario).HasColumnName("beneficiario");
                entity.Property(m => m.PerteneceConcepto).HasColumnName("pertenece_concepto");
                entity.Property(m => m.CreadoEn).HasColumnName("creado_en");

                entity.HasOne(m => m.Poliza).WithMany(p => p.Movimientos).HasForeignKey(m => m.PolizaId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
