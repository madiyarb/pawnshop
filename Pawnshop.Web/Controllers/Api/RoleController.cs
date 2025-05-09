using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.List;
using System.Collections.Generic;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.RoleView)]
    public class RoleController : Controller
    {
        private readonly RoleRepository _repository;

        public RoleController(RoleRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<Role> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<Role>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public Role Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var role = _repository.Get(id);
            if (role == null) throw new InvalidOperationException();

            return role;
        }

        [HttpPost, Authorize(Permissions.RoleManage)]
        [Event(EventCode.RoleSaved, EventMode = EventMode.Response)]
        public Role Save([FromBody] Role role)
        {
            ModelState.Validate();

            if (role.Id > 0)
            {
                _repository.Update(role);
            }
            else
            {
                _repository.Insert(role);
            }
            return role;
        }

        [HttpPost, Authorize(Permissions.RoleManage)]
        [Event(EventCode.RoleDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить роль, так как она привязана к пользователям");
            }

            _repository.Delete(id);
            return Ok();
        }

        [HttpGet("api/role/list/for-online-function")]
        [AllowAnonymous]
        public ActionResult<IList<Role>> GetListForOnlineFunction()
        {
            var entities = _repository.ListForOnlineFunction();

            return Ok(entities);
        }
    }
}