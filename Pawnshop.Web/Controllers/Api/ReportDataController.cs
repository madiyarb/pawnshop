using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ReportData;
using Pawnshop.Services.ReportDatas;
using Pawnshop.Web.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    public class ReportDataController : Controller
    {

        private readonly IReportDataService _reportDataService;
        private readonly ISessionContext _sessionContext;
        private readonly GroupRepository _groupRepository;
        public ReportDataController(ISessionContext sessionContext, IReportDataService reportDataService, GroupRepository groupRepository)
        {
            _reportDataService = reportDataService;
            _sessionContext = sessionContext;
            _groupRepository = groupRepository;
        }

        //[HttpPost("/api/reportdata/saveTasOnline"), Authorize(Permissions.ReportDataManage), ProducesResponseType(typeof(ReportDataResponseModel),200)]
        //public IActionResult Create([FromBody] ReportDataModel model)
        //{
        //    var branch = _groupRepository.Find(new { Name = Constants.TSO});

        //    var actionResult = _reportDataService.Create(model, _sessionContext.OrganizationId,  branch.Id);
        //    return Ok(actionResult);
        //}
    }
}

