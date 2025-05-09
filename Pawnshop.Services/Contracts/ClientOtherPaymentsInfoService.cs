using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pawnshop.Services.Contracts
{
    public class ClientOtherPaymentsInfoService : IClientOtherPaymentsInfoService
    {
        private readonly ContractKdnDetailRepository _contractKdnDetailRepository;
        private readonly ISessionContext _sessionContext;
        private readonly IContractService _contractService;
        private readonly IClientService _clientService;
        private readonly ContractKdnCalculationLogRepository _contractKdnCalculationLogRepository;

        public ClientOtherPaymentsInfoService(ContractKdnDetailRepository contractKdnDetailRepository,
                                              ISessionContext sessionContext,
                                              IContractService contractService,
                                              IClientService clientService,
                                              ContractKdnCalculationLogRepository contractKdnCalculationLogRepository)
        {
            _contractKdnDetailRepository = contractKdnDetailRepository;
            _sessionContext = sessionContext;
            _contractService = contractService;
            _clientService = clientService;
            _contractKdnCalculationLogRepository = contractKdnCalculationLogRepository;
        }

        public List<ContractKdnDetail> GetClientOtherPayments(int contractId)
        {
            _contractService.GetOnlyContract(contractId);
            var clientOtherPayments = _contractKdnDetailRepository.GetListByContractId(contractId);
            return clientOtherPayments;
        }

        private List<ContractKdnDetailModel> GetClientOtherPayments(List<ContractKdnDetailModel> contractKdnDetailModels, int contractId, Client client, bool isSubject)
        {
            List<ContractKdnDetail> contractKdnDetails;

            var contractKdnDetailModel = contractKdnDetailModels.Where(x => x.ClientId == client.Id).FirstOrDefault();

            if (contractKdnDetailModel != null)
            {
                contractKdnDetails = contractKdnDetailModel.ContractKdnDetails;
            }
            else
            {
                contractKdnDetails = new List<ContractKdnDetail>();
                contractKdnDetailModel = new ContractKdnDetailModel()
                {
                    ClientId = client.Id,
                    IsSubject = isSubject,
                    FIO = client.FullName,
                    ContractKdnDetails = _contractKdnDetailRepository.GetListByClientIdAndContractId(contractId, client.Id)
                };
                contractKdnDetailModels.Add(contractKdnDetailModel);
            }

            return contractKdnDetailModels;
        }

        public List<ContractKdnDetailModel> GetClientOtherPaymentsModels(int contractId)
        {
            var contractKdnDetailModels = new List<ContractKdnDetailModel>();

            var contract = _contractService.Get(contractId);

            GetClientOtherPayments(contractKdnDetailModels, contract.Id, contract.Client, false);

            contract.Subjects?.ForEach(z =>
            {
                GetClientOtherPayments(contractKdnDetailModels, contract.Id, z.Client, true);
            });

            return contractKdnDetailModels;
        }

        public List<ContractKdnDetail> GetClientOtherPayments(int contractId, int clientId)
        {
            var clientOtherPayments = _contractKdnDetailRepository.GetListByClientIdAndContractId(contractId, clientId);
            return clientOtherPayments.Where(x => x.IsLoanPaid).ToList();
        }

        public decimal GetClientOtherPaymentsVal(int contractId, int clientId, decimal debt, List<string> validationErrors)
        {
            var clientPayments = GetClientOtherPayments(contractId, clientId);
            var clientPaymentsSum = clientPayments.Select(x => x.Amount4Kdn).Sum();

            if (clientPaymentsSum > debt)
            {
                validationErrors.Add($"Суммы погашений по кредитам, занесенные менеджером, {clientPaymentsSum} БОЛЬШЕ суммы Debt {debt} от ПКБ");

                return debt;
            }

            return clientPaymentsSum;
        }

        public async Task<ContractKdnDetailModel> GetClientOtherPaymentsModels(Stream xmlStream, int contractId, int clientId, int subjectTypeId, int authorId, bool IsFromAdditionRequest)
        {
            var result = new ContractKdnDetailModel();
            var contractFC = _contractService.Get(contractId);
            result.IsSubject = subjectTypeId == 2;
            var client = _clientService.Get(clientId);
            result.ClientId = clientId;
            result.FIO = client.FullName;

            _contractKdnDetailRepository.DeleteTodayListByClientIdAndContractId(contractId, clientId);

            XmlSerializer serializer = new XmlSerializer(typeof(Models.Contracts.Kdn.ContractKdnXml.Root));
            var xml = (Models.Contracts.Kdn.ContractKdnXml.Root)serializer.Deserialize(xmlStream);
            if (xml != null)
            {
                var contracts = new List<ContractKdnDetail>();
                foreach (var contract in xml.ExistingContracts.Contract)
                {
                    var ckdd = new ContractKdnDetail
                    {
                        ClientId = clientId,
                        ContractId = contractId,
                        SubjectTypeId = subjectTypeId,
                        MonthlyPaymentAmount = ConvertAmount(contract.MonthlyInstalmentAmount.value),
                        OverdueAmount = ConvertAmount(contract.OverdueAmount.value),
                        AuthorId = authorId,
                        CreateDate = DateTime.Now,
                        CreditorName = contract.FinancialInstitution.value,
                        ContractNumber = contract.AgreementNumber.value,
                        ContractStartDate = DateTime.ParseExact(contract.DateOfCreditStart.value, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        ContractEndDate = DateTime.ParseExact(contract.DateOfCreditEnd.value, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        ContractTotalAmount = ConvertAmount(contract.TotalAmount.value),
                        ForthcomingPaymentCount = !string.IsNullOrEmpty(contract.NumberOfOutstandingInstalments.value) ? Convert.ToInt32(contract.NumberOfOutstandingInstalments.value) : 0,
                        OutstandingAmount = ConvertAmount(contract.OutstandingAmount.value),
                        OverdueDaysCount = !string.IsNullOrEmpty(contract.NumberOfOverdueInstalments.value) ? Convert.ToInt32(contract.NumberOfOverdueInstalments.value) : 0,
                        CollateralType = contract.Collateral != null && contract.Collateral.TypeOfGuarantee != null ? contract.Collateral.TypeOfGuarantee.value : null,
                        CollateralCost = contract.Collateral != null && contract.Collateral.ValueOfGuarantee != null ? ConvertAmount(contract.Collateral.ValueOfGuarantee.value) : 0,
                        IsCreditCard = contract.TypeOfFounding.id == "9",
                        IsFromAdditionRequest = IsFromAdditionRequest,
                        CreditLimitAmount = ConvertAmount(contract.CreditLimit.value)
                    };
                    if(IsFromAdditionRequest)
                        ckdd.AdditionRequestDate = DateTime.Now;

                    if(ckdd.ContractEndDate.Date > DateTime.Now.Date)
                    {
                        if(ckdd.IsCreditCard)
                        {
                            ckdd.Amount4Kdn = Math.Max(ckdd.CreditLimitAmount / 100 * 10, ckdd.MonthlyPaymentAmount) + ckdd.OverdueAmount;
                        }
                        else
                        {
                            if (contract.PeriodicityOfPayments.id == "9")
                            {
                                ckdd.Amount4Kdn = ckdd.OutstandingAmount;
                            }
                            else
                            {
                                ckdd.Amount4Kdn = ckdd.MonthlyPaymentAmount + ckdd.OverdueAmount;
                            }
                        }
                    }
                    else
                    {
                        ckdd.Amount4Kdn = ckdd.OutstandingAmount + ckdd.OverdueAmount;
                    }

                    if(contract.TypeOfFounding.id == "20")
                    {
                        ckdd.Amount4Kdn = 0;
                    }
                    _contractKdnDetailRepository.Insert(ckdd);
                    contracts.Add(ckdd);
                }
                result.Amount4KdnSum = contracts.Sum(x => x.Amount4Kdn);
                result.ContractKdnDetails = contracts;
            }

            return result;
        }

        private decimal ConvertAmount(string amount)
        {
            if (string.IsNullOrEmpty(amount))
                return 0;
            return Convert.ToDecimal(amount.Replace(" KZT", "").Replace(" ", "").Replace(".", ","), new NumberFormatInfo() { NumberDecimalSeparator = "," });
        }

        public async Task UpdateFcbContract(UpdateFcbContractRequest request, int UserId)
        {
            var contract = _contractKdnDetailRepository.Get(request.Id);
            if(contract == null)
                throw new PawnshopApplicationException("Контракт не найден");
            if(request.IsLoanPaid && request.FileRowId == null)
                throw new PawnshopApplicationException("Вложите файл, подтверждающий погашение кредита в других финансовых организациях");

            contract.IsLoanPaid = request.IsLoanPaid;
            contract.FileRowId = request.IsLoanPaid ? request.FileRowId : null;
            contract.UserUpdated = UserId;
            contract.DateUpdated = DateTime.Now;
            _contractKdnDetailRepository.Update(contract);
        }

        public async Task UpdateContractId(int Id, int newContractId)
        {
            var contract = _contractKdnDetailRepository.Get(Id);
            if (contract == null)
                throw new PawnshopApplicationException("Контракт не найден");
            if(contract.IsFromAdditionRequest)
            {
                contract.ContractId = newContractId;
                _contractKdnDetailRepository.UpdateContractId(contract);
            }
        }

        public async Task<ContractKdnDetailModel> GetExistingContracts(FcbContractsExistsRequest request)
        {
            var result = new ContractKdnDetailModel();
            result.IsSubject = request.SubjectTypeId == 2;
            var client = _clientService.Get(request.ClientId);
            result.ClientId = request.ClientId;
            result.FIO = client.FullName;

            var clientOtherPayments = _contractKdnDetailRepository.GetTodayListByClientIdAndContractId(request.ContractId, request.ClientId);
            result.Amount4KdnSum = clientOtherPayments.Sum(x => x.Amount4Kdn);
            result.ContractKdnDetails = clientOtherPayments;

            result.PositionEstimatedCost = _contractKdnCalculationLogRepository.GetByContractIdAndAddition(request.ContractId)?.PositionEstimatedCost;
            return result;
        }
    }
}
