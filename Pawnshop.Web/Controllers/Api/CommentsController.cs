using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Comments;
using Pawnshop.Web.Models.Comment;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Web.Models;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/comments")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly CommentsRepository _commentsRepository;
        private readonly ISessionContext _sessionContext;
        private readonly ITasOnlinePermissionValidatorService _permissionValidatorService;

        public CommentsController(ITasOnlinePermissionValidatorService permissionValidatorService, 
            CommentsRepository commentsRepository,
            ISessionContext sessionContext)
        {
            _commentsRepository = commentsRepository;
            _sessionContext = sessionContext;
            _permissionValidatorService = permissionValidatorService;
        }

        [HttpPost]
        public ActionResult<CommentView> Create(CommentCreateView view)
        {
            if (!_permissionValidatorService.AnyRole())
            {
                return StatusCode((int)HttpStatusCode.Forbidden, 
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }
            var entity = new Comment
            {
                AuthorId = _sessionContext.UserId,
                CommentText = view.Comment
            };

            if (view.EntityType == CommentEntityType.Application)
            {
                if (Guid.TryParse(view.EntityId, out Guid appOnlineId))
                {
                    entity.ApplicationOnlineComment = new ApplicationOnlineComment
                    {
                        ApplicationOnlineId = appOnlineId,
                        CommentType = ApplicationOnlineCommentTypes.Application,
                    };
                }
                else
                {
                    return BadRequest("Ошибка чтения идентификатора сущности.");
                }
            }

            _commentsRepository.Insert(entity);

            entity = _commentsRepository.Get(entity.Id);

            GetEntityInfo(entity, out string entityId, out CommentEntityType entityType);

            return Ok(new CommentView
            {
                AuthorId = entity.AuthorId,
                AuthorName = entity.Author?.Fullname ?? entity.AuthorName,
                Comment = entity.CommentText,
                CreateDate = entity.CreateDate,
                EntityId = entityId,
                EntityType = entityType,
                Id = entity.Id,
            });
        }

        [HttpGet("list")]
        public ActionResult<CommentListView> List([FromQuery] CommentListQueryView query)
        {
            var comments = _commentsRepository.List(null, new { query.ApplicationOnlineId, query.ClientId, query.Offset, query.Limit });
            var commentsCount = _commentsRepository.Count(null, new { query.ApplicationOnlineId, query.ClientId });

            if (!comments.Any())
                return NoContent();

            var response = new CommentListView();

            response.Count = commentsCount;
            response.Comments = comments
                .Select(x =>
                {
                    GetEntityInfo(x, out string entityId, out CommentEntityType entityType);

                    return new CommentView
                    {
                        AuthorId = x.AuthorId,
                        AuthorName = x.Author?.Fullname ?? x.AuthorName,
                        Comment = x.CommentText,
                        CreateDate = x.CreateDate,
                        EntityId = entityId,
                        EntityType = entityType,
                        Id = x.Id,
                    };
                })
                .OrderByDescending(x => x.CreateDate);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var comment = _commentsRepository.Get(id);

            if (comment == null)
                return NotFound($"Комментарий {id} не найден.");

            if (!_permissionValidatorService.AnyRole())
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            _commentsRepository.Delete(id);

            return NoContent();
        }

        [HttpGet("entity-type")]
        public ActionResult<IDictionary<int, string>> GetEntityTypes()
        {
            var types = Enum.GetValues(typeof(CommentEntityType))
               .Cast<CommentEntityType>()
               .ToDictionary(t => (int)t, t => t.GetDisplayName());

            return Ok(types);
        }


        private void GetEntityInfo(Comment comment, out string entityId, out CommentEntityType entityType)
        {
            entityId = null;
            entityType = CommentEntityType.Other;

            if (comment == null)
                return;

            if (comment.ApplicationOnlineComment != null)
            {
                entityId = comment.ApplicationOnlineComment.ApplicationOnlineId.ToString();
                entityType = CommentEntityType.Application;
                return;
            }
        }
    }
}
