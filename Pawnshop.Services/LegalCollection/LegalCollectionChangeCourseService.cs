using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.ChangeCourse;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionChangeCourseService : ILegalCollectionChangeCourseService
    {
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ISessionContext _sessionContext;
        private readonly ILegalCollectionCheckClientDeathService _legalCollectionCheckClientDeathService;
        private readonly ILegalCaseContractsStatusRepository _legalCaseContractsRepository;

        public LegalCollectionChangeCourseService(
            ILegalCaseHttpService legalCaseHttpService,
            ISessionContext sessionContext,
            ILegalCollectionCheckClientDeathService legalCollectionCheckClientDeathService,
            ILegalCaseContractsStatusRepository legalCaseContractsRepository
            )
        {
            _legalCaseHttpService = legalCaseHttpService;
            _sessionContext = sessionContext;
            _legalCollectionCheckClientDeathService = legalCollectionCheckClientDeathService;
            _legalCaseContractsRepository = legalCaseContractsRepository;
        }

        public async Task<List<ChangeCourseActionDto>> GetChangeCourseVariousAsync(int legalCaseId)
        {
            if (!UserHasRightsChangeCourseLegalCase())
            {
                throw new PawnshopApplicationException("Нет прав для смены направления в Legal collection");
            }
            
            var response = await _legalCaseHttpService.GetChangeCourseVarious(legalCaseId);
            return response;
        }

        public async Task<List<LegalCaseDetailsResponse>> ChangeCourseAsync(ChangeLegalCaseCourseCommand request)
        {
            if (!UserHasRightsChangeCourseLegalCase())
            {
                throw new PawnshopApplicationException("Нет прав для смены направления в Legal collection");
            }
            
            if (request.ChangeCourseAction.CourseToCode == "DEADCLIENTSWORK")
            {
                await _legalCollectionCheckClientDeathService.CheckBlackListClientDeath(request.ClientId);
            }
            
            var legalCaseContract = await _legalCaseContractsRepository.GetByLegalCaseIdAsync(request.LegalCaseId);
            if (legalCaseContract is null)
            {
                throw new PawnshopApplicationException($"дело с Id: {request.LegalCaseId} не найдено");
            }

            var changeCourseRequest = new ChangeLegalCaseCourseRequest
            {
                LegalCaseId = request.LegalCaseId,
                ActionCode = request.ChangeCourseAction.ActionCode,
                Note = request.Note,
                AuthorId = request.AuthorId
            };
            
            var response = await _legalCaseHttpService.ChangeCourse(changeCourseRequest);
            return response;
        }
        
        private bool UserHasRightsChangeCourseLegalCase()
        {
            return _sessionContext.UserId == Constants.ADMINISTRATOR_IDENTITY ||
                   _sessionContext.Permissions.Contains(Permissions.LegalCollectionChangeCourse);
        }
    }
}