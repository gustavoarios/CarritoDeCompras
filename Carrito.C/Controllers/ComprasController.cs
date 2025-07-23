using CarritoC.Data;
using CarritoC.Helpers;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarritoC.Controllers
{
    [Authorize(Roles = "EmpleadoRol, ClienteRol")]
    public class ComprasController : Controller
    {
        private readonly CarritoContext _context;

        public ComprasController(CarritoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? clienteId)
        {
            IQueryable<Compra> comprasQuery = _context.Compras
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .Include(c => c.Carrito)
                    .ThenInclude(car => car.CarritoItems)
                        .ThenInclude(ci => ci.Producto);

            if (User.IsInRole("ClienteRol"))
            {
                int idUsuario = (int)UsuariosManager.GetUserId(User);
                comprasQuery = comprasQuery.Where(c => c.ClienteId == idUsuario);
            }
            else if (clienteId.HasValue)
            {
                comprasQuery = comprasQuery.Where(c => c.ClienteId == clienteId.Value);
            }

            var compras = await comprasQuery.ToListAsync();

            if (User.IsInRole("EmpleadoRol"))
            {
                var clientes = await _context.Clientes
                    .Select(c => new
                    {
                        Id = c.Id,
                        NombreCompleto = c.Apellido + ", " + c.Nombre
                    })
                    .ToListAsync();

                ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);
            }

            return View(compras);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var compra = await _context.Compras
                .Include(c => c.Carrito)
                    .ThenInclude(car => car.CarritoItems)
                        .ThenInclude(ci => ci.Producto)
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (compra == null)
                return NotFound();

            return View(compra);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comprar(int SucursalId, int? clienteId = null)
        {
            int clienteIdReal = User.IsInRole("EmpleadoRol") && clienteId.HasValue
                ? clienteId.Value
                : (int)UsuariosManager.GetUserId(User);

            var carrito = await _context.Carritos
                .Include(c => c.CarritoItems)
                    .ThenInclude(ci => ci.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteIdReal && c.Activo);

            if (carrito == null)
            {
                TempData["Error"] = "No se encontró un carrito activo.";
                return RedirectToAction("Index", "CarritoItems");
            }

            if (!carrito.CarritoItems.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction("Index", "CarritoItems");
            }

            bool yaComprado = await _context.Compras.AnyAsync(c => c.CarritoId == carrito.Id);
            if (yaComprado)
            {
                TempData["Error"] = "Este carrito ya fue usado en una compra.";
                return RedirectToAction("Index", "CarritoItems");
            }

            var stockFaltante = carrito.CarritoItems.FirstOrDefault(item =>
            {
                var stock = _context.StockItem
                    .Where(s => s.ProductoId == item.ProductoId && s.SucursalId == SucursalId)
                    .Select(s => s.Cantidad)
                    .FirstOrDefault();

                return stock < item.Cantidad;
            });

            if (stockFaltante != null)
            {
                var sucursales = await _context.Sucursal.Where(s => s.Activa).ToListAsync();
                var stockItems = await _context.StockItem.ToListAsync();

                var sucursalesConStock = sucursales.Where(s => carrito.CarritoItems.All(item =>
                    stockItems.Any(stock =>
                        stock.SucursalId == s.Id &&
                        stock.ProductoId == item.ProductoId &&
                        stock.Cantidad >= item.Cantidad))).ToList();

                // Aquí agrego la lógica para preseleccionar una sucursal válida
                if (!sucursalesConStock.Any(s => s.Id == SucursalId))
                {
                    SucursalId = sucursalesConStock.FirstOrDefault()?.Id ?? 0;
                }

                ViewBag.Sucursales = new SelectList(sucursalesConStock, "Id", "Nombre", SucursalId);

                var stockDisponible = stockItems
                    .Where(s => s.ProductoId == stockFaltante.ProductoId && s.SucursalId == SucursalId)
                    .Select(s => s.Cantidad)
                    .FirstOrDefault();

                ViewBag.Error = stockDisponible > 0
                    ? $"La sucursal seleccionada no tiene stock suficiente para el producto \"{stockFaltante.Producto.Nombre}\". Seleccione la sucursal disponible"
                    : $"La sucursal seleccionada no tiene stock para el producto \"{stockFaltante.Producto.Nombre}\".";

                if (User.IsInRole("EmpleadoRol"))
                {
                    var clientes = _context.Clientes
                        .Select(c => new { c.Id, NombreCompleto = c.Apellido + ", " + c.Nombre })
                        .ToList();
                    ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);
                }

                return View("ConfirmarCompra");
            }

            foreach (var item in carrito.CarritoItems)
            {
                var stock = await _context.StockItem
                    .FirstOrDefaultAsync(s => s.ProductoId == item.ProductoId && s.SucursalId == SucursalId);

                if (stock != null)
                {
                    stock.Cantidad -= item.Cantidad;
                    _context.Update(stock);
                }
            }

            var compra = new Compra
            {
                ClienteId = clienteIdReal,
                CarritoId = carrito.Id,
                SucursalId = SucursalId,
                Fecha = DateTime.Now,
                Total = carrito.Subtotal
            };

            carrito.Activo = false;
            _context.Compras.Add(compra);
            _context.Update(carrito);

            var nuevoCarrito = new Carrito
            {
                ClienteId = clienteIdReal,
                Activo = true
            };
            _context.Carritos.Add(nuevoCarrito);
            await VerificarYDesactivarProductosSinStock();
            await _context.SaveChangesAsync();

            TempData["Success"] = "Compra realizada exitosamente.";
            TempData["CompraId"] = compra.Id;

            return RedirectToAction("Agradecimiento");
        }
        public async Task<IActionResult> Agradecimiento()
        {
            if (TempData["CompraId"] == null)
                return RedirectToAction("Index", "Home");

            int compraId = (int)TempData["CompraId"];

            var compra = await _context.Compras
                .Include(c => c.Sucursal)
                .FirstOrDefaultAsync(c => c.Id == compraId);

            if (compra == null)
                return RedirectToAction("Index", "Home");

            ViewBag.CodigoCompra = compra.Id;
            return View(compra.Sucursal);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmarCompra(int? clienteId)
        {
            ViewBag.Sucursales = new SelectList(
                await _context.Sucursal.Where(s => s.Activa).ToListAsync(), "Id", "Nombre");

            if (User.IsInRole("EmpleadoRol"))
            {
                var clientes = await _context.Clientes
                    .Select(c => new { c.Id, NombreCompleto = c.Apellido + ", " + c.Nombre })
                    .ToListAsync();

                ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);
            }

            return View();
        }

        private bool CompraExists(int id)
        {
            return _context.Compras.Any(e => e.Id == id);
        }

        private async Task VerificarYDesactivarProductosSinStock()
        {
            var productos = await _context.Productos
                .Include(p => p.StockItems)
                .ToListAsync();

            foreach (var producto in productos)
            {
                int totalStock = producto.StockItems.Sum(si => si.Cantidad);
                if (totalStock == 0 && producto.Activo)
                {
                    producto.Activo = false;
                    _context.Update(producto);
                }
            }
        }

        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> ComprasDelMes()
        {
            var primerDiaDelMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var comprasDelMes = await _context.Compras
                .Include(c => c.Cliente)
                .Where(c => c.Fecha >= primerDiaDelMes)
                .OrderByDescending(c => c.Total)
                .ToListAsync();

            return View(comprasDelMes);
        }

    }
}
    
    