using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    public class InsuranceOnlineRequestController : Controller
    {
        private readonly IInsuranceOnlineRequestService _service;

        public InsuranceOnlineRequestController(IInsuranceOnlineRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        [Event(EventCode.InsuranceOnlineRequestSave, EventMode = EventMode.All, EntityType = EntityType.Contract)]
        public IActionResult Save([FromBody] InsuranceRequestData requestData)
        {
            return Ok(_service.Save(requestData));
        }
    }
}