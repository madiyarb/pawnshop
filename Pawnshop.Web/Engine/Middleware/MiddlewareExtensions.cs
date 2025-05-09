using Microsoft.AspNetCore.Builder;

namespace Pawnshop.Web.Engine.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionContext(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionContextMiddleware>();
        }
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
        public static IApplicationBuilder UseBranchContext(this IApplicationBuilder app)
        {
            return app.UseMiddleware<BranchContextMiddleware>();
        }
    }
}