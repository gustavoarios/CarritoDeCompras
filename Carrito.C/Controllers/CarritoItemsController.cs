using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarritoC.Data;
using CarritoC.Helpers;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarritoC.Controllers
{
    [Authorize(Roles = "EmpleadoRol, ClienteRol")]
    public class CarritoItemsController : Controller
    {
        private readonly CarritoContext _context;

        public CarritoItemsController(CarritoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? clienteId)
        {
            if (User.IsInRole("ClienteRol"))
            {
                int? userId = UsuariosManager.GetUserId(User).Value;

                var carritoActivo = await _context.Carritos
                    .FirstOrDefaultAsync(c => c.ClienteId == userId && c.Activo);

                if (carritoActivo == null)
                {
                    TempData["Error"] = "No tenés un carrito activo.";
                    ViewData["CarritoActivo"] = false;
                    return View(new List<CarritoItem>());
                }

                ViewData["CarritoActivo"] = true;

                var carritoItems = await _context.CarritoItem
                    .Include(c => c.Carrito)
                    .Include(c => c.Producto)
                        .ThenInclude(p => p.StockItems)
                        .ThenInclude(si => si.Sucursal)
                    .Where(ci => ci.CarritoId == carritoActivo.Id)
                    .ToListAsync();

                ViewBag.Sucursales = await _context.Sucursal.ToListAsync();

                return View(carritoItems);
            }

            if (User.IsInRole("EmpleadoRol"))
            {
                var clientes = await _context.Clientes
                    .Select(c => new
                    {
                        Id = c.Id,
                        NombreCompleto = c.Apellido + ", " + c.Nombre
                    }).ToListAsync();

                ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);

                if (clienteId.HasValue)
                {
                    var carritoEmpleado = await _context.Carritos
                        .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value && c.Activo);

                    if (carritoEmpleado == null)
                    {
                        TempData["Error"] = "El cliente no tiene un carrito activo.";
                        ViewData["CarritoActivo"] = false;
                        return View(new List<CarritoItem>());
                    }

                    ViewData["CarritoActivo"] = true;

                    var carritoItemsEmpleado = await _context.CarritoItem
                        .Include(c => c.Carrito)
                        .Include(c => c.Producto)
                            .ThenInclude(p => p.StockItems)
                            .ThenInclude(si => si.Sucursal)
                        .Where(ci => ci.CarritoId == carritoEmpleado.Id)
                        .ToListAsync();

                    ViewBag.Sucursales = await _context.Sucursal.ToListAsync();

                    return View(carritoItemsEmpleado);
                }
            }

            ViewData["CarritoActivo"] = false;
            return View(new List<CarritoItem>());
        }
        public async Task<IActionResult> Details(int? carritoId, int? productoId, int? clienteId)
        {
            if (carritoId == null || productoId == null)
                return NotFound();

            var item = await _context.CarritoItem
                .Include(c => c.Carrito)
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(m => m.CarritoId == carritoId && m.ProductoId == productoId);

            if (item == null)
                return NotFound();

            if (User.IsInRole("EmpleadoRol"))
            {
                ViewBag.ClienteId = clienteId;
            }

            return View(item);
        }

        public IActionResult Create(int? productoId, int? clienteId)
        {
            if (productoId.HasValue)
            {
                var producto = _context.Productos.FirstOrDefault(p => p.Id == productoId.Value && p.Activo);
                if (producto == null)
                    return NotFound();

                // Calcular stock total del producto
                var totalStock = _context.StockItem
                    .Where(si => si.ProductoId == productoId.Value)
                    .Sum(si => si.Cantidad);

                ViewBag.MaxStock = totalStock;

                // Mostrar solo ese producto y bloquear selección
                ViewBag.ProductoId = new SelectList(new[] { producto }, "Id", "Nombre", producto.Id);
                ViewBag.BloquearProducto = true;
            }
            else
            {
                // Lista completa de productos activos, mostrando NOMBRE
                ViewBag.ProductoId = new SelectList(_context.Productos.Where(p => p.Activo), "Id", "Nombre");
                ViewBag.BloquearProducto = false;

                // En este caso no sabemos qué producto se eligió, no se puede calcular stock máximo
                ViewBag.MaxStock = null;
            }

            if (User.IsInRole("EmpleadoRol"))
            {
                var clientes = _context.Clientes.Select(c => new
                {
                    c.Id,
                    NombreCompleto = c.Apellido + ", " + c.Nombre
                }).ToList();

                ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);
                ViewBag.ClienteSeleccionado = clienteId;

                if (clienteId == null)
                {
                    TempData["Error"] = "Debe seleccionar un cliente antes de agregar productos.";
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EmpleadoRol, ClienteRol")]
        public async Task<IActionResult> Create([Bind("Cantidad,ProductoId")] CarritoItem item, int? clienteId)
        {
            int? userId = null;

            if (User.IsInRole("EmpleadoRol"))
            {
                if (!clienteId.HasValue)
                {
                    TempData["Error"] = "Debe seleccionar un cliente.";
                    return RedirectToAction("Create");
                }

                userId = clienteId;
            }
            else
            {
                userId = UsuariosManager.GetUserId(User).Value;
            }

            // 🔧 Buscar carrito activo o crearlo si no existe
            var carrito = await _context.Carritos
                .FirstOrDefaultAsync(c => c.ClienteId == userId && c.Activo);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    ClienteId = userId.Value,
                    Activo = true
                };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync(); // Guardar para obtener el ID
            }

            item.CarritoId = carrito.Id;

            if (ModelState.IsValid)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null)
                {
                    return NotFound();
                }

                var totalStock = await _context.StockItem
                    .Where(si => si.ProductoId == item.ProductoId)
                    .SumAsync(si => si.Cantidad);

                if (!producto.Activo || totalStock <= 0)
                {
                    ModelState.AddModelError("", "No se puede agregar el producto: sin stock o inactivo.");
                }
                else
                {
                    var existente = await _context.CarritoItem
                        .FirstOrDefaultAsync(ci => ci.CarritoId == carrito.Id && ci.ProductoId == item.ProductoId);

                    if (existente != null)
                    {
                        existente.Cantidad += item.Cantidad;
                        existente.ValorUnitario = producto.PrecioVigente;
                        _context.Update(existente);
                    }
                    else
                    {
                        item.ValorUnitario = producto.PrecioVigente;
                        _context.CarritoItem.Add(item);
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Producto agregado correctamente.";
                    return RedirectToAction("Index", new { clienteId = userId });
                }
            }

            // Si hubo errores de validación, recargar combos
            ViewBag.ProductoId = new SelectList(_context.Productos.Where(p => p.Activo), "Id", "Nombre", item.ProductoId);

            if (User.IsInRole("EmpleadoRol"))
            {
                var clientes = await _context.Clientes
                    .Select(c => new
                    {
                        Id = c.Id,
                        NombreCompleto = c.Apellido + ", " + c.Nombre
                    }).ToListAsync();

                ViewBag.Clientes = new SelectList(clientes, "Id", "NombreCompleto", clienteId);
                ViewBag.ClienteSeleccionado = clienteId;
            }

            return View(item);
        }
        // GET: CarritoItems/Edit
        public async Task<IActionResult> Edit(int? carritoId, int? productoId, int? clienteId)
        {
            if (carritoId == null || productoId == null)
                return NotFound();

            var item = await _context.CarritoItem.FindAsync(carritoId, productoId);
            if (item == null)
                return NotFound();

            ViewData["CarritoId"] = new SelectList(_context.Carritos, "Id", "Id", item.CarritoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", item.ProductoId);
            if (User.IsInRole("EmpleadoRol"))
            {
                ViewBag.ClienteId = clienteId;
            }
            return View(item);
        }
        // POST: CarritoItems/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ClienteRol, EmpleadoRol")]
        public async Task<IActionResult> Edit(int carritoId, int productoId, [Bind("Cantidad,CarritoId,ProductoId")] CarritoItem item, int? clienteId)
        {
            if (carritoId != item.CarritoId || productoId != item.ProductoId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null)
                        return NotFound();

                    item.ValorUnitario = producto.PrecioVigente;

                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarritoItemExists(item.CarritoId, item.ProductoId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { clienteId = Request.Form["clienteId"].ToString() });
            }

            ViewData["CarritoId"] = new SelectList(_context.Carritos, "Id", "Id", item.CarritoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", item.ProductoId);


            return View(item);
        }

        // GET: CarritoItems/Delete
        [Authorize(Roles = "ClienteRol, EmpleadoRol")]
        public async Task<IActionResult> Delete(int? carritoId, int? productoId, int? clienteId)
        {
            if (carritoId == null || productoId == null)
                return NotFound();

            var item = await _context.CarritoItem
                .Include(c => c.Carrito)
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(m => m.CarritoId == carritoId && m.ProductoId == productoId);

            if (item == null)
                return NotFound();

            if (User.IsInRole("EmpleadoRol"))
            {
                ViewBag.ClienteId = clienteId;
            }

            return View(item);
        }

        // POST: CarritoItems/DeleteConfirmed
        [Authorize(Roles = "ClienteRol, EmpleadoRol")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int carritoId, int productoId, int? clienteId)
        {
            var item = await _context.CarritoItem.FindAsync(carritoId, productoId);
            if (item != null)
                _context.CarritoItem.Remove(item);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { clienteId = Request.Form["clienteId"].ToString() });
        }
        private bool CarritoItemExists(int carritoId, int productoId)
        {
            return _context.CarritoItem.Any(e => e.CarritoId == carritoId && e.ProductoId == productoId);
        }

        [HttpPost]
        [Authorize(Roles = "EmpleadoRol, ClienteRol")]
        public async Task<IActionResult> VaciarCarrito(int? clienteId)
        {
            int? userId = null;

            if (User.IsInRole("EmpleadoRol"))
            {
                if (!clienteId.HasValue)
                {
                    TempData["Error"] = "Debe seleccionar un cliente.";
                    return RedirectToAction(nameof(Index));
                }

                userId = clienteId;
            }
            else
            {
                userId = UsuariosManager.GetUserId(User).Value;
            }

            var carrito = await _context.Carritos
                .Include(c => c.CarritoItems)
                .FirstOrDefaultAsync(c => c.ClienteId == userId && c.Activo);

            if (carrito == null)
            {
                TempData["Error"] = "No hay un carrito activo para vaciar.";
                return RedirectToAction(nameof(Index), new { clienteId = clienteId });
            }

            _context.CarritoItem.RemoveRange(carrito.CarritoItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El carrito fue vaciado correctamente.";
            return RedirectToAction(nameof(Index), new { clienteId = clienteId });
        }

        [HttpPost]
        public async Task<IActionResult> IncrementarCantidad(int carritoId, int productoId, int? clienteId)
        {
            var item = await _context.CarritoItem
                .Include(ci => ci.Producto)
                .FirstOrDefaultAsync(ci => ci.CarritoId == carritoId && ci.ProductoId == productoId);

            if (item == null)
                return Json(new { ok = false, mensaje = "Ítem no encontrado." });

            var stockDisponible = await _context.StockItem
                .Where(s => s.ProductoId == productoId)
                .SumAsync(s => s.Cantidad);

            if (item.Cantidad < stockDisponible)
            {
                item.Cantidad++;

                // Actualizar ValorUnitario con el precio vigente del producto
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto != null)
                {
                    item.ValorUnitario = producto.PrecioVigente;
                }

                _context.Update(item);
                await _context.SaveChangesAsync();

                var subtotal = item.Cantidad * item.ValorUnitario;
                var total = await _context.CarritoItem
                    .Where(ci => ci.CarritoId == carritoId)
                    .SumAsync(ci => ci.Cantidad * ci.ValorUnitario);

                return Json(new
                {
                    ok = true,
                    nuevaCantidad = item.Cantidad,
                    subtotalActualizado = $"${subtotal:N2}",
                    totalActualizado = $"${total:N2}"
                });
            }
            else
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "No se puede agregar más. Stock insuficiente."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReducirCantidad(int carritoId, int productoId, int? clienteId)
        {
            var item = await _context.CarritoItem
                .Include(ci => ci.Producto)
                .FirstOrDefaultAsync(ci => ci.CarritoId == carritoId && ci.ProductoId == productoId);

            if (item == null)
                return Json(new { ok = false, mensaje = "Ítem no encontrado." });

            if (item.Cantidad > 1)
            {
                item.Cantidad--;

                // Actualizar ValorUnitario con el precio vigente del producto
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto != null)
                {
                    item.ValorUnitario = producto.PrecioVigente;
                }

                _context.Update(item);
                await _context.SaveChangesAsync();
            }

            var subtotal = item.Cantidad * item.ValorUnitario;
            var total = await _context.CarritoItem
                .Where(ci => ci.CarritoId == carritoId)
                .SumAsync(ci => ci.Cantidad * ci.ValorUnitario);

            return Json(new
            {
                ok = true,
                nuevaCantidad = item.Cantidad,
                subtotalActualizado = $"${subtotal:N2}",
                totalActualizado = $"${total:N2}"
            });
        }
    }
    }
