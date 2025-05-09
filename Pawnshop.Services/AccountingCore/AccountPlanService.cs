using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// План счетов
    /// </summary>
    public class AccountPlanService : IDictionaryService<AccountPlan>
    {
        private readonly AccountPlanRepository _repository;

        public AccountPlanService(AccountPlanRepository repository)
        {
            _repository = repository;
        }

        public AccountPlan Save(AccountPlan model)
        {
            var m = new Data.Models.AccountingCore.AccountPlan(model);
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

        public async Task<AccountPlan> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<AccountPlan> List(ListQuery listQuery)
        {
            return new ListModel<AccountPlan>
            {
                List = _repository.List(listQuery).AsEnumerable<AccountPlan>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }
    }
}
