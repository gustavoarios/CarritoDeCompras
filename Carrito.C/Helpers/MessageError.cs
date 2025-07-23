using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace CarritoC.Helpers
{
    public static class MessageError
    {
        public const string Requerido = "El campo {0} es requerido";
        public const string Comprendido = "El campo {0} debe tener entre {2} y {1} caracteres";
        public const string Rango = "El/La {0} debe estar entre {1} y {2}";
        public const string SoloNumeros = "El campo {0} solo admite numeros.";
        public const string SoloLetras = "El campo {0} solo admite caracteres de la A a la Z ";
        public const string NoValido = "Debe ingresar una direccion de email valida";
        public const string SoloLetrasAZ = @"^[a-zA-Z áéíóú]*";
        public const string SoloNumeros12 = @"^[0-9]+([.,][0-9]{1,2})?$";
        public const string NumeroMayorACero = "Cantidad no válida. Debe ser mayor a 0";
        public const string CantidadDebeSerEntera = "La cantidad debe ser un número entero positivo.";
        public const string PrecioValido = "El precio debe ser un número positivo.";
        public const string UserNameValido = "El nombre de usuario debe ser un correo electrónico válido.";
    }
}