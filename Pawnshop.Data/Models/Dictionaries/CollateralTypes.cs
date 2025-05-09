using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class CollateralTypes
    {
        public List<CollateralTypesInfo> All()
        {
            var result = new List<CollateralTypesInfo>();
            foreach (var value in Enum.GetValues(typeof(CollateralType)))
            {
                var type = Enum.Parse<CollateralType>(value.ToString());
                var fieldInfo = value.GetType().GetField(value.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                var name = descriptionAttributes == null ? string.Empty : (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();

                result.Add(new CollateralTypesInfo
                {
                    Id = (int)type,
                    Name = name,
                });
            }

            return result;
        }
    }

    public class CollateralTypesInfo : IDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
