using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Clients.Profiles;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/additionalContacts"), Authorize(Permissions.ClientView)]
    public class ClientAdditionalContactController : Controller
    {
        private readonly IClientAdditionalContactService _clientAdditionalContactService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        public ClientAdditionalContactController(IClientAdditionalContactService clientAdditionalContactService, IClientQuestionnaireService clientQuestionnaireService)
        {
            _clientAdditionalContactService = clientAdditionalContactService;
            _clientQuestionnaireService = clientQuestionnaireService;
        }

        /// <summary>
        /// Получает список дополнительных контактов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("list"), ProducesResponseType(typeof(List<ClientAdditionalContactDto>), 200)]
        public IActionResult GetList([FromRoute] int clientId)
        {
            List<ClientAdditionalContact> contacts = _clientAdditionalContactService.Get(clientId);
            return Ok(contacts.Select(c => new ClientAdditionalContactDto
            {
                ContactListName = c.ContactListName,
                FromContactList = c.FromContactList,
                Id = c.Id,
                PhoneNumber = c.PhoneNumber,
                ContactOwnerTypeId = c.ContactOwnerTypeId,
                ContactOwnerFullname = c.ContactOwnerFullname,
                IsMainPayer = c.IsMainPayer
            }));
        }

        /// <summary>
        /// Сохраняет список дополнительных контактов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("save"), ProducesResponseType(typeof(List<ClientAdditionalContactDto>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] SaveClientAdditionalContactsRequest request,[FromServices] IMediator mediator)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            if (request.AdditionalContacts.Where(x => x.IsMainPayer).Count() > 1)
                throw new PawnshopApplicationException("Нельзя указывать больше одного фактического плательщика");
            
            ModelState.Validate();
            List<ClientAdditionalContact> contacts = _clientAdditionalContactService.Save(clientId, request.AdditionalContacts);

            if (contacts.Any())
            {
                mediator.Send(new SendAdditionalContactCommand() { ContactList = contacts.ToArray(), ClientId = clientId }).Wait();
            }

            return Ok(contacts.Select(c => new ClientAdditionalContactDto
            {
                Id = c.Id,
                PhoneNumber = c.PhoneNumber,
                ContactOwnerTypeId = c.ContactOwnerTypeId,
                ContactOwnerFullname = c.ContactOwnerFullname,
                IsMainPayer = c.IsMainPayer
            }));
        }
    }
}
