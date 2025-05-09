using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.HttpServices.Interfaces
{
    public interface ILegalCollectionStatusesHttpService
    {
        public Task<List<LegalCaseStatusDto>> List();
    }
}