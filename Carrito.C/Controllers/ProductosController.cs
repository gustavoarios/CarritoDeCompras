using CarritoC.Data;
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
    public class ProductosController : Controller
    {
        private readonly CarritoContext _context;

        public ProductosController(CarritoContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(int? categoriaId)
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", categoriaId);

            var productosQuery = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.StockItems)
                    .ThenInclude(s => s.Sucursal)
                .AsQueryable();

            if (categoriaId.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.CategoriaId == categoriaId.Value);
            }

            var productos = await productosQuery.ToListAsync();

            // ✅ Evitar desactivación si venís de edición o creación
            bool desdeEdicion = TempData["DesdeEdicion"] as bool? ?? false;
            bool desdeCreacion = TempData["DesdeCreacion"] as bool? ?? false;

            if (!desdeEdicion && !desdeCreacion)
            {
                foreach (var producto in productos)
                {
                    int totalStock = producto.StockItems.Sum(si => si.Cantidad);
                    if (totalStock == 0 && producto.Activo && producto.StockItems.Any(si =>
                    {
                        var entry = _context.Entry(si);
                        var cantidadActual = si.Cantidad;
                        var cantidadAnterior = (int)(entry.Property("Cantidad").OriginalValue ?? 0);
                        return cantidadActual > 0 || cantidadAnterior > 0;
                    })){

                        producto.Activo = false;
                        _context.Update(producto);
                    }
                }
                await _context.SaveChangesAsync();
            }

            var stockPorProducto = productos.ToDictionary(
                p => p.Id,
                p => p.StockItems
                    .Where(si => si.Cantidad > 0)
                    .Select(si => new
                    {
                        Sucursal = si.Sucursal,
                        Cantidad = si.Cantidad
                    })
                    .ToList()
            );

            ViewBag.SucursalesConStock = stockPorProducto;

            return View(productos);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }


            var stockPorSucursal = await _context.StockItems
            .Where(si => si.ProductoId == producto.Id && si.Cantidad > 0)
            .Include(si => si.Sucursal)
            .ToListAsync();

            ViewBag.StockPorSucursal = stockPorSucursal;
            return View(producto);
        }

        // GET: Productos/Create
        [Authorize(Roles = "EmpleadoRol")]
        public IActionResult Create(bool desdeNuevaEntidad = false)
        {
            if (desdeNuevaEntidad)
                ViewBag.Mensaje = "✅ Marca creada correctamente. Completa los datos del producto.";

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> Create([Bind("Id,Activo,Nombre,Descripcion,PrecioVigente,Imagen,MarcaId,CategoriaId")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();

                TempData["DesdeCreacion"] = true; // ✅ evita desactivación inmediata
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Nombre", producto.MarcaId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Nombre", producto.MarcaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Activo,Nombre,Descripcion,PrecioVigente,Imagen,MarcaId,CategoriaId")] Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id)) return NotFound();
                    else throw;
                }

                TempData["DesdeEdicion"] = true; // ✅ evitar desactivación inmediata al volver a Index
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Nombre", producto.MarcaId);
            return View(producto);
        }

        
        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        [Authorize(Roles = "AdministradorRol,EmpleadoRol")]
        [HttpPost]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            producto.Activo = !producto.Activo;
            _context.Update(producto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { desactivar = false }); // ✅ evita desactivación inmediata
        }

        public async Task<IActionResult> PorCategoria()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .OrderBy(p => p.Categoria.Nombre)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            var agrupados = productos
                .GroupBy(p => p.Categoria.Nombre)
                .ToList();

            return View(agrupados);
        }

        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> VerificarStockYDesactivar()
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

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Verificación realizada. Productos sin stock desactivados.";
            return RedirectToAction(nameof(Index));
        }
    }
}