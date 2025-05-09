using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionPrintTemplateHttpService
    {
        Task<List<LegalCasePrintTemplate>> List();
    }
}
