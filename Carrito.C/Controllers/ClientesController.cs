using CarritoC.Data;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Authorize(Roles = "ClienteRol, EmpleadoRol")]
    public class ClientesController : Controller
    {
        private readonly CarritoContext _context;
        private readonly UserManager<Persona> _userManager;


        public ClientesController(CarritoContext context, UserManager<Persona> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Clientes
        [Authorize(Roles = "EmpleadoRol, AdministradorRol")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        [Authorize(Roles = "EmpleadoRol, AdministradorRol")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "EmpleadoRol, AdministradorRol")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdentificacionUnica,Id,UserName,Nombre,Apellido,DNI,Telefono,Direccion,Email")] Cliente cliente)
        {
            // Validación: Email debe coincidir con UserName
            if (cliente.Email != cliente.UserName)
            {
                ModelState.AddModelError("Email", "El Email debe coincidir con el Nombre de Usuario.");
            }

            // Validación anticipada: DNI duplicado
            bool dniExistente = await _context.Clientes.AnyAsync(c => c.DNI == cliente.DNI);
            if (dniExistente)
            {
                ModelState.AddModelError("DNI", "Ya existe un cliente con ese DNI.");
            }

            // Validación anticipada: Identificación única duplicada
            bool identificacionDuplicada = await _context.Clientes.AnyAsync(c => c.IdentificacionUnica == cliente.IdentificacionUnica);
            if (identificacionDuplicada)
            {
                ModelState.AddModelError("IdentificacionUnica", "Ya existe un cliente con esa identificación.");
            }

            // Validación anticipada: UserName duplicado (evita error Identity)
            bool userNameDuplicado = await _context.Users.AnyAsync(u => u.UserName == cliente.UserName);
            if (userNameDuplicado)
            {
                ModelState.AddModelError("UserName", "Ya existe un cliente con ese nombre de usuario (Email).");
            }

            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, devolver la vista con los errores
                return View(cliente);
            }

            cliente.FechaAlta = DateTime.Now;

            try
            {
                var resultado = await _userManager.CreateAsync(cliente, "Password1!");

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(cliente, "ClienteRol");
                    return RedirectToAction(nameof(Index));
                }

                // Si falló creación, agrego los errores
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (DbUpdateException dbex)
            {
                SqlException innerException = dbex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    // Como no sabemos cuál falló, chequeamos
                    if (await _context.Clientes.AnyAsync(c => c.DNI == cliente.DNI))
                        ModelState.AddModelError("DNI", "Ya existe un cliente con ese DNI.");

                   
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error inesperado: " + dbex.Message);
                }
            }

            // Si llegamos acá es porque hubo algún error, mostrar formulario con mensajes
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        [Authorize(Roles = "ClienteRol, EmpleadoRol")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (User.IsInRole("ClienteRol"))
            {
                var userid = Int32.Parse(_userManager.GetUserId(User));
                if (userid != id)
                {
                    return RedirectToAction("Edit", new { id = userid });
                }
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [Authorize(Roles = "ClienteRol, EmpleadoRol")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,Nombre,Apellido,DNI,Telefono,Direccion,Email,IdentificacionUnica,FechaAlta")] Cliente clienteEditado)
        {
            if (id != clienteEditado.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(clienteEditado);
            }

            var clienteBD = await _context.Clientes.FindAsync(id);
            if (clienteBD == null)
            {
                return NotFound();
            }

            // Validación anticipada: verificar si otro cliente ya tiene el mismo DNI
            bool dniDuplicado = await _context.Clientes
                .AnyAsync(c => c.Id != clienteEditado.Id && c.DNI == clienteEditado.DNI);
            if (dniDuplicado)
            {
                ModelState.AddModelError("DNI", "Ya existe otro cliente con ese DNI.");
            }

            // Verificar si otro cliente ya tiene la misma Identificación Única
            bool identificacionDuplicada = await _context.Clientes
                .AnyAsync(c => c.Id != clienteEditado.Id && c.IdentificacionUnica == clienteEditado.IdentificacionUnica);
            if (identificacionDuplicada)
            {
                ModelState.AddModelError("IdentificacionUnica", "Ya existe otro cliente con esa identificación.");
            }

            if (!ModelState.IsValid)
            {
                return View(clienteEditado);
            }

            // Actualizar los campos que se pueden modificar
            clienteBD.Telefono = clienteEditado.Telefono;
            clienteBD.Direccion = clienteEditado.Direccion;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = clienteEditado.Id });
            }
            catch (DbUpdateException dbex)
            {
                SqlException inner = dbex.InnerException as SqlException;
                if (inner != null && (inner.Number == 2627 || inner.Number == 2601))
                {
                    // Reconfirmar cuál campo causó el error por si cambió luego de la validación anticipada
                    if (await _context.Clientes.AnyAsync(c => c.Id != clienteEditado.Id && c.DNI == clienteEditado.DNI))
                        ModelState.AddModelError("DNI", "Ya existe otro cliente con ese DNI.");

                   
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error inesperado: " + dbex.Message);
                }

                return View(clienteEditado);
            }
        }


        // GET: Clientes/Delete/5
        [Authorize(Roles = "EmpleadoRol, AdministradorRol")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [Authorize(Roles = "EmpleadoRol, AdministradorRol")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    
    }
}