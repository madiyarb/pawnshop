using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Настройки планов счетов
    /// </summary>
    public class AccountPlanSettingService : IAccountPlanSettingService
    {
        private readonly AccountPlanSettingRepository _repository;
        private readonly IDictionaryService<Type> _typeService;

        public AccountPlanSettingService(AccountPlanSettingRepository repository,
            IDictionaryService<Type> typeService)
        {
            _repository = repository;
            _typeService = typeService;
        }

        public AccountPlanSetting Save(AccountPlanSetting model)
        {
            var m = new Data.Models.AccountingCore.AccountPlanSetting(model);
            if (model.Id > 0)
            {
                _repository.Update(m);
            }
            else
            {
                _repository.Insert(m);
            }

            model.Id = m.Id;
            return m;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public async Task<AccountPlanSetting> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<AccountPlanSetting> List(ListQuery listQuery)
        {
            return new ListModel<AccountPlanSetting>
            {
                List = _repository.List(listQuery).AsEnumerable<AccountPlanSetting>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<AccountPlanSetting> List(ListQueryModel<AccountPlanSettingFilter> listQuery)
        {

            return new ListModel<AccountPlanSetting>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<AccountPlanSetting>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        public AccountPlanSetting Find(int organizationId, int accountSettingId, int branchId, int contractTypeId, int periodTypeId)
        {
            var contractType = _typeService.GetAsync(contractTypeId).Result;
            var periodType = _typeService.GetAsync(periodTypeId).Result;

            var tempContractType = contractType;
            var tempPeriodType = periodType;

            var accountPlanSetting = List(new ListQueryModel<AccountPlanSettingFilter>()
            {
                Page = null,
                Model = new AccountPlanSettingFilter
                {
                    AccountSettingId = accountSettingId,
                    OrganizationId = organizationId,
                    BranchId = branchId,
                    ContractTypeId = tempContractType.Id,
                    PeriodTypeId = tempPeriodType.Id
                }
            });

            if (accountPlanSetting.List.Any()) return accountPlanSetting.List.FirstOrDefault();

            while (true)
            {
                tempPeriodType = periodType;

                accountPlanSetting = List(new ListQueryModel<AccountPlanSettingFilter>()
                {
                    Page = null,
                    Model = new AccountPlanSettingFilter
                    {
                        AccountSettingId = accountSettingId,
                        OrganizationId = organizationId,
                        BranchId = branchId,
                        ContractTypeId = tempContractType.Id,
                        PeriodTypeId = tempPeriodType.Id
                    }
                });

                if (accountPlanSetting.List.Any()) return accountPlanSetting.List.FirstOrDefault();

                var isNotParent = tempPeriodType.ParentId.HasValue;

                while (isNotParent)
                {
                    accountPlanSetting = List(new ListQueryModel<AccountPlanSettingFilter>()
                    {
                        Page = null,
                        Model = new AccountPlanSettingFilter
                        {
                            AccountSettingId = accountSettingId,
                            OrganizationId = organizationId,
                            BranchId = branchId,
                            ContractTypeId = tempContractType.Id,
                            PeriodTypeId = tempPeriodType.Id
                        }
                    });

                    if (accountPlanSetting.List.Any())
                    {
                        if (accountPlanSetting.List.Any()) return accountPlanSetting.List.FirstOrDefault();
                    }
                    else if (!accountPlanSetting.List.Any() && tempPeriodType.ParentId.HasValue)
                    {
                        tempPeriodType = tempPeriodType.Parent;
                    }
                    else if (!accountPlanSetting.List.Any() && !tempPeriodType.ParentId.HasValue)
                    {
                        isNotParent = false;
                    }
                }

                if (tempContractType.ParentId.HasValue)
                {
                    tempContractType = tempContractType.Parent;
                }
                else return null;
            }

        }
    }
}
