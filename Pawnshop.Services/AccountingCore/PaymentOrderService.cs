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
    /// Порядок погашения
    /// </summary>
    public class PaymentOrderService : IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter>
    {
        private readonly PaymentOrderRepository _repository;

        public PaymentOrderService(PaymentOrderRepository repository)
        {
            _repository = repository;
        }

        public PaymentOrder Save(PaymentOrder model)
        {
            var m = new Data.Models.AccountingCore.PaymentOrder(model);
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

        public async Task<PaymentOrder> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<PaymentOrder> List(ListQuery listQuery)
        {
            return new ListModel<PaymentOrder>
            {
                List = _repository.List(listQuery).AsEnumerable<PaymentOrder>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<PaymentOrder> List(ListQueryModel<PaymentOrderFilter> listQuery)
        {
            return new ListModel<PaymentOrder>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<PaymentOrder>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }
    }
}
