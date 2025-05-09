using Pawnshop.Core.Queries;

namespace Pawnshop.Data.Models.DebtorRegistry
{
    public class DebtorRegistriesQuery
    {
        public Page Page { get; set; }
        public DebtRegistryModel? Model { get; set; }
    }
}