using System.ComponentModel.DataAnnotations;
using CarritoC.Helpers;

namespace CarritoC.Models
{
    public class Empleado : Persona
    {
        public int Legajo {  get; set; }
    }
}
