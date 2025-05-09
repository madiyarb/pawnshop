using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Dictionaries
{
    public class BranchService : IDictionaryWithSearchService<Group, BranchFilter>
    {
        private readonly GroupRepository _repository;

        public BranchService(GroupRepository repository)
        {
            _repository = repository;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public Task<Group> GetAsync(int id)
        {
            return Task.Run(() => _repository.Get(id));
        }

        public ListModel<Group> List(ListQueryModel<BranchFilter> listQuery)
        {
            return new ListModel<Group>()
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model),
            };
        }

        public ListModel<Group> List(ListQuery listQuery)
        {
            return new ListModel<Group>()
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery),
            };
        }

        public Group Save(Group model)
        {
            if (model.Id > 0) _repository.Update(model);
            else _repository.Insert(model);

            return model;
        }
    }
}