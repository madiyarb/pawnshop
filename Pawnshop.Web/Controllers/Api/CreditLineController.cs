using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.CreditLines;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines.Buyout;
using Pawnshop.Services.CreditLines.PartialPayment;
using Pawnshop.Services.CreditLines.Payment;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Web.Engine.Jobs;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.CreditLine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.Auction.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView)]
    public sealed class CreditLineController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        
        public CreditLineController(ISessionContext sessionContext, BranchContext branchContext)
        {
            _sessionContext = sessionContext;
            _branchContext = branchContext;
        }


        [HttpGet("/api/creditlines/{creditlineid}/duty"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(ContractDuty), 200)]
        [Event(EventCode.ContractDebtCheck, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public async Task<IActionResult> GetContractDuty([FromServices] IContractDutyService contractDutyService,
            [FromServices] IContractService contractService, [FromRoute] int creditlineid)
        {
            var tranches = await contractService.GetAllSignedTranches(creditlineid);

            if (!tranches.Any())
            {
                throw new PawnshopApplicationException($"Для кредитной линии : {creditlineid} не найдено не одного транша");
            }
            List<ContractDutyViewModel> contractDuties = new List<ContractDutyViewModel>();

            decimal todayPaymentAmount = 0;
            foreach (var contract in tranches)
            {
                ContractDutyCheckModel model = new ContractDutyCheckModel
                {
                    ContractId = contract.Id,
                    ActionType = ContractActionType.Buyout,
                    Date = DateTime.Now.Date
                };
                ContractDuty contractDuty = contractDutyService.GetContractDuty(model);
                if (DateTime.Now.Date == contract.NextPaymentDate && contract.DisplayStatus
                    != ContractDisplayStatus.Overdue)
                {
                    todayPaymentAmount += contractDuty.Rows.Where(row => row.BusinessOperationSetting.Code == "PAYMENT_PROFIT")
                        .Sum(row => row.Cost);
                }

                contractDuties.Add(new ContractDutyViewModel
                {
                    Date = contractDuty.Date,
                    Checks = contractDuty.Checks,
                    Cost = contractDuty.Cost,
                    Discount = contractDuty.Discount,
                    DisplayAmountForOnlinePayment = contractDuty.DisplayAmountForOnlinePayment,
                    ExtraContractExpenses = contractDuty.ExtraContractExpenses,
                    ExtraExpensesCost = contractDuty.ExtraExpensesCost,
                    Reason = contractDuty.Reason,
                    Rows = contractDuty.Rows.Select(row => new ContractActionRowViewModel
                    {
                        Id = row.Id,
                        ContractNumber = contract.ContractNumber,
                        ActionId = row.ActionId,
                        LoanSubjectId = row.LoanSubjectId,
                        PaymentType = row.PaymentType,
                        Period = row.Period,
                        OriginalPercent = row.OriginalPercent,
                        Percent = row.Percent,
                        Cost = row.Cost,
                        DebitAccountId = row.DebitAccountId,
                        DebitAccount = row.DebitAccount,
                        CreditAccount = row.CreditAccount,
                        CreditAccountId = row.CreditAccountId,
                        OrderId = row.OrderId,
                        BusinessOperationSettingId = row.BusinessOperationSettingId,
                        BusinessOperationSetting = row.BusinessOperationSetting
                    }).ToList()
                });
            }


            var firstItem = contractDuties.FirstOrDefault();
            if (firstItem == null)
            {
                return Ok(new CreditLineDutyViewModel() { });
            }

            decimal ExtraExpensesCost = firstItem.ExtraExpensesCost;

            return Ok(new CreditLineDutyViewModel
            {
                ContractDuties = contractDuties,
                BuyoutCost = contractDuties.Sum(contractDuty => contractDuty.Cost)
                             - ExtraExpensesCost * (contractDuties.Count - 1), // Доп росходы на кредитной линии но считаются для каждого из договоров поэтому вычитаем столько сколько еще траншей -1 
                TotalCost = contractDuties.Sum(contractDuty => contractDuty.Rows.Where(row =>
                    row.BusinessOperationSetting.Code == "PAYMENT_OVERDUE_ACCOUNT" ||
                    row.BusinessOperationSetting.Code == "PAYMENT_OVERDUE_PROFIT" ||
                    row.BusinessOperationSetting.Code == "REVENUE_PENY_ACCOUNT" ||
                    row.BusinessOperationSetting.Code == "PAYMENT_PENY_PROFIT"
                ).Sum(row => row.Cost)) + ExtraExpensesCost + todayPaymentAmount, // Учитываем доп расход один раз
                ExtraExpensesCost = ExtraExpensesCost // Сам доп расход берем из первого договора 
            });
        }

        [HttpGet("/api/creditlines/{creditlineid}/balance"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> GetBalanceForPayment([FromRoute] int creditlineid, [FromQuery] decimal amount, [FromQuery] DateTime? date,
            [FromServices] ICreditLinePaymentService service)
        {
            if (date != null)
            {
                date = date.Value.ToUniversalTime();
            }
            return Ok(await service.GetCreditLineAccountBalancesDistribution(creditlineid, amount: amount, date: date));
        }

        [HttpGet("/api/creditlines/{creditlineid}/balanceforbuyout"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> GetBalanceForBuyOut(
            [FromRoute] int creditlineid,
            [FromQuery] List<int> buyOutContracts,
            [FromQuery] int? expenseId,
            [FromQuery] DateTime? date,
            [FromServices] ICreditLinesBuyoutService service)
        {
            if (date != null)
            {
                date = date.Value.ToUniversalTime();
            }
            return Ok(await service.GetCreditLineAccountBalancesDistributionForBuyBack(creditlineid, buyOutContracts, expenseId, date: date));
        }

        [HttpGet("/api/creditlines/{creditlineid}/partialBalance"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> GetBalanceForPartialPayment(
            [FromRoute] int creditlineid,
            [FromQuery] int partialPaymentContractId,
            [FromQuery] decimal amount,
            [FromQuery] DateTime? date,
            [FromServices] ICreditLinePartialPaymentService service)
        {
            if (date != null)
            {
                date = date.Value.ToUniversalTime();
            }
            return Ok(await service.GetCreditLineAccountBalancesDistribution(creditlineid, partialPaymentContractId, amount, date: date));
        }

        [HttpPost("/api/creditlines/{creditlineid}/payment"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> Payment([FromRoute] int creditlineid, [FromBody] CreditLinePaymentBinding binding,
            [FromServices] ICreditLinePaymentService service, [FromServices] ContractService _contractService, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            if (binding.Date != null)
            {
                binding.Date = binding.Date.Value.ToUniversalTime();
            }
            int authorId = _sessionContext.UserId;
            int branchId;
            if (_branchContext.IsInitialized)
            {
                branchId = _branchContext.Branch.Id;
            }
            else
            {
                var contract = _contractService.Get(creditlineid);
                branchId = contract.BranchId;
            }
            await service.TransferPrepaymentAndPayment(creditlineid, authorId, binding.PayTypeId, branchId, binding.Date, amount: binding.Amount);
            return Ok();
        }

        [HttpPost("/api/creditlines/{creditlineid}/buyout"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> BuyOut(
            [FromRoute] int creditlineid,
            [FromBody] CreditLineBuyOutBinding binding,
            [FromServices] ICreditLinesBuyoutService service,
            [FromServices] ContractService _contractService,
            [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            if (binding.Date != null)
            {
                binding.Date = binding.Date.Value.ToUniversalTime();
            }
            int authorId = _sessionContext.UserId;
            int branchId;
            if (_branchContext.IsInitialized)
            {
                branchId = _branchContext.Branch.Id;
            }
            else
            {
                var contract = _contractService.Get(creditlineid);
                branchId = contract.BranchId;
            }
            await service.TransferPrepaymentAndBuyBack(creditlineid, authorId, binding.PayTypeId, branchId,
                binding.BuyOutContracts, binding.BuyoutReasonId, binding.BuyOutCreditLine, expenseId: binding.ExpenseId, date: binding.Date);
            return Ok();
        }
        
        [HttpPost("/api/creditlines/{creditlineid}/buyoutAuction"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> BuyOutByAuctionAsync(
            [FromRoute] int creditlineid,
            [FromServices] FunctionSettingRepository functionSettingRepository,
            [FromServices] ICreditLineBuyOutByAuctionService auctionCreditLineBuyOutService)
        {
            var depoMasteringSetting = await functionSettingRepository.GetByCodeAsync(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }
            
            if (_sessionContext.Permissions.All(x => x != Permissions.AuctionManage))
            {
                throw new PawnshopApplicationException(Constants.NotEnoughRights);
            }

            await auctionCreditLineBuyOutService.BuyoutByAuctionAsync(creditlineid);
            
            return Ok();
        }

        [HttpPost("/api/creditlines/{creditlineid}/partialPayment"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(CreditLineBalance), 200)]
        public async Task<IActionResult> PartialPayment([FromRoute] int creditlineid, [FromBody] CreditLinePartialPaymentBinding binding,
            [FromServices] ICreditLinePartialPaymentService service, [FromServices] ContractService _contractService, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            if (binding.Date != null)
            {
                binding.Date = binding.Date.Value.ToUniversalTime();
            }
            int branchId;
            if (_branchContext.IsInitialized)
            {
                branchId = _branchContext.Branch.Id;
            }
            else
            {
                var contract = _contractService.Get(creditlineid);
                branchId = contract.BranchId;
            }
            bool unsecuredContractSignNotallowed = _sessionContext
                .Permissions.Where(x => x.Equals(Permissions.UnsecuredContractSign))
                .FirstOrDefault() != Permissions.UnsecuredContractSign;

            return Ok(await service.PartialPaymentAndTransfer(creditLineId: creditlineid, authorId: _sessionContext.UserId,
                binding.PartialPaymentContractId, payTypeId: binding.PayTypeId, branchId: branchId,
                amount: binding.Amount, binding.Date, unsecuredContractSignNotallowed: unsecuredContractSignNotallowed, binding.CategoryChanged.Value));
        }

        [HttpGet("/api/creditlines/paymentjob"), AllowAnonymous]
        public async Task<IActionResult> PaymentJob(
            [FromQuery] int creditLineid,
            [FromServices] UsePrepaymentForCreditLineForMonthlyPaymentJob job,
            [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            job.UsePrepaymentForCreditLine(creditLineid, 4);
            return Ok();
        }

        [HttpGet("api/creditLines/{creditlineid}/validationForNewTranche")]
        public async Task<ActionResult<ValidationForNewTrancheViewModel>> ValidateForNewTranche(
            [FromRoute] int creditlineid,
            [FromServices] ICreditLineService creditLineService,
            [FromServices] IContractService contractService)
        {
            var creditLine = contractService.GetOnlyContract(creditlineid, true);

            if (creditLine == null || creditLine.ContractClass != ContractClass.CreditLine || creditLine.Status != ContractStatus.Signed)
                return NotFound(new ValidationForNewTrancheViewModel
                {
                    IsCanOpen = false,
                    Message = $"Кредитная линия [{creditlineid}] не найдена!",
                });

            var checkResult = await creditLineService.CheckForOpenTranche(creditlineid, creditLine);

            return Ok(new ValidationForNewTrancheViewModel
            {
                CountPayment = checkResult.IsCanOpen ? checkResult.CountPayment : (int?)default,
                IsCanOpen = checkResult.IsCanOpen,
                Message = checkResult.Message,
            });
        }
    }
}
