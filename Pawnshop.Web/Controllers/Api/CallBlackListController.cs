using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Utilities;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Calls;
using Pawnshop.Web.Models.CallBlackList;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/call/black-list")]
    [ApiController]
    [Authorize]
    public class CallBlackListController : ControllerBase
    {
        private readonly CallBlackListRepository _callBlackListRepository;
        private readonly ISessionContext _sessionContext;

        public CallBlackListController(CallBlackListRepository callBlackListRepository, ISessionContext sessionContext)
        {
            _callBlackListRepository = callBlackListRepository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ActionResult<CallBlackListView> Create([FromBody] CallBlackListCreateView rq)
        {
            var phoneNumber = RegexUtilities.GetNumbers(rq.PhoneNumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(phoneNumber))
                return BadRequest($"Неверно указан номер телефона {rq.PhoneNumber}. Укажите в формате 77ccdddxxzz.");

            var entity = new CallBlackList
            {
                AuthorId = _sessionContext.UserId,
                ExpireDate = rq.ExpireDate,
                PhoneNumber = phoneNumber,
                Reason = rq.Reason,
            };

            _callBlackListRepository.Insert(entity);

            return Ok(GetView(entity));
        }

        [HttpGet("{id}")]
        public ActionResult<CallBlackListView> Get([FromRoute] int id)
        {
            var entity = _callBlackListRepository.Get(id);

            if (entity == null)
                return NotFound($"Запись {id} не найдена.");

            return Ok(GetView(entity));
        }

        [HttpPut("{id}")]
        public ActionResult<CallBlackList> Update([FromRoute] int id, [FromBody] CallBlackListUpdateView rq)
        {
            var entity = _callBlackListRepository.Get(id);

            if (entity == null)
                return NotFound($"Запись {id} не найдена.");

            entity.ExpireDate = rq.ExpireDate;
            entity.Reason = rq.Reason;
            entity.AuthorId = _sessionContext.UserId;

            _callBlackListRepository.Update(entity);

            return Ok(GetView(entity));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var entity = _callBlackListRepository.Get(id);

            if (entity == null)
                return NotFound($"Запись {id} не найдена.");

            _callBlackListRepository.Delete(id);

            return Ok();
        }

        [HttpGet]
        public ActionResult<List<CallBlackListView>> GetAll([FromQuery] string phoneNumber)
        {
            var phoneNumberResult = RegexUtilities.GetNumbers(phoneNumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(phoneNumberResult))
                return BadRequest($"Неверно указан номер телефона {phoneNumber}.");

            var entities = _callBlackListRepository.List(null, new { PhoneNumber = phoneNumberResult });

            return Ok(entities.Select(x => GetView(x)));
        }


        private CallBlackListView GetView(CallBlackList entity)
        {
            return new CallBlackListView
            {
                AuthorId = entity.AuthorId,
                AuthorName = entity.Author?.Fullname,
                CreateDate = entity.CreateDate,
                ExpireDate = entity.ExpireDate,
                Id = entity.Id,
                PhoneNumber = entity.PhoneNumber,
                Reason = entity.Reason,
                UpdateDate = entity.UpdateDate,
            };
        }
    }
}
