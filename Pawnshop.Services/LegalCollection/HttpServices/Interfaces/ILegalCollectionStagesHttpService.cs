using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseStage;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionStagesHttpService
    {
        public Task<LegalCaseStageDto> Create(CreateLegalCaseStageCommand request);
        public Task<LegalCaseStageDto> Details(DetailsLegalCaseStageQuery request);
        public Task<LegalCaseStagesList> List();
        public Task<LegalCaseStageDto> Update(UpdateLegalCaseStageCommand request);
        public Task<int> Delete(DeleteLegalCaseStageCommand request);
    }
}