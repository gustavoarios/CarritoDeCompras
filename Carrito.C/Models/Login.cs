using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class Login
    {
        [Required(ErrorMessage = MessageError.Requerido)]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool Recordarme { get; set; } = false;
    }
}