using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Documents;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionDocumentsService
    {
        public Task<int> UploadDocumentAsync(UploadLegalCaseDocumentCommand request);
        public Task<int> DeleteDocumentAsync(DeleteLegalCaseDocumentCommand request);
    }
}