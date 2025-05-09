using Pawnshop.Web.Engine.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Services.Contracts;
using Pawnshop.Data.Access;
using System.Linq;
using Pawnshop.Data.Models.Crm;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.MobileAppAccess)]
    public class BitrixIVRController : Controller
    {
        private readonly IClientContactService _clientContactService;
        private readonly IContractService _contractService;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;

        public BitrixIVRController(IClientContactService clientContactService,
            IContractService contractService, ContractPaymentScheduleRepository contractPaymentScheduleRepository)
        {
            _clientContactService = clientContactService;
            _contractService = contractService;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
        }

        [HttpGet]
        [Route("api/bitrixIVR/get")]
        public async Task<IActionResult> Get(string phoneNumber)
        {
            var clientId = _clientContactService.GetClientIdByDefaultPhone(phoneNumber);

            if (clientId == null || clientId == 0)
            {
                return NotFound("Client not found");
            }

            var contracts = _contractService.GetActiveContractsByClientId(clientId.Value).Where(contract => contract.ContractClass != Data.Models.Contracts.ContractClass.CreditLine).Select(contract => contract).ToList();

            if (contracts == null || contracts.Count == 0)
            {
                return NotFound("Contracts not found");
            }

            var balances = _contractService.GetBalances(contracts.Select(contract => contract.Id).ToList()).ToList();

            if (balances == null || balances.Count == 0)
            {
                return NotFound("Contract balances not found");
            }

            var ivrModel = new CrmBitrixIVRModel()
            {
                ClientId = clientId.Value,
                overduePayments = (await Task.WhenAll(contracts.Select(async contract => {
                    var paymentSchedule = await _contractPaymentScheduleRepository.GetContractPaymentSchedules(contract.Id);

                    var nextPayment = paymentSchedule.OrderBy(paymentSc => Math.Abs((paymentSc.Date - DateTime.Now).Ticks)).First();

                    return new CrmBitrixIVROverduePayment()
                    {
                        ContractId = contract.Id,
                        ContractNumber = contract.ContractNumber,
                        NextPaymentDate = nextPayment.Date,
                        NextPaymentAmount = nextPayment.DebtCost + nextPayment.PercentCost,
                        OverduePaymentsCount = paymentSchedule.Count(payment => payment.ActualDate is null && payment.Date < DateTime.Now),
                        ContractBalance = balances.Find(balance => balance.ContractId == contract.Id),
                    };
                }))).ToList()
            };

            return Ok(ivrModel);
        }
    }
}