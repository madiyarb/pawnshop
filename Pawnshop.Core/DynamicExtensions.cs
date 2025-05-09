using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;

namespace Pawnshop.Core
{
    public static class DynamicExtensions
    {
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static T Val<T>(this object value, string propertyName, T defVal = default(T))
        {
            if (value == null) return defVal;

            var type = value.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null) return defVal;

            var val = property.GetValue(value);
            return (T)val;
        }
    }
}
