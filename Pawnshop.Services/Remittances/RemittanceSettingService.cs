using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Настройки переводов между филиалами
    /// </summary>
    public class RemittanceSettingService : IDictionaryWithSearchService<RemittanceSetting, RemittanceSettingFilter>
    {
        private readonly RemittanceSettingRepository _repository;

        public RemittanceSettingService(RemittanceSettingRepository repository)
        {
            _repository = repository;
        }

        public RemittanceSetting Save(RemittanceSetting model)
        {
            //var m = new Data.Models.AccountingCore.RemittanceSetting(model);
            if (model.Id > 0)
            {
                _repository.Update(model);
            }
            else
            {
                _repository.Insert(model);
            }

            //model.Id = m.Id;
            return model;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public async Task<RemittanceSetting> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<RemittanceSetting> List(ListQuery listQuery)
        {
            return new ListModel<RemittanceSetting>
            {
                List = _repository.List(listQuery).AsEnumerable<RemittanceSetting>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<RemittanceSetting> List(ListQueryModel<RemittanceSettingFilter> listQuery)
        {
            return new ListModel<RemittanceSetting>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<RemittanceSetting>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }
    }
}
