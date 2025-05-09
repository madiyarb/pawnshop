using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface ICreateLogged
    {
        int AuthorId { get; set; }
        DateTime CreateDate { get; set; }
    }
}
