using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionTaskStatusService : ILegalCollectionTaskStatusService
    {
        private readonly ILegalCollectionTaskStatusHttpService _taskStatusHttpService;

        public LegalCollectionTaskStatusService(ILegalCollectionTaskStatusHttpService taskStatusHttpService)
        {
            _taskStatusHttpService = taskStatusHttpService;
        }

        public async Task<PagedResponse<LegalCaseTaskStatusDto>> List()
        {
            // return await _taskStatusHttpService.List(new LegalCaseTaskStatusesHttpQuery());
            return await _taskStatusHttpService.List();
        }
    }
}