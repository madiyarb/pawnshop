using Pawnshop.Data.Models.Collection;
using System.Threading.Tasks;

namespace Pawnshop.Services.Collection
{
    public interface ICollectionService
    {
        CollectionModel GetCollection(int contractId);
        bool ChangeCollectionStatus(CollectionModel model);
        bool ParkingChangeStatus(int contractId);
        bool OverdueChangeStatus(CollectionOverdueContract contract = null);
        bool CloseContractCollection(CollectionClose close);
        void CancelCloseCollection(int contractId, int actionId);
        int GetFincoreStatus(string statusCode);
    }
}
