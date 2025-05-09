using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Web.Engine.Middleware
{
    public class BranchContextMiddleware
    {
        private const string _branchHeader = "Branch";
        private readonly RequestDelegate _next;

        public BranchContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            StringValues branchValues;
            int brachId;
            if (context?.Request != null &&
                context.Request.Headers.TryGetValue(_branchHeader, out branchValues) &&
                branchValues.Count > 0 && int.TryParse(branchValues.First(), out brachId))
            {
                var scope = (ILifetimeScope)context.RequestServices.GetService(typeof(ILifetimeScope));
                if (scope == null)
                    throw new PawnshopApplicationException($"Сервис {nameof(ILifetimeScope)} не найден");

                var configurationContext = scope.Resolve<BranchContext>();
                configurationContext.Init(brachId);
            }

            await _next.Invoke(context);
        }
    }
}