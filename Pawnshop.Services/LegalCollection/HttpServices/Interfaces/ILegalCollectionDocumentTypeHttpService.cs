using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionDocumentTypeHttpService
    {
        public Task<LegalCollectionDocumentTypeDto> Create(CreateDocumentTypeHttpRequest request);
        public Task<LegalCollectionDocumentTypeDto> Details(DetailsDocumentTypeHttRequest request);
        public Task<ListModel<LegalCollectionDocumentTypeDto>> List(DocumentTypesHttpRequest request);
        public Task<LegalCollectionDocumentTypeDto> Update(UpdateDocumentTypeHttpRequest request);
        public Task<int> Delete(DeleteDocumentTypeHttpRequest request);
    }
}