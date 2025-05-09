using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Interaction;
using System.Collections.Generic;

namespace Pawnshop.Services.Interactions
{
    public interface IInteractionService
    {
        public int Count(object query);
        public Interaction Create(Interaction interaction);
        public void Delete(int id);
        public Interaction Get(int id);
        public List<Interaction> GetList(ListQuery listQuery, object query);
        public Interaction Update(Interaction interaction);
    }
}
