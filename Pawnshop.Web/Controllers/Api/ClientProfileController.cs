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
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/profile"), Authorize(Permissions.ClientView)]
    public class ClientProfileController : Controller
    {
        private readonly IClientProfileService _clientProfileService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        public ClientProfileController(IClientProfileService clientProfileService, IClientQuestionnaireService clientQuestionnaireService)
        {
            _clientProfileService = clientProfileService;
            _clientQuestionnaireService = clientQuestionnaireService;
        }

        /// <summary>
        /// Получает профиль клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("get"), ProducesResponseType(typeof(ClientProfileDto), 200)]
        public IActionResult Get([FromRoute] int clientId)
        {
            ClientProfile profile = _clientProfileService.Get(clientId);
            if (profile == null)
                return NoContent();

            return Ok(new ClientProfileDto
            {
                EducationTypeId = profile.EducationTypeId,
                TotalWorkExperienceId = profile.TotalWorkExperienceId,
                MaritalStatusId = profile.MaritalStatusId,
                SpouseFullname = profile.SpouseFullname,
                SpouseIncome = profile.SpouseIncome,
                ChildrenCount = profile.ChildrenCount,
                AdultDependentsCount = profile.AdultDependentsCount,
                UnderageDependentsCount = profile.UnderageDependentsCount,
                ResidenceAddressTypeId = profile.ResidenceAddressTypeId,
                IsWorkingNow = profile.IsWorkingNow,
                HasAssets = profile.HasAssets
            });
        }

        /// <summary>
        /// Сохраняет профиль клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("save"), ProducesResponseType(typeof(ClientProfileDto), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] ClientProfileDto request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            ModelState.Validate();
            ClientProfile profile = _clientProfileService.Save(clientId, request);
            return Ok(new ClientProfileDto
            {
                EducationTypeId = profile.EducationTypeId,
                TotalWorkExperienceId = profile.TotalWorkExperienceId,
                MaritalStatusId = profile.MaritalStatusId,
                SpouseFullname = profile.SpouseFullname,
                SpouseIncome = profile.SpouseIncome,
                ChildrenCount = profile.ChildrenCount,
                AdultDependentsCount = profile.AdultDependentsCount,
                UnderageDependentsCount = profile.UnderageDependentsCount,
                ResidenceAddressTypeId = profile.ResidenceAddressTypeId,
                IsWorkingNow = profile.IsWorkingNow,
                HasAssets = profile.HasAssets
            });
        }
    }
}
