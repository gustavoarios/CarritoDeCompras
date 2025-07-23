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
    [Authorize(Roles = "EmpleadoRol, ClienteRol")]
    public class MarcasController : Controller
    {
        private readonly CarritoContext _context;

        public MarcasController(CarritoContext context)
        {
            _context = context;
        }

        // GET: Marcas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Marcas.ToListAsync());
        }

        // GET: Marcas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (marca == null)
            {
                return NotFound();
            }

            return View(marca);
        }

        // GET: Marcas/Create
        [Authorize(Roles = "EmpleadoRol")]
        public IActionResult Create(string retorno = null)
        {
            ViewData["Retorno"] = retorno;
            return View();
        }

        // POST: Marcas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "EmpleadoRol")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Marca marca, string retorno)
        {
            if (ModelState.IsValid)
            {
                _context.Add(marca);
                await _context.SaveChangesAsync();

                if (retorno == "ProductosCreate")
                {
                    // Redirige a la acción Create de Productos pasando un flag para mostrar mensaje
                    return RedirectToAction("Create", "Productos", new { desdeNuevaEntidad = true });
                }

                return RedirectToAction(nameof(Index));
            }
            return View(marca);
        }
       

        private bool MarcaExists(int id)
        {
            return _context.Marcas.Any(e => e.Id == id);
        }
    }
}