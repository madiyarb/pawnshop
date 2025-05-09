using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ContractActionTypes
    {
        public List<ContractActionTypeInfo> All()
        {
            var result = new List<ContractActionTypeInfo>();
            foreach (var value in Enum.GetValues(typeof(ContractActionType)))
            {
                var type = Enum.Parse<ContractActionType>(value.ToString());
                var fieldInfo = value.GetType().GetField(value.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                var name = descriptionAttributes == null ? string.Empty : (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();

                result.Add(new ContractActionTypeInfo
                {
                    Id = (int)type,
                    Code = Enum.GetName(typeof(ContractActionType), type),
                    Name = name
                });
            }

            return result;
        }
    }

    public class ContractActionTypeInfo : IDictionary
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
