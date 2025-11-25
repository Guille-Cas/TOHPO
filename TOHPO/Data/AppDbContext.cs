using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Modelos existentes
        public DbSet<Agente_Ventas> Agente_Ventas { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Impuesto> Impuesto { get; set; }
        public DbSet<Materia_Prima> Materia_Prima { get; set; }
        public DbSet<Metodo_Pago> Metodo_Pago { get; set; }
        public DbSet<Motivo_Recordatorio> Motivo_Recordatorio { get; set; }
        public DbSet<Presentacion> Presentacion { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Producto_Proveedor> Producto_Proveedor { get; set; }
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Receta> Receta { get; set; }
        public DbSet<Recordatorio> Recordatorio { get; set; }

        // Nuevos modelos para producción
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<Pedido_Detalle> Pedido_Detalle { get; set; }
        public DbSet<Venta> Venta { get; set; }
        public DbSet<Detalle_Venta> Detalle_Venta { get; set; }
        public DbSet<Venta_Metodo_Pago> Venta_Metodo_Pago { get; set; }
        public DbSet<Compra> Compra { get; set; }
        public DbSet<Compra_Detalle> Compra_Detalle { get; set; }
        public DbSet<Compra_Metodo_Pago> Compra_Metodo_Pago { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<Detalle_Inventario> Detalle_Inventario { get; set; }
        public DbSet<Movimiento_Inventario> Movimiento_Inventario { get; set; }
        public DbSet<Produccion> Produccion { get; set; }
        public DbSet<Produccion_Detalle> Produccion_Detalle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de claves compuestas existentes
            modelBuilder.Entity<Producto_Proveedor>()
                .HasKey(ac => new { ac.Codigo_Producto, ac.Id_Proveedor });

            // Configuraciones de precisión decimal
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Abono)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Saldo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido_Detalle>()
                .Property(p => p.Precio_Unitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Costo_Total_Gravado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Iva)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            // Configuraciones de relaciones
            modelBuilder.Entity<Pedido_Detalle>()
                .HasOne(pd => pd.Pedido)
                .WithMany(p => p.Pedido_Detalles)
                .HasForeignKey(pd => pd.Id_Pedido)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Detalle_Venta>()
                .HasOne(dv => dv.Venta)
                .WithMany(v => v.Detalle_Ventas)
                .HasForeignKey(dv => dv.Id_Venta)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Compra_Detalle>()
                .HasOne(cd => cd.Compra)
                .WithMany(c => c.Compra_Detalles)
                .HasForeignKey(cd => cd.Id_Compra)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Produccion_Detalle>()
                .HasOne(pd => pd.Produccion)
                .WithMany(p => p.Produccion_Detalles)
                .HasForeignKey(pd => pd.Id_Produccion)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para optimización
            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.Fecha)
                .HasDatabaseName("IX_Venta_Fecha");

            modelBuilder.Entity<Compra>()
                .HasIndex(c => c.Fecha)
                .HasDatabaseName("IX_Compra_Fecha");

            modelBuilder.Entity<Produccion>()
                .HasIndex(p => p.Fecha)
                .HasDatabaseName("IX_Produccion_Fecha");
        }
    }
}
