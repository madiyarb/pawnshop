using Pawnshop.Core;
using System.Security.Claims;

namespace Pawnshop.Data.Helpers
{
    internal static class UserExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            int userId;

            if (int.TryParse(claimsPrincipal.FindFirst(x => x.Type.Equals(ClaimTypes.Sid))?.Value, out userId))
            {
                return userId;
            }
            else
            {
                return Constants.ADMINISTRATOR_IDENTITY;
            }
        }

    }
}
