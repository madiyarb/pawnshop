using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.AccountingCore
{
    public interface IAccountPlanSettingService : IDictionaryWithSearchService<AccountPlanSetting, AccountPlanSettingFilter>
    {
        AccountPlanSetting Find(int organizationId, int accountSettingId, int branchId, int contractTypeId,
            int periodTypeId);
    }
}
