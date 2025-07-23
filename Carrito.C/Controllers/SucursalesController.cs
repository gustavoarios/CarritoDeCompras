using CarritoC.Data;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarritoC.Controllers
{
    public class SucursalesController : Controller
    {
        private readonly CarritoContext _context;

        public SucursalesController(CarritoContext context)
        {
            _context = context;
        }

        // GET: Sucursales
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sucursal.ToListAsync());
        }

        // GET: Sucursales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sucursal = await _context.Sucursal
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sucursal == null)
            {
                return NotFound();
            }

            return View(sucursal);
        }

        // GET: Sucursales/Create
        [Authorize(Roles = "EmpleadoRol")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sucursales/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Direccion,Telefono,Email,Activa")] Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sucursal);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbex)
                {
                    SqlException innerException = dbex.InnerException as SqlException;
                    if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                    {
                        ModelState.AddModelError("Nombre", "El nombre de la sucursal ya existe, debe ser único.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbex.Message);
                    }
                }
            }

            return View(sucursal);
        }

        // GET: Sucursales/Delete/5
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sucursal = await _context.Sucursal
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sucursal == null)
            {
                return NotFound();
            }

            return View(sucursal);
        }

        // POST: Sucursales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sucursal = await _context.Sucursal.FindAsync(id);

            if (sucursal == null)
            {
                return NotFound();
            }

            // 🔍 Verificar si hay productos con stock en esta sucursal
            bool tieneStock = await _context.StockItem
                .AnyAsync(s => s.SucursalId == id && s.Cantidad > 0);

            if (tieneStock)
            {
                TempData["Error"] = "No se puede deshabilitar la sucursal porque tiene productos con stock disponible.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ No hay stock: se puede deshabilitar (baja lógica)
            sucursal.Activa = false;
            _context.Update(sucursal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sucursal deshabilitada correctamente.";
            return RedirectToAction(nameof(Index));
        }

    

        private bool SucursalActiva(int id)
        {
            return _context.Sucursal.Any(e => e.Id == id && e.Activa);
        }

        [Authorize(Roles = "EmpleadoRol")]
        public IActionResult AgregarStock(int sucursalId)
        {
            var sucursal = _context.Sucursal.FirstOrDefault(s => s.Id == sucursalId);
            if (sucursal == null)
            {
                return NotFound();
            }

            ViewData["SucursalId"] = sucursalId;
            ViewData["SucursalNombre"] = sucursal.Nombre;

            var productosExistentes = _context.StockItem
                .Where(s => s.SucursalId == sucursalId)
                .Select(s => s.ProductoId)
                .ToList();

            var productosDisponibles = _context.Productos
                .Where(p => !productosExistentes.Contains(p.Id))
                .ToList();

            ViewBag.Productos = new SelectList(productosDisponibles, "Id", "Nombre");

            return View();
        }
        [Authorize(Roles = "EmpleadoRol")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarStock([Bind("ProductoId,SucursalId,Cantidad")] StockItem stockItem)
        {
            // Verificar si ya existe ese producto en esa sucursal (por seguridad)
            bool yaExiste = await _context.StockItem.AnyAsync(si =>
                si.SucursalId == stockItem.SucursalId && si.ProductoId == stockItem.ProductoId);

            if (yaExiste)
            {
                TempData["Error"] = "El producto ya está en el stock de esta sucursal.";
                return RedirectToAction("Index", "StockItems");
            }

            _context.StockItem.Add(stockItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto agregado al stock correctamente.";

            // 🔁 Redirecciona al listado general de stock:
            return RedirectToAction("Index", "StockItems");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var sucursal = await _context.Sucursal.FindAsync(id);
            if (sucursal == null)
            {
                return NotFound();
            }

            sucursal.Activa = true;
            _context.Update(sucursal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sucursal reactivada correctamente.";
            return RedirectToAction(nameof(Index));
        }

    }
}
