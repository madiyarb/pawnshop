using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Dictionaries
{
    public class BlackoutService : IDictionaryWithSearchService<Blackout, BlackoutFilter>
    {
        private readonly IRepository<Blackout> _repository;

        public BlackoutService(IRepository<Blackout> repository)
        {
            _repository = repository;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public Task<Blackout> GetAsync(int id)
        {
            return Task.Run(() => _repository.Get(id));
        }

        public ListModel<Blackout> List(ListQueryModel<BlackoutFilter> listQuery)
        {
            return new ListModel<Blackout>()
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model),
            };
        }

        public ListModel<Blackout> List(ListQuery listQuery)
        {
            return new ListModel<Blackout>()
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery),
            };
        }

        public Blackout Save(Blackout model)
        {
            if (model.Id > 0) _repository.Update(model);
            else _repository.Insert(model);

            return model;
        }
    }
}