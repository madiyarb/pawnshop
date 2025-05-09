using MediatR;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class IsContractInHardCollectionQuery : IRequest<bool>
    {
        public int ContractId { get; set; }
    }
}
