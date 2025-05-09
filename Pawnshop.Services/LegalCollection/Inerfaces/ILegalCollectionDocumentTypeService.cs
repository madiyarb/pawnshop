using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionDocumentTypeService
    {
        public Task<LegalCollectionDocumentTypeDto> Create(CreateDocumentTypeHttpRequest request);
        public Task<LegalCollectionDocumentTypeDto> Details(DetailsDocumentTypeHttRequest request);
        public Task<ListModel<LegalCollectionDocumentTypeDto>> List(ListQuery query);
        public Task<LegalCollectionDocumentTypeDto> Update(UpdateDocumentTypeHttpRequest request);
        public Task<int> Delete(DeleteDocumentTypeHttpRequest request);
    }
}