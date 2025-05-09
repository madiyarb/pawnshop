using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using System.Collections.Generic;
using System;
using Pawnshop.Web.Models.Calls;
using Pawnshop.Data.Models.Calls;
using System.Linq;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/calls")]
    [ApiController]
    [Authorize]
    public class CallsController : ControllerBase
    {
        private readonly CallsRepository _callsRepository;
        private readonly ISessionContext _sessionContext;

        public CallsController(CallsRepository callsRepository, ISessionContext sessionContext)
        {
            _callsRepository = callsRepository;
            _sessionContext = sessionContext;
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpDelete("{id}")]
        public IActionResult DeleteCall([FromRoute] int id)
        {
            var entity = _callsRepository.Get(id);

            if (entity == null)
                return NotFound($"Запись {id} не найдена.");

            _callsRepository.Delete(id);

            return Ok();
        }

        [HttpGet("{id}")]
        public ActionResult<CallView> Get([FromRoute] int id)
        {
            var entity = _callsRepository.Get(id);

            if (entity == null)
                return NotFound($"Запись {id} не найдена");

            return Ok(GetView(entity));
        }

        [HttpGet]
        public ActionResult<List<CallView>> GetList([FromQuery] int? clientId, [FromQuery] Guid? appId)
        {
            if (!clientId.HasValue && !appId.HasValue)
                return BadRequest("Не указаны параметры запроса!");

            var calls = _callsRepository.List(new { ClientId = clientId, applicationOnlineId = appId });

            return Ok(calls?.Select(x => GetView(x)));
        }


        private CallView GetView(Call entity)
        {
            return new CallView
            {
                CallPbxId = entity.CallPbxId,
                ClientId = entity.ClientId,
                ClientName = entity.Client?.FullName,
                CreateDate = entity.CreateDate,
                Direction = entity.Direction,
                Duration = entity.Duration,
                Id = entity.Id,
                Language = entity.Language,
                PhoneNumber = entity.PhoneNumber,
                RecordFile = entity.RecordFile,
                Status = entity.Status,
                UpdateDate = entity.UpdateDate,
                UserId = entity.UserId,
                UserInternalPhone = entity.UserInternalPhone,
                UserName = entity.User?.Fullname,
            };
        }
    }
}
