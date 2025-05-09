using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Bitrix;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Clients;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Controllers.Api
{
    public class BitrixController : Controller
    {
        private readonly IClientContactService _clientContactService;
        private readonly IClientService _clientService;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly IContractActionService _contractActionService;
        private readonly CollectionStatusRepository _collectionStatusRepository;
        private readonly ContractRepository _contractRepository;

        public BitrixController(
            IClientContactService clientContactService,
            IClientService clientService,
            IContractService contractService,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            IContractActionService contractActionService,
            CollectionStatusRepository collectionStatusRepository,
            ContractRepository contractRepository)
        {
            _clientContactService = clientContactService;
            _clientService = clientService;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _contractActionService = contractActionService;
            _collectionStatusRepository = collectionStatusRepository;
            _contractRepository = contractRepository;
        }

        [HttpGet]
        [Route("api/bitrix/ContractsLatestPayment")] 
        public async Task<IActionResult> ContractsLatestPayment(string identityNumber)
        {
            var clientId = await _clientService.GetClientIdAsync(identityNumber);
            var client = await _clientService.GetOnlyClientAsync(clientId);

            if (client == null || client.Id == 0)
            {
                return NotFound("Клиент не найден.");
            }

            var contracts = await _contractRepository.GetContractsByClientIdAndContractClasesAsync(clientId, new List<int> { 1, 3 });
            if (contracts == null || !contracts.Any())
            {
                return NotFound($"У клиента {client.FullName} нет активных договоров для определения необходимой информации.");
            }

            var bitrixLastPaymentModels = new List<BitrixLastPaymentModel>();

            foreach (var contract in contracts.Where(x => x.DeleteDate == null && x.Status == ContractStatus.Signed))
            {
                var contractActions = await _contractActionService.GetContractActionsByContractId(contract.Id);
                var lastPayment = contractActions
                    .Where(x => x.ActionType == ContractActionType.Payment)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();

                var collectionContractStatus = await _collectionStatusRepository.GetByContractIdAsync(contract.Id);

                var LastPaymentModel = new BitrixLastPaymentModel
                        {
                            ContractNumber = contract.ContractNumber,
                            ContractDate = contract.ContractDate,
                            LastPaymentCost = lastPayment?.Cost ?? 0,
                            LastPaymentDate = lastPayment?.Date ?? null,
                            ContractExpired = collectionContractStatus?.IsActive ?? false
                        };
                    bitrixLastPaymentModels.Add(LastPaymentModel);
            }

            var bitrixModel = new BitrixModel
            {
                ClientFullName = client.FullName,
                Contracts = bitrixLastPaymentModels
            };

            return Ok(bitrixModel);
        }
    }
}
