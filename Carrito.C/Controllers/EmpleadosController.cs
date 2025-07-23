using CarritoC.Data;
using CarritoC.Helpers;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarritoC.Controllers
{
    [Authorize(Roles = "EmpleadoRol, AdministradorRol")]
    public class EmpleadosController : Controller
    {
        private readonly CarritoContext _context;
        private readonly UserManager<Persona> _userManager;

        public EmpleadosController(CarritoContext context, UserManager<Persona> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Empleados.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null) return NotFound();

            return View(empleado);
        }

        public IActionResult Create()
        {
            var empleado = new Empleado
            {
                Legajo = ObtenerProximoLegajo()
            };
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Legajo,Id,UserName,Nombre,Apellido,DNI,Telefono,Direccion,Email")] Empleado empleado)
        {
            if (empleado.Email != empleado.UserName)
                ModelState.AddModelError("Email", "El Email debe coincidir con el Nombre de Usuario.");

            bool dniDuplicado = await _context.Empleados.AnyAsync(e => e.DNI == empleado.DNI);
            if (dniDuplicado)
                ModelState.AddModelError("DNI", "Ya existe un usuario con ese DNI.");

            bool userNameDuplicado = await _context.Users.AnyAsync(u => u.UserName == empleado.UserName);
            if (userNameDuplicado)
                ModelState.AddModelError("UserName", "Ya existe un usuario con ese nombre de usuario (Email).");

            int nuevoLegajo = ObtenerProximoLegajo();

            bool legajoDuplicado = await _context.Empleados.AnyAsync(e => e.Legajo == nuevoLegajo);
            if (legajoDuplicado)
                ModelState.AddModelError("Legajo", "Error al generar el número de legajo. Intente nuevamente.");

            if (!ModelState.IsValid)
            {
                ViewBag.Debug = true;
                return View(empleado);
            }

            empleado.Legajo = nuevoLegajo;
            empleado.FechaAlta = DateTime.Now;

            try
            {
                var resultado = await _userManager.CreateAsync(empleado, "Password1!");

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(empleado, "EmpleadoRol");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (DbUpdateException dbex)
            {
                SqlException inner = dbex.InnerException as SqlException;
                if (inner != null && (inner.Number == 2627 || inner.Number == 2601))
                {
                    if (!ModelState.ContainsKey("Legajo") &&
                        await _context.Empleados.AnyAsync(e => e.Legajo == nuevoLegajo))
                    {
                        ModelState.AddModelError("Legajo", "Ya existe un empleado con ese legajo.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error inesperado: " + dbex.Message);
                }
            }

            ViewBag.Debug = true;
            return View(empleado);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            ViewBag.EsEmpleadoEditandose = User.IsInRole("EmpleadoRol") && id.ToString() == _userManager.GetUserId(User);
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Legajo,Id,UserName,Nombre,Apellido,DNI,Telefono,Direccion,Email")] Empleado empleadoEditado)
        {
            if (id != empleadoEditado.Id) return NotFound();

            var empleadoBD = await _context.Empleados.FindAsync(id);
            if (empleadoBD == null) return NotFound();

            if (User.IsInRole("EmpleadoRol") && id.ToString() == _userManager.GetUserId(User))
            {
                empleadoEditado.UserName = empleadoBD.UserName;
                empleadoEditado.Nombre = empleadoBD.Nombre;
                empleadoEditado.Apellido = empleadoBD.Apellido;
                empleadoEditado.DNI = empleadoBD.DNI;
                empleadoEditado.Email = empleadoBD.Email;
            }

            if (empleadoEditado.Email != empleadoEditado.UserName)
                ModelState.AddModelError("Email", "El Email debe coincidir con el Nombre de Usuario.");

            bool dniDuplicado = await _context.Empleados
                .AnyAsync(e => e.Id != empleadoEditado.Id && e.DNI == empleadoEditado.DNI);
            if (dniDuplicado)
                ModelState.AddModelError("DNI", "Ya existe otro empleado con ese DNI.");

            bool legajoDuplicado = await _context.Empleados
                .AnyAsync(e => e.Id != empleadoEditado.Id && e.Legajo == empleadoEditado.Legajo);
            if (legajoDuplicado)
                ModelState.AddModelError("Legajo", "Ya existe otro empleado con ese legajo.");

            if (!ModelState.IsValid)
            {
                ViewBag.EsEmpleadoEditandose = User.IsInRole("EmpleadoRol") && id.ToString() == _userManager.GetUserId(User);
                return View(empleadoEditado);
            }

            empleadoBD.Telefono = empleadoEditado.Telefono;
            empleadoBD.Direccion = empleadoEditado.Direccion;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbex)
            {
                if (dbex is DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleadoEditado.Id)) return NotFound();
                    else throw;
                }

                SqlException inner = dbex.InnerException as SqlException;
                if (inner != null && (inner.Number == 2627 || inner.Number == 2601))
                {
                    if (!ModelState.ContainsKey("Legajo") &&
                        await _context.Empleados.AnyAsync(e => e.Id != empleadoEditado.Id && e.Legajo == empleadoEditado.Legajo))
                    {
                        ModelState.AddModelError("Legajo", "Ya existe otro empleado con ese legajo.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error inesperado: " + dbex.Message);
                }
            }

            ViewBag.EsEmpleadoEditandose = User.IsInRole("EmpleadoRol") && id.ToString() == _userManager.GetUserId(User);
            return View(empleadoEditado);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null) return NotFound();

            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }

        private int ObtenerProximoLegajo()
        {
            var legajoMaximo = _context.Empleados
                .OrderByDescending(e => e.Legajo)
                .Select(e => e.Legajo)
                .FirstOrDefault();

            return legajoMaximo + 1;
        }
    }
}