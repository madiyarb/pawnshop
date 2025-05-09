using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Services.Models.List;
using Pawnshop.Core.Queries;
using Pawnshop.Web.Engine;
using Pawnshop.Core.Exceptions;
using Pawnshop.Services.Contracts.LoanFinancePlans;
using static Pawnshop.Services.Contracts.LoanFinancePlans.LoanFinancePlanSerivce;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView)]
    public class LoanFinancePlanController : Controller
    {
        private readonly ILoanFinancePlanSerivce _loanFinancePlanSerivce;

        public LoanFinancePlanController(ILoanFinancePlanSerivce loanFinancePlanSerivce)
        {
            _loanFinancePlanSerivce = loanFinancePlanSerivce;
        }

        [HttpPost("api/loanFinancePlans/{contractId:int}/list"), ProducesResponseType(typeof(List<LoanFinancePlan>), 200)]
        public IActionResult List([FromRoute] int contractId) => Ok(_loanFinancePlanSerivce.GetList(contractId));

        [HttpPost("api/loanFinancePlans/{contractId:int}/save"), Authorize(Permissions.ContractManage), ProducesResponseType(typeof(List<LoanFinancePlan>), 200)]
        public IActionResult Save([FromRoute] int contractId, [FromBody] List<LoanFinancePlan> loanFinancePlansRequest)
        {
            ModelState.Validate();
            List<LoanFinancePlan> loanFinancePlans = _loanFinancePlanSerivce.SaveFinancePlans(contractId, loanFinancePlansRequest);
            return Ok(loanFinancePlans);
        }

        [HttpPost("api/loanFinancePlans/{clientId:int}/availBalance"), ProducesResponseType(typeof(decimal), 200)]
        public IActionResult AvailBalance([FromRoute] int clientId, [FromBody] AvailBalanceRequest availBalanceRequest) => Ok(_loanFinancePlanSerivce.GetAvailBalance(clientId, availBalanceRequest));
    }
}