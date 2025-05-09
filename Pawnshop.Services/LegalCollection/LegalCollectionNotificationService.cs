using System.Threading.Tasks;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionNotificationService : ILegalCollectionNotificationService
    {
        private readonly ILegalCaseContractsStatusRepository _legalCaseContracts;
        private readonly ILegalCollectionNotificationHttpService _httpNotificationService;

        public LegalCollectionNotificationService(ILegalCaseContractsStatusRepository legalCaseContracts,
            ILegalCollectionNotificationHttpService httpNotificationService)
        {
            _legalCaseContracts = legalCaseContracts;
            _httpNotificationService = httpNotificationService;
        }

        public async Task SendPrepaymentReceivedToLegalCase(int contractId, int userId)
        {
            var legalCaseContract = await _legalCaseContracts.GetContractLegalCaseAsync(contractId);
            if (legalCaseContract != null)
            {
                await _httpNotificationService.Create(
                    new HttpServices.Dtos.LegalCaseNotificationHistory.CreateLegalCaseNotificationHistoryCommand
                    {
                        NotificationTemplateCode = "ADVANCE_PAYMENT_FOR_CASE",
                        CreatedBy = userId,
                        LegalCaseId = legalCaseContract.LegalCaseId
                    });
            }
        }
    }
}