using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Interaction;
using System.Collections.Generic;

namespace Pawnshop.Services.Interactions
{
    public class InteractionService : IInteractionService
    {
        private readonly InteractionsRepository _interactionsRepository;

        public InteractionService(InteractionsRepository interactionsRepository)
        {
            _interactionsRepository = interactionsRepository;
        }

        public int Count(object query)
        {
            return _interactionsRepository.Count(null, query);
        }

        public Interaction Create(Interaction interaction)
        {
            _interactionsRepository.Insert(interaction);

            return _interactionsRepository.Get(interaction.Id);
        }

        public void Delete(int id)
        {
            _interactionsRepository.Delete(id);
        }

        public Interaction Get(int id)
        {
            return _interactionsRepository.Get(id);
        }

        public List<Interaction> GetList(ListQuery listQuery,object query)
        {
            return _interactionsRepository.List(listQuery, query);
        }

        public Interaction Update(Interaction interaction)
        {
            _interactionsRepository.Update(interaction);

            return _interactionsRepository.Get(interaction.Id);
        }
    }
}
