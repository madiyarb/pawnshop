using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.AbsOnline
{
    /// <summary>
    /// Реализация интерфейса для запросов AbsOnline
    /// </summary>
    public class AbsOnlineService : IAbsOnlineService
    {
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly IContractService _contractService;
        private readonly IInsuranceService _insuranceService;
        private readonly IInsuranceOnlineRequestService _insuranceOnlineRequestService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IInsurancePolicyService _insurancePolicyService;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly OnlineApplicationRetryInsuranceRepository _onlineApplicationRetryInsuranceRepository;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly UserRepository _userRepository;

        public AbsOnlineService(
            OnlineApplicationRetryInsuranceRepository onlineApplicationRetryInsuranceRepository,
            ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            IContractService contractService,
            IInsuranceService insuranceService,
            IInsuranceOnlineRequestService insuranceOnlineRequestService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IInsurancePolicyService insurancePolicyService,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            OuterServiceSettingRepository outerServiceSettingRepository,
            UserRepository userRepository
            )
        {
            _onlineApplicationRetryInsuranceRepository = onlineApplicationRetryInsuranceRepository;
            _contractAdditionalInfoRepository = contractAdditionalInfoRepository;
            _contractService = contractService;
            _insuranceService = insuranceService;
            _insuranceOnlineRequestService = insuranceOnlineRequestService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _insurancePolicyService = insurancePolicyService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _userRepository = userRepository;
        }

        /// <inheritdoc />
        public void CreateInsurancePolicy(int contractId, Contract contract = null)
        {
            contract ??= _contractService.Get(contractId);

            if (contract == null || (contract.Status != ContractStatus.Draft 
                                     && contract.Status != ContractStatus.InsuranceApproved 
                                     && contract.Status != ContractStatus.AwaitForInsuranceSend)
                || !contract.CreatedInOnline || !contract.Setting.IsInsuranceAvailable)
                return;

            if (!_insurancePremiumCalculator.LoanCostCanUseInsurance(contract.LoanCost))
                return;

            var lastPolicy = _insurancePoliceRequestService.GetLastInsurancePolicy(contractId);

            if (lastPolicy != null && lastPolicy.InsuranceAmount == contract.LoanCost && lastPolicy.CreateDate.Date == DateTime.Now.Date)
                return;

            if (lastPolicy != null)
                DeleteInsuranceRecords(contract.Id);

            var insurancePoliceRequest = GetNewInsurancePoliceRequest(contract);
            var insuranceOnlineRequest = GetNewInsuranceOnlineRequest(insurancePoliceRequest);
            var insurancePolicy = GetNewInsurancePolicy(insurancePoliceRequest);
        }

        /// <inheritdoc />
        public void DeleteInsuranceRecords(int contractId)
        {
            var contract = _contractService.GetOnlyContract(contractId, true);

            if (contract.Status == ContractStatus.Signed)
                return;

            var insurancePoliceRequestList = _insurancePoliceRequestService.List(null, new { ContractId = contractId }).List;

            var completedRequestPolicy = insurancePoliceRequestList.FirstOrDefault(x => x.Status == InsuranceRequestStatus.Completed);
            var completedRequestPolicyId = completedRequestPolicy?.Id ?? 0;
            var completedOnlinePolicyId = completedRequestPolicy?.OnlineRequestId ?? 0;

            insurancePoliceRequestList.Where(x => !x.DeleteDate.HasValue && x.Id != completedRequestPolicyId).ToList()
                .ForEach(x => _insurancePoliceRequestService.Delete(x.Id));

            var insuranceOnlineRequestList = _insuranceOnlineRequestService.List(null, new { ContractId = contractId }).List;
            insuranceOnlineRequestList.Where(x => !x.DeleteDate.HasValue && x.Id != completedOnlinePolicyId).ToList()
                .ForEach(x => _insuranceOnlineRequestService.Delete(x.Id));

            var insurancePolicyList = _insurancePolicyService.List(null, new { ContractId = contractId }).List;
            insurancePolicyList.Where(x => !x.DeleteDate.HasValue && x.PoliceRequestId != completedRequestPolicyId).ToList()
                .ForEach(x => _insurancePolicyService.Delete(x.Id));
        }

        /// <inheritdoc />
        public string GetSmsCode(int contractId)
        {
            var contractAddInfo = _contractAdditionalInfoRepository.Get(contractId);

            return contractAddInfo?.SmsCode;
        }

        /// <inheritdoc />
        public string GetUrlPdfApi()
        {
            var absOnlinePdfConfig = _outerServiceSettingRepository.Find(new { Code = Constants.ABS_ONLINE_PDF_INTEGRATION_SETTINGS_CODE });
            return absOnlinePdfConfig.URL;
        }

        /// <inheritdoc />
        public string RegisterPolicy(int contractId, Contract contract = null)
        {
            try
            {
                contract ??= _contractService.Get(contractId);

                if (contract == null)
                    throw new Exception($"Контракт для страхования не найден: {contractId}!");

                if (!(contract.Status == ContractStatus.AwaitForMoneySend || contract.Status == ContractStatus.Signed))
                    throw new Exception($"Не найден контракт {contractId} для страхования с доступным статусом!");

                if (!contract.CreatedInOnline || !contract.Setting.IsInsuranceAvailable)
                    return string.Empty;

                if (!_insurancePremiumCalculator.LoanCostCanUseInsurance(contract.LoanCost))
                    return string.Empty;

                if (_insurancePoliceRequestService.ContractHasCompletedInsurancePolicy(contractId))
                    return string.Empty;

                DeleteInsuranceRecords(contract.Id);

                var insurancePoliceRequest = GetNewInsurancePoliceRequest(contract);
                var insuranceOnlineRequest = GetNewInsuranceOnlineRequest(insurancePoliceRequest);
                var actualInsurancePoliceRequest = _insurancePoliceRequestService.GetNewPoliceRequest(contract.Id);

                _insuranceService.BPMRegisterPolicy(actualInsurancePoliceRequest).Wait(); //toDo выше что - то не понятное пока происходит. Выключить до выяснения обстоятельств
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <inheritdoc />
        public void SaveAdditionalInfo(int contractId, string smsCode = null, int? branchId = null, string partnerCode = null)
        {
            var contractAddInfo = _contractAdditionalInfoRepository.Get(contractId);

            if (contractAddInfo == null)
            {
                contractAddInfo = new ContractAdditionalInfo
                {
                    Id = contractId,
                    SmsCode = smsCode,
                    SelectedBranchId = branchId,
                    PartnerCode = partnerCode,
                };

                _contractAdditionalInfoRepository.Insert(contractAddInfo);
            }
            else
            {
                contractAddInfo.SmsCode = !string.IsNullOrEmpty(smsCode) ? smsCode : contractAddInfo.SmsCode;
                contractAddInfo.SelectedBranchId = branchId ?? contractAddInfo.SelectedBranchId;
                contractAddInfo.PartnerCode = !string.IsNullOrEmpty(partnerCode) ? partnerCode : contractAddInfo.PartnerCode;

                _contractAdditionalInfoRepository.Update(contractAddInfo);
            }
        }

        /// <inheritdoc />
        public void SaveRetrySendInsurance(int contractId)
        {
            try
            {
                var record = _onlineApplicationRetryInsuranceRepository.Find(new { ContractId = contractId });

                if (record != null)
                    return;

                _onlineApplicationRetryInsuranceRepository.Insert(new OnlineApplicationRetryInsurance
                {
                    ContractId = contractId
                });
            }
            catch
            {
                // TODO: посмотреть поведение работы и возможно добавить обработку
            }
        }

        /// <inheritdoc />
        public async Task<string> SendNotificationCloseContractAsync(int contractId, Contract contract = null)
        {
            try
            {
                contract ??= _contractService.Get(contractId);

                if (contract == null || contract.Status != ContractStatus.BoughtOut || !contract.CreatedInOnline)
                    return string.Empty;

                var absOnlineConfig = _outerServiceSettingRepository.Find(new { Code = Constants.ABS_ONLINE_INTEGRATION_SETTINGS_CODE });

                using (var client = new HttpClient())
                {
                    var request = JsonConvert.SerializeObject(new { contractId = contract.ContractNumber });
                    var stringContent = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(absOnlineConfig.URL + "/contract/close", stringContent);

                    var contractAdditionalInfo = _contractAdditionalInfoRepository.Get(contractId);

                    if (contractAdditionalInfo.Id > 0)
                    {
                        contractAdditionalInfo.ClosedIsSend = response.IsSuccessStatusCode;
                        _contractAdditionalInfoRepository.Update(contractAdditionalInfo);
                    }
                    else
                    {
                        contractAdditionalInfo.Id = contractId;
                        contractAdditionalInfo.ClosedIsSend = response.IsSuccessStatusCode;
                        _contractAdditionalInfoRepository.Insert(contractAdditionalInfo);
                    }

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <inheritdoc />
        public async Task SendNotificationCreditLineChangedAsync(int contractId, Contract contract = null)
        {
            try
            {
                contract ??= _contractService.GetOnlyContract(contractId, true);

                if (contract == null || !_contractService.IsOnline(contractId))
                    return;

                var contractNumber = contract.ContractNumber;

                if (contract.ContractClass == ContractClass.Tranche)
                    contractNumber = _contractService.GetOnlyContract(contract.CreditLineId.Value, true).ContractNumber;

                var absOnlineConfig = _outerServiceSettingRepository.Find(new { Code = Constants.ABS_ONLINE_INTEGRATION_SETTINGS_CODE });

                using (var client = new HttpClient())
                {
                    var requestData = new { CreditLineNumber = contract.ContractNumber };
                    var request = JsonConvert.SerializeObject(requestData);
                    var stringContent = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(absOnlineConfig.URL + "/creditline/changed-event", stringContent);
                }
            }
            catch
            {
            }
        }

        /// <inheritdoc />
        public async Task SendNotificationOverdueContractListAsync(List<OverdueForCrm> overdueContracts)
        {
            var absOnlineConfig = _outerServiceSettingRepository.Find(new { Code = Constants.ABS_ONLINE_INTEGRATION_SETTINGS_CODE });

            using (var client = new HttpClient())
            {
                var requestData = overdueContracts.Select(x => new
                {
                    value = x.DebtCost + x.PercentCost + x.PercentCost,
                    pay_date = x.NextPaymentDate.ToString("dd.MM.yyyy"),
                    uin = x.IdentityNumber,
                    tel = x.MobilePhone,
                    contract_id = x.ContractNumber,
                    debt_main = x.DebtCost,
                    procent = x.PercentCost,
                    penalties = x.PenaltyCost,
                    current = x.ExpiredToday ? 1 : 0
                });

                var request = JsonConvert.SerializeObject(requestData);
                var stringContent = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(absOnlineConfig.URL + "/contract/overdue-list", stringContent);
            }
        }


        /// <summary>
        /// Метод возвращает модель метаданных для сервиса СК
        /// </summary>
        /// <param name="contract">Контракт</param>
        /// <returns>Методанные для сервиса СК</returns>
        private LogMetaData GetLogMetaData(Contract contract)
        {
            var adminUser = _userRepository.Get(1);

            var logMetaData = new LogMetaData
            {
                BranchId = contract.Branch.Id,
                BranchName = contract.Branch.Name,
                UserId = adminUser.Id,
                UserName = adminUser.Login,
                ContractId = contract.Id,
                EntityType = EntityType.Contract
            };

            return logMetaData;
        }

        /// <summary>
        /// Метод создает в БД новую запись онлайн запроса в СК
        /// </summary>
        /// <param name="insurancePoliceRequest">Запрос заявки на страхование</param>
        /// <returns>Онлайн запрос в СК</returns>
        private InsuranceOnlineRequest GetNewInsuranceOnlineRequest(InsurancePoliceRequest insurancePoliceRequest)
        {
            var insuranceOnlineRequest = _insuranceOnlineRequestService.Find(new { ContractId = insurancePoliceRequest.ContractId });

            if (insuranceOnlineRequest != null)
                _insuranceOnlineRequestService.Delete(insuranceOnlineRequest.Id);

            return _insuranceOnlineRequestService.Save(insurancePoliceRequest.RequestData);
        }

        /// <summary>
        /// Метод создает в БД новую запись для запроса заявки на страхование
        /// </summary>
        /// <param name="contract">Контракт</param>
        /// <returns>Запрос заявки на страхование</returns>
        private InsurancePoliceRequest GetNewInsurancePoliceRequest(Contract contract)
        {
            var insurancePoliceRequests = _insurancePoliceRequestService.List(null, new { ContractId = contract.Id });

            if (insurancePoliceRequests.List.Any())
                insurancePoliceRequests.List.ForEach(x => _insurancePoliceRequestService.Delete(x.Id));

            var contractModel = new ContractModel
            {
                Contract = contract,
                PoliceRequests = new List<InsurancePoliceRequest>
                {
                    new InsurancePoliceRequest
                    {
                        IsInsuranceRequired = true,
                        InsuranceCompanyId = contract.Setting.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId,
                        RequestData = new InsuranceRequestData
                        {
                            InsurancePremium = contract.LoanCost - _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(contract.LoanCost)
                        }
                    }
                }
            };

            var newInsurancePoliceRequest = _insurancePoliceRequestService.GetPoliceRequest(contractModel);
            _insurancePoliceRequestService.SetContractIdAndNumber(newInsurancePoliceRequest, contract);
            _insurancePoliceRequestService.Save(newInsurancePoliceRequest);

            return newInsurancePoliceRequest;
        }

        /// <summary>
        /// Метод создает в БД новую запись страхового полиса
        /// </summary>
        /// <param name="policeRequest">Запрос заявки на страхование</param>
        /// <returns>Страховой полис</returns>
        private InsurancePolicy GetNewInsurancePolicy(InsurancePoliceRequest policeRequest)
        {
            var insurancePolicies = _insurancePolicyService.List(null, new { ContractId = policeRequest.ContractId });

            if (insurancePolicies.List.Any())
                insurancePolicies.List.ForEach(x => _insurancePolicyService.Delete(x.Id));

            var newInsurancePolicy = _insurancePolicyService.GetInsurancePolicy(policeRequest);
            _insurancePolicyService.Save(newInsurancePolicy);

            return newInsurancePolicy;
        }
    }
}
