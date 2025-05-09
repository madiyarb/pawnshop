using MediatR;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Web.Models.ClientContacts;
using Pawnshop.Services.Models.UKassa;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/contacts"), Authorize(Permissions.ClientView)]
    public class ClientContactController : Controller
    {
        private readonly ISessionContext _sessionContext;

        private readonly IClientContactService _clientContactService;
        public ClientContactController(IClientContactService clientContactService, ISessionContext sessionContext)
        {
            _clientContactService = clientContactService;
            _sessionContext = sessionContext;
        }

        [HttpPost("list"), ProducesResponseType(typeof(List<ClientContactDto>), 200)]
        public IActionResult List([FromRoute] int clientId)
        {
            return Ok(_clientContactService.GetList(clientId).Select(c => new ClientContactDto
            {
                Address = c.Address,
                ContactTypeId = c.ContactTypeId,
                Id = c.Id,
                IsDefault = c.IsDefault,
                VerificationExpireDate = c.VerificationExpireDate,
                SendUkassaCheck = c.SendUkassaCheck,
                ContactCategoryId = c.ContactCategoryId,
                ContactCategoryCode = c.ContactCategoryCode,
                IsActual = c.IsActual,
                SourceId = c.SourceId,
                Note = c.Note
            }).ToList());
        }

        [HttpPost("save"), ProducesResponseType(typeof(List<ClientContactDto>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] SaveClientContactsRequest request, [FromServices] IMediator mediator)
        {
            ModelState.Validate();
            List<ClientContact> contacts = _clientContactService.Save(clientId, request.Contacts, request.OTP);

            if (contacts.Any())
            {
                mediator.Send(new SendContactCommand() { ContactList = contacts.ToArray(), ClientId = clientId }).Wait();
            }

            return Ok(contacts.Select(c => new ClientContactDto
            {
                Address = c.Address,
                ContactTypeId = c.ContactTypeId,
                Id = c.Id,
                IsDefault = c.IsDefault,
                VerificationExpireDate = c.VerificationExpireDate,
                SendUkassaCheck = c.SendUkassaCheck,
                ContactCategoryId = c.ContactCategoryId,
                ContactCategoryCode = c.ContactCategoryCode,
                IsActual = c.IsActual,
                SourceId = c.SourceId,
                Note = c.Note
            }).ToList());
        }

        [HttpPost("UkassaCheckReceive"), Authorize(Permissions.ClientManage), ProducesResponseType(typeof(List<ClientContactDto>), 200)]
        public async Task<IActionResult> UkassaCheckReceive([FromQuery] int contactId, bool receive)
        {
            _clientContactService.UpdateUkassaCheckReceive(contactId, receive);
            return Ok();
        }


        [HttpPost("mobile_app/changedefaultphone")]
        public async Task<IActionResult> ChangeDefaultPhone(
            [FromRoute] int clientId,
            [FromServices] ClientRepository clientRepository,
            [FromServices] ClientContactRepository repository,
            [FromServices] IDomainService domainService,
            [FromBody] MobileChangeDefaultClientContactBinding binding
        )
        {
            var client = clientRepository.GetOnlyClient(clientId);
            if (client == null)
                return NotFound();

            var clientContacts = repository
                .List(new ListQuery(), new { ClientId = clientId });

            foreach (var contact in clientContacts.Where(contact => contact.Address != binding.MobilePhone && contact.IsDefault))
            {
                contact.IsDefault = false;
                repository.Update(contact);
            }

            var contactType = domainService
                .GetDomainValue(Constants.DOMAIN_CONTACT_TYPE_CODE, Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE);

            var contactCategory = domainService
                .GetDomainValue(Constants.DOMAIN_CONTACT_CATEGORY, Constants.DOMAIN_VALUE_CONTACT_CONTRACT);

            var source = domainService
                .GetDomainValue(Constants.DOMAIN_CONTACT_SOURCE, Constants.DOMAIN_VALUE_CONTRACT_CLIENT_PROFILE);

            var updatedContact = clientContacts.FirstOrDefault(c => c.Address == binding.MobilePhone);
            if (updatedContact != null)
            {
                 updatedContact.UpdateDefaultPhoneFromMobile(
                     address: binding.MobilePhone, 
                     contactTypeId: contactType.Id,//TODO в репе оно не инсертиться надо ли потом апдейтнуть?
                     isDefault: true,
                     authorId: _sessionContext.UserId,
                     isActual: true,
                     verificationExpireDate: DateTime.Now.AddYears(1));
                 repository.Update(updatedContact);
            }
            else
            {
                await repository.InsertFullContact(new ClientContact
                {
                    CreateDate = DateTime.Now,
                    AuthorId = _sessionContext.UserId,
                    Address = binding.MobilePhone,
                    ContactTypeId = contactType.Id, //TODO в репе оно не инсертиться надо ли потом апдейтнуть?
                    IsDefault = true,
                    ClientId = clientId,
                    ContactCategoryId = contactCategory.Id,
                    SourceId = source.Id,             
                    VerificationExpireDate = DateTime.Now.AddYears(1),
                    IsActual = true
                });
            }

            return Ok();
        }
    }
}
