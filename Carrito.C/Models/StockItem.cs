using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class StockItem
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int SucursalId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = MessageError.Rango)]
        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        public int Cantidad { get; set; }

        public Producto Producto { get; set; }
        public Sucursal Sucursal { get; set; }
    }
}