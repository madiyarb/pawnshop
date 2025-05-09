using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionCloseService
    {
        public Task CloseAsync(int contractId);
        public void Close(int contractId);
    }
}