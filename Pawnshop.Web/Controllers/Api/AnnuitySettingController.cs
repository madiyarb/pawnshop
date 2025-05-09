using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    public class AnnuitySettingController : Controller
    {
        private readonly AnnuitySettingRepository _repository;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;

        public AnnuitySettingController(AnnuitySettingRepository repository, ISessionContext sessionContext, BranchContext branchContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
            _branchContext = branchContext;
        }

        [HttpPost("/api/annuitySetting/list")]
        public ListModel<AnnuitySetting> List([FromBody] ListQuery listQuery)
        {
            var query = new
            {
                OrganizationId = _sessionContext.OrganizationId,
            };

            return new ListModel<AnnuitySetting>
            {
                List = _repository.List(listQuery, query),
                Count = _repository.Count(listQuery, query)
            };
        }

        [HttpPost("/api/annuitySetting/card")]
        public AnnuitySetting Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var setting = _repository.Get(id);
            if (setting == null) throw new InvalidOperationException();

            return setting;
        }

        [HttpPost("/api/annuitySetting/find")]
        public AnnuitySetting Find([FromBody] LoanPercentQueryModel query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            query.BranchId = _branchContext.Branch.Id;

            return _repository.Find(query);
        }

        [HttpPost("/api/annuitySetting/save"), Authorize(Permissions.AnnuitySettingManage)]
        [Event(EventCode.AnnuitySettingSaved, EventMode = EventMode.Response)]
        public AnnuitySetting Save([FromBody] AnnuitySetting setting)
        {
            if (setting.Id == 0)
            {
                setting.CreateDate = DateTime.Now;
                setting.CreatedBy = _sessionContext.UserId;
            }

            ModelState.Clear();
            TryValidateModel(setting);
            ModelState.Validate();

            if (setting.Id > 0)
            {
                _repository.Update(setting);
            }
            else
            {
                _repository.Insert(setting);
            }

            return setting;
        }

        [HttpPost("/api/annuitySetting/delete"), Authorize(Permissions.AnnuitySettingManage)]
        [Event(EventCode.AnnuitySettingDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }

    }
}