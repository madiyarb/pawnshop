using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class AccountPlanSetting : Pawnshop.AccountingCore.Models.AccountPlanSetting, IEntity, ILoggableToEntity
    {
        public AccountPlanSetting()
        {

        }

        public AccountPlanSetting(Pawnshop.AccountingCore.Models.AccountPlanSetting model)
        {
            Id = model.Id;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            AccountSettingId = model.AccountSettingId;
            AccountPlanId = model.AccountPlanId;
            ContractTypeId = model.ContractTypeId;
            PeriodTypeId = model.PeriodTypeId;
            AccountId = model.AccountId;
            Account = model.Account;
            OrganizationId = model.OrganizationId;
            BranchId = model.BranchId;
        }

        public int GetLinkedEntityId()
        {
            return AccountPlanId ?? 0;
        }
    }
}
