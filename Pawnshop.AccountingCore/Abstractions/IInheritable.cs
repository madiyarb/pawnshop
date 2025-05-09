using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    interface IInheritable
    {
        int? ParentId { get; set; }
    }
}
