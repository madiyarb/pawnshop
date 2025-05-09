using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ICancelCloseLegalCollectionService
    {
        public Task CancelCloseLegalCaseAsync(int contractId);
        public void CancelCloseLegalCase(int contractId);
    }
}