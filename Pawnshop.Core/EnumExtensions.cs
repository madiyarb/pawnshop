using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pawnshop.Core
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var enumMember = enumValue.GetType().GetMember(enumValue.ToString());

            DisplayAttribute displayAttribute = null;
            if (enumMember.Any())
            {
                displayAttribute = enumMember.First().GetCustomAttribute<DisplayAttribute>();
            }

            return displayAttribute?.Name;
        }
    }
}
