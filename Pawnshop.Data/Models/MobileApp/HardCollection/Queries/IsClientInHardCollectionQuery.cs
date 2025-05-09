using MediatR;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class IsClientInHardCollectionQuery : IRequest<bool>
    {
        public int ClientId { get; set; }
    }
}
