using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class CarritoItem
    {
        public Carrito Carrito { get; set; }
        public Producto Producto { get; set; }

        public decimal ValorUnitario { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [Range(1, int.MaxValue, ErrorMessage = MessageError.NumeroMayorACero)]
        [RegularExpression(@"^\d+$", ErrorMessage = MessageError.CantidadDebeSerEntera)]
        public int Cantidad { get; set; }

        [Key]
        public int CarritoId { get; set; }

        [Key]
        public int ProductoId { get; set; }
        public decimal SubTotal => (ValorUnitario * Cantidad);

    }
}
