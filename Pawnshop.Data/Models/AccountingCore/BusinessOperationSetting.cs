using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class BusinessOperationSetting : Pawnshop.AccountingCore.Models.BusinessOperationSetting, IEntity, ILoggableToEntity
    {
        public BusinessOperationSetting()
        {

        }

        public BusinessOperationSetting(Pawnshop.AccountingCore.Models.BusinessOperationSetting model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            BusinessOperationId = model.BusinessOperationId;
            OrderBy = model.OrderBy;
            DebitSettingId = model.DebitSettingId;
            CreditSettingId = model.CreditSettingId;
            AmountType = model.AmountType;
            Reason = model.Reason;
            ReasonKaz = model.ReasonKaz;
            IsActive = model.IsActive;
            PayTypeId = model.PayTypeId;
            OrderType = model.OrderType;
            DefaultArticleTypeId = model.DefaultArticleTypeId;
        }
        public int GetLinkedEntityId()
        {
            return BusinessOperationId;
        }
    }
}
