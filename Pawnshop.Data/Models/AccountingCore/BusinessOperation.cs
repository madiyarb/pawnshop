using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class BusinessOperation : Pawnshop.AccountingCore.Models.BusinessOperation, IEntity
    {
        public BusinessOperation()
        {

        }

        public BusinessOperation(Pawnshop.AccountingCore.Models.BusinessOperation model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            TypeId = model.TypeId;
            OrganizationId = model.OrganizationId;
            BranchId = model.BranchId;
            IsManual = model.IsManual;
            OrdersCreateStatus = model.OrdersCreateStatus;
            HasExpenseArticleType = model.HasExpenseArticleType;
        }
    }
}
