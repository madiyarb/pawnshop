using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Web.Engine.Security
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly UserRepository _userRepository;
        private readonly MemberRepository _memberRepository;
        private readonly SaltedHash _saltedHash;
        private readonly OrganizationRepository _organizationRepository;

        public const string PermissionClaim = "devman.Permission";

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            UserRepository userRepository, SaltedHash saltedHash, MemberRepository memberRepository,
            OrganizationRepository organizationRepository)
            : base(options, logger, encoder, clock)
        {
            _userRepository = userRepository;
            _saltedHash = saltedHash;
            _memberRepository = memberRepository;
            _organizationRepository = organizationRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            User user = null;
            List<Role> roles = null;
            Organization organization = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

                var username = credentials[0];
                var password = credentials[1];

                var existUser = _userRepository.Find(new { login = username });
                if (existUser != null)
                {
                    _userRepository.GetPasswordAndSalt(existUser.Id, out var pwd, out var salt);
                    var verified = _saltedHash.VerifyHashString(password, pwd, salt);
                    if (verified)
                    {
                        user = await _userRepository.FindAsync(new { login = existUser.Login });
                        organization = _organizationRepository.Get(user.OrganizationId);
                        if (organization != null && !organization.Locked)
                        {
                            roles = _memberRepository.Roles(user.Id, true);
                        }
                    }
                    else throw new PawnshopApplicationException("Invalid Username or Password");
                }
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if (user == null)
                return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Sid, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Login));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Login));
            claims.AddRange(PermissionsToClaims(roles.SelectMany(r => r.Permissions.Permissions).Distinct().ToArray()));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
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
