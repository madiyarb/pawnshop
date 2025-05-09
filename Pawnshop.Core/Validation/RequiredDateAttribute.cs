using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Core.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var dateTimeValue = (DateTime)(value ?? DateTime.MinValue);
            return dateTimeValue != DateTime.MinValue;
        }
    }
}
