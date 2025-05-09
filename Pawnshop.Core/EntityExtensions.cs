using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Core
{
    public static class EntityExtensions
    {
        public static List<T> Diff<T>(this List<T> source, List<T> target) where T : IEntity
        {
            if (source == null) return new List<T>();
            if (target == null) return source;

            return source.Where(s => target.All(t => t.Id != s.Id)).ToList();
        }
    }
}