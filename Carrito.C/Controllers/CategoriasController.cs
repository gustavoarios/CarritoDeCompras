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
    [Authorize(Roles = "EmpleadoRol, ClienteRol")]

    public class CategoriasController : Controller
    {
        private readonly CarritoContext _context;

        public CategoriasController(CarritoContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categorias/Create
        [Authorize(Roles = "EmpleadoRol")]
        public IActionResult Create(string retorno = null)
        {
            ViewBag.Retorno = retorno;
            return View();
        }

        [Authorize(Roles = "EmpleadoRol")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Categoria categoria, string retorno)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoria);

                try
                {
                    await _context.SaveChangesAsync();

                    if (retorno == "ProductosCreate")
                    {
                        // Redirigir a Productos/Create con flag para mostrar mensaje
                        return RedirectToAction("Create", "Productos", new { desdeNuevaEntidad = true });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbex)
                {
                    SqlException innerException = dbex.InnerException as SqlException;
                    if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                    {
                        ModelState.AddModelError("Nombre", "El nombre de la categoría ya existe, debe ser único.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbex.Message);
                    }
                }
            }

            return View(categoria);
        }


    
    }
}