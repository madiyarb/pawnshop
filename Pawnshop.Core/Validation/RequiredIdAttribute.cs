using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Core.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var intValue = (int)(value ?? 0);
            return intValue > 0;
        }
    }
}
