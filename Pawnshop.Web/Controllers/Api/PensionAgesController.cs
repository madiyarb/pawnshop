using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.PensionAge;
using Pawnshop.Services.PensionAges;
using Pawnshop.Web.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    public class PensionAgesController : Controller
    {
        private readonly IPensionAgesService _pensionAgesService;
        private readonly ISessionContext _sessionContext;
        public PensionAgesController(IPensionAgesService pensionAgesService, ISessionContext sessionContext)
        {
            _pensionAgesService = pensionAgesService;
            _sessionContext = sessionContext;
        }
        [HttpPost]
        public IActionResult Save([FromBody] PensionAge model)
        {
            model = _pensionAgesService.Save(model);
            return Ok(model);
        }
        [HttpPost]
        public IActionResult Delete([FromBody] int id)
        {
            _pensionAgesService.Delete(id);
            return Ok();
        }
        [HttpPost]
        public List<PensionAge> List()
        {
            return _pensionAgesService.List();
        }

        [HttpPost]
        public IActionResult Card([FromBody] int id)
        {
            return Ok(_pensionAgesService.Get(id));
        }
    }
}
