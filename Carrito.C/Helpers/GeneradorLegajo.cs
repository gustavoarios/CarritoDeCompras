using System;
using Microsoft.EntityFrameworkCore;

namespace CarritoC.Helpers
{
    public class GeneradorLegajo
    {
        private static int _ultimoLegajo = 0;

        public static int GenerarNuevoLegajo()
        {
            _ultimoLegajo++;
            return _ultimoLegajo;
        }
        public static void ReiniciarContador()
        {
            _ultimoLegajo = 0;
        }
    }
}
