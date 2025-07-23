using System;
using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class Registrar
    {
        [Display(AutoGenerateField = false)]
        public string UserName => Email;

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [StringLength(8, ErrorMessage = MessageError.Comprendido)]
        [Display(Name = "Documento")]
        public string DNI { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        [StringLength(15, MinimumLength = 8, ErrorMessage = MessageError.Comprendido)]
        public string Telefono { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = MessageError.Comprendido)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [Display(Name = "CUIT/CUIL")]
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string IdentificacionUnica { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [EmailAddress(ErrorMessage = MessageError.NoValido)]
        [Display(Name = "Correo Electronico")]
        public string Email { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+=\[{\]};:<>|./?,-]).{8,}$",
    ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula, un número y un carácter especial.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}