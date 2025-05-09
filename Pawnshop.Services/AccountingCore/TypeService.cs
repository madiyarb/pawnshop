using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.List;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Типы и их иерархия
    /// </summary>
    public class TypeService : IDictionaryService<Type>
    {
        private readonly TypeRepository _repository;

        public TypeService(TypeRepository repository)
        {
            _repository = repository;
        }

        public Type Save(Type model)
        {
            var m = new Data.Models.AccountingCore.Type(model);
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

        public async Task<Type> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<Type> List(ListQuery listQuery)
        {
            return new ListModel<Type>
            {
                List = _repository.List(listQuery).AsEnumerable<Type>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }
    }
}
