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
    [Route("api/clients/{clientId:int}/assets"), Authorize(Permissions.ClientView)]
    public class ClientAssetController : Controller
    {
        private readonly IClientAssetService _clientAssetService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        public ClientAssetController(IClientAssetService clientAssetService, IClientQuestionnaireService clientQuestionnaireService)
        {
            _clientAssetService = clientAssetService;
            _clientQuestionnaireService = clientQuestionnaireService;
        }

        /// <summary>
        /// Получает список активов
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("list"), ProducesResponseType(typeof(List<ClientAssetDto>), 200)]
        public IActionResult GetList([FromRoute] int clientId)
        {
            List<ClientAsset> assets = _clientAssetService.Get(clientId);
            return Ok(assets.Select(a => new ClientAssetDto
            {
                Id = a.Id,
                AssetTypeId = a.AssetTypeId,
                Count = a.Count
            }));
        }

        /// <summary>
        /// Сохраняет список активов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("save"), ProducesResponseType(typeof(List<ClientAssetDto>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] SaveClientAssetsRequest request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            ModelState.Validate();
            List<ClientAsset> assets = _clientAssetService.Save(clientId, request.Assets);
            return Ok(assets.Select(a => new ClientAssetDto
            {
                Id = a.Id,
                AssetTypeId = a.AssetTypeId,
                Count = a.Count
            }));
        }
    }
}
