using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class AccountSetting : Pawnshop.AccountingCore.Models.AccountSetting, IEntity
    {
        public AccountSetting()
        {

        }

        public AccountSetting(Pawnshop.AccountingCore.Models.AccountSetting model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            IsConsolidated = model.IsConsolidated;
            TypeId = model.TypeId;
            DefaultAmountType = model.DefaultAmountType;
        }
    }
}
