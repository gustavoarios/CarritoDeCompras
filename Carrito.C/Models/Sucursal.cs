using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class Sucursal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Nombre { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = MessageError.Comprendido)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        [StringLength(15, MinimumLength = 8, ErrorMessage = MessageError.Comprendido)]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = MessageError.NoValido)]
        public string Email { get; set; }
        public bool Activa { get; set; }
        public List<StockItem> StockItems { get; set; }
        public List<Compra> Compras { get; set; }
    }
}
