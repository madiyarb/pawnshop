using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Dictionaries
{
    /// <summary>
    /// Статьи расхода
    /// </summary>
    public class ExpenseArticleTypeService : IDictionaryService<ExpenseArticleType>
    {
        private readonly ExpenseArticleTypeRepository _repository;

        public ExpenseArticleTypeService(ExpenseArticleTypeRepository repository)
        {
            _repository = repository;
        }

        public ExpenseArticleType Save(ExpenseArticleType model)
        {
            var m = new Pawnshop.Data.Models.CashOrders.ExpenseArticleType(model);
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

        public async Task<ExpenseArticleType> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<ExpenseArticleType> List(ListQuery listQuery)
        {
            return new ListModel<ExpenseArticleType>
            {
                List = _repository.List(listQuery).AsEnumerable<ExpenseArticleType>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }
    }
}
