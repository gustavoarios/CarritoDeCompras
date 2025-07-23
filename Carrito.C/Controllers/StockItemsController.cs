using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarritoC.Data;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;

namespace CarritoC.Controllers
{
    [Authorize(Roles = "EmpleadoRol")]
    public class StockItemsController : Controller
    {
        private readonly CarritoContext _context;

        public StockItemsController(CarritoContext context)
        {
            _context = context;
        }

        // GET: StockItems
        public async Task<IActionResult> Index()
        {
            var carritoContext = _context.StockItem.Include(s => s.Producto).Include(s => s.Sucursal);
            return View(await carritoContext.ToListAsync());
        }

        // GET: StockItems/Details/5
        public async Task<IActionResult> Details(int? productoId, int? sucursalId)
        {
            if (productoId == null || sucursalId == null)
            {
                return NotFound();
            }

            var stockItem = await _context.StockItem
                .Include(s => s.Producto)
                .Include(s => s.Sucursal)
                .FirstOrDefaultAsync(m => m.ProductoId == productoId && m.SucursalId == sucursalId);
            if (stockItem == null)
            {
                return NotFound();
            }

            return View(stockItem);
        }

        // GET: StockItems/Create
        public IActionResult Create()
        {
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion");
            ViewData["SucursalId"] = new SelectList(
                _context.Sucursal.Where(s => s.Activa), "Id", "Nombre"); // Solo activas
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cantidad,ProductoId,SucursalId")] StockItem stockItem)
        {
            if (ModelState.IsValid)
            {
                var existente = await _context.StockItem
                    .FirstOrDefaultAsync(s => s.ProductoId == stockItem.ProductoId && s.SucursalId == stockItem.SucursalId);

                if (existente != null)
                {
                    // Ya existe, sumar cantidad. Se queda sin stock, remover
                    existente.Cantidad += stockItem.Cantidad;

                    if (existente.Cantidad == 0)
                    {
                        _context.Remove(existente);
                    }
                    else
                    {
                        _context.Update(existente);
                    }
                }
                else
                {
                    // No existe, crear nuevo
                    _context.Add(stockItem);
                }

                await _context.SaveChangesAsync();
                ActualizarEstadoProductoPorStock(stockItem.ProductoId); // <= agregado
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", stockItem.ProductoId);
            ViewData["SucursalId"] = new SelectList(
                _context.Sucursal.Where(s => s.Activa), "Id", "Nombre", stockItem.SucursalId); // Solo activas
            return View(stockItem);
        }
        // GET: StockItems/Edit/5
        public async Task<IActionResult> Edit(int? productoId, int? sucursalId)
        {
            if (productoId == null || sucursalId == null)
            {
                return NotFound();
            }

            var stockItem = await _context.StockItem
                .Include(s => s.Producto)
                .Include(s => s.Sucursal)
                .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId);

            if (stockItem == null)
            {
                return NotFound();
            }

            return View(stockItem);
        }

        // POST: StockItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int productoId, int sucursalId, [Bind("Cantidad,ProductoId,SucursalId")] StockItem stockItem)
        {
            if (productoId != stockItem.ProductoId || sucursalId != stockItem.SucursalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (stockItem.Cantidad == 0)
                    {
                        _context.Remove(stockItem);
                    }
                    else
                    {
                        _context.Update(stockItem);
                    }
                    await _context.SaveChangesAsync();
                    ActualizarEstadoProductoPorStock(stockItem.ProductoId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockItemExists(stockItem.ProductoId, stockItem.SucursalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", stockItem.ProductoId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursal, "Id", "Nombre", stockItem.SucursalId);
            return View(stockItem);
        }
       
        private bool StockItemExists(int productoId, int sucursalId)
        {
            return _context.StockItem.Any(e => e.ProductoId == productoId && e.SucursalId == sucursalId);
        }
        private void ActualizarEstadoProductoPorStock(int productoId)
        {
            var producto = _context.Productos
                .Include(p => p.StockItems)
                .FirstOrDefault(p => p.Id == productoId);

            if (producto != null)
            {
                int totalStock = producto.StockItems.Sum(s => s.Cantidad);
                producto.Activo = totalStock > 0;
                _context.Update(producto);
                _context.SaveChanges();
            }
        }
    }
}