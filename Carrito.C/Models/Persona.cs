using System;
using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;
using Microsoft.AspNetCore.Identity;

namespace CarritoC.Models
{
    public abstract class Persona : IdentityUser<int>
    {
        //public int Id { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [StringLength(50, MinimumLength = 3, ErrorMessage = MessageError.Comprendido)]
        [EmailAddress(ErrorMessage = MessageError.UserNameValido)]
        public override string UserName
        {
            get { return base.UserName; }
            set { base.UserName = value; }
        }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Apellido { get; set; }

        [StringLength(8, ErrorMessage = MessageError.Comprendido)]
        [Display(Name = "Documento")]
        public string DNI { get; set; }


        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        [StringLength(15, MinimumLength = 8, ErrorMessage = MessageError.Comprendido)]
        public string Telefono { get; set; }


        [StringLength(100, MinimumLength = 1, ErrorMessage = MessageError.Comprendido)]
        public string Direccion { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime FechaAlta { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [EmailAddress(ErrorMessage = MessageError.NoValido)]
        [Display(Name = "Correo Electrónico")]
        public override string Email
        {
            get { return base.Email; }
            set { base.Email = value; }
        }
    }
}
