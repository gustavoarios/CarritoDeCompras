using System.Collections.Generic;
using CarritoC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarritoC.Data
{
    public class CarritoContext : IdentityDbContext<IdentityUser<int>,IdentityRole<int>, int>
    {
        public CarritoContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CarritoItem>().HasKey(ci => new {ci.CarritoId, ci.ProductoId });

            modelBuilder.Entity<CarritoItem>()
                .HasOne(ci => ci.Carrito)
                .WithMany(c => c.CarritoItems)
                .HasForeignKey(ci => ci.CarritoId)
                ;

            modelBuilder.Entity<CarritoItem>()
               .HasOne(ci => ci.Producto)
               .WithMany(p => p.CarritoItems)
               .HasForeignKey(ci => ci.ProductoId);

            modelBuilder.Entity<StockItem>().HasKey(st => new { st.ProductoId, st.SucursalId });

            modelBuilder.Entity<StockItem>()
                .HasOne(st => st.Producto)
                .WithMany(p => p.StockItems)
                .HasForeignKey(st => st.ProductoId);

            modelBuilder.Entity<StockItem>()
               .HasOne(st => st.Sucursal)
               .WithMany(p => p.StockItems)
               .HasForeignKey(st => st.SucursalId);


            modelBuilder.Entity<Sucursal>()
            .HasIndex(s => s.Nombre)
             .IsUnique();

            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.Nombre)
                .IsUnique();



            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.DNI)
                .IsUnique();


            modelBuilder.Entity<Empleado>()
                .HasIndex(c => c.Legajo)
                .IsUnique();




            modelBuilder.Entity<IdentityUser<int>>().ToTable("Persona");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("PerasonasRoles");


        }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Sucursal> Sucursal { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<CarritoC.Models.Carrito> Carritos { get; set; }
        public DbSet<CarritoC.Models.StockItem> StockItem { get; set; }
        public DbSet<CarritoC.Models.CarritoItem> CarritoItem { get; set; }

        public DbSet<Rol> MisRoles { get; set; }
    }
}
