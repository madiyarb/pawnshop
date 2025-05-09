using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.ChangeCourse;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionChangeCourseService
    {
        public Task<List<ChangeCourseActionDto>> GetChangeCourseVariousAsync(int legalCaseId);
        public Task<List<LegalCaseDetailsResponse>> ChangeCourseAsync(ChangeLegalCaseCourseCommand request);
    }
}