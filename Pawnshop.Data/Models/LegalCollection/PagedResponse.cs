using System.Collections.Generic;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class PagedResponse<TResponse>
    {
        public int Count { get; set; }
        public IEnumerable<TResponse> List { get;  set;   }
    }
}