using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Services.Restructuring;
using Pawnshop.Web.Engine;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    public class RestructuringController : Controller
    {
        private readonly IRestructuringService _restructuringService;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;

        public RestructuringController(
            IRestructuringService restructuringService,
            BranchContext branchContext,
            ISessionContext sessionContext)
        {
            _restructuringService = restructuringService;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/restructuring/restructurePaymentSchedule")]
        public async Task<IActionResult> RestructurePaymentSchedule([FromForm] RestructuringModel model)
        {
            if (_sessionContext.ForSupport)
                return Ok(await _restructuringService.BuildRestructuredSchedule(model));
            throw new PawnshopApplicationException("Действие Реструктуризация запрещено");
        }

        [HttpPost("/api/restructuring/saveRestructuredPaymentSchedule")]
        public async Task<IActionResult> SaveRestructuredPaymentSchedule([FromForm] RestructuringSaveModel restructuringSaveModel)
        {
            if (_sessionContext.ForSupport)
            {
                var branchId = _branchContext.Branch.Id;
                await _restructuringService.SaveRestructuredPaymentSchedule(restructuringSaveModel, branchId);
                return Ok();
            }
            throw new PawnshopApplicationException("Действие Реструктуризация запрещено");
        }
    }
}
