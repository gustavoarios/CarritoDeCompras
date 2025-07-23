using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public bool Activo { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [RegularExpression(MessageError.SoloLetrasAZ, ErrorMessage = MessageError.SoloLetras)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [DataType(DataType.MultilineText)]
        [StringLength(int.MaxValue, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = MessageError.Requerido)]
        [Range(0.01, double.MaxValue, ErrorMessage = MessageError.PrecioValido)]
        [DataType(DataType.Currency)]
        public decimal PrecioVigente { get; set; }

        public string Imagen { get; set; }
        public Categoria Categoria { get; set; }
        public Marca Marca { get; set; }
        public List<StockItem> StockItems { get; set; }

        public List<CarritoItem> CarritoItems { get; set; }

        [Required]
        public int MarcaId { get; set; }

        [Required]
        public int CategoriaId { get; set; }

    }
}

