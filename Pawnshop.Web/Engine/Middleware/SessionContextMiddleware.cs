using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using Pawnshop.Core;

namespace Pawnshop.Web.Engine.Middleware
{
    public class SessionContextMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context?.User?.Identity is ClaimsIdentity && context.User.Identity.IsAuthenticated)
            {
                var scope = (ILifetimeScope) context.RequestServices.GetService(typeof(ILifetimeScope));
                var sessionContext = scope.Resolve<ISessionContext>();

                var identity = (ClaimsIdentity)context.User.Identity;
                sessionContext.InitFromClaims(identity.Claims.ToArray());
            }

            await _next.Invoke(context);
        }
    }
}