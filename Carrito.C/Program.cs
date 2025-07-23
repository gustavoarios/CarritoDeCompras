using CarritoC.Data;
using CarritoC.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace CarritoC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<CarritoContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CarritoContext"))
            );

            builder.Services.AddIdentity<Persona, Rol>().AddEntityFrameworkStores<CarritoContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/IniciarSesion";
                options.AccessDeniedPath = "/Account/AccesoDenegado";
            });

            var app = builder.Build();

            // ⚠️ Asegurar que la base esté creada y migrada
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CarritoContext>();
                context.Database.Migrate(); // Aplica migraciones pendientes o crea la base si no existe
                
            }

            // Configuración regional para coma como separador decimal
            var cultureInfo = new CultureInfo("es-AR");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

        }

    }
}