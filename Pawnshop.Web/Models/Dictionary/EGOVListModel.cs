using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Dictionary
{
    public class EGOVListModel<T> : IEGOVListModel<T>
    {
        public int TotalCount { get; set; }
        public List<T> Data { get; set; }
    }
}
