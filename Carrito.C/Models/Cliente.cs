using System.Collections.Generic;
using CarritoC.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CarritoC.Models
{
    public class Cliente : Persona
    {
       
        public List<Carrito> Carritos { get; set; }
        public List<Compra> Compras { get; set; }

        [Display(Name = "CUIT/CUIL")]
        [RegularExpression(MessageError.SoloNumeros12, ErrorMessage = MessageError.SoloNumeros)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MessageError.Comprendido)]
        public string IdentificacionUnica { get; init; }

    }
}
