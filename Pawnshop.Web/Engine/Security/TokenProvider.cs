using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Web.Engine.Security
{
    public class TokenProvider
    {
        public const string OIdClaim = "devman.OId";
        public const string OUidClaim = "devman.OUid";
        public const string ONameClaim = "devman.OName";
        public const string ForSupportClaim = "devman.ForSupport";
        public const string PermissionClaim = "devman.Permission";

        private readonly JwtIssuerOptions _options;

        public TokenProvider(IOptions<JwtIssuerOptions> options)
        {
            _options = options.Value;
        }

        public string CreateToken(User user, Organization organization, Role[] roles)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Sid, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Login));
            claims.Add(new Claim(OIdClaim, organization.Id.ToString()));
            claims.Add(new Claim(OUidClaim, organization.Uid));
            claims.Add(new Claim(ONameClaim, organization.Name));
            claims.Add(new Claim(ForSupportClaim, user.ForSupport.ToString()));
            claims.AddRange(PermissionsToClaims(roles.SelectMany(r => r.Permissions.Permissions).Distinct().ToArray()));

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: _options.NotBefore,
                expires: _options.Expiration,
                signingCredentials: _options.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateApplicationToken(User user, Organization organization, Role[] roles)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Sid, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Login));
            claims.Add(new Claim(OIdClaim, organization.Id.ToString()));
            claims.Add(new Claim(OUidClaim, organization.Uid));
            claims.Add(new Claim(ONameClaim, organization.Name));
            claims.Add(new Claim(ForSupportClaim, user.ForSupport.ToString()));
            claims.AddRange(PermissionsToClaims(roles.SelectMany(r => r.Permissions.Permissions).Distinct().ToArray()));

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: _options.NotBefore,
                expires: DateTime.Today.AddYears(10),
                signingCredentials: _options.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken ReadToken(string tokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);
            return token;
        }

        private Claim[] PermissionsToClaims(PermissionDefinition[] permissions)
        {
            var deniedPermissions = permissions
                .Where(p => p.GrantType == GrantType.Forbidden)
                .Select(p => p.Name).ToArray();

            var result = permissions.Where(p => deniedPermissions.All(d => d != p.Name))
                .Select(p => new Claim(PermissionClaim, p.Name))
                .ToArray();

            return result;
        }
    }
}