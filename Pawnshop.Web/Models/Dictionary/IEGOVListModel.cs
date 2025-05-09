using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Dictionary
{
    public interface IEGOVListModel<T>
    {
        int TotalCount { get; set; }
        List<T> Data { get; set; }
    }
}
