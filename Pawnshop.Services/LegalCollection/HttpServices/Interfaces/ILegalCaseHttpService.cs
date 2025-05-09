using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Action.HttpService;
using Pawnshop.Data.Models.LegalCollection.Create;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;
using Pawnshop.Data.Models.LegalCollection.Documents;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.GetFiltered.HttpService;
using Pawnshop.Data.Models.LegalCollection.HttpService;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCaseHttpService
    {
        public Task<int> CreateLegalCaseHttpRequest(CreateLegalCaseCommand request);
        public Task<LegalCaseActionOptionsResponse> GetLegalCaseActionOptions(int legalCaseId);
        public Task<List<LegalCaseDetailsResponse>> GetLegalCaseDetails(int legalCaseId);
        public Task<PagedResponse<FilteredLegalCasesResponse>> GetFilteredLegalCase(FilteredHttpRequest request);
        public Task<List<LegalCaseDetailsResponse>> UpdateLegalCase(UpdateLegalCaseCommand request);
        public Task<int> CloseLegalCaseHttpRequest(int legalCaseId);
        Task<int> CloseLegalCaseHttpRequest(int legalCaseId, int authorId);
        Task<int> CancelLegalCaseHttpRequest(int legalCaseId);
        public Task<int> CreateLegalCaseDocument(CreateLegalCaseDocumentCommand command);
        public Task<int> DeleteLegalCaseDocument(int documentId);
        public Task<List<LegalCaseDetailsResponse>> RollbackLegalCaseHttpRequest(int legalCaseId);
        public Task<List<ChangeCourseActionDto>> GetChangeCourseVarious(int legalCaseId);
        public Task<List<LegalCaseDetailsResponse>> ChangeCourse(ChangeLegalCaseCourseRequest request);
    }
}