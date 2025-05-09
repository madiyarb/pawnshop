using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Services.PensionAges;

namespace Pawnshop.Services.Insurance
{
    public class InsurancePoliceRequestService : BaseService<InsurancePoliceRequest>, IInsurancePoliceRequestService
    {
        private readonly ISessionContext _sessionContext;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IContractService _contractService;
        private readonly InsurancePoliceRequestRepository _insurancePoliceRequestRequestRepository;
        private readonly InsurancePolicyRepository _insurancePolicyRepository;
        private readonly IPensionAgesService _pensionAgesService;

        public InsurancePoliceRequestService(
            ClientContactRepository clientContactRepository,
            ISessionContext sessionContext,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            IContractService contractService,
            InsurancePoliceRequestRepository insurancePoliceRequestRepository,
            InsurancePolicyRepository insurancePolicyRepository, IPensionAgesService pensionAgesService) : base(insurancePoliceRequestRepository)
        {
            _sessionContext = sessionContext;
            _clientContactRepository = clientContactRepository;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _contractService = contractService;
            _insurancePoliceRequestRequestRepository = insurancePoliceRequestRepository;
            _insurancePolicyRepository = insurancePolicyRepository;
            _pensionAgesService = pensionAgesService;
        }

        public void FillRequest(InsurancePoliceRequest insurancePoliceRequest)
        {
            if (insurancePoliceRequest is null)
                throw new PawnshopApplicationException("Запрос в страховую компанию не найден");

            if (insurancePoliceRequest.IsInsuranceRequired && insurancePoliceRequest.InsuranceCompanyId == 0)
                throw new PawnshopApplicationException("Страховая компания не выбрана");

            if (!insurancePoliceRequest.IsInsuranceRequired)
            {
                insurancePoliceRequest.Contract = _contractService.Get(insurancePoliceRequest.ContractId);
                if (!isPensioner(insurancePoliceRequest))
                {
                    var policeRequestFromBD = _repository.Get(insurancePoliceRequest.Id);

                    if (policeRequestFromBD.IsInsuranceRequired != insurancePoliceRequest.IsInsuranceRequired && !_sessionContext.Permissions.Any(t => t == Permissions.IsInsuranceRequiredManage))
                        throw new PawnshopApplicationException("Недостачно прав для того чтобы отменить обязательное страхование");

                    insurancePoliceRequest.RequestData = policeRequestFromBD.RequestData;
                }
            }

            if (insurancePoliceRequest.Id == 0)
            {
                insurancePoliceRequest.AuthorId = _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY;
                insurancePoliceRequest.CreateDate = DateTime.Now;
            }
            else
                UpdateInsuranceRequired(insurancePoliceRequest);
        }

        public InsurancePoliceRequest GetPoliceRequest(ContractModel model)
        {
            InsurancePoliceRequest insurancePoliceRequest = null;
            var setting = _contractService.GetSettingForContract(model.Contract);


            if (setting.IsInsuranceAvailable)
            {
                var newPoliceRequest = model.PoliceRequests.FirstOrDefault(t => t.Id == 0);
                var latestRequest = model.PoliceRequests.OrderByDescending(t => t.CreateDate)
                    .FirstOrDefault(t => t.Status != InsuranceRequestStatus.Rejected && t.Id != 0);

                insurancePoliceRequest = newPoliceRequest ?? latestRequest;
                insurancePoliceRequest.Contract = model.Contract;


                FillRequest(insurancePoliceRequest);


                if (insurancePoliceRequest.RequestData is null && insurancePoliceRequest.IsInsuranceRequired)
                    throw new PawnshopApplicationException("Ожидалось, что страховая премия будет заполнена");

                if (insurancePoliceRequest.RequestData.InsurancePremium > 0 && insurancePoliceRequest.IsInsuranceRequired)
                {
                    insurancePoliceRequest.RequestData = GetInsuranceRequestData(model.Contract, insurancePoliceRequest);

                    var costForPremiumCalculator = model.Contract.LoanCost - insurancePoliceRequest.RequestData.InsurancePremium;

                    var insuranceData = _insurancePremiumCalculator.GetInsuranceDataV2(costForPremiumCalculator,
                        insurancePoliceRequest.InsuranceCompanyId, setting.Id);


                    SetInsuranceData(insurancePoliceRequest.RequestData, insuranceData);
                    insurancePoliceRequest.AlgorithmVersion = insuranceData.AlgorithmVersion;
                }
            }

            return insurancePoliceRequest;
        }

        public void DeletePoliceRequestsForContract(int contractId)
        {
            var policeRequests = _repository.List(new ListQuery(), new { ContractId = contractId });

            if (policeRequests.Any())
                foreach (var policeRequest in policeRequests)
                    _repository.Delete(policeRequest.Id);
        }

        public InsuranceRequestData SetInsuranceRequestData(InsuranceRequestDataModel model)
        {
            var policeRequest = model.InsurancePoliceRequest;

            if (policeRequest is null)
                throw new PawnshopApplicationException("Заявка на добор не заполнена");

            if (model.Cost <= 0)
                throw new PawnshopApplicationException("Сумма заявки на добор должна быть больше 0");

            if (policeRequest.Contract is null)
                throw new PawnshopApplicationException("Договор в заявке на добор не заполнен");

            if (policeRequest.Contract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException("Для заявки на добор договор должен быть подписан");

            //var setting = _contractService.GetSettingForContract(policeRequest.Contract);
            var setting = _contractService.GetSettingForContract(policeRequest.Contract, model.SettingId);

            if (setting.IsInsuranceAvailable)
            {
                var insuranceAmount = _insurancePremiumCalculator.GetAdditionInsuranceAmount(policeRequest.Contract.Id, model.Cost, model.AdditionDate, policeRequest.Contract.ClosedParentId);

                var insuranceData = _insurancePremiumCalculator.GetInsuranceDataV2(insuranceAmount, policeRequest.InsuranceCompanyId, setting.Id, model.AdditionDate, policeRequest.Contract.Id, policeRequest.Contract.ClosedParentId);

                policeRequest.RequestData = GetInsuranceRequestData(policeRequest.Contract, policeRequest);
                SetInsuranceData(policeRequest.RequestData, insuranceData);
                policeRequest.AlgorithmVersion = insuranceData.AlgorithmVersion;

                if (isPensioner(policeRequest))
                {
                    ChangeInsuranceRequestDataForPensioner(policeRequest.RequestData);

                }

                var debtLeft = insuranceAmount - model.Cost;
                var insuranceAmountWithoutPremium = insuranceData.SurchargeAmount != 0 ? (insuranceData.LoanCost - insuranceData.SurchargeAmount) : (insuranceData.LoanCost - insuranceData.InsurancePremium);

                decimal addCost = insuranceAmountWithoutPremium - debtLeft;

                //если сумма не в пределах которые мы выдаем, то увеличиваем до нижнего предела следующего диапазона
                if (insuranceData.AmountToAddIfBorder > 0)
                {
                    addCost += insuranceData.AmountToAddIfBorder;
                }

                policeRequest.RequestData.AdditionCost = addCost;
            }

            return policeRequest.RequestData;
        }

        public void SetInsuranceData(InsuranceRequestData requestData, InsuranceRequestData insuranceData)
        {
            requestData.InsuranceEndDate = insuranceData.InsuranceEndDate;
            requestData.InsurancePeriod = insuranceData.InsurancePeriod;
            requestData.InsuranceAmount = insuranceData.InsuranceAmount;
            requestData.InsuranceRate = insuranceData.InsuranceRate;
            requestData.InsurancePremium = insuranceData.InsurancePremium;
            requestData.LoanCost = insuranceData.LoanCost;
            requestData.SurchargeAmount = insuranceData.SurchargeAmount;
            requestData.YearPremium = insuranceData.YearPremium;
            requestData.DescrOfInsuranceCalc = insuranceData.DescrOfInsuranceCalc;
            requestData.Eds = insuranceData.Eds;
            requestData.PrevPolicyNumber = insuranceData.PrevPolicyNumber;
            requestData.LastPolicyNumber = insuranceData.LastPolicyNumber;
            requestData.AlgorithmVersion = insuranceData.AlgorithmVersion;
            requestData.Premium2 = insuranceData.Premium2;
            requestData.EsbdAmount = insuranceData.EsbdAmount;
        }

        public InsuranceRequestData GetInsuranceRequestData(Contract contract, InsurancePoliceRequest insurancePoliceRequest)
        {
            var client = contract.Client;

            var lastDocument = GetLastClientDocumentForInsurance(client);
            var clientAddress = GetActualAdressForInsurance(client);
            var clientContact = GetDefaultClientContact(client);

            var contractNumber = insurancePoliceRequest.RequestData == null || string.IsNullOrWhiteSpace(insurancePoliceRequest.RequestData.ContractNumber)
                ? GetContractNumber(contract.ContractNumber, contract.Id)
                : insurancePoliceRequest.RequestData.ContractNumber;

            return new InsuranceRequestData()
            {
                FullName = client.FullName,
                IdentityNumber = client.IdentityNumber,
                BirthDay = client.BirthDay.Value,
                DocumentNumber = lastDocument.Number,
                DocumentDate = lastDocument.Date.Value,
                Provider = lastDocument.Provider.Name,
                DocumentType = CompareDocumentTypes(lastDocument.DocumentType.Code),
                DocumentTypeName = lastDocument.DocumentType.Name,
                Address = clientAddress.FullPathRus,
                PhoneNumber = clientContact.Address,
                Email = client.Email,
                IsResident = Convert.ToInt32(client.IsResident),
                InsuranceStartDate = DateTime.Now,
                InsuranceType = Constants.INSURANCE_TYPE_CODE,
                InsurancePremium = insurancePoliceRequest.RequestData == null ? 0 : insurancePoliceRequest.RequestData.InsurancePremium,
                Login = insurancePoliceRequest.RequestData == null ? string.Empty : insurancePoliceRequest.RequestData.Login,
                ContractNumber = contractNumber
            };
        }

        private string GetContractNumber(string contractNumber, int contractId)
        {
            var countOfAllInsurancePolicies = _insurancePolicyRepository.GetCountOfAllPoliciesByContractId(contractId);

            countOfAllInsurancePolicies++;

            return string.Concat(contractNumber, $"-{countOfAllInsurancePolicies}");
        }

        public void SetContractIdAndNumber(InsurancePoliceRequest insurancePoliceRequest, Contract contract)
        {
            insurancePoliceRequest.ContractId = contract.Id;
            insurancePoliceRequest.RequestData.ContractNumber = GetContractNumber(contract.ContractNumber, contract.Id);
        }

        public InsurancePoliceRequest GetLatestPoliceRequest(int contractId) => Find(new { ContractId = contractId, IsRejected = false, NotInStatus = new List<int>() { (int)InsuranceRequestStatus.Completed } });

        public InsurancePoliceRequest GetLatestPoliceRequestAllStatus(int contractId) => Find(new { ContractId = contractId, IsRejected = false });

        public InsurancePoliceRequest GetNewPoliceRequest(int contractId) =>
            Find(new { ContractId = contractId, IsCanceled = false, Status = new List<int>() { (int)InsuranceRequestStatus.New } });

        public InsurancePoliceRequest GetErrorPoliceRequest(int contractId) =>
            Find(new { ContractId = contractId, IsCanceled = false, Status = new List<int>() { (int)InsuranceRequestStatus.Sent } });

        public InsurancePoliceRequest GetApprovedPoliceRequest(int contractId) =>
            Find(new { ContractId = contractId, IsCanceled = false, Status = new List<int>() { (int)InsuranceRequestStatus.Completed } });

        public InsurancePoliceRequest GetPoliceRequestForBuyCarCopy(int contractId) =>
            Find(new { ContractId = contractId, Status = new List<int>() { (int)InsuranceRequestStatus.Canceled, (int)InsuranceRequestStatus.Error, (int)InsuranceRequestStatus.Rejected } });
        
        public InsurancePoliceRequest GetByGUID(Guid guid) =>
            Find(new { Guid = guid });

        public IEnumerable<InsurancePoliceRequest> GetActivePoliceRequests(int contractId)
        {
            return _insurancePoliceRequestRequestRepository.GetInsurancePolicyRequestsToCancel(contractId);
        }

        public void UpdateInsuranceRequired(InsurancePoliceRequest actualPoliceRequest)
        {
            if (actualPoliceRequest is null)
                throw new PawnshopApplicationException("Для договора не найден актуальный запрос для отправки в страховую компанию");

            if (!actualPoliceRequest.IsInsuranceRequired && string.IsNullOrWhiteSpace(actualPoliceRequest.CancelReason))
                throw new PawnshopApplicationException("Не указана причина отмены страхования");

            if (actualPoliceRequest.IsInsuranceRequired)
            {
                actualPoliceRequest.CancelledUserId = null;
                actualPoliceRequest.CancelDate = null;
                actualPoliceRequest.CancelReason = null;
            }
            else
            {
                actualPoliceRequest.CancelledUserId = isPensioner(actualPoliceRequest) ? Constants.ADMINISTRATOR_IDENTITY : _sessionContext.UserId;
                actualPoliceRequest.CancelDate = DateTime.Now;
                actualPoliceRequest.RequestData.InsuranceAmount -= actualPoliceRequest.RequestData.InsurancePremium;
                actualPoliceRequest.RequestData.InsurancePremium = 0;
                actualPoliceRequest.RequestData.SurchargeAmount = 0;
                actualPoliceRequest.RequestData.LoanCost = actualPoliceRequest.RequestData.InsuranceAmount;
                actualPoliceRequest.RequestData.YearPremium = 0;
                //actualPoliceRequest.Status = InsuranceRequestStatus.Completed;
            }
        }

        public ClientDocument GetLastClientDocumentForInsurance(Client client)
        {
            var documentsForInsurance = new List<string>()
                {Constants.IDENTITYCARD, Constants.RESIDENCE, Constants.PASSPORTKZ};

            var documents = client.Documents.Where(t => t.DeleteDate is null && documentsForInsurance.Contains(t.DocumentType.Code));

            var lastDocument = documents.FirstOrDefault(t => t.CreateDate == documents.Max(t => t.CreateDate));

            if (lastDocument is null)
                throw new PawnshopApplicationException(
                    "У клиента не найден документ, который должен быть отправлен в страховую компанию");

            return lastDocument;
        }

        public ClientAddress GetActualAdressForInsurance(Client client)
        {
            var clientAddress = client.Addresses.FirstOrDefault(t => t.DeleteDate is null &&
                client.LegalForm.IsIndividual
                    ? t.AddressType.Code.Equals(Constants.REGISTRATION)
                    : t.AddressType.Code.Equals(Constants.LEGALPLACE));

            if (clientAddress is null)
                throw new PawnshopApplicationException(
                    "У клиента не найден адрес, который должен быть отправлен в страховую компанию");

            return clientAddress;
        }

        public ClientContact GetDefaultClientContact(Client client)
        {
            var clientDefaultContact = _clientContactRepository.Find(new { ClientId = client.Id, IsDefault = true });

            if (clientDefaultContact is null)
                throw new PawnshopApplicationException(
                    "У клиента не найден основной номер телефона");

            return clientDefaultContact;
        }

        private int CompareDocumentTypes(string documentType) =>

            documentType switch
            {
                Constants.IDENTITYCARD => 1,
                Constants.RESIDENCE => 2,
                Constants.PASSPORTKZ => 3,
                _ => throw new NotImplementedException(),
            };

        public InsurancePoliceRequest CopyInsurancePoliceRequest(int contractId)
        {
            if (contractId == 0)
                throw new PawnshopApplicationException("Id договора равен 0");

            var contract = _contractService.Get(contractId);

            if (contract is null)
                throw new PawnshopApplicationException($"Договор с Id {contractId} не найден");

            var latestPoliceRequest = GetPoliceRequestForBuyCarCopy(contractId);

            if (latestPoliceRequest is null)
                throw new PawnshopApplicationException($"Для договора {contract.ContractNumber} не найдена заявка в СК");

            var newPoliceRequest = new InsurancePoliceRequest()
            {
                InsuranceCompanyId = latestPoliceRequest.InsuranceCompanyId,
                IsInsuranceRequired = latestPoliceRequest.IsInsuranceRequired,
                Status = InsuranceRequestStatus.New,
                ContractId = contractId
            };

            FillRequest(newPoliceRequest);

            newPoliceRequest.RequestData = GetInsuranceRequestData(contract, newPoliceRequest);

            SetInsuranceData(newPoliceRequest.RequestData, latestPoliceRequest.RequestData);

            newPoliceRequest.RequestData.AdditionCost = latestPoliceRequest.RequestData.AdditionCost;

            using (var transaction = BeginTransaction())
            {
                Save(newPoliceRequest);
                transaction.Commit();
            }

            return newPoliceRequest;
        }

        public void DeleteInsurancePoliceRequestsByContractId(int contractId) =>
            _insurancePoliceRequestRequestRepository.DeleteInsurancePoliceRequestsByContractId(contractId);

        public bool isPensioner(InsurancePoliceRequest policeRequest)
        {
            if (policeRequest.Contract == null)
                policeRequest.Contract = _contractService.Get(policeRequest.ContractId);

            var client = policeRequest.Contract.Client;
            if (!client.BirthDay.HasValue || !client.IsMale.HasValue)
                return false;
            if (client.IsMale.Value)
            {
                double malePensionAge = _pensionAgesService.GetMalePensionAge();
                if (client.BirthDay.Value.AddYears((int)malePensionAge).AddMonths((int)(Constants.MONTHS_IN_YEAR * (malePensionAge - (int)malePensionAge))) <= DateTime.Today)
                {
                    return true;
                }
            }
            else
            {
                double femalePensionAge = _pensionAgesService.GetFemalePensionAge();
                if (client.BirthDay.Value.AddYears((int)femalePensionAge).AddMonths((int)(Constants.MONTHS_IN_YEAR * (femalePensionAge - (int)femalePensionAge))) <= DateTime.Today)
                {
                    return true;
                }
            }
            return false;
        }

        public InsurancePoliceRequest ChangeForPensioner(InsurancePoliceRequest policeRequest)
        {
            policeRequest.IsInsuranceRequired = false;
            policeRequest.CancelReason = "Пенсионер";
            policeRequest.Status = InsuranceRequestStatus.New;
            policeRequest.CancelDate = DateTime.Now;
            policeRequest.CancelledUserId = Constants.ADMINISTRATOR_IDENTITY;


            if (policeRequest.RequestData.InsuranceAmount >= policeRequest.RequestData.LoanCost)
                policeRequest.RequestData.InsuranceAmount = policeRequest.RequestData.InsuranceAmount - policeRequest.RequestData.InsurancePremium;

            policeRequest.RequestData.LoanCost = policeRequest.RequestData.LoanCost - policeRequest.RequestData.InsurancePremium;

            return policeRequest;
        }

        private void ChangeInsuranceRequestDataForPensioner(InsuranceRequestData model)
        {
            model.SurchargeAmount = 0;
            model.InsuranceAmount = 0;
            model.InsurancePremium = 0;
            model.InsuranceRate = 0;
            model.Eds = 0;
            model.LoanCost = 0;
            model.YearPremium = 0;
        }

        public bool ContractHasCompletedInsurancePolicy(int contractId)
        {
            return _insurancePolicyRepository.ContractHasCompletedInsurancePolicy(contractId);
        }

        public InsurancePolicy GetLastInsurancePolicy(int contractId)
        {
            return _insurancePolicyRepository.GetLasted(contractId);
        }
    }
}