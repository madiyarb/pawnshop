using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionDetailsService : ILegalCollectionDetailsService
    {
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ILegalCasesDetailConverter _legalCasesDetailConverterService;

        public LegalCollectionDetailsService(ILegalCaseHttpService legalCaseHttpService,
            ILegalCasesDetailConverter legalCasesDetailConverterService)
        {
            _legalCaseHttpService = legalCaseHttpService;
            _legalCasesDetailConverterService = legalCasesDetailConverterService;
        }

        public async Task<List<LegalCasesDetailsViewModel>> GetDetailsAsync(LegalCaseDetailsQuery query)
        {
            var detailsResponse = await _legalCaseHttpService.GetLegalCaseDetails(query.LegalCaseId);

            if (detailsResponse is null)
            {
                throw new PawnshopApplicationException($"Legal case c Id: {query.LegalCaseId} не найден");
            }

            return await _legalCasesDetailConverterService.ConvertAsync(detailsResponse);
        }
    }
}