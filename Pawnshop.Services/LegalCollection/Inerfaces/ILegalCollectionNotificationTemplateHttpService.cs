using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationTemplate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionNotificationTemplateHttpService
    {
        Task<LegalCaseNotificationTemplateDto> Create(CreateLegalCaseNotificationTemplateCommand request);
        Task<LegalCaseNotificationTemplateDto> Card(LegalCaseNotificationTemplateCardQuery query);
        Task<LegalCaseNotificationTemplateDto> Update(UpdateLegalCaseNotificationTemplateCommand request);
        Task<int> Delete(DeleteLegalCaseNotificationTemplateCommand request);
        Task<List<LegalCaseNotificationTemplateDto>> List();
    }
}
