using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionChangeCourseHttpService
    {
        public Task<ChangeCourseActionsResponse> List();
    }
}