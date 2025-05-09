using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Files;
using Pawnshop.Services.Audit;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Services.Storage;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class EventLogController : Controller
    {
        private readonly EventLogService _service;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly EventLogExcelBuilder _excelBuilder;
        private readonly IStorage _storage;

        public EventLogController(EventLogService service, ISessionContext sessionContext,
            BranchContext branchContext, EventLogExcelBuilder excelBuilder, IStorage storage)
        {
            _service = service;
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _excelBuilder = excelBuilder;
            _storage = storage;
        }

        [HttpPost]
        public ListModel<EventLogItem> List([FromBody] ListQueryModel<EventLogFilter> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<EventLogFilter>();
            if (listQuery.Model == null) listQuery.Model = new EventLogFilter();

            if (!_sessionContext.HasPermission(Permissions.EventLogFullView))
            {
                listQuery.Model.BranchId = _branchContext.Branch.Id;
            }
            if (!_sessionContext.HasPermission(Permissions.EventLogView) && !listQuery.Model.EntityId.HasValue)
            {
                throw new PawnshopApplicationException("Недостаточно прав для выполнения данной операции.");
            }

            if (listQuery.Model.EntityId == null)
                return null;

            return _service.List(listQuery);
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] List<EventLogItem> eventLogs)
        {
            using (var stream = _excelBuilder.Build(eventLogs))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");
                string contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }
    }
}