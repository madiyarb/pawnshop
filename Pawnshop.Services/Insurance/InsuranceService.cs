using Newtonsoft.Json;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using System;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Core.Exceptions;
using System.Threading.Tasks;
using System.Net;
using Pawnshop.Services.Insurance.HttpHelper;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.Sms;
using Pawnshop.Data.Models.Notifications.Interfaces;
using Pawnshop.Services.Notifications;

namespace Pawnshop.Services.Insurance
{
    public class InsuranceService : IInsuranceService
    {
        private readonly IClientService _clientService;
        private readonly ContractService _contractService;
        private readonly ISessionContext _sessionContext;
        private readonly IEventLog _eventLog;
        private readonly InsurancePolicyService _insurancePolicyService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly AddressATERepository _addressATERepository;
        private readonly ClientProfileRepository _clientProfileRepository;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly GroupRepository _groupRepository;
        private readonly NotificationTemplateRepository _notificationTemplateRepository;
        private readonly IHttpRequestSenderService _httpSender;
        private readonly ISmsNotificationService _smsNotificationService;
        private readonly INotificationTemplateService _notificationTemplateService;
        public InsuranceService(
            IClientService clientService,
            ContractService contractService,
            ISessionContext sessionContext,
            IEventLog eventLog,
            InsurancePolicyService insurancePolicyService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            NotificationReceiverRepository notificationReceiverRepository,
            NotificationRepository notificationRepository,
            InnerNotificationRepository innerNotificationRepository,
            AddressATERepository addressATERepository,
            ClientProfileRepository clientProfileRepository,
            DomainValueRepository domainValueRepository,
            GroupRepository groupRepository,
            NotificationTemplateRepository notificationTemplateRepository,
            IHttpRequestSenderService httpSender,
            ISmsNotificationService smsNotificationService,
            INotificationTemplateService notificationTemplateService
            )
        {
            _clientService = clientService;
            _contractService = contractService;
            _sessionContext = sessionContext;
            _eventLog = eventLog;
            _insurancePolicyService = insurancePolicyService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _addressATERepository = addressATERepository;
            _clientProfileRepository = clientProfileRepository;
            _domainValueRepository = domainValueRepository;
            _groupRepository = groupRepository;
            _notificationTemplateRepository = notificationTemplateRepository;
            _httpSender = httpSender;
            _smsNotificationService = smsNotificationService;
            _notificationTemplateService = notificationTemplateService;
        }

        /// <summary>
        /// Запуск BPM процесса по созданию страхового полиса
        /// </summary>
        /// <param name="policeRequest"></param>
        /// <returns></returns>
        public async Task BPMRegisterPolicy(InsurancePoliceRequest policeRequest)
        {
            try
            {
                var oldRequest = _insurancePoliceRequestService.GetActivePoliceRequests(policeRequest.ContractId);

                foreach (var request in oldRequest)
                {
                    await BPMCancelPolicy(request);
                }

                var userId = Constants.ADMINISTRATOR_IDENTITY;
                var contract = _contractService.GetOnlyContract(policeRequest.ContractId);
                var branch = _groupRepository.Get(contract.BranchId);
                var requestData = policeRequest.RequestData;
                var client = _clientService.Get(contract.ClientId);
                var requestGuid = Guid.NewGuid();

                var highestAteKatoCode = _addressATERepository.GetKatoCodeOfRegion(client.Addresses.FirstOrDefault(x => x.IsActual && x.DeleteDate is null && x.AddressType.IsIndividual).ATE.Id);

                var clientProfile = _clientProfileRepository.Get(contract.ClientId);
                var maritalStatusName = _domainValueRepository.Get(clientProfile.MaritalStatusId.Value);

                if (_sessionContext.IsInitialized)
                {
                    userId = _sessionContext.UserId;
                }

                var variables = new InsuranceCreatePolicyRequestVariablesBPM(requestGuid, policeRequest, client, contract, userId, maritalStatusName.Name, highestAteKatoCode, branch.DisplayName);

                var requestModel = new InsuranceCreatePolicyRequestBPM()
                { variables = variables };

                var requestModelString = JsonConvert.SerializeObject(requestModel, Formatting.None, new InsuranceRequestBPMJsonConverter(typeof(InsuranceCreatePolicyRequestBPM)));

                policeRequest.Guid = requestGuid;
                policeRequest.RequestDataBPM = requestModelString;
                policeRequest.Status = InsuranceRequestStatus.Sent;
                _insurancePoliceRequestService.Save(policeRequest);

                var response = await _httpSender.SendCreateRequest(requestModelString);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new PawnshopApplicationException($"Ошибка в ответе от BPM responseStatus={response.StatusCode} responseBody= {responseBody}");

                if (contract.Status != ContractStatus.Signed)
                {
                    contract.Status = ContractStatus.InsuranceApproved;
                    _contractService.Save(contract);
                }

                _eventLog.Log(EventCode.RegisterInsurancePolice, EventStatus.Success, EntityType.InsurancePolicy, policeRequest.ContractId, requestModelString, responseBody);
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.RegisterInsurancePolice, EventStatus.Failed, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(e));
                throw;
            }
        }

        /// <summary>
        /// Данный метод вызывается BPM процессом при созданий страхового полиса
        /// </summary>
        /// <param name="policeRequest"></param>
        /// <param name="companyCode"></param>
        public void BPMAcceptPolicy(InsurancePoliceRequest policeRequest, int companyCode)
        {
            try
            {
                var company = new Client();
                switch (companyCode)
                {
                    case 10:
                        company = _clientService.Get(Constants.FREEDOM_LIFE_INSURANCE_CLIENT_ID); // Компания по страхованию жизни «Freedom Finance Life»
                        break;
                    case 20:
                        company = _clientService.Get(Constants.NOMAD_LIFE_INSURANCE_CLIENT_ID); // КCЖ «Nomad Life»
                        break;
                    default:
                        break;
                }
                var contract = _contractService.GetOnlyContract(policeRequest.ContractId);
                var requestData = policeRequest.RequestData;
                policeRequest.InsuranceCompanyId = company.Id;
                policeRequest.Status = InsuranceRequestStatus.Completed;
                InsurancePolicy insurancePolicy = _insurancePolicyService.GetInsurancePolicy(policeRequest);
                policeRequest.RequestData.ContractNumber = insurancePolicy.PoliceNumber;
                _insurancePoliceRequestService.Save(policeRequest);
                _insurancePolicyService.Save(insurancePolicy);

                NotifyBranch(contract, "Страховой полис оформлен", @$"Страховой полис оформлен в компании {company.FullName}, 
                    страховая премия - {insurancePolicy.InsurancePremium}, 
                    сумма страхования - {insurancePolicy.InsuranceAmount}");

                _eventLog.Log(EventCode.AcceptPolicy, EventStatus.Success, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(policeRequest), JsonConvert.SerializeObject(insurancePolicy));
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.AcceptPolicy, EventStatus.Failed, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        /// <summary>
        /// Данный метод вызывается BPM процессом когда страховой полис не создан до 22:00
        /// </summary>
        /// <param name="policeRequest"></param>
        public InsurancePolicy BPMKillPolicy(InsurancePoliceRequest policeRequest)
        {
            try
            {
                var contract = _contractService.GetOnlyContract(policeRequest.ContractId);
                InsurancePolicy insurancePolicy = _insurancePolicyService.GetInsurancePolicy(policeRequest);
                insurancePolicy.Contract = contract;
                if (contract.Status == ContractStatus.Signed)
                {
                    var template = _notificationTemplateRepository.GetByCode(Constants.INSURANCE_FAIL);
                    //уведомление клиенту
                    NotifyClient(contract);
                    //уведомление филиала 
                    NotifyBranch(contract, template.Subject, template.Message);
                }

                policeRequest.Status = InsuranceRequestStatus.Canceled;
                _insurancePoliceRequestService.Save(policeRequest);

                _eventLog.Log(EventCode.KillPolicy, EventStatus.Success, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(policeRequest), JsonConvert.SerializeObject(insurancePolicy));

                return insurancePolicy;
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.KillPolicy, EventStatus.Failed, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        /// <summary>
        /// Запуск BPM процесса по аннулированию страхового полиса
        /// </summary>
        /// <param name="policeRequest"></param>
        /// <returns></returns>
        public async Task BPMCancelPolicy(InsurancePoliceRequest policeRequest)
        {
            try
            {
                var userId = Constants.ADMINISTRATOR_IDENTITY;

                if (_sessionContext.IsInitialized)
                {
                    userId = _sessionContext.UserId;
                }

                var requestVariableModel = new InsuranceCancelPolicyRequestVariablesBPM()
                {
                    Request = policeRequest.Guid.ToString(),
                    CancelAuthorId = userId.ToString(),
                };

                var requestModel = new InsuranceCancelPolicyRequestBPM()
                {
                    variables = requestVariableModel
                };

                var requestModelString = JsonConvert.SerializeObject(requestModel, Formatting.None, new InsuranceRequestBPMJsonConverter(typeof(InsuranceCancelPolicyRequestBPM)));
                var response = await _httpSender.SendCancelRequest(requestModelString);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new PawnshopApplicationException($"Ошибка в ответе от BPM responseStatus={response.StatusCode} responseBody= {responseBody}");

                policeRequest.Status = InsuranceRequestStatus.SentCancel;
                _insurancePoliceRequestService.Save(policeRequest);

                _eventLog.Log(EventCode.CancelInsurancePolice, EventStatus.Success, EntityType.InsurancePolicy, policeRequest.ContractId, requestModelString, responseBody);
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.CancelInsurancePolice, EventStatus.Failed, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        /// <summary>
        /// Данный метод вызывается BPM процессом когда страховой полис аннулирован
        /// </summary>
        /// <param name="policeRequest"></param>
        public void BPMAcceptCancelPolicy(InsurancePoliceRequest policeRequest)
        {
            try
            {
                var contract = _contractService.GetOnlyContract(policeRequest.ContractId);

                policeRequest.Status = InsuranceRequestStatus.Annuled;
                policeRequest.RequestData.InsurancePremium = 0;
                _insurancePoliceRequestService.Save(policeRequest);

                InsurancePolicy insurancePolicy = _insurancePolicyService.GetInsurancePolicy(policeRequest.Id, true);
                if (insurancePolicy != null)
                {
                    insurancePolicy.DeleteDate = DateTime.Now;
                    _insurancePolicyService.Save(insurancePolicy);
                    NotifyBranch(contract, "Страховой полис аннулирован", "Страховой полис был аннулирован");
                }

                _eventLog.Log(EventCode.AcceptCancelPolicy, EventStatus.Success, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(policeRequest), JsonConvert.SerializeObject(insurancePolicy));
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.AcceptCancelPolicy, EventStatus.Failed, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        /// <summary>
        /// Данный метод вызывается BPM процессом когда страховой полис не аннулирован до 22:00
        /// </summary>
        /// <param name="policeRequest"></param>
        public void BPMKillCancelPolicy(InsurancePoliceRequest policeRequest)
        {
            try
            {
                var contract = _contractService.GetOnlyContract(policeRequest.ContractId);

                policeRequest.Status = InsuranceRequestStatus.Error;
                _insurancePoliceRequestService.Save(policeRequest);

                InsurancePolicy insurancePolicy = _insurancePolicyService.GetInsurancePolicy(policeRequest.Id);
                if (insurancePolicy != null)
                {
                    var template = _notificationTemplateRepository.GetByCode(Constants.INSURANCE_FAIL);
                    NotifyBranch(contract, template.Subject, template.Message);
                }

                _eventLog.Log(EventCode.KillCancelPolicy, EventStatus.Success, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(policeRequest));
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.KillCancelPolicy, EventStatus.Failed, EntityType.InsurancePolicy, policeRequest.ContractId, JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        private void NotifyClient(Contract contract)
        {
            var template = _notificationTemplateService.GetTemplate(Constants.INSURANCE_FAIL);
            var smsModel = new SmsCreateNotificationModel(
                contract.ClientId,
                contract.BranchId,
                template.Subject,
                template.Message,
                contract.Id,
                isPrivate: true);

            _smsNotificationService.CreateSmsNotification(smsModel);
        }

        private void NotifyBranch(Contract contract, string subject, string message)
        {
            var notification = new InnerNotification
            {
                CreateDate = DateTime.Now,
                CreatedBy = Constants.ADMINISTRATOR_IDENTITY,
                EntityType = EntityType.Contract,
                EntityId = contract.Id,
                Message = message,
                ReceiveBranchId = contract.BranchId,
                Status = InnerNotificationStatus.Sent
            };
            _innerNotificationRepository.Insert(notification);
        }

    }
}
