using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.PrintTemplateView)]
    public class PrintTemplateController : Controller
    {
        private readonly PrintTemplateRepository _repository;
        private readonly ISessionContext _sessionContext;

        public PrintTemplateController(PrintTemplateRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<PrintTemplate> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<PrintTemplate>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public PrintTemplate Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var template = _repository.Get(id);
            if (template == null) throw new InvalidOperationException();

            return template;
        }

        [HttpPost]
        public PrintTemplateCounterConfig GetConfig([FromBody] int templateId)
        {
            if (templateId <= 0) throw new ArgumentOutOfRangeException(nameof(templateId));

            var config = _repository.GetConfigByTemplate(templateId);
            if (config == null) throw new InvalidOperationException();

            return config;
        }

        [HttpPost, Authorize(Permissions.PrintTemplateManage)]
        [Event(EventCode.DictPrintTemplateSaved, EventMode = EventMode.Response)]
        public PrintTemplate Save([FromBody] PrintTemplate model)
        {
            ModelState.Validate();

            if (model.Id > 0)
            {
                _repository.Update(model);
            }
            else
            {
                model.CreateDate = DateTime.Now;
                model.AuthorId = _sessionContext.UserId;
                _repository.Insert(model);
            }
            return model;
        }

        [HttpPost, Authorize(Permissions.PrintTemplateManage)]
        [Event(EventCode.DictPrintTemplateSaved, EventMode = EventMode.Response)]
        public PrintTemplateCounterConfig SaveConfig([FromBody] PrintTemplateCounterConfig model)
        {
            ModelState.Validate();

            if (model.Id > 0)
            {
                _repository.UpdateConfig(model);
            }
            else
            {
                _repository.InsertConfig(model);
            }
            return model;
        }

        [HttpPost, Authorize(Permissions.PrintTemplateManage)]
        [Event(EventCode.DictPrintTemplateDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}