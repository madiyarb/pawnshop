using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class PaymentOrder : Pawnshop.AccountingCore.Models.PaymentOrder, IEntity, ILoggableToEntity
    {
        public PaymentOrder()
        {

        }

        public PaymentOrder(Pawnshop.AccountingCore.Models.PaymentOrder model)
        {
            Id = model.Id;
            SequenceNumber = model.SequenceNumber;
            AccountSettingId = model.AccountSettingId;
            NotOnScheduleDateAllowed = model.NotOnScheduleDateAllowed;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            IsActive = model.IsActive;
        }
        public int GetLinkedEntityId()
        {
            return AccountSettingId;
        }
    }
}
