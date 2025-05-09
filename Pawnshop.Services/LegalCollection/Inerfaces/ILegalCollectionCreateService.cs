using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Create;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionCreateService
    {
        public int Create(CreateLegalCaseCommand request);
        public Task<int> CreateAsync(CreateLegalCaseCommand request);
    }
}