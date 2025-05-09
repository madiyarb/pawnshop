using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.CashOrders
{
    public class ExpenseArticleType : Pawnshop.AccountingCore.Models.ExpenseArticleType, IEntity
    {
        public ExpenseArticleType()
        {
        }

        public ExpenseArticleType(Pawnshop.AccountingCore.Models.ExpenseArticleType model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
        }
    }
}
