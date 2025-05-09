using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Services.CollateralTypes
{
    public class CollateralTypeService : ICollateralTypeService
    {
        public List<CollateralTypeInfo> GetCollateralTypes()
        {
            var collateralTypeValues = Enum.GetValues(typeof(CollateralType));
            var collateralTypes = new List<CollateralTypeInfo>();

            foreach (CollateralType value in collateralTypeValues)
            {
                var displayName = GetDisplayName(value);
                collateralTypes.Add(new CollateralTypeInfo { Value = (int)value, DisplayName = displayName });
            }

            return collateralTypes;
        }

        private string GetDisplayName(CollateralType collateralType)
        {
            var memberInfo = typeof(CollateralType).GetMember(collateralType.ToString()).FirstOrDefault();

            if (memberInfo != null)
            {
                if (memberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() is DisplayAttribute displayAttribute)
                {
                    return displayAttribute.Name;
                }
            }

            return collateralType.ToString();
        }
    }
}