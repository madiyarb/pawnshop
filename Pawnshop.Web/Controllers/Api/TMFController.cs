using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.TasOnline;
using Pawnshop.Services.Models.TMF;
using Pawnshop.Services.TasOnline;
using Pawnshop.Services.TMF;
using Pawnshop.Web.Engine;


namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class TMFController : Controller
    {
        private readonly ITMFRequestService _requestService;
        private readonly BranchContext _branchContext;

        public TMFController(
            ITMFRequestService requestService,
            TasOnlinePaymentRepository onlinePaymentRepository,
            BranchContext branchContext)
        {
            _requestService = requestService;
            _branchContext = branchContext;
        }

        [HttpPost]
        public IActionResult GetContractsByIdentityNumber([FromBody] string identityNumber)
        {
            var model = _requestService.GetContractsByIdentityNumber(identityNumber);

            return Ok(model);
        }

        [HttpPost]
        public IActionResult SavePayment([FromBody] TMFPaymentModel payment)
        {
            var branchId = _branchContext.Branch.Id;
            var cashOrders = _requestService.SavePayment(payment, branchId);

            return Ok(cashOrders);
        }

        [HttpPost]
        public IActionResult PaymentList([FromBody] ListQueryModel<TmfPaymentFilter> listQuery)
        {
            return Ok(_requestService.GetTmfPaymentList(listQuery));
        }
    }
}
