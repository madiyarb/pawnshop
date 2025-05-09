using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionPrintTemplateService
    {
        public Task<LegalCasePrintTemplateModel> GetAsync(LegalCasePrintTemplateQuery query);
        public Task<List<LegalCasePrintTemplate>> GetListAsync(int contractId);
    }
}