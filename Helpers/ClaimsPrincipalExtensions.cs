using System.Security.Claims;

namespace CosmeticPharmacy.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
       
        public static int? GetCustomerId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out int id) ? id : null;
        }
    }
}
