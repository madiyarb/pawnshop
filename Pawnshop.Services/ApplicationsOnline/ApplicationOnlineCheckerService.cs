using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Reports;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Integrations.Fcb;
using Serilog;

namespace Pawnshop.Services.ApplicationsOnline
{
    public sealed class ApplicationOnlineCheckerService : IApplicationOnlineCheckerService
    {
        private readonly AccountRepository _accountRepository;
        private readonly NotionalRateRepository _notionalRateRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly ClientProfileRepository _clientProfileRepository;
        private readonly ClientExpenseRepository _expenseRepository;
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly ClientEmploymentRepository _clientEmploymentRepository;
        private readonly ApplicationOnlineInsuranceRepository _insuranceRepository;
        private readonly ApplicationOnlineTemplateChecksRepository _applicationOnlineTemplateChecksRepository;
        private readonly ApplicationOnlineChecksRepository _applicationOnlineChecksRepository;
        private readonly ILogger _logger;
        private readonly IDomainService _domainService;
        private readonly IFcb4Kdn _fcb;
        private readonly ApplicationOnlineRejectionReasonsRepository _rejectionReasonsRepository;
        private readonly IProducerAccessor _producers;
        private readonly ContractRepository _contractRepository;

        public ApplicationOnlineCheckerService(
            AccountRepository accountRepository,
            NotionalRateRepository notionalRateRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            ClientRepository clientRepository,
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ClientProfileRepository clientProfileRepository,
            ClientExpenseRepository expenseRepository,
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            ClientEmploymentRepository clientEmploymentRepository,
            ApplicationOnlineInsuranceRepository insuranceRepository,
            ApplicationOnlineTemplateChecksRepository applicationOnlineTemplateChecksRepository,
            ApplicationOnlineChecksRepository applicationOnlineChecksRepository,
            ILogger logger,
            IDomainService domainService,
            IFcb4Kdn fcb,
            ApplicationOnlineRejectionReasonsRepository rejectionReasonsRepository,
            IProducerAccessor producers,
            ContractRepository contractRepository)
        {
            _accountRepository = accountRepository;
            _notionalRateRepository = notionalRateRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _clientRepository = clientRepository;
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _clientProfileRepository = clientProfileRepository;
            _expenseRepository = expenseRepository;
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _clientEmploymentRepository = clientEmploymentRepository;
            _insuranceRepository = insuranceRepository;
            _applicationOnlineTemplateChecksRepository = applicationOnlineTemplateChecksRepository;
            _applicationOnlineChecksRepository = applicationOnlineChecksRepository;
            _logger = logger;
            _domainService = domainService;
            _fcb = fcb;
            _rejectionReasonsRepository = rejectionReasonsRepository;
            _producers = producers;
            _contractRepository = contractRepository;
        }

        public List<string> ReadyForVerification(Guid applicationOnlineId)
        {
            try
                {
                List<string> emptyFields = new List<string>();
                var application = _applicationOnlineRepository.Get(applicationOnlineId);
                emptyFields.AddRange(application.CheckForVerification());
                var client = _clientRepository.Get(application.ClientId);
                DateTime businessPurposesReferenceDate = DateTime.Parse(Constants.BUSINESS_PURPOSES_REFERENCE_DATE);
                bool isOldApplication = application.CreateDate != null && IsBusinessLoanPurpose(applicationOnlineId, client)
                    ? application.CreateDate < businessPurposesReferenceDate
                    : false;
                if (IsBusinessLoanPurpose(applicationOnlineId, client))
                {
                    if (isOldApplication)
                    {
                        if (!application.BusinessLoanPurposeId.HasValue)
                            emptyFields.Add("Не заполнена сфера деятельности");
                    }
                    else 
                    {
                        if (!application.OkedForIndividualsPurposeId.HasValue)
                            emptyFields.Add("Не заполнен ОКЭД");
                        if (!application.TargetPurposeId.HasValue)
                            emptyFields.Add("Не заполнено целевое использование");
                    }
                }
                emptyFields.AddRange(client.emptyFieldsList());
                var car = _applicationOnlineCarRepository.Get(application.ApplicationOnlinePositionId);
                emptyFields.AddRange(car.EmptyFieldsList());
                if (!client.Documents.Any())
                    emptyFields.Add("Нет ни одного документа клиента");
                var clientProfile = _clientProfileRepository.Get(client.Id) ?? new ClientProfile();
                emptyFields.AddRange(clientProfile.EmptyFields());
                var expenses = _expenseRepository.Get(client.Id);
                if (expenses == null)
                    emptyFields.Add("Не заполнены поля расходов");
                else
                    emptyFields.AddRange(expenses.EmptyFields());
                var position = _applicationOnlinePositionRepository.Get(application.ApplicationOnlinePositionId);
                if (!position.EstimatedCost.HasValue)
                    emptyFields.Add("Доход от стоимости авто (рыночная стоимость авто)");
                if (_fcb.ValidateIsOverdueClient(application.ClientId).Result)
                {
                    var rejectReason = _rejectionReasonsRepository.GetFiltredRejectionReasons(code: Constants.APPLICATION_ONLINE_REJECT_REASON_NEGATIVECREDIT_HISTORY_FCB).Result;
                    emptyFields.Add("Клиент имеет просрочку свыше 90 дней. Заявка отклонена");
                    application.Reject(Constants.ADMINISTRATOR_IDENTITY, rejectReason.List.SingleOrDefault().Id, rejectReason.List.SingleOrDefault().Code);
                    _applicationOnlineRepository.Update(application).Wait();

                    ApplicationOnlineContractInfo contractInfo = new ApplicationOnlineContractInfo();
                    if (application.ContractId.HasValue)
                    {
                        var contract = _contractRepository.Get(application.ContractId.Value);
                        contractInfo.MaturityDate = contract.MaturityDate;
                        var cps = contract.PaymentSchedule.OrderBy(psc => psc.Date).FirstOrDefault();
                        contractInfo.MonthlyPaymentAmount = cps?.DebtCost + cps?.PercentCost;
                    }

                    var message = new ApplicationOnlineStatusChanged
                    {
                        ApplicationOnline = application,
                        Status = application.Status.ToString(),
                        ApplicationOnlineContractInfo = contractInfo
                    };

                    _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message).Wait();
                }

                return emptyFields;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public List<string> ReadyForApprove(Guid applicationOnlineId)
        {
            try
            {
                List<string> emptyFields = new List<string>();
                var application = _applicationOnlineRepository.Get(applicationOnlineId);
                emptyFields.AddRange(application.CheckForApprove());
                var client = _clientRepository.Get(application.ClientId);
                var clientRegistrationAdress = client.Addresses.Where(address => address.AddressType.Code == "REGISTRATION");
                if (!clientRegistrationAdress.Any())
                    emptyFields.Add("Место прописки");
                var clientEmployment = _clientEmploymentRepository.GetListByClientId(client.Id);
                if (!clientEmployment.Any())
                    emptyFields.Add("Место работы заемщика");

                return emptyFields;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public List<string> ReadyForSign(Guid applicationOnlineId)
        {
            try
            {
                List<string> emptyFields = new List<string>();
                var application = _applicationOnlineRepository.Get(applicationOnlineId);
                var client = _clientRepository.Get(application.ClientId);
                if (!client.Requisites.Any(requisite => requisite.IsDefault &&
                                                        (requisite.RequisiteTypeId == 2
                                                         && !string.IsNullOrEmpty(requisite.Value)
                                                         && !string.IsNullOrEmpty(requisite.CardExpiryDate)
                                                         && !string.IsNullOrEmpty(requisite.CardHolderName) ||
                                                         (requisite.RequisiteTypeId == 1 &&
                                                          !string.IsNullOrEmpty(requisite.Value)))))
                {
                    emptyFields.Add("Нету заполненых реквизитов для вывода");
                }

                return emptyFields;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public bool AgePermittedForInsurance(Guid applicationOnlineId)
        {
            try
            {
                var application = _applicationOnlineRepository.Get(applicationOnlineId);

                var client = _clientRepository.GetOnlyClient(application.ClientId);

                var insurance = _insuranceRepository.GetByApplicationId(applicationOnlineId).Result;

                if (client.IsPensioner && insurance != null)
                {
                    var templateCheck =
                        _applicationOnlineTemplateChecksRepository.GetByCode("AgePermittedForInsurance");
                    if (templateCheck != null)
                    {
                        _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                        {
                            ApplicationOnlineId = applicationOnlineId,
                            TemplateId = templateCheck.Id,
                            CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                            CreateDate = DateTime.Now,
                        });
                    }

                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }

        }

        private bool IsBusinessLoanPurpose(Guid applicationOnlineId, Client client)
        {
            try
            {
                var application = _applicationOnlineRepository.Get(applicationOnlineId);

                return (application.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.BUSINESS_LOAN_PURPOSE).Id ||
                application.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.INVESTMENTS_LOAN_PURPOSE).Id ||
                application.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.CURRENT_ASSETS_LOAN_PURPOSE).Id ||
                application.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.INVESTMENTS_AND_CURRENT_ASSETS_LOAN_PURPOSE).Id) &&
                client.LegalForm.IsIndividual;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }

        }

        public async Task<CheckClientCoborrowerResult> CheckClientCoborrowerAccountAmountLimitAsync(
            ApplicationOnline application)
        {
            var domainValue = _domainService.GetDomainValue(
                Constants.NOTIONAL_RATE_TYPES,
                Constants.NOTIONAL_RATE_TYPES_MRP);
            var nationalRate = await _notionalRateRepository.GetByTypeOfLastYearAsync(domainValue.Id) ??
                throw new PawnshopApplicationException($"{Constants.NOTIONAL_RATE_TYPES_MRP} не указан!");
            var client = await _clientRepository.GetOnlyClientAsync(application.ClientId);

            var coborrowerContractsAccountBalance = await _accountRepository.GetCoborrowerContractsAccountBalance(application.ClientId);
            var clientActiveContractsAccountBalance = await _accountRepository.GetClientActiveContractsAccountBalance(application.ClientId);

            var generalAccountAmount = coborrowerContractsAccountBalance + clientActiveContractsAccountBalance;

            var generalAmount = generalAccountAmount + application.ApplicationAmount;

            var availableLoanCost = nationalRate.RateValue * Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP - generalAccountAmount;

            var result = new CheckClientCoborrowerResult();
            if (generalAccountAmount > nationalRate.RateValue * Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP)
            {
                result.Message =
                    $"По Субъекту ИИН/БИН {client.IdentityNumber}, сумма ОД {generalAmount} превышают {Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP} МРП. Доступная сумма 0 тенге";
                result.IsValid = false;
            }
            else if (generalAmount > nationalRate.RateValue * Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP)
            {
                result.Message =
                    $"По Субъекту ИИН/БИН {client.IdentityNumber} сумма ОД {generalAmount} превышают {Constants.COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP} МРП. Доступная сумма {availableLoanCost} тенге";
                result.IsValid = false;
            }

            return result;
        }
    }
}