using System.Security.Claims;

namespace CarritoC.Helpers
{
    public static class UsuariosManager
    {
        public static int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(int.TryParse(userIdClaim,out int userid))
            {
                return userid;
            }
            return null; //si falla
        }
    }
}
