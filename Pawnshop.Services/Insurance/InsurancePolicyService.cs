using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance.InsuranceCompanies;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace Pawnshop.Services.Insurance
{
    public class InsurancePolicyService : BaseService<InsurancePolicy>, IInsurancePolicyService
    {
        private static readonly Dictionary<int, OuterServiceCompanyConfig> outerServiceCompanyConfigs = new Dictionary<int, OuterServiceCompanyConfig>();
        private readonly ClientContactRepository _clientContactRepository;
        private readonly IContractService _contractService;
        private readonly IInsuranceCompanyServiceFactory _insuranceCompanyServiceFactory;
        private readonly InsurancePoliceRequestRepository _insurancePoliceRequestRepository;
        private readonly InsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly string tokenCacheIdentifier = "insuranceToken";
        private readonly int _minutesToStoreToken = 8;

        public InsurancePolicyService(IRepository<InsurancePolicy> insurancePolicyRepository,
                                      IInsuranceCompanyServiceFactory insuranceCompanyServiceFactory,
                                      IRepository<OuterServiceSetting> outerServiceSettingRepository,
                                      IContractService contractService,
                                      InsurancePoliceRequestRepository insurancePoliceRequestRepository,
                                      ClientContactRepository clientContactRepository,
                                      InsurancePremiumCalculator insurancePremiumCalculator
,
                                      IMemoryCache memoryCache) : base(insurancePolicyRepository)
        {
            _insuranceCompanyServiceFactory = insuranceCompanyServiceFactory;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _contractService = contractService;
            _insurancePoliceRequestRepository = insurancePoliceRequestRepository;
            _clientContactRepository = clientContactRepository;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _memoryCache = memoryCache;
        }

        public void CancelNewPolicy(LogMetaData logMetaData, InsurancePoliceRequest insurancePoliceRequest)
        {
            insurancePoliceRequest.CancelledUserId = logMetaData.UserId;
            insurancePoliceRequest.CancelDate = DateTime.Now;
            insurancePoliceRequest.CancelReason = "Изменение состояния полиса джобом InsurancePoliciesCancelJob";

            UpdatePoliceRequestStatus(InsuranceRequestStatus.Canceled, insurancePoliceRequest, logMetaData);
        }

        public CancelInsuranceResponse CancelPolicy(LogMetaData logMetaData, InsurancePoliceRequest insurancePoliceRequest)
        {
            var insurancePolicy = Find(new { PoliceRequestId = insurancePoliceRequest.Id });

            // Удалить после 01.12.2023 до этого времени проверяем альтернативный метод
            //int secondsAfterCreatingPolicy = Convert.ToInt32((DateTime.Now - insurancePolicy.CreateDate).TotalSeconds);
            //if (secondsAfterCreatingPolicy < Constants.SECONDS_TO_CANCEL_POLICY_IN_SK)
            //{
            //    throw new PawnshopApplicationException($"Повторите попытку аннулирования страхового полиса через {Constants.SECONDS_TO_CANCEL_POLICY_IN_SK - secondsAfterCreatingPolicy} секунд");
            //}

            var contract = _contractService.Get(insurancePoliceRequest.ContractId);
            if (contract is null)
                throw new PawnshopApplicationException($"Не найден договор с Id {insurancePoliceRequest.ContractId}");

            if (contract.Status != ContractStatus.InsuranceApproved && contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.BoughtOut && contract.Status != ContractStatus.AwaitForOrderApprove)
                throw new PawnshopApplicationException($"Договор {contract.ContractNumber} не находится в состоянии 'Подтвержден в СК' чтобы отменить Страховой полис");

            string token = String.Empty;
            if (_memoryCache.TryGetValue(tokenCacheIdentifier, out token))
            {
                logMetaData.Token = token;
            }

            //если это подписание и контракт уже подписан, выкуплен, реализован, то не даем анулировать полис
            if (insurancePoliceRequest.RequestData.AdditionCost == 0 && //но если это добор, то анулировать полис можно
                (contract.Status == ContractStatus.Signed || contract.Status == ContractStatus.BoughtOut
                || contract.Status == ContractStatus.SoldOut || contract.Status == ContractStatus.Disposed))
                throw new PawnshopApplicationException("Аннулирование полиса недоступно");

            //CheckClientContactVerification(contract.ClientId);

            IInsuranceCompanyService insuranceService = null;
            CancelInsuranceResponse result = null;
            InsuranceRequestStatus status = InsuranceRequestStatus.SentCancel;

            UpdatePoliceRequestStatus(status, insurancePoliceRequest);

            try
            {
                using (var transaction = _repository.BeginTransaction())
                {
                    insuranceService = GetInsuranceService(insurancePoliceRequest);

                    result = insuranceService.CancelPolicy(FillLogMetaData(logMetaData, insurancePoliceRequest));
                    var isBuyCar = contract.SettingId.HasValue && contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR;

                    if (result.success)
                    {
                        if (result.response.response != "ПОЛИС НЕЛЬЗЯ АННУЛИРОВАТЬ ИЛИ УЖЕ АННУЛИРОВАН")
                        {
                            if (insurancePolicy != null)
                                Delete(insurancePolicy.Id);

                            status = InsuranceRequestStatus.Canceled;
                        }
                        else
                        {
                            status = InsuranceRequestStatus.Approved;
                            UpdatePoliceRequestStatus(status, insurancePoliceRequest);
                            transaction.Commit();
                            throw new PawnshopApplicationException($"Страховая компания в данный момент не может аннулировать страховой полис. Повторите попытку аннулирования страхового полиса через 3-5 мин");
                        }
                    }

                    else
                    {
                        result.success = false;
                        if (result.response != null)
                        {
                            result.message = result.response.response ?? result.response.error ?? result.response.message;
                            status = InsuranceRequestStatus.RejectedCancel;
                        }
                        else
                            status = InsuranceRequestStatus.Error;
                    }

                    if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.BoughtOut && contract.Status != ContractStatus.AwaitForOrderApprove)
                        contract.Status = ContractStatus.Draft;

                    if (isBuyCar)
                        contract.Status = ContractStatus.AwaitForInsuranceCopy;

                    _contractService.Save(contract);
                    UpdatePoliceRequestStatus(status, insurancePoliceRequest, logMetaData);
                    if ((token is null || token == String.Empty) && !String.IsNullOrEmpty(result.response.token))
                    {
                        _memoryCache.Set(tokenCacheIdentifier, result.response.token,
                             new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_minutesToStoreToken) });
                    }
                    transaction.Commit();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new PawnshopApplicationException(ex.Message);
            }
            finally
            {
                insuranceService?.Dispose();
            }
        }

        public void CheckClientContactVerification(int clientId)
        {
            var clientDefaultContact = _clientContactRepository.GetActualDefaultContact(clientId);

            if (clientDefaultContact is null)
                throw new PawnshopApplicationException(
                    "У клиента не найден основной номер телефона, который должен быть отправлен в страховую компанию");
        }

        public InsurancePolicy GetInsurancePolicy(int policeRequestId, bool isCancel = false) => Find(new { PoliceRequestId = policeRequestId, IsCancel = isCancel });

        public InsurancePolicy GetInsurancePolicy(InsurancePoliceRequest insurancePoliceRequest)
        {
            var today = DateTime.Now.Date;
            return new InsurancePolicy
            {
                AlgorithmVersion = insurancePoliceRequest.AlgorithmVersion,
                AuthorId = insurancePoliceRequest.AuthorId,
                ContractId = insurancePoliceRequest.ContractId,
                CreateDate = DateTime.Now,
                EndDate = insurancePoliceRequest.RequestData.InsuranceEndDate,
                EsbdAmount = insurancePoliceRequest.RequestData.EsbdAmount,
                InsuranceAmount = insurancePoliceRequest.RequestData.InsuranceAmount,
                InsurancePremium = insurancePoliceRequest.RequestData.InsurancePremium,
                PoliceNumber = $"TFG-{today.Day.ToString().PadLeft(2, '0')}{today.Month.ToString().PadLeft(2, '0')}{today.Year}-{insurancePoliceRequest.ContractId}",
                PoliceRequestId = insurancePoliceRequest.Id,
                RootContractId = insurancePoliceRequest.ContractId,
                StartDate = insurancePoliceRequest.RequestData.InsuranceStartDate,
                SurchargeAmount = insurancePoliceRequest.RequestData.SurchargeAmount,
                YearPremium = insurancePoliceRequest.RequestData.YearPremium,
            };
        }

        public InsurancePolicy GetInsurancePolicyForAddition(Contract parentContract, decimal actionCost, DateTime actionDate, decimal? childContractCost = null, int? settingId = null)
        {
            if (childContractCost is null)
                childContractCost = _contractService.GetChildContractCost(parentContract.Id, actionDate, actionCost);

            InsurancePolicy insurancePolicy = null;

            var setting = !settingId.HasValue ? _contractService.GetSettingForContract(parentContract, childContractCost) :
                                                _contractService.GetSettingForContract(parentContract, settingId.Value);

            if (setting.IsInsuranceAvailable)
            {
                var additionInsuranceAmount = _insurancePremiumCalculator.GetAdditionInsuranceAmount(parentContract.Id, actionCost, actionDate, parentContract.ClosedParentId);

                if (additionInsuranceAmount > 0)
                {
                    var insuranceCompanyId = setting.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId;

                    var insurancePremium = _insurancePremiumCalculator.GetInsuranceDataV2(additionInsuranceAmount, insuranceCompanyId, setting.Id, actionDate, parentContract.Id, parentContract.ClosedParentId).InsurancePremium;

                    if (insurancePremium > 0 && !parentContract.Client.IsPensioner)
                    {
                        var latestPoliceRequest = _insurancePoliceRequestRepository.Find(new { ContractId = parentContract.Id, IsRejected = false, NotInStatus = new List<int>() { (int)InsuranceRequestStatus.Completed, (int)InsuranceRequestStatus.Canceled } });

                        if (latestPoliceRequest is null)
                            throw new PawnshopApplicationException("Введенная сумма добора с текущими остатками по ОД превышает минимальную сумму по тарифу, создайте заявку на добор");

                        if (!latestPoliceRequest.IsInsuranceRequired && actionCost > latestPoliceRequest.RequestData.AdditionCost)
                            throw new PawnshopApplicationException(
                                $"Сумма добора должна быть меньше или равна сумме, разрешенной без страхования ({latestPoliceRequest.RequestData.AdditionCost} тг.)");

                        if (latestPoliceRequest.IsInsuranceRequired)
                        {
                            if (latestPoliceRequest.OnlineRequestId is null)
                                throw new PawnshopApplicationException("Пройдите по сформированной ссылке на сайт ТасКредит и сохраните данные");

                            insurancePolicy = Find(new { PoliceRequestId = latestPoliceRequest.Id });

                            if (insurancePolicy is null)
                                throw new PawnshopApplicationException("Страховой полис не найден");
                        }
                    }
                }
            }

            return insurancePolicy;
        }

        public RegisterInsuranceResponse RegisterPolicy(LogMetaData logMetaData, InsurancePoliceRequest insurancePoliceRequest)
        {
            var contract = _contractService.Get(insurancePoliceRequest.ContractId);

            string token = String.Empty;
            if(_memoryCache.TryGetValue(tokenCacheIdentifier, out token))
            {
                logMetaData.Token = token;
            }

            CheckClientContactVerification(contract.ClientId);

            IInsuranceCompanyService insuranceService = null;
            RegisterInsuranceResponse result = null;
            InsuranceRequestStatus status = InsuranceRequestStatus.Sent;

            UpdatePoliceRequestStatus(status, insurancePoliceRequest);

            try
            {
                insuranceService = GetInsuranceService(insurancePoliceRequest);

                using (var transaction = _repository.BeginTransaction())
                {
                    result = insuranceService.RegisterPolicy(FillLogMetaData(logMetaData, insurancePoliceRequest));
                    var isResultSuccess = result.success && result.response != null && result.response.insuranceNum != null && result.response.insuranceNum.Equals(insurancePoliceRequest.RequestData.ContractNumber);


                    var requestData = insurancePoliceRequest.RequestData;

                    if (isResultSuccess)
                    {
                        var insurancePolicy = new InsurancePolicy
                        {
                            RootContractId = contract.ClosedParentId ?? contract.Id,
                            StartDate = requestData.InsuranceStartDate,
                            EndDate = requestData.InsuranceEndDate,
                            InsuranceAmount = requestData.InsuranceAmount,
                            InsurancePremium = requestData.InsurancePremium,
                            PoliceNumber = result.response.insuranceNum,
                            CreateDate = DateTime.Now,
                            AuthorId = logMetaData.UserId ?? 0,
                            PoliceRequestId = insurancePoliceRequest.Id,
                            SurchargeAmount = requestData.SurchargeAmount,
                            YearPremium = requestData.YearPremium,
                            AlgorithmVersion = insurancePoliceRequest.AlgorithmVersion,
                            EsbdAmount = insurancePoliceRequest.RequestData.EsbdAmount,
                        };

                        status = InsuranceRequestStatus.Approved;

                        if (contract.Status != ContractStatus.Signed)
                        {
                            insurancePolicy.ContractId = contract.Id;
                            contract.Status = ContractStatus.InsuranceApproved;
                        }

                        if (contract.CreatedInOnline)
                        {
                            insurancePolicy.ContractId = contract.Id;
                        }

                        base.Save(insurancePolicy);
                    }
                    else
                    {
                        var isBuyCar = contract.SettingId.HasValue && contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR;

                        result.success = false;
                        if (result.response != null)
                        {
                            result.message = result.response.insuranceNum ?? result.response.error ?? result.response.message;

                            status = InsuranceRequestStatus.Rejected;

                            if (contract.Status != ContractStatus.Signed && !isBuyCar)
                            {
                                contract.Status = ContractStatus.Draft;

                                if (contract.ContractClass != ContractClass.Tranche)
                                {
                                    if (!contract.Positions.Any())
                                        throw new PawnshopApplicationException("Позиции пусты");

                                    var position = contract.Positions.FirstOrDefault();
                                    contract.LoanCost = position.LoanCost;
                                }
                            }
                        }
                        else
                            status = InsuranceRequestStatus.Error;

                        if (isBuyCar)
                            contract.Status = ContractStatus.AwaitForInsuranceCopy;
                    }

                    if ((token is null || token == String.Empty) && !String.IsNullOrEmpty(result.response.token))
                    {
                        _memoryCache.Set(tokenCacheIdentifier, result.response.token,
                             new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_minutesToStoreToken) });
                    }

                    _contractService.Save(contract);
                    UpdatePoliceRequestStatus(status, insurancePoliceRequest);
                    transaction.Commit();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new PawnshopApplicationException(ex.Message);
            }
            finally
            {
                insuranceService?.Dispose();
            }
        }

        public InsurancePoliceRequest Save(InsurancePoliceRequest model)
        {
            if (model.Id > 0) _insurancePoliceRequestRepository.Update(model);
            else _insurancePoliceRequestRepository.Insert(model);

            return model;
        }

        private InsurancePoliceRequest FillLogMetaData(LogMetaData logMetaData, InsurancePoliceRequest insurancePoliceRequest)
        {
            insurancePoliceRequest.RequestData.MetaDataItem = logMetaData;
            return insurancePoliceRequest;
        }

        private OuterServiceCompanyConfig GetInsuranceCompanyConfig(Client insuranceCompany)
        {
            OuterServiceCompanyConfig outerServiceCompanyConfig;

            if (!outerServiceCompanyConfigs.TryGetValue(insuranceCompany.Id, out outerServiceCompanyConfig))
            {
                if (string.IsNullOrEmpty(insuranceCompany.IdentityNumber))
                    throw new PawnshopApplicationException($"БИН Страховой компаний {insuranceCompany.Name} не может быть пустым");

                var insuranceCompanySettings = _outerServiceSettingRepository.List(new ListQuery(), new { ServiceCompanyId = insuranceCompany.Id });
                if (insuranceCompanySettings == null)
                    throw new PawnshopApplicationException($"Настройка Страховой компаний для {insuranceCompany.IdentityNumber}  не найдена");

                /*outerServiceCompanyConfig = new OuterServiceCompanyConfig();
                insuranceCompanySettings.ForEach(item =>
                {
                    outerServiceCompanyConfig.InsuranceCompanyCode = Enum.Parse<InsuranceCompaniesCode>(item.ServiceType.Code);

                    outerServiceCompanyConfig.Url = item.URL;
                    outerServiceCompanyConfig.ControllerUrl = item.ControllerURL;

                    outerServiceCompanyConfig.BasicAuth = item.Login;
                    outerServiceCompanyConfig.BasicPass = item.Password;                    
                });*/

                outerServiceCompanyConfig = insuranceCompanySettings.Select(item => new OuterServiceCompanyConfig
                {
                    InsuranceCompanyCode = Enum.Parse<InsuranceCompaniesCode>(item.ServiceType.Code),
                    Url = item.URL,
                    ControllerUrl = item.ControllerURL,
                    BasicAuth = item.Login,
                    BasicPass = item.Password
                }).ToList().FirstOrDefault();

                outerServiceCompanyConfigs[insuranceCompany.Id] = outerServiceCompanyConfig;
            }

            return outerServiceCompanyConfig;
        }

        private IInsuranceCompanyService GetInsuranceService(InsurancePoliceRequest insurancePoliceRequest)
        {
            var insuranceCompany = insurancePoliceRequest.InsuranceCompany;
            var outerServiceCompanyConfig = GetInsuranceCompanyConfig(insuranceCompany);
            IInsuranceCompanyService insuranceService = _insuranceCompanyServiceFactory.createInsuranceService(outerServiceCompanyConfig);

            return insuranceService;
        }

        private void UpdatePoliceRequestStatus(InsuranceRequestStatus status, InsurancePoliceRequest insurancePoliceRequest, LogMetaData? logMetaData = null)
        {
            insurancePoliceRequest.Status = status;
            _insurancePoliceRequestRepository.Update(insurancePoliceRequest);
        }
    }
}