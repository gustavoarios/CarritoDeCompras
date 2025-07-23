using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public Cliente Cliente { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Fecha { get; set; }
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        public decimal Total { get; set; }

        public Sucursal Sucursal { get; set; }
        public Carrito Carrito { get; set; }
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int SucursalId { get; set; }

        [Required]
        public int CarritoId { get; set; }
    }
}
