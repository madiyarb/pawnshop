using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class AccountPlan : Pawnshop.AccountingCore.Models.AccountPlan, IEntity
    {
        public AccountPlan()
        {

        }
        public AccountPlan(Pawnshop.AccountingCore.Models.AccountPlan model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            OrganizationId = model.OrganizationId;
            IsActive = model.IsActive;
            IsBalance = model.IsBalance;
        }
    }
}
