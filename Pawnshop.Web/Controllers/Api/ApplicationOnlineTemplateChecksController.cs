using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Web.Models.ApplicationOnlineTemplateCheck;
using System.Linq;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/applicationOnline/templateCheck")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineTemplateChecksController : ControllerBase
    {
        private readonly ApplicationOnlineTemplateChecksRepository _applicationOnlineTemplateChecksRepository;
        private readonly ISessionContext _sessionContext;

        public ApplicationOnlineTemplateChecksController(
            ApplicationOnlineTemplateChecksRepository applicationOnlineTemplateChecksRepository,
            ISessionContext sessionContext)
        {
            _applicationOnlineTemplateChecksRepository = applicationOnlineTemplateChecksRepository;
            _sessionContext = sessionContext;
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpPost]
        public ActionResult<ApplicationOnlineTemplateCheckView> Create([FromBody] ApplicationOnlineTemplateCheckCreateView view)
        {
            var createEntity = new ApplicationOnlineTemplateCheck
            {
                AttributeCode = view.AttributeCode,
                AttributeName = view.AttributeName,
                Code = view.Code,
                CreateBy = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                IsActual = view.IsActual,
                IsManual = view.IsManual,
                Stage = view.Stage,
                Title = view.Title,
                ToManager = view.ToManager,
                ToTranche = view.ToTranche,
                ToVerificator = view.ToVerificator,
            };

            _applicationOnlineTemplateChecksRepository.Insert(createEntity);

            var entity = _applicationOnlineTemplateChecksRepository.Get(createEntity.Id);

            return Ok(EntityToView(entity));
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var entity = _applicationOnlineTemplateChecksRepository.Get(id);

            if (entity == null)
                return BadRequest($"Шаблон проверки {id} не найден.");

            _applicationOnlineTemplateChecksRepository.Delete(id);

            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<ApplicationOnlineTemplateCheckView> Get([FromRoute] int id)
        {
            var entity = _applicationOnlineTemplateChecksRepository.Get(id);

            if (entity == null)
                return NotFound();

            return Ok(EntityToView(entity));
        }

        [HttpGet("list")]
        public ActionResult<ApplicationOnlineTemplateCheckListView> List([FromQuery] ApplicationOnlineTemplateCheckQuery query)
        {
            if (!query.Offset.HasValue)
                query.Offset = 0;

            if (!query.Limit.HasValue)
                query.Limit = 20;

            var entities = _applicationOnlineTemplateChecksRepository.List(null, query);
            var count = _applicationOnlineTemplateChecksRepository.Count(null, query);

            var response = new ApplicationOnlineTemplateCheckListView
            {
                Count = count,
                List = entities.Select(x => EntityToView(x))
            };

            return Ok(response);
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpPut("{id}")]
        public ActionResult<ApplicationOnlineTemplateCheckView> Update([FromRoute] int id, [FromBody] ApplicationOnlineTemplateCheckUpdateView view)
        {
            var entity = _applicationOnlineTemplateChecksRepository.Get(id);

            if (entity == null)
                return BadRequest($"Шаблон проверки {id} не найден.");

            entity.Code = view.Code;
            entity.UpdateDate = DateTime.Now;
            entity.UpdateBy = _sessionContext.UserId;
            entity.Title = view.Title;
            entity.IsActual = view.IsActual;
            entity.IsManual = view.IsManual;
            entity.Stage = view.Stage;
            entity.ToVerificator = view.ToVerificator;
            entity.ToManager = view.ToManager;
            entity.ToTranche = view.ToTranche;
            entity.AttributeName = view.AttributeName;
            entity.AttributeCode = view.AttributeCode;

            _applicationOnlineTemplateChecksRepository.Update(entity);

            var updateEntity = _applicationOnlineTemplateChecksRepository.Get(id);

            return Ok(EntityToView(updateEntity));
        }


        private ApplicationOnlineTemplateCheckView EntityToView(ApplicationOnlineTemplateCheck entity)
        {
            return new ApplicationOnlineTemplateCheckView
            {
                AttributeCode = entity.AttributeCode,
                AttributeName = entity.AttributeName,
                Code = entity.Code,
                CreateBy = entity.CreateBy,
                CreateByName = entity.Author.Fullname,
                CreateDate = entity.CreateDate,
                Id = entity.Id,
                IsActual = entity.IsActual,
                IsManual = entity.IsManual,
                Stage = entity.Stage,
                Title = entity.Title,
                ToManager = entity.ToManager,
                ToTranche = entity.ToTranche,
                ToVerificator = entity.ToVerificator,
                UpdateBy = entity.UpdateBy,
                UpdateByName = entity.UpdateAuthor?.Fullname,
                UpdateDate = entity.UpdateDate,
            };
        }
    }
}
