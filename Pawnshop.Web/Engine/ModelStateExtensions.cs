using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Web.Engine
{
    public static class ModelStateExtensions
    {
        public static void Validate(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                var messages = modelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToArray();
                throw new PawnshopApplicationException(messages);
            }
        }
    }
}