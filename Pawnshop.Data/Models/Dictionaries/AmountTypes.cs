using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class AmountTypes
    {
        public List<AmountTypeInfo> All()
        {
            var result = new List<AmountTypeInfo>();
            foreach (var value in Enum.GetValues(typeof(AmountType)))
            {
                var type = Enum.Parse<AmountType>(value.ToString());
                var fieldInfo = value.GetType().GetField(value.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                var name = descriptionAttributes == null ? string.Empty : (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();

                result.Add(new AmountTypeInfo
                {
                    Id = (int)type,
                    Code = Enum.GetName(typeof(AmountType), type),
                    Name = name
                });
            }

            return result;
        }
    }

    public class AmountTypeInfo : IDictionary
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
