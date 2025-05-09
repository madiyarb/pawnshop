using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services
{
    public interface IDictionaryService<T>
    {
        T Save(T model);
        void Delete(int id);
        Task<T> GetAsync(int id);
        ListModel<T> List(ListQuery listQuery);
    }
}
