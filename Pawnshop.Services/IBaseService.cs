using System.Data;
using Pawnshop.Core.Queries;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services
{
    public interface IBaseService<T>
    {
        T Save(T model);
        T Find(object query);
        void Delete(int id);
        T Get(int id);
        ListModel<T> List(ListQuery listQuery);
        ListModel<T> List(ListQuery listQuery, object query = null);
        public IDbTransaction BeginTransaction();
    }
}