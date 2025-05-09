using System.Collections.Generic;

namespace Pawnshop.Services.Models.List
{
    public class ListModel<T>
    {
        public int Count { get; set; }

        public List<T> List { get; set; } 
    }
}