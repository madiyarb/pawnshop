using System.Collections.Generic;

namespace Pawnshop.Data.Models
{
    public sealed class BaseListView<T>
    {
        public IEnumerable<T> List { get; set; }
        public int Count { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}
