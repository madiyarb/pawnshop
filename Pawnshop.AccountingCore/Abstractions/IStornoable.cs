using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IStornoable
    {
        int? StornoId { get; set; }
    }
}
