using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CarritoC.Models
{
    public class Carrito
    {
        public int Id { get; set; }
        public bool Activo { get; set; }
        public Cliente Cliente { get; set; }
        public List<CarritoItem> CarritoItems { get; set; }
        public Compra Compra { get; set; }
        public decimal Subtotal => CarritoItems?.Sum(item => item.SubTotal) ?? 0m;
        [Required]
        public int ClienteId { get; set; }

        
         
    }
}
