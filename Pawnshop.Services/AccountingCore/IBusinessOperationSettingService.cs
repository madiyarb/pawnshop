using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Services.Models.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.AccountingCore
{
    public interface IBusinessOperationSettingService: IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter>
    {
        BusinessOperationSetting Get(int id);
        BusinessOperationSetting GetByCode(string code);
        List<BusinessOperationSetting> ListOnly(ListQuery listQuery, object query = null);
    }
}