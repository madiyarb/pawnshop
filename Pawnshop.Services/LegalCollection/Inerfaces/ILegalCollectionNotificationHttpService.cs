using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationHistory;
using Pawnshop.Services.Models.List;
using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionNotificationHttpService
    {
        Task<ListModel<LegalCaseNotificationHistoryDto>> List();
        Task<ListModel<LegalCaseNotificationHistoryDto>> PagedList(int pageSize, int pageNumber);
        Task<int> Update(UpdateLegalCaseNotificationHistoryCommand command);
        Task<int> Create(CreateLegalCaseNotificationHistoryCommand command);
    }
}