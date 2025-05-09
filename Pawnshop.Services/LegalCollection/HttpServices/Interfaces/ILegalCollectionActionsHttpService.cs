using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseAction;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionActionsHttpService
    {
        public Task<LegalCaseActionDto> Create(CreateLegalCaseActionsCommand request);
        public Task<LegalCaseActionDto> Details(LegalCaseActionDetailsQuery request);
        public Task<LegalCaseActionsList> List();
        public Task<LegalCaseActionDto> Update(UpdateLegalCaseActionsCommand request);
        public Task<int> Delete(DeleteLegalCaseActionCommand request);
    }
}