using System.Security.Claims;

namespace CosmeticPharmacy.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>معرّف العميل الحالي من الـ Claims (بديل Session["UserId"] في المشروع القديم).</summary>
        public static int? GetCustomerId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out int id) ? id : null;
        }
    }
}
