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
using Pawnshop.Services.TasOnline;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class TasOnlineController : Controller
    {
        private readonly ITasOnlineRequestService _requestService;
        private readonly TasOnlinePaymentRepository _onlinePaymentRepository;
        private readonly BranchContext _branchContext;

        public TasOnlineController(
            ITasOnlineRequestService requestService, 
            TasOnlinePaymentRepository onlinePaymentRepository,
            BranchContext branchContext)
        {
            _requestService = requestService;
            _onlinePaymentRepository = onlinePaymentRepository;
            _branchContext = branchContext;
        }

        [HttpPost]
        public IActionResult GetContractsByIdentityNumber([FromBody] string identityNumber)
        {
            var model = _requestService.GetContractsByIdentityNumber(identityNumber);

            return Ok(model);
        }

        [HttpPost]
        public IActionResult SavePayment([FromBody] PaymentModel payment)
        {
            var branchId = _branchContext.Branch.Id;
            var cashOrders = _requestService.SavePayment(payment, branchId);

            return Ok(cashOrders);
        }

        [HttpPost]
        public IActionResult CheckPayment([FromBody] int cashOrderId)
        {
            var payment = _requestService.CheckPayment(cashOrderId);

            return Ok(payment);
        }

        [HttpPost]
        public IActionResult RePayment([FromBody] int cashOrderId)
        {
            var payment = _requestService.RePayment(cashOrderId);

            return Ok(payment);
        }

        [HttpPost, Authorize(Permissions.TasOnlinePaymentView)]
        public IActionResult PaymentList([FromBody] ListQueryModel<TasOnlinePaymentFilter> listQuery)
        {
            return Ok(new ListModel<TasOnlinePayment>()
            {
                List = _onlinePaymentRepository.List(listQuery, listQuery.Model),
                Count = _onlinePaymentRepository.Count(listQuery, listQuery.Model),
            });
        }
    }
}