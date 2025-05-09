using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionCoursesHttpService
    {
        public Task<LegalCaseCourseDto> Create(CreateLegalCaseCourseCommand request);
        public Task<LegalCaseCourseDto> Details(int id);
        public Task<LegalCaseCourseList> List();
        public Task<LegalCaseCourseDto> Update(UpdateLegalCaseCourseCommand request);
        Task<int> Delete(DeleteLegalCaseCourseCommand request);
    }
}