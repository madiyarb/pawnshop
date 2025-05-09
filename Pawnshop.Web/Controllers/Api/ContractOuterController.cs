using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Calculation;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.Contract;
using System;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Calculation;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractOuterView)]
    public class ContractOuterController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ContractRepository _contractRepository;
        private readonly IContractAmount _contractAmount;

        public ContractOuterController(IContractService contractService,
            IContractAmount contractAmount,
            ContractRepository contractRepository)
        {
            _contractService = contractService;
            _contractAmount = contractAmount;
            _contractRepository = contractRepository;
        }

        [Event(EventCode.WebsiteContractFind, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public ContractOuterModel Find([FromBody] ContractOuterQueryModel query)
        {
            ModelState.Validate();

            var entity = _contractRepository.Check(query.ContractNumber, query.IdentityNumber);
            if (entity == null) throw new PawnshopApplicationException("Договор не найден");
            var contract = _contractService.Get(entity.Id);
            return new ContractOuterModel
            {
                ContractNumber = contract.ContractNumber,
                ContractDate = contract.ContractDate,
                MaturityDate = contract.MaturityDate,
                LoanCost = contract.LoanCost,
                BalanceCost = (int)Math.Round(contract.LeftLoanCost)
            };
        }

        [Event(EventCode.WebsiteContractCheck, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public ContractOuterForCheckModel Check([FromBody] ContractOuterQueryModel query)
        {
            ModelState.Validate();

            var entity = _contractRepository.Check(query.ContractNumber, query.IdentityNumber);
            if (entity == null) throw new PawnshopApplicationException("Договор не найден");

            var contract = _contractService.Get(entity.Id);
            _contractAmount.Init(contract);

            ContractOuterForCheckModel checkModel = new ContractOuterForCheckModel
            {
                ContractNumber = contract.ContractNumber,
                ContractDate = contract.ContractDate,
                MaturityDate = contract.MaturityDate,
                LoanCost = contract.LoanCost,
                Status = contract.Status,
                Amount = _contractAmount
            };

            if(contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
            contract.PercentPaymentType == PercentPaymentType.Product)
            {
                var schedule = contract.PaymentSchedule.FirstOrDefault();
                checkModel.MonthlyPayment = Math.Round(schedule.DebtCost + schedule.PercentCost, 2);
                checkModel.DelayedMonthlyPayments = contract.PaymentSchedule.Count(x => x.Status == ScheduleStatus.Overdue);
                checkModel.FutureMonthlyPayments = contract.PaymentSchedule.Count(x => x.Status == ScheduleStatus.FuturePayment);
            }

            return checkModel;
        }
    }
}
