using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;

namespace Pawnshop.Core
{
    public interface IRepository
    {
        IDbTransaction BeginTransaction();
    }

    public interface IRepository<T> : IRepository where T : IEntity
    {
        void Insert(T entity);
        void Update(T entity);
        void Delete(int id);
        T Get(int id);
        T Find(object query);
        List<T> List(ListQuery listQuery, object query = null);
        int Count(ListQuery listQuery, object query = null);
    }
}