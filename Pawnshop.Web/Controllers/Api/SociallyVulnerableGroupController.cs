using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.SociallyVulnerableGroupView)]
    public class SociallyVulnerableGroupController : Controller
    {
        private readonly SociallyVulnerableGroupRepository _repository;

        public SociallyVulnerableGroupController(SociallyVulnerableGroupRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<SociallyVulnerableGroup> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<SociallyVulnerableGroup>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public SociallyVulnerableGroup Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var sociallyVulnerableGroup = _repository.Get(id);
            if (sociallyVulnerableGroup == null) throw new InvalidOperationException();

            return sociallyVulnerableGroup;
        }

        [HttpPost, Authorize(Permissions.SociallyVulnerableGroupManage)]
        [Event(EventCode.DictSociallyVulnerableGroupSaved, EventMode = EventMode.Response)]
        public SociallyVulnerableGroup Save([FromBody] SociallyVulnerableGroup sociallyVulnerableGroup)
        {
            ModelState.Validate();

            if (sociallyVulnerableGroup.Id > 0)
            {
                _repository.Update(sociallyVulnerableGroup);
            }
            else
            {
                _repository.Insert(sociallyVulnerableGroup);
            }
            return sociallyVulnerableGroup;
        }

        [HttpPost, Authorize(Permissions.SociallyVulnerableGroupManage)]
        [Event(EventCode.DictSociallyVulnerableGroupDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}