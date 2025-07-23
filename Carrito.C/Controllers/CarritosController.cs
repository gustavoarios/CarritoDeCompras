using System.Linq;
using System.Threading.Tasks;
using CarritoC.Data;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarritoC.Controllers
{
    [Authorize(Roles = "EmpleadoRol, ClienteRol")]
    public class CarritosController : Controller
    {
        private readonly CarritoContext _context;

        public CarritosController(CarritoContext context)
        {
            _context = context;
        }

        // GET: Carritos
        public IActionResult Index()
        {
            return NotFound();
        }

        // GET: Carritos/Details/5
        public IActionResult Details(int? id)
        {
            return NotFound();
        }

        // GET: Carritos/Create
        [Authorize(Roles = "EmpleadoRol")]
        public IActionResult Create(int clienteId)
        {
            var cliente = _context.Clientes.Find(clienteId);
            if (cliente == null)
            {
                return NotFound();
            }

            ViewData["ClienteNombre"] = cliente.Apellido + ", " + cliente.Nombre;
            ViewData["ClienteId"] = cliente.Id;
            return View();
        }

        // POST: Carritos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EmpleadoRol")]
        public async Task<IActionResult> Create([Bind("Activo,ClienteId")] Carrito model)
        {
            // Validación inicial
            if (!ModelState.IsValid)
            {
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                ViewData["ClienteNombre"] = cliente?.Apellido + ", " + cliente?.Nombre;
                ViewData["ClienteId"] = model.ClienteId;
                return View(model);
            }

            // Verifica si ya hay un carrito activo para ese cliente
            var carritoExistente = await _context.Carritos
                .FirstOrDefaultAsync(c => c.ClienteId == model.ClienteId && c.Activo);

            if (carritoExistente != null)
            {
                TempData["Warning"] = "El cliente ya tiene un carrito activo. Se redirigirá para agregar productos.";
                return RedirectToAction("Create", "CarritoItems", new { clienteId = model.ClienteId });
            }

            // Crear nuevo carrito
            var nuevoCarrito = new Carrito
            {
                Activo = true,
                ClienteId = model.ClienteId
            };

            _context.Carritos.Add(nuevoCarrito);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Carrito creado correctamente. Ahora puede agregar productos.";

            // Redirigir al formulario para agregar productos al carrito
            return RedirectToAction("Create", "CarritoItems", new { clienteId = model.ClienteId });
        }

        // GET: Carritos/Edit/5
        public IActionResult Edit(int? id)
        {
            return NotFound();
        }

        // POST: Carritos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Activo,ClienteId")] Carrito carrito)
        {
            return NotFound();
        }

        private bool CarritoExists(int id)
        {
            return _context.Carritos.Any(e => e.Id == id);
        }
    }
}
