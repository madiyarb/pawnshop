using System;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.ClientGeoPositions;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService;
using Pawnshop.Web.Engine.Services.Interfaces;
using Serilog;

namespace Pawnshop.Web.Engine.Services//TODO this service must located in pawnshop.Services.Contracts
                                      //namespace and folder, has dependency by IContractActionSignService located in some folder as this service
                                      //On refactoring first of all must migrate dependency of IContractActionSignService before migrate this service
{
    public sealed class ContractSigningService : IContractSigningService
    {
        private readonly ContractRepository _contractRepository;
        private readonly ClientRepository _clientRepository;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractActionSignService _contractActionSignService;
        private readonly IContractService _contractService;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly IClientGeoPositionsService _clientGeoPositionsService;
        private readonly IApplicationOnlineRefinancesService _applicationOnlineRefinancesService;
        private readonly IAbsOnlineService _absOnlineService;
        private readonly ILogger _logger;

        public ContractSigningService(ContractRepository contractRepository,
            ClientRepository clientRepository,
            IContractDutyService contractDutyService,
            IContractActionSignService contractActionSignService,
            IContractService contractService,
            PayTypeRepository payTypeRepository,
            ClientGeoPositionsService clientGeoPositionsService,
            IApplicationOnlineRefinancesService applicationOnlineRefinancesService,
            IAbsOnlineService absOnlineService,
            ILogger logger)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _contractDutyService = contractDutyService ?? throw new ArgumentNullException(nameof(contractDutyService));
            _contractActionSignService = contractActionSignService ??
                                         throw new ArgumentNullException(nameof(contractActionSignService));
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _payTypeRepository = payTypeRepository ?? throw new ArgumentNullException(nameof(payTypeRepository));
            _clientGeoPositionsService = clientGeoPositionsService ??
                                         throw new ArgumentNullException(nameof(clientGeoPositionsService));
            _applicationOnlineRefinancesService = applicationOnlineRefinancesService ??
                                                  throw new ArgumentNullException(
                                                      nameof(applicationOnlineRefinancesService));
            _absOnlineService = absOnlineService ?? throw new ArgumentNullException(nameof(absOnlineService));
            _logger = logger;
        }

        public async Task SignTrancheAndCreditLine(int contractId, int authorId, int branchId, int? requisiteId = null, bool onlyOnline = true, int? cashIssueBranchId = null)
        {
            var contract = _contractRepository.Get(contractId);
            if (contract == null)
            {
                throw new ContractNotFoundException(contractId);
            }
            if (contract.ContractClass != ContractClass.Tranche)
            {
                throw new ContractClassWrongException(contractId, contract.ContractClass);
            }

            if (!(contract.Status == ContractStatus.Draft || contract.Status == ContractStatus.AwaitForInsuranceSend))
            {
                throw new ContractInWrongStatus(contractId, contract.Status);
            }
            var creditLine = _contractRepository.Get(contract.CreditLineId.Value);
            if (creditLine == null)
            {
                throw new CreditLineNotFoundException(contract.CreditLineId.Value);
            }

            if (await _applicationOnlineRefinancesService.IsRefinance(contract.Id))
            {
                var refinanceAllow = await _applicationOnlineRefinancesService.EnoughMoneyForRefinancing(contract.Id);
                if (!string.IsNullOrEmpty(refinanceAllow))
                {
                    throw new NotEnoughMoneyRefinancing(refinanceAllow);
                }
            }

            var client = _clientRepository.Get(contract.ClientId);
            var clientRequisite = client.Requisites.FirstOrDefault(x => x.IsDefault);
            if (requisiteId != null)
            {
                clientRequisite = client.Requisites
                    .FirstOrDefault(requisite => requisite.Id == requisiteId.Value);
            }
            if (clientRequisite == null)
            {
                throw new ClientRequisiteNotFoundException(client.Id, requisiteId);
            }

            var payType = cashIssueBranchId.HasValue ? _payTypeRepository.Find(new { Code = Constants.PAY_TYPE_CASH })
                : await _payTypeRepository.GetByRequisiteType(clientRequisite.RequisiteTypeId);

            if (onlyOnline && !cashIssueBranchId.HasValue)
            {
                if (payType.OperationCode != "IBAN" && payType.OperationCode != "CREDIT_CARD")
                {
                    throw new PayTypeNotAllowedException(client.Id, clientRequisite.Id);
                }

                if (!_clientGeoPositionsService.HasActualGeoPosition(client.Id))
                {
                    throw new ClientGeopositionNotActualException();
                }
            }

            try
            {
                if (await _applicationOnlineRefinancesService.IsRefinance(contract.Id))
                {
                    var successRefinance = await
                        _applicationOnlineRefinancesService.MovePrepaymentForRefinance(contract.Id, branchId);
                    if (!successRefinance)
                        throw new TransferMoneyForRefinanceFailed();
                }
                using (var transaction = _contractRepository.BeginTransaction())
                {

                    if (creditLine.Status != ContractStatus.Signed)
                    {
                        SignCreditLine(creditLine, authorId, clientRequisite, payType);
                        creditLine.SignDate = DateTime.Now;
                        creditLine.Status = ContractStatus.Signed;
                        _contractService.Save(creditLine);
                    }
                    SignTranche(contract, authorId, clientRequisite, payType, cashIssueBranchId);
                    transaction.Commit();
                }
                if (await _applicationOnlineRefinancesService.IsRefinance(contract.Id))
                {
                    await _applicationOnlineRefinancesService.InternalRefinance(contractId);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ContractSignFailedException(exception.Message);
            }
        }

        public void SignTranche(Contract contract, int authorId, ClientRequisite clientRequisite, PayType payType, int? cashIssueBranchId = null)
        {
            decimal amount = 0;

            amount = _applicationOnlineRefinancesService.CalculateRefinanceAmountForContract(contract.Id).Result;

            if (contract.ContractClass != ContractClass.CreditLine)
                _absOnlineService.CreateInsurancePolicy(contract.Id);

            ContractDutyCheckModel checkModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Sign,
                ContractId = contract.Id,
                Cost = contract.LoanCost,
                Date = contract.ContractDate,
                PayTypeId = payType.Id,
                Refinance = amount
            };

            var contractDuty = _contractDutyService.GetContractDuty(checkModel);

            var action = new ContractAction
            {
                ActionType = ContractActionType.Sign,
                Checks = contractDuty.Checks.Select((x, i) => new ContractActionCheckValue { Check = x, CheckId = x.Id, Value = true }).ToList(),
                ContractId = contract.Id,
                Date = contractDuty.Date,
                Discount = contractDuty.Discount,
                Expense = contractDuty.ExtraContractExpenses?.FirstOrDefault(),
                ExtraExpensesCost = contractDuty.ExtraExpensesCost,
                PayTypeId = payType.Id,
                Reason = contractDuty.Reason,
                RequisiteCost = contract.ContractClass == ContractClass.CreditLine ? (int?)0 : null,
                RequisiteId = clientRequisite.Id,
                Rows = contractDuty.Rows.ToArray(),
                TotalCost = contractDuty.Cost,
            };

            _contractActionSignService.Exec(action, authorId, contract.BranchId, false, ignoreCheckQuestionnaireFilledStatus: true,
                orderStatus: contract.ContractClass == ContractClass.CreditLine ? (OrderStatus?)OrderStatus.Approved : null,
                cashIssueBranchId: cashIssueBranchId);
        }

        public void SignCreditLine(Contract contract, int authorId, ClientRequisite clientRequisite, PayType payType)
        {
            ContractDutyCheckModel checkModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Sign,
                ContractId = contract.Id,
                Cost = contract.LoanCost,
                Date = contract.ContractDate,
                PayTypeId = payType.Id,
            };

            var contractDuty = _contractDutyService.GetContractDuty(checkModel);

            var action = new ContractAction
            {
                ActionType = ContractActionType.Sign,
                Checks = contractDuty.Checks.Select((x, i) => new ContractActionCheckValue { Check = x, CheckId = x.Id, Value = true }).ToList(),
                ContractId = contract.Id,
                Date = contractDuty.Date,
                Discount = contractDuty.Discount,
                Expense = contractDuty.ExtraContractExpenses?.FirstOrDefault(),
                ExtraExpensesCost = contractDuty.ExtraExpensesCost,
                PayTypeId = payType.Id,
                Reason = contractDuty.Reason,
                RequisiteCost = contract.ContractClass == ContractClass.CreditLine ? (int?)0 : null,
                RequisiteId = clientRequisite.Id,
                Rows = contractDuty.Rows.ToArray(),
                TotalCost = contractDuty.Cost,
            };

            _contractActionSignService.Exec(action, authorId, contract.BranchId, false, ignoreCheckQuestionnaireFilledStatus: true,
                orderStatus: contract.ContractClass == ContractClass.CreditLine ? (OrderStatus?)OrderStatus.Approved : null);
        }
    }
}
