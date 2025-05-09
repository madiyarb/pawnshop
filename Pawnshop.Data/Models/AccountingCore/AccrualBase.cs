using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class AccrualBase : Pawnshop.AccountingCore.Models.AccrualBase, IEntity, ILoggableToEntity
    {
        public AccrualBase()
        {

        }

        public AccrualBase(Pawnshop.AccountingCore.Models.AccrualBase model)
        {
            Id = model.Id;
            AccrualType = model.AccrualType;
            BaseSettingId = model.BaseSettingId;
            AmountType = model.AmountType;
            Name = model.Name;
            NameAlt = model.NameAlt;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            IsActive = model.IsActive;
        }
        public int GetLinkedEntityId()
        {
            return BaseSettingId;
        }
    }
}
