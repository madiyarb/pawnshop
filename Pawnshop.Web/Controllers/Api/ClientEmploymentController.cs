using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientFamilyStatus;
using Pawnshop.Web.Models.ClientWorkAndEducationInfo;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/employments"), Authorize(Permissions.ClientView)]
    public class ClientEmploymentController : Controller
    {
        private readonly IClientEmploymentService _clientEmploymentService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        private readonly ISessionContext _sessionContext;

        public ClientEmploymentController(
            IClientEmploymentService clientEmploymentService,
            IClientQuestionnaireService clientQuestionnaireService,
            ISessionContext sessionContext)
        {
            _clientEmploymentService = clientEmploymentService;
            _clientQuestionnaireService = clientQuestionnaireService;
            _sessionContext = sessionContext;
        }

        /// <summary>
        /// Получает список мест работ клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("list"), ProducesResponseType(typeof(List<ClientEmploymentDto>), 200)]
        public IActionResult GetList([FromRoute] int clientId)
        {
            List<ClientEmployment> employments = _clientEmploymentService.Get(clientId);
            return Ok(employments.Select(c => new ClientEmploymentDto
            {
                Id = c.Id,
                EmployeeCountId = c.EmployeeCountId,
                IsDefault = c.IsDefault,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                BusinessScopeId = c.BusinessScopeId,
                WorkExperienceId = c.WorkExperienceId,
                PositionName = c.PositionName,
                PositionTypeId = c.PositionTypeId//,
                //Income = c.Income
            }));
        }

        /// <summary>
        /// Сохраняет список мест работ клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("save"), ProducesResponseType(typeof(List<ClientEmploymentDto>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] SaveClientEmploymentsRequest request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            List<ClientEmployment> employments = _clientEmploymentService.Save(clientId, request.Employments);
            return Ok(employments.Select(c => new ClientEmploymentDto
            {
                Id = c.Id,
                EmployeeCountId = c.EmployeeCountId,
                IsDefault = c.IsDefault,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                BusinessScopeId = c.BusinessScopeId,
                WorkExperienceId = c.WorkExperienceId,
                PositionName = c.PositionName,
                PositionTypeId = c.PositionTypeId//,
                //Income = c.Income
            }));
        }

        [Authorize(Permissions.ClientManage)]
        [HttpPost("ClientWorkAndEducationInfo")]
        [ProducesResponseType(typeof(ClientWorkAndEducationInfoView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> SetClientWorkAndEducationInfo(
            [FromRoute] int clientId,
            [FromServices] ClientProfileRepository repository,
            [FromServices] ClientRepository clientRepository,
            [FromBody] ClientWorkAndEducationInfoBinding binding)
        {
            var profile = repository.Get(clientId);

            if (profile == null)
            {
                var client = clientRepository.GetOnlyClient(clientId);

                if (client == null)
                {
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найден клиент с идентификатором {clientId}"));
                }

                repository.Insert(new ClientProfile
                {
                    ClientId = clientId,
                    AuthorId = _sessionContext.UserId,
                });

                profile = repository.Get(clientId);
            }

            profile.SetWorkAndEducation(binding.EducationTypeId, binding.TotalWorkExperienceId, binding.IsWorkingNow);
            repository.Update(profile);

            return Ok(new ClientWorkAndEducationInfoView
            {
                EducationTypeId = profile.EducationTypeId,
                IsWorkingNow = profile.IsWorkingNow,
                TotalWorkExperienceId = profile.TotalWorkExperienceId
            });
        }

        [Authorize(Permissions.ClientManage)]
        [HttpGet("ClientWorkAndEducationInfo")]
        [ProducesResponseType(typeof(ClientWorkAndEducationInfoView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> GetClientWorkAndEducationInfo(
            [FromRoute] int clientId,
            [FromServices] ClientProfileRepository repository)
        {
            var profile = repository.Get(clientId);

            if (profile == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найден клиентский профиль с идентификатором {clientId}"));
            }

            return Ok(new ClientWorkAndEducationInfoView
            {
                EducationTypeId = profile.EducationTypeId,
                IsWorkingNow = profile.IsWorkingNow,
                TotalWorkExperienceId = profile.TotalWorkExperienceId
            });
        }

    }
}
