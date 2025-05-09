using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Настройки счетов
    /// </summary>
    public class AccountSettingService : IDictionaryWithSearchService<AccountSetting, AccountSettingFilter>
    {
        private readonly AccountSettingRepository _repository;
        private readonly IDictionaryWithSearchService<AccountPlanSetting, AccountPlanSettingFilter> _accountPlanSettingService;

        public AccountSettingService(AccountSettingRepository repository, 
            IDictionaryWithSearchService<AccountPlanSetting, AccountPlanSettingFilter> accountPlanSettingService)
        {
            _repository = repository;
            _accountPlanSettingService = accountPlanSettingService;
        }

        public AccountSetting Save(AccountSetting model)
        {
            var m = new Data.Models.AccountingCore.AccountSetting(model);
            if (model.Id > 0)
            {
                _repository.Update(m);
            }
            else
            {
                _repository.Insert(m);
            }

            model.Id = m.Id;
            return model;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public async Task<AccountSetting> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<AccountSetting> List(ListQuery listQuery)
        {
            return new ListModel<AccountSetting>
            {
                List = _repository.List(listQuery).AsEnumerable<AccountSetting>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<AccountSetting> List(ListQueryModel<AccountSettingFilter> listQuery)
        {
            return new ListModel<AccountSetting>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<AccountSetting>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }
    }
}