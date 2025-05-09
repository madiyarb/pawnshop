using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionDocumentTypeService : ILegalCollectionDocumentTypeService
    {
        private readonly ILegalCollectionDocumentTypeHttpService _httpService;

        public LegalCollectionDocumentTypeService(ILegalCollectionDocumentTypeHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<LegalCollectionDocumentTypeDto> Create(CreateDocumentTypeHttpRequest request)
        {
            return await _httpService.Create(request);
        }

        public async Task<LegalCollectionDocumentTypeDto> Details(DetailsDocumentTypeHttRequest request)
        {
            return await _httpService.Details(request);
        }

        public async Task<ListModel<LegalCollectionDocumentTypeDto>> List(ListQuery query)
        {
            var httpRequest = new DocumentTypesHttpRequest
            {
                Page = query.Page.Offset,
                Size = query.Page.Limit
            };
            
            var result = await _httpService.List(httpRequest);
            
            return new ListModel<LegalCollectionDocumentTypeDto>
            {
                Count = result?.Count ?? 0,
                List = result?.List?.ToList() ?? new List<LegalCollectionDocumentTypeDto>()
            };
        }

        public async Task<LegalCollectionDocumentTypeDto> Update(UpdateDocumentTypeHttpRequest request)
        {
            return await _httpService.Update(request);
        }

        public async Task<int> Delete(DeleteDocumentTypeHttpRequest request)
        {
            return await _httpService.Delete(request);
        }
    }
}