using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Pawnshop.Web.Extensions
{
    internal static class UrlHelperExtensions
    {
        public static string UrlToAction<TController>(this ControllerBase controllerBase, string action, object values)
            where TController : ControllerBase
        {
            var controllerTypeName = typeof(TController).Name;

            if (!controllerTypeName.EndsWith("Controller"))
                throw new ArgumentException("Type name must ended `Controller` string");

            var controllerName = controllerTypeName[..^"Controller".Length];

            return controllerBase.Url.Action(new UrlActionContext
            {
                Action = action,
                Controller = controllerName,
                Values = values,
                Protocol = controllerBase.Request.Scheme,
                Host = controllerBase.Request.Host.ToString(),
                Fragment = null
            })!;
        }

        public static string BaseUrl(this ControllerBase controllerBase)
        {
            return (controllerBase.Request.Scheme + "://" + controllerBase.Request.Host).Trim('/') + "/";
        }
    }
}
