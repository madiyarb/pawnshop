using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.InteractionResults;
using Pawnshop.Web.Models.InteractionResult;
using System.Linq;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/interactionresults")]
    [ApiController]
    [Authorize]
    public class InteractionResultsController : ControllerBase
    {
        private readonly InteractionResultsRepository _interactionResultsRepository;
        private readonly ISessionContext _sessionContext;

        public InteractionResultsController(InteractionResultsRepository interactionResultsRepository, ISessionContext sessionContext)
        {
            _interactionResultsRepository = interactionResultsRepository;
            _sessionContext = sessionContext;
        }
        [HttpPost]
        public ActionResult<InteractionResultView> Create([FromBody] InteractionResultCreateBinding binding)
        {
            var entity = new InteractionResult
            {
                AuthorId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                Title = binding.Title,
            };

            _interactionResultsRepository.Insert(entity);

            entity = _interactionResultsRepository.Get(entity.Id);

            return Ok(ToView(entity));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var entity = _interactionResultsRepository.Get(id);

            if (entity == null)
                return NotFound("Запись не найдена");

            _interactionResultsRepository.Delete(id);

            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<InteractionResultView> Get([FromRoute] int id)
        {
            var entity = _interactionResultsRepository.Get(id);

            if (entity == null)
                return NotFound("Запись не найдена");

            return Ok(ToView(entity));
        }

        [HttpGet("list")]
        public ActionResult<InteractionResultListView> GetList()
        {
            var entities = _interactionResultsRepository.List(null);
            var count = _interactionResultsRepository.Count(null);

            var response = new InteractionResultListView
            {
                Count = count,
                List = entities.Select(x => new InteractionResultListItemView { Id = x.Id, Title = x.Title }).ToList(),
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public ActionResult<InteractionResultView> Update([FromRoute] int id, [FromBody] InteractionResultCreateBinding binding)
        {
            var entity = _interactionResultsRepository.Get(id);

            if (entity == null)
                return NotFound("Запись не найдена");

            entity.Title = binding.Title;
            entity.UpdateAuthorId = _sessionContext.UserId;
            entity.UpdateDate = DateTime.Now;

            _interactionResultsRepository.Update(entity);
            entity = _interactionResultsRepository.Get(id);

            return Ok(ToView(entity));
        }


        private InteractionResultView ToView(InteractionResult entity)
        {
            return new InteractionResultView
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
