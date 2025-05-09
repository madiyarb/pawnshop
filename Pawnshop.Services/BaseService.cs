using System.Data;
using System.Linq;
using Pawnshop.Core.Queries;
using Pawnshop.Services.Models.List;
using System.Threading.Tasks;
using Pawnshop.Core;

namespace Pawnshop.Services
{
    public class BaseService<T> : IBaseService<T>, IService where T : IEntity
    {
        protected readonly IRepository<T> _repository;
        public BaseService(IRepository<T> repository)
        {
            _repository = repository;
        }
        virtual public void Delete(int id)
        {
            _repository.Delete(id);
        }

        virtual public ListModel<T> List(ListQuery listQuery)
        {
            return new ListModel<T>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        virtual public ListModel<T> List(ListQuery listQuery, object query = null)
        {
            return new ListModel<T>
            {
                List = _repository.List(listQuery, query),
                Count = _repository.Count(listQuery, query)
            };
        }

        virtual public T Get(int id)
        {
            return _repository.Get(id);
        }

        virtual public T Save(T model)
        {
            if (model.Id > 0)
                _repository.Update(model);
            else
                _repository.Insert(model);

            return model;
        }

        virtual public T Find(object query) => _repository.Find(query);

        public IDbTransaction BeginTransaction() => _repository.BeginTransaction();
    }
}