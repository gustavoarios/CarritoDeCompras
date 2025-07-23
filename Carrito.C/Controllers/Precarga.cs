using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarritoC.Data;
using CarritoC.Helpers;
using CarritoC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarritoC.Controllers
{
    public class Precarga : Controller
    {
        private readonly UserManager<Persona> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly CarritoContext _context;
        private List<string> roles = new List<string>() { "Admin", "Cliente", "Empleado", "Usuario" };
        public Precarga(UserManager<Persona> userManager, RoleManager<Rol> roleManager, CarritoContext context)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._context = context;
        }

        public IActionResult Seed()
        {
                _context.Database.EnsureDeleted();
                _context.Database.Migrate();
                CrearRoles().Wait();
                CrearClientes().Wait();
                CrearEmpleados().Wait();
                PrecargarCategoriasYMarcas();
                PrecargarProductos();
                PrecargarSucursales();
                PrecargarCarritos();
                PrecargarStockItems();
                
                return RedirectToAction("Index", "Home");
           
        }

        public async Task<IActionResult> CrearRoles()
        {
            if (!await _roleManager.RoleExistsAsync("AdministradorRol"))
                await _roleManager.CreateAsync(new Rol("AdministradorRol"));

            if (!await _roleManager.RoleExistsAsync("ClienteRol"))
                await _roleManager.CreateAsync(new Rol("ClienteRol"));

            if (!await _roleManager.RoleExistsAsync("EmpleadoRol"))
                await _roleManager.CreateAsync(new Rol("EmpleadoRol"));

            return RedirectToAction("Index", "Home", new { mensaje = "Finalizado" });
        }
        private async Task CrearEmpleados()
        {
            string emailEmpleado = "empleado1@ort.edu.ar";
            string passwordEmpleado = "Password1!";

            var userEmpleado = await _userManager.FindByEmailAsync(emailEmpleado);
            if (userEmpleado == null)
            {
                var nuevoEmpleado = new Empleado
                {
                    UserName = emailEmpleado,
                    Email = emailEmpleado,
                    EmailConfirmed = true,
                    Nombre = "Empleado",
                    Apellido = "Uno",
                    Legajo = ObtenerProximoLegajo(),
                    FechaAlta = DateTime.Now,


                };

                var result = await _userManager.CreateAsync(nuevoEmpleado, passwordEmpleado);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(nuevoEmpleado, "EmpleadoRol");
                }
            }
        }

        private async Task CrearClientes()
        {
            string emailCliente = "cliente1@ort.edu.ar";
            string passwordCliente = "Password1!";

            var usuarioAdmin = await _userManager.FindByEmailAsync(emailCliente);
            if (usuarioAdmin == null)
            {
                var nuevoCliente = new Cliente
                {
                    UserName = emailCliente,
                    Email = emailCliente,
                    EmailConfirmed = true,
                    Nombre = "Cliente",
                    Apellido = "Uno",
                    FechaAlta = DateTime.Now,
                };

                var result = await _userManager.CreateAsync(nuevoCliente, passwordCliente);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(nuevoCliente, "ClienteRol");
                }
            }
        }

        private void PrecargarCategoriasYMarcas()
        {
            if (!_context.Categorias.Any())
            {
                _context.Categorias.AddRange(
                    new Categoria { Nombre = "Remeras", Descripcion = "Prendas comodas y versatiles" },
                    new Categoria { Nombre = "Pantalones", Descripcion = "Comodos y resistentes" },
                    new Categoria { Nombre = "Camperas", Descripcion = "Abrigo contra frio y lluvia" },
                    new Categoria { Nombre = "Zapatillas", Descripcion = "Calzado deportivo y casual" },
                    new Categoria { Nombre = "Accesorios", Descripcion = "Gorros, bufandas y más" },
                    new Categoria { Nombre = "Mochilas", Descripcion = "Bolsos para todo uso" },
                    new Categoria { Nombre = "Ropa interior", Descripcion = "Comoda y funcional" },
                    new Categoria { Nombre = "Trajes de baño", Descripcion = "Para piscina y playa" }
                );
                _context.SaveChanges();
            }

            if (!_context.Marcas.Any())
            {
                _context.Marcas.AddRange(
                    new Marca { Nombre = "Nike", Descripcion = "Marca deportiva innovadora" },
                    new Marca { Nombre = "Adidas", Descripcion = "Referente mundial deportivo" },
                    new Marca { Nombre = "Puma", Descripcion = "Estilo y rendimiento" },
                    new Marca { Nombre = "Reebok", Descripcion = "Marca deportiva reconocida" },
                    new Marca { Nombre = "Under Armour", Descripcion = "Ropa y accesorios deportivos" },
                    new Marca { Nombre = "New Balance", Descripcion = "Calzado deportivo y casual" }
                );
                _context.SaveChanges();
            }
        }
        private void PrecargarProductos()
        {
            // Evitar duplicados si ya hay productos cargados
            if (_context.Productos.Any())
                return;

            // Obtener una marca y una categoría existentes
            var nike = _context.Marcas.FirstOrDefault(m => m.Nombre == "Nike");
            var adidas = _context.Marcas.FirstOrDefault(m => m.Nombre == "Adidas");
            var puma = _context.Marcas.FirstOrDefault(m => m.Nombre == "Puma");
            var reebok = _context.Marcas.FirstOrDefault(m => m.Nombre == "Reebok");
            var ua = _context.Marcas.FirstOrDefault(m => m.Nombre == "Under Armour");
            var nb = _context.Marcas.FirstOrDefault(m => m.Nombre == "New Balance");

            // Buscar categorías
            var remeras = _context.Categorias.FirstOrDefault(c => c.Nombre == "Remeras");
            var pantalones = _context.Categorias.FirstOrDefault(c => c.Nombre == "Pantalones");
            var camperas = _context.Categorias.FirstOrDefault(c => c.Nombre == "Camperas");
            var zapatillas = _context.Categorias.FirstOrDefault(c => c.Nombre == "Zapatillas");
            var accesorios = _context.Categorias.FirstOrDefault(c => c.Nombre == "Accesorios");
            var mochilas = _context.Categorias.FirstOrDefault(c => c.Nombre == "Mochilas");
            var ropaInterior = _context.Categorias.FirstOrDefault(c => c.Nombre == "Ropa interior");
            var trajesDeBaño = _context.Categorias.FirstOrDefault(c => c.Nombre == "Trajes de baño");


            // Validar existencia
            if (nike == null || adidas == null || puma == null || reebok == null || ua == null || nb == null ||
                remeras == null || pantalones == null || camperas == null || zapatillas == null || accesorios == null ||
                mochilas == null || ropaInterior == null || trajesDeBaño == null)
                return;

            // Crear productos
            var productos = new List<Producto>
            {
new Producto {
    Nombre = "Zapatillas Running",
    Descripcion = "Zapatillas ligeras diseñadas específicamente para correr largas distancias, fabricadas con materiales transpirables que mejoran la ventilación del pie. Incorporan una amortiguación avanzada que reduce el impacto en las articulaciones y brindan un soporte ergonómico que se adapta a la pisada para mayor estabilidad y rendimiento.",
    PrecioVigente = 12500,
    Imagen = "zapatillas_running.jpeg",
    MarcaId = reebok.Id,
    CategoriaId = zapatillas.Id,
    Activo = true
},
new Producto {
    Nombre = "Gorra deportiva",
    Descripcion = "Gorra deportiva ajustable con diseño moderno y visera curva, fabricada en tela liviana y respirable que permite una excelente ventilación. Ideal para protegerse del sol durante caminatas, entrenamientos, actividades al aire libre o simplemente como accesorio urbano de uso diario.",
    PrecioVigente = 1500,
    Imagen = "gorra_deportiva.jpeg",
    MarcaId = ua.Id,
    CategoriaId = accesorios.Id,
    Activo = true
},
new Producto {
    Nombre = "Mochila urbana",
    Descripcion = "Mochila urbana de gran capacidad con múltiples compartimientos internos y externos, pensada para el uso cotidiano en la ciudad o la universidad. Su diseño ergonómico incluye correas acolchadas y ajustables, materiales resistentes al desgaste y un compartimento especial para laptops o tablets.",
    PrecioVigente = 8900,
    Imagen = "mochila_urbana.jpg",
    MarcaId = nb.Id,
    CategoriaId = mochilas.Id,
    Activo = true
},
new Producto {
    Nombre = "Boxer algodón",
    Descripcion = "Boxer para hombre confeccionado en algodón de alta calidad, con costuras planas que evitan roces y una pretina elástica que brinda un ajuste suave. Ideal para el uso diario, ofrece frescura, comodidad y libertad de movimiento durante todo el día, incluso en jornadas largas o climas cálidos.",
    PrecioVigente = 1200,
    Imagen = "boxer_algodon.webp",
    MarcaId = nike.Id,
    CategoriaId = ropaInterior.Id,
    Activo = true
},
new Producto {
    Nombre = "Traje de baño hombre",
    Descripcion = "Traje de baño clásico para hombre, con diseño funcional y tela de secado rápido que permite mayor comodidad después de nadar. Su cintura elástica y cordón ajustable ofrecen un calce perfecto, convirtiéndolo en una prenda ideal para disfrutar de días de verano en la playa o la pileta.",
    PrecioVigente = 4200,
    Imagen = "traje_bano_hombre.jpeg",
    MarcaId = adidas.Id,
    CategoriaId = trajesDeBaño.Id,
    Activo = true
},
new Producto {
    Nombre = "Bufanda térmica",
    Descripcion = "Bufanda térmica fabricada con tejidos suaves y aislantes que mantienen el calor corporal incluso en las temperaturas más bajas. Su textura delicada la hace confortable al contacto con la piel, mientras que su diseño clásico y elegante la convierte en un accesorio versátil para el invierno.",
    PrecioVigente = 2100,
    Imagen = "bufanda_termica.webp",
    MarcaId = puma.Id,
    CategoriaId = accesorios.Id,
    Activo = true
},
new Producto {
    Nombre = "Calzas deportivas",
    Descripcion = "Calzas deportivas de alto rendimiento diseñadas para actividades como yoga, fitness, running o entrenamiento funcional. Confeccionadas en tela elástica y respirable que se adapta al cuerpo y ofrece libertad de movimiento, además de paneles reforzados que brindan mayor sujeción y comodidad.",
    PrecioVigente = 4800,
    Imagen = "calzas_deportivas.jpeg",
    MarcaId = reebok.Id,
    CategoriaId = ropaInterior.Id,
    Activo = true
},
new Producto {
    Nombre = "Zapatillas casual",
    Descripcion = "Zapatillas de uso diario con estilo moderno y detalles urbanos, ideales para combinar con distintos outfits. Cuentan con una suela flexible que brinda buen agarre y comodidad en caminatas prolongadas, además de una capellada resistente al desgaste que mantiene el calzado como nuevo por más tiempo.",
    PrecioVigente = 9800,
    Imagen = "zapatillas_casual.jpg",
    MarcaId = ua.Id,
    CategoriaId = zapatillas.Id,
    Activo = true
},
new Producto {
    Nombre = "Chaleco deportivo",
    Descripcion = "Chaleco deportivo ultraliviano, ideal para proteger el torso del viento sin comprometer la movilidad. Su diseño sin mangas y materiales resistentes a la intemperie lo hacen ideal para entrenamientos al aire libre, senderismo o como prenda adicional en días frescos. Se pliega fácilmente para guardarlo.",
    PrecioVigente = 6900,
    Imagen = "chaleco_deportivo.webp",
    MarcaId = nb.Id,
    CategoriaId = camperas.Id,
    Activo = true
},
new Producto {
    Nombre = "Short de baño mujer",
    Descripcion = "Short de baño para mujer confeccionado en tela liviana y de secado rápido, ideal para actividades acuáticas o días de playa. Posee una cintura ajustable con cordón para mayor seguridad, y su corte cómodo permite moverse con libertad tanto dentro como fuera del agua.",
    PrecioVigente = 3100,
    Imagen = "traje_bano_mujer.webp",
    MarcaId = nike.Id,
    CategoriaId = trajesDeBaño.Id,
    Activo = true
},
new Producto {
    Nombre = "Remera básica",
    Descripcion = "Remera básica de algodón suave y liviano, con cuello redondo reforzado y corte clásico que se adapta cómodamente al cuerpo. Ideal para el uso diario, ya que combina fácilmente con jeans, joggers o shorts, ofreciendo una prenda versátil, fresca y duradera para cualquier ocasión casual.",
    PrecioVigente = 2500,
    Imagen = "remera_basica.webp",
    MarcaId = nike.Id,
    CategoriaId = remeras.Id,
    Activo = true
},
new Producto {
    Nombre = "Pantalón jean",
    Descripcion = "Pantalón de jean en tono azul oscuro con corte recto y costuras reforzadas que garantizan una gran durabilidad. Ideal tanto para el uso diario como para salidas informales, su diseño clásico permite combinarlo con una amplia variedad de prendas y estilos, manteniendo siempre una buena presencia.",
    PrecioVigente = 5500,
    Imagen = "pantalon_jean.webp",
    MarcaId = adidas.Id,
    CategoriaId = pantalones.Id,
    Activo = true
},
new Producto {
    Nombre = "Campera deportiva",
    Descripcion = "Campera rompeviento confeccionada en materiales resistentes al agua y al viento. Cuenta con capucha ajustable, bolsillos laterales con cierre y un diseño liviano que permite moverse con libertad. Ideal para entrenar o salir a correr en exteriores, incluso en condiciones climáticas adversas.",
    PrecioVigente = 8900,
    Imagen = "campera_deportiva.webp",
    MarcaId = puma.Id,
    CategoriaId = camperas.Id,
    Activo = true
},
new Producto {
    Nombre = "Buzo canguro",
    Descripcion = "Buzo con capucha y bolsillo tipo canguro frontal, fabricado en algodón afelpado para mayor abrigo. Cuenta con cordón ajustable en la capucha, puños elastizados y un calce cómodo, convirtiéndolo en una prenda ideal para el otoño o invierno, ya sea para entrenar o para el día a día.",
    PrecioVigente = 7200,
    Imagen = "buzo_canguro.webp",
    MarcaId = reebok.Id,
    CategoriaId = camperas.Id,
    Activo = true
},
new Producto {
    Nombre = "Remera estampada",
    Descripcion = "Remera de algodón con diseño gráfico moderno estampado en el frente, perfecta para quienes buscan un estilo urbano y expresivo. Su tela suave y liviana brinda comodidad durante todo el día, mientras que su diseño original permite destacar en cualquier conjunto informal o relajado.",
    PrecioVigente = 3100,
    Imagen = "remera_estampada.webp",
    MarcaId = ua.Id,
    CategoriaId = remeras.Id,
    Activo = true
},
new Producto {
    Nombre = "Pantalón cargo",
    Descripcion = "Pantalón cargo de tela resistente y duradera, con múltiples bolsillos laterales que aportan funcionalidad y estilo. Ideal para actividades al aire libre, trekking o uso urbano, gracias a su corte cómodo y diseño práctico que ofrece espacio adicional para llevar objetos personales.",
    PrecioVigente = 6200,
    Imagen = "pantalon_cargo.webp",
    MarcaId = nb.Id,
    CategoriaId = pantalones.Id,
    Activo = true
},
new Producto {
    Nombre = "Short deportivo",
    Descripcion = "Short deportivo ultraliviano con cintura elástica y cordón regulable, fabricado en tela fresca de secado rápido. Ideal para entrenamientos intensos, actividades al aire libre o el gimnasio, ya que proporciona total libertad de movimiento y ventilación para mayor confort durante el esfuerzo físico.",
    PrecioVigente = 2800,
    Imagen = "short_deportivo.webp",
    MarcaId = nike.Id,
    CategoriaId = pantalones.Id,
    Activo = true
},
new Producto {
    Nombre = "Calza larga",
    Descripcion = "Calza larga de compresión térmica, diseñada para ofrecer soporte muscular y mantener la temperatura corporal durante entrenamientos en climas fríos. Ideal como prenda base o primera capa, su tejido elástico proporciona libertad de movimiento, mientras que su ajuste al cuerpo mejora el rendimiento físico.",
    PrecioVigente = 3900,
    Imagen = "calza_larga.webp",
    MarcaId = adidas.Id,
    CategoriaId = ropaInterior.Id,
    Activo = true
},
new Producto {
    Nombre = "Camisa leñadora",
    Descripcion = "Camisa leñadora a cuadros de tela gruesa y suave, con botones al frente y bolsillos en el pecho. Su diseño clásico es ideal para media estación, ya que puede usarse como camisa o como sobrecamisa. Perfecta para quienes buscan un estilo relajado y abrigado con un toque rústico y moderno.",
    PrecioVigente = 5400,
    Imagen = "camisa_lenadora.webp",
    MarcaId = puma.Id,
    CategoriaId = remeras.Id,
    Activo = true
},
new Producto {
    Nombre = "Chomba polo",
    Descripcion = "Chomba tipo polo con cuello tejido, botones en la parte frontal y tela piqué de excelente calidad. Su diseño atemporal la convierte en una opción ideal para un look semi formal, tanto para la oficina como para salidas informales. Combina estilo, frescura y comodidad en una sola prenda.",
    PrecioVigente = 3600,
    Imagen = "chomba_polo.webp",
    MarcaId = reebok.Id,
    CategoriaId = remeras.Id,
    Activo = true
},
new Producto {
    Nombre = "Campera inflable",
    Descripcion = "Campera inflable ultraliviana con relleno térmico sintético que proporciona abrigo sin agregar peso. Cuenta con capucha desmontable, cierre frontal reforzado y diseño compacto que permite guardarla fácilmente. Es ideal para climas fríos y para quienes buscan movilidad y protección al mismo tiempo.",
    PrecioVigente = 9700,
    Imagen = "campera_inflable.webp",
    MarcaId = ua.Id,
    CategoriaId = camperas.Id,
    Activo = true
},
new Producto {
    Nombre = "Musculosa deportiva",
    Descripcion = "Musculosa deportiva de tejido liviano y secado rápido, diseñada para entrenamientos intensos o climas cálidos. Su diseño sin mangas brinda mayor ventilación y libertad de movimiento, lo que la hace ideal para actividades como running, gimnasio o deportes al aire libre en verano.",
    PrecioVigente = 2500,
    Imagen = "musculosa_deportiva.webp",
    MarcaId = nb.Id,
    CategoriaId = remeras.Id,
    Activo = true
}   };

            _context.Productos.AddRange(productos);
            _context.SaveChanges();
        }

        private void PrecargarSucursales()
        {
            if (!_context.Sucursal.Any())
            {
                var sucursales = new List<Sucursal>
        {
            new Sucursal
            {
                Nombre = "Sucursal Centro",
                Direccion = "Av. Siempre Viva 742",
                Telefono = "1123456789",
                Email = "sucursalr1@ort.edu.ar",
                Activa = true
            },
            new Sucursal
            {
                Nombre = "Sucursal Norte",
                Direccion = "Ruta 8 km 42",
                Telefono = "1134567890",
                Email = "sucursal2@ort.edu.ar",
                Activa = true
            }
        };

                _context.Sucursal.AddRange(sucursales);
                _context.SaveChanges();
            }
        }
        private void PrecargarCarritos()
        {
            if (!_context.Carritos.Any())
            {
                var cliente = _context.Users.OfType<Cliente>().FirstOrDefault();
                if (cliente == null) return;

                var nuevoCarrito = new Carrito
                {
                    ClienteId = cliente.Id,
                    Activo = true,
                    CarritoItems = new List<CarritoItem>() // Podés dejarlo vacío por ahora
                };

                _context.Carritos.Add(nuevoCarrito);
                _context.SaveChanges();
            }
        }
        private int ObtenerProximoLegajo()
        {
            var ultimo = _context.Empleados
                .OrderByDescending(e => e.Legajo)
                .Select(e => e.Legajo)
                .FirstOrDefault();

            return ultimo + 1;
        }
        private void PrecargarStockItems()
        {
            if (_context.StockItem.Any())
                return;

            var productos = _context.Productos.ToList();
            var sucursales = _context.Sucursal.ToList();

            if (!productos.Any() || !sucursales.Any())
                return;

            var stockItems = new List<StockItem>();

            foreach (var sucursal in sucursales)
            {
                foreach (var producto in productos)
                {
                    // Cantidad válida según DataAnnotations: >= 1 y solo números
                    var cantidad = 100;

                    var stockItem = new StockItem
                    {
                        ProductoId = producto.Id,
                        SucursalId = sucursal.Id,
                        Cantidad = cantidad,
                        Producto = producto, // Relación opcional, útil si usás tracking
                        Sucursal = sucursal
                    };

                    stockItems.Add(stockItem);
                }
            }

            _context.StockItem.AddRange(stockItems);
            _context.SaveChanges();
        }

    }

}

