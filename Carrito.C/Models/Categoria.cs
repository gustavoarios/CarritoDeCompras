using System.Collections.Generic;
using CarritoC.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CarritoC.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [DataType(DataType.MultilineText)]
        [StringLength(int.MaxValue, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Descripcion { get; set; }

        public List<Producto> Productos { get; set; } 
        
    }
}
