using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Web.Models.ApplicationOnlineCheck;
using Pawnshop.Web.Models;
using System.Linq;
using System.Net;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/applicationOnline/check")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineChecksController : ControllerBase
    {
        private readonly ApplicationOnlineChecksRepository _applicationOnlineChecksRepository;
        private readonly ISessionContext _sessionContext;
        private readonly ITasOnlinePermissionValidatorService _permissionValidator;

        public ApplicationOnlineChecksController(
            ApplicationOnlineChecksRepository applicationOnlineChecksRepository,
            ISessionContext sessionContext,
            ITasOnlinePermissionValidatorService permissionValidator)
        {
            _applicationOnlineChecksRepository = applicationOnlineChecksRepository;
            _sessionContext = sessionContext;
            _permissionValidator = permissionValidator;
        }

        [HttpPut("{id}/checked")]
        public IActionResult Checked([FromRoute] int id, [FromServices] ApplicationOnlineRepository appOnlineRepository)
        {
            var entity = _applicationOnlineChecksRepository.Get(id);

            if (entity == null)
                return BadRequest($"Верификация {id} не найдена.");

            var application = appOnlineRepository.Get(entity.ApplicationOnlineId);

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!application.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
            }

            entity.UpdateDate = DateTime.UtcNow;
            entity.UpdateBy = _sessionContext.UserId;
            entity.Checked = true;

            _applicationOnlineChecksRepository.Update(entity);

            return Ok();
        }

        [HttpGet("{id}")]
        public ActionResult<ApplicationOnlineCheckView> Get([FromRoute] int id)
        {
            var entity = _applicationOnlineChecksRepository.Get(id);

            if (entity == null)
                return BadRequest($"Верификация {id} не найдена.");

            return EntityToView(entity);
        }

        [HttpGet("list")]
        public ActionResult<ApplicationOnlineCheckListView> List(
            [FromQuery] ApplicationOnlineCheckQuery query,
            [FromServices] ApplicationOnlineRepository appOnlineRepository,
            [FromServices] IApplicationOnlineChecksService applicationOnlineChecksService)
        {
            if (query.ApplicationOnlineId == (Guid)default)
                return BadRequest($"Не правильный ID заявки : {query.ApplicationOnlineId}.");

            var application = appOnlineRepository.Get(query.ApplicationOnlineId);

            if (application == null)
                return BadRequest($"Не найдена заявка с ID: {query.ApplicationOnlineId}.");

            var list = applicationOnlineChecksService.GetList(application.Id, application.Status);

            var response = new ApplicationOnlineCheckListView
            {
                Checks = list.Select(x => EntityToView(x))
            };

            return Ok(response);
        }


        [HttpPut("{id}/unchecked")]
        public IActionResult Unchecked([FromRoute] int id, [FromServices] ApplicationOnlineRepository appOnlineRepository)
        {
            var entity = _applicationOnlineChecksRepository.Get(id);
            if (entity == null)
                return BadRequest($"Верификация {id} не найдена.");

            var application = appOnlineRepository.Get(entity.ApplicationOnlineId);

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!application.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
            }

            entity.UpdateDate = DateTime.UtcNow;
            entity.UpdateBy = _sessionContext.UserId;
            entity.Checked = false;

            _applicationOnlineChecksRepository.Update(entity);

            return Ok();
        }


        [HttpPut("list")]
        public IActionResult UpdateList(
            [FromBody] ApplicationOnlineCheckListView list,
            [FromServices] ApplicationOnlineRepository appOnlineRepository)
        {
            var appIds = list.Checks.Select(x => x.ApplicationOnlineId)
                .Distinct()
                .ToList();

            foreach (var appId in appIds)
            {
                var application = appOnlineRepository.Get(appId);
                if (!application.CanEditing(_sessionContext.UserId))
                {
                    return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
                }
                if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
                }
            }

            foreach (var item in list.Checks)
            {
                var entity = _applicationOnlineChecksRepository.Get(item.Id);

                entity.UpdateDate = DateTime.UtcNow;
                entity.UpdateBy = _sessionContext.UserId;
                entity.Checked = item.Checked;

                _applicationOnlineChecksRepository.Update(entity);
            }

            return Ok();
        }

        #region only test use
        [HttpPost]
        [Obsolete]
        public ActionResult<ApplicationOnlineCheckView> Create([FromBody] ApplicationOnlineCheck entity)
        {
            entity.CreateDate = DateTime.UtcNow;
            entity.CreateBy = _sessionContext.UserId;

            _applicationOnlineChecksRepository.Insert(entity);

            var createdEntity = _applicationOnlineChecksRepository.Get(entity.Id);

            return Ok(EntityToView(createdEntity));
        }
        #endregion

        private ApplicationOnlineCheckView EntityToView(ApplicationOnlineCheck entity)
        {
            return new ApplicationOnlineCheckView
            {
                Id = entity.Id,
                CreateBy = entity.CreateBy,
                CreateByName = entity.Author?.Fullname,
                CreateDate = entity.CreateDate,
                UpdateBy = entity.UpdateBy,
                UpdateByName = entity.UpdateAuthor?.Fullname,
                UpdateDate = entity.UpdateDate,
                AdditionalInfo = entity.AdditionalInfo,
                ApplicationOnlineId = entity.ApplicationOnlineId,
                Checked = entity.Checked,
                TemplateId = entity.TemplateId,
                CheckName = entity.TemplateCheck?.Title,
                Note = entity.Note
            };
        }
    }
}
