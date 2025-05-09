using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Core.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Проверка что Коллекция null или пуста
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }
    }
}