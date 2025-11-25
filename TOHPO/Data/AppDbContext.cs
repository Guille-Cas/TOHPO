using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto_Proveedor>()
                .HasKey(ac => new { ac.Codigo_Producto, ac.Id_Proveedor });
        }
    }
}
