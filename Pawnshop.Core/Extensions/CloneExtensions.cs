using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Core.Extensions
{
    public static class CloneExtensions
    {
        public static T Clone<T>(this T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
