using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class OrderTypes
    {
        public List<OrderTypeInfo> All()
        {
            var result = new List<OrderTypeInfo>();
            foreach (var value in Enum.GetValues(typeof(OrderType)))
            {
                var type = Enum.Parse<OrderType>(value.ToString());
                var fieldInfo = value.GetType().GetField(value.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                var name = descriptionAttributes == null ? string.Empty : (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
                var shortName = descriptionAttributes == null ? string.Empty : (descriptionAttributes.Length > 0) ? descriptionAttributes[0].ShortName : value.ToString();

                result.Add(new OrderTypeInfo
                {
                    Id = (int)type,
                    Code = Enum.GetName(typeof(OrderType), type),
                    Name = name,
                    ShortName = shortName
                });
            }

            return result;
        }
    }

    public class OrderTypeInfo : IDictionary
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
