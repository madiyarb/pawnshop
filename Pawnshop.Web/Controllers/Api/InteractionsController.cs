using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Core.Utilities;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Interaction;
using Pawnshop.Services.Interactions;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Interaction;
using Pawnshop.Web.Models;
using System.Linq;
using System.Net;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/interactions")]
    [ApiController]
    [Authorize]
    public class InteractionsController : ControllerBase
    {
        private readonly AttractionChannelRepository _attractionChannelRepository;
        private readonly CallPurposesRepository _callPurposesRepository;
        private readonly IClientContactService _clientContactService;
        private readonly IInteractionService _interactionService;
        private readonly ISessionContext _sessionContext;

        public InteractionsController(
            AttractionChannelRepository attractionChannelRepository,
            CallPurposesRepository callPurposesRepository,
            IClientContactService clientContactService,
            IInteractionService interactionService,
            ISessionContext sessionContext)
        {
            _attractionChannelRepository = attractionChannelRepository;
            _callPurposesRepository = callPurposesRepository;
            _clientContactService = clientContactService;
            _interactionService = interactionService;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ActionResult<InteractionView> Create([FromBody] InteractionCreateBinding binding)
        {
            var editPhoneNumber = RegexUtilities.GetNumbers(binding.ExternalPhone);

            if (!RegexUtilities.IsValidKazakhstanPhone(editPhoneNumber))
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Не правильный номер телефона: {binding.ExternalPhone}."));

            var entity = new Interaction(
                _sessionContext.UserId,
                binding.InteractionType,
                binding.InternalPhone,
                binding.ExternalPhone,
                binding.Result,
                binding.CarYear,
                binding.Firstname,
                binding.Surname,
                binding.Patronymic,
                binding.PreferredLanguage,
                binding.ClientId,
                binding.ApplicationOnlineId,
                binding.CallPurposeId,
                binding.AttractionChannelId,
                binding.CallId,
                binding.SmsNotificationId);

            var view = ToView(_interactionService.Create(entity));

            return Ok(view);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            _interactionService.Delete(id);

            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<InteractionView> Get([FromRoute] int id)
        {
            var entity = _interactionService.Get(id);

            if (entity == null)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Запись {id} не найдена."));

            return Ok(ToView(entity));
        }

        [HttpGet("list")]
        public ActionResult<InteractionListView> GetList([FromQuery] InteractionListQueryBinding query)
        {
            var entitiesList = _interactionService.GetList(new ListQuery { Page = new Page { Limit = query.Limit, Offset = query.Offset } }, query);
            var count = _interactionService.Count(query);
            var attractionChannelList = _attractionChannelRepository.List(new ListQuery { Page = new Page { Limit = 100, Offset = 0 } }, null);

            var reponse = new InteractionListView
            {
                Count = count,
                List = entitiesList.Select(x => ToListItemView(x, attractionChannelList.FirstOrDefault(a => a.Id == x.AttractionChannelId)?.Name)).ToList()
            };

            return Ok(reponse);
        }

        [HttpPut("{id}")]
        public ActionResult<InteractionView> Update([FromRoute] int id, [FromBody] InteractionUpdateBinding binding)
        {
            var entity = _interactionService.Get(id);

            if (entity == null)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Запись {id} не найдена."));

            entity.Update(
                _sessionContext.UserId,
                binding.Result,
                binding.CarYear,
                binding.Firstname,
                binding.Surname,
                binding.Patronymic,
                binding.PreferredLanguage,
                binding.CallPurposeId,
                binding.AttractionChannelId);

            var updatedEntity = _interactionService.Update(entity);

            return Ok(ToView(updatedEntity));
        }


        private InteractionListItemView ToListItemView(Interaction entity, string attractionChannelName)
        {
            return new InteractionListItemView
            {
                AttractionChannelId = entity.AttractionChannelId,
                AttractionChannelName = attractionChannelName,
                AuthorId = entity.AuthorId,
                AuthorName = entity.Call?.User?.Fullname ?? entity.Author?.Fullname,
                CallStatus = entity.Call?.Status,
                ClientFullName = entity.ClientId.HasValue ? entity.Client.FullName
                    : $"{entity.Surname} {entity.Firstname} {entity.Patronymic}",
                CreateDate = entity.CreateDate,
                ExternalPhone = entity.ExternalPhone,
                Id = entity.Id,
                InternalPhone = entity.InternalPhone,
                RecordFileUrl = entity.Call?.RecordFile,
                Result = entity.Result,
                SmsMessage = entity.SmsNotification?.Message,
                InteractionType = entity.InteractionType.ToString()
            };
        }

        private InteractionView ToView(Interaction entity)
        {
            string attractionChannelName = null;

            if (entity.AttractionChannelId.HasValue)
                attractionChannelName = _attractionChannelRepository.Get(entity.AttractionChannelId.Value)?.Name;

            string callPurposeName = null;

            if (entity.CallPurposeId.HasValue)
                callPurposeName = _callPurposesRepository.Get(entity.CallPurposeId.Value)?.Title;

            string mainPhone = null;
            string additionalPhone = null;

            if (entity.ClientId.HasValue)
            {
                var mobileList = _clientContactService.GetMobilePhoneContacts(entity.ClientId.Value);
                mainPhone = mobileList?.FirstOrDefault(x => x.IsDefault)?.Address;
                additionalPhone = mobileList?.OrderByDescending(x => x.CreateDate)?.FirstOrDefault(x => !x.IsDefault)?.Address;
            }

            return new InteractionView
            {
                AdditionalPhone = additionalPhone,
                ApplicationOnlineId = entity.ApplicationOnlineId,
                AttractionChannelId = entity.AttractionChannelId,
                AttractionChannelName = attractionChannelName,
                AuthorId = entity.AuthorId,
                AuthorName = entity.Author?.Fullname,
                BirthDay = entity.Client?.BirthDay,
                CallPurposeId = entity.CallPurposeId,
                CallPurposeName = callPurposeName,
                CarYear = entity.CarYear,
                ClientId = entity.ClientId,
                CreateDate = entity.CreateDate,
                ExternalPhone = entity.ExternalPhone,
                Firstname = entity.ClientId.HasValue ? entity.Client.Name : entity.Firstname,
                Id = entity.Id,
                IIN = entity.Client?.IdentityNumber,
                InteractionTypeName = entity.InteractionType.GetDisplayName(),
                InternalPhone = entity.InternalPhone,
                MainPhone = mainPhone,
                Patronymic = entity.ClientId.HasValue ? entity.Client.Patronymic : entity.Patronymic,
                PreferredLanguage = entity.PreferredLanguage,
                Result = entity.Result,
                SmsMessage = entity.SmsNotification?.Message,
                Surname = entity.ClientId.HasValue ? entity.Client.Surname : entity.Surname,
                UpdateAuthorId = entity.UpdateAuthorId,
                UpdateAuthorName = entity.UpdateAuthor?.Fullname,
                UpdateDate = entity.UpdateDate,
            };
        }
    }
}
