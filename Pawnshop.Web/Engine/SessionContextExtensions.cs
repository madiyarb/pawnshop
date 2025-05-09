using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Pawnshop.Core;
using Pawnshop.Web.Engine.Security;

namespace Pawnshop.Web.Engine
{
    public static class SessionContextExtensions
    {
        public static void InitFromClaims(this ISessionContext sessionContext, Claim[] claims)
        {
            sessionContext.Init(
                GetInt(claims, ClaimTypes.Sid),
                GetStr(claims, ClaimTypes.Name),
                GetInt(claims, TokenProvider.OIdClaim),
                GetStr(claims, TokenProvider.ONameClaim),
                GetStr(claims, TokenProvider.OUidClaim),
                GetBool(claims, TokenProvider.ForSupportClaim),
                GetList(claims, TokenProvider.PermissionClaim));
        }

        private static string[] GetList(IEnumerable<Claim> claims, string claimName)
        {
            return claims.Where(c => c.Type == claimName).Select(c => c.Value).ToArray();
        }

        private static string GetStr(IEnumerable<Claim> claims, string claimName)
        {
            var claim = claims.FirstOrDefault(c => c.Type == claimName);
            return claim?.Value;
        }

        private static int GetInt(IEnumerable<Claim> claims, string claimName)
        {
            var claim = claims.FirstOrDefault(c => c.Type == claimName);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private static bool GetBool(IEnumerable<Claim> claims, string claimName)
        {
            var claim = claims.FirstOrDefault(c => c.Type == claimName);
            return claim != null ? bool.Parse(claim.Value) : false;
        }
    }
}