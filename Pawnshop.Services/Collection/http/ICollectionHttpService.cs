using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Collection.http
{
    public interface ICollectionHttpService<T> where T : class
    {
        Task<List<T>> List();
        Task<T> Get(string id);
        Task<List<T>> GetByContractId(string contractId);
        Task<int> Create(T item);
        Task<int> Update(T item);
        Task<int> Delete(string id);

    }
}
