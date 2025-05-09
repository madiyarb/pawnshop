using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionNotificationService
    {
        Task SendPrepaymentReceivedToLegalCase(int contractId, int userId);
    }
}