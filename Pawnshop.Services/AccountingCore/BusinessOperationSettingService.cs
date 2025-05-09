using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Бизнес-операции
    /// </summary>
    public class BusinessOperationSettingService : IBusinessOperationSettingService
    {
        private readonly BusinessOperationSettingRepository _repository;

        public BusinessOperationSettingService(BusinessOperationSettingRepository repository)
        {
            _repository = repository;
        }

        public BusinessOperationSetting Save(BusinessOperationSetting model)
        {
            var m = new Data.Models.AccountingCore.BusinessOperationSetting(model);
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

        public async Task<BusinessOperationSetting> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public BusinessOperationSetting Get(int id)
        {
            return _repository.Get(id);
        }

        public BusinessOperationSetting GetByCode(string code)
        {
            var penaltyBOSettingsListQuery = new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    Code = code,
                    IsActive = true
                }
            };

            ListModel<BusinessOperationSetting> penaltyBOSettingsListModel = List(penaltyBOSettingsListQuery);
            if (penaltyBOSettingsListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(penaltyBOSettingsListModel)} не будет null");

            if (penaltyBOSettingsListModel.List == null)
                throw new PawnshopApplicationException(
                    $"Ожидалось что {nameof(penaltyBOSettingsListModel)}.{nameof(penaltyBOSettingsListModel.List)} не будет null");

            List<BusinessOperationSetting> penaltyBOSettings = penaltyBOSettingsListModel.List;
            if (penaltyBOSettings.Count > 1)
                throw new PawnshopApplicationException($"Найдены более одной настройки бизнес операции по коду {code}");

            return penaltyBOSettings.SingleOrDefault();
        }
        
        public ListModel<BusinessOperationSetting> List(ListQuery listQuery)
        {
            return new ListModel<BusinessOperationSetting>
            {
                List = _repository.List(listQuery).AsEnumerable<BusinessOperationSetting>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<BusinessOperationSetting> List(ListQueryModel<BusinessOperationSettingFilter> listQuery)
        {
            return new ListModel<BusinessOperationSetting>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<BusinessOperationSetting>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        public List<BusinessOperationSetting> ListOnly(ListQuery listQuery, object query = null)
        {
            return _repository.List(listQuery, query).AsEnumerable<BusinessOperationSetting>().ToList();
        }
    }
}
