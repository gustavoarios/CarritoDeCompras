using CarritoC.Data;
using CarritoC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Threading.Tasks;
using static CarritoC.Controllers.CarritosController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarritoC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly CarritoContext _context;
        private readonly UserManager<Persona> _userManager;
        private readonly SignInManager<Persona> _signInManager;
        private readonly RoleManager <Rol> _roleManager;

    public AccountController(
        CarritoContext context,
        UserManager<Persona> userManager,
        SignInManager<Persona> signInManager,
        RoleManager<Rol> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Vista GET del login
        [AllowAnonymous]
        public IActionResult IniciarSesion(string returnurl)
        {
            TempData["returnurl"] = returnurl;
            return View();
        }

        // Acción POST del login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> IniciarSesion(Models.Login model)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    model.Recordarme,
                    false
                );

                if (resultado.Succeeded)
                {
                    string returnurl = TempData["returnurl"] as string;
                    if (returnurl is not null)
                    {
                        return Redirect(returnurl);
                    }
                    
                    // Sesión iniciada correctamente
                    return RedirectToAction("Index", "Home");
                }

                // Fallo de login
                ModelState.AddModelError(string.Empty, "Inicio de sesión incorrecto");
            }

            return View(model);
        }

        // Acción para cerrar sesión
      
        public async Task<IActionResult> CerrarSesion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("IniciarSesion");
        }

        // Vista GET del registro
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Registrar(Registrar model)
        {
            // Validar DNI duplicado
            if (await _context.Clientes.AnyAsync(c => c.DNI == model.DNI))
                ModelState.AddModelError("DNI", "Ya existe un cliente con ese DNI.");

            // Validar CUIT duplicado
            if (await _context.Clientes.AnyAsync(c => c.IdentificacionUnica == model.IdentificacionUnica))
                ModelState.AddModelError("IdentificacionUnica", "Ya existe un cliente con ese CUIT.");

            // Validar Email duplicado
            if (await _context.Clientes.AnyAsync(c => c.Email == model.Email))
                ModelState.AddModelError("Email", "Ya existe un cliente con ese Email.");

            // Si hay errores, volver a la vista
            if (!ModelState.IsValid)
                return View(model);

            // Crear el cliente
            Cliente cliente = new Cliente
            {
                UserName = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Email = model.Email,
                DNI = model.DNI,
                Telefono = model.Telefono,
                Direccion = model.Direccion,
                IdentificacionUnica = model.IdentificacionUnica,
                FechaAlta = DateTime.Now
            };

            try
            {
                var resultado = await _userManager.CreateAsync(cliente, model.Password);

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(cliente, "ClienteRol");
                    _context.Carritos.Add(new Carrito { Activo = true, Cliente = cliente });
                    await _context.SaveChangesAsync();

                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(cliente, isPersistent: false);

                    return RedirectToAction("Index", "Home");
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
                    if (!ModelState.ContainsKey("DNI"))
                        ModelState.AddModelError("DNI", "Ya existe un cliente con ese DNI.");
                  
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error inesperado: " + dbex.Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> CrearRoles()
        {
            await _roleManager.CreateAsync(new Rol("AdministradorRol"));
            await _roleManager.CreateAsync(new Rol("ClienteRol"));
            await _roleManager.CreateAsync(new Rol("EmpleadoRol"));

            return RedirectToAction("Index", "Home", new {mensaje = "Finalizado"});

        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}

