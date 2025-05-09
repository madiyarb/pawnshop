using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CallPurpose;
using Pawnshop.Web.Models.CallPurpose;
using System;
using System.Linq;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/callpurposes")]
    [ApiController]
    [Authorize]
    public class CallPurposesController : ControllerBase
    {
        private readonly CallPurposesRepository _callPurposesRepository;
        private readonly ISessionContext _sessionContext;

        public CallPurposesController(CallPurposesRepository callPurposesRepository, ISessionContext sessionContext)
        {
            _callPurposesRepository = callPurposesRepository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ActionResult<CallPurposeView> Create([FromBody] CallPurposeCreateBinding binding)
        {
            var entity = new CallPurpose
            {
                AuthorId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                Title = binding.Title,
            };

            _callPurposesRepository.Insert(entity);

            entity = _callPurposesRepository.Get(entity.Id);

            return Ok(ToView(entity));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var entity = _callPurposesRepository.Get(id);

            if (entity == null)
                return NotFound("Запись не найдена");

            _callPurposesRepository.Delete(id);

            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<CallPurposeView> Get([FromRoute] int id)
        {
            var entity = _callPurposesRepository.Get(id);

            if (entity == null)
                return NotFound("Запись не найдена");

            return Ok(ToView(entity));
        }

        [HttpGet("list")]
        public ActionResult<CallPurposeListView> GetList()
        {
            var entities = _callPurposesRepository.List(null);
            var count = _callPurposesRepository.Count(null);

            var response = new CallPurposeListView
            {
                Count = count,
                List = entities.Select(x => new CallPurposeListItemView { Id = x.Id, Title = x.Title }).ToList(),
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public ActionResult<CallPurposeView> Update([FromRoute] int id, [FromBody] CallPurposeCreateBinding binding)
        {
            var entity = _callPurposesRepository.Get(id);

            if (entity == null)
                return NotFound("Запись не найдена");

            entity.Title = binding.Title;
            entity.UpdateAuthorId = _sessionContext.UserId;
            entity.UpdateDate = DateTime.Now;

            _callPurposesRepository.Update(entity);
            entity = _callPurposesRepository.Get(id);

            return Ok(ToView(entity));
        }


        private CallPurposeView ToView(CallPurpose entity)
        {
            return new CallPurposeView
            {
                Id = entity.Id,
                AuthorId = entity.AuthorId,
                AuthorName = entity.Author?.Fullname,
                CreateDate = entity.CreateDate,
                Title = entity.Title,
                UpdateAuthorId = entity.UpdateAuthorId,
                UpdateAuthorName = entity.UpdateAuthor?.Fullname,
                UpdateDate = entity.UpdateDate
            };
        }
    }
}
