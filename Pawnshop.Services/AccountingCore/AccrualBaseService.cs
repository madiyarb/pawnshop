using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Базы начисления
    /// </summary>
    public class AccrualBaseService : IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter>
    {
        private readonly AccrualBaseRepository _repository;

        public AccrualBaseService(AccrualBaseRepository repository)
        {
            _repository = repository;
        }

        public AccrualBase Save(AccrualBase model)
        {
            var m = new Data.Models.AccountingCore.AccrualBase(model);
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

        public async Task<AccrualBase> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<AccrualBase> List(ListQuery listQuery)
        {
            return new ListModel<AccrualBase>
            {
                List = _repository.List(listQuery).AsEnumerable<AccrualBase>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<AccrualBase> List(ListQueryModel<AccrualBaseFilter> listQuery)
        {
            return new ListModel<AccrualBase>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<AccrualBase>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }
    }
}
