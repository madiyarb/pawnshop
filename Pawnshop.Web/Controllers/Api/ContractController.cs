using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Query;
using Pawnshop.Data.Models.Contracts.Views;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries.PrintTemplate;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.Parking;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Services.Applications;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Collection;
using Pawnshop.Services.Contracts.ContractActionOnlineExecutionCheckerService;
using Pawnshop.Services.Contracts.PartialPayment;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines.Buyout;
using Pawnshop.Services.CreditLines.PartialPayment;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Exceptions;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Services.PDF;
using Pawnshop.Services.Positions;
using Pawnshop.Services.Storage;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Services;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Contract;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Web.Controllers.Api
{
    public class ContractController : Controller
    {
        public const string LOAN_PURPOSE_DOMAIN = "LOAN_PURPOSE";
        public const string OTHER_LOAN_PURPOSE_DOMAIN_VALUE = "OTHER";
        private readonly ContractRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly ContractNumberCounterRepository _counterRepository;
        private readonly ContractsExcelBuilder _excelBuilder;
        private readonly IStorage _storage;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly IClientService _clientService;
        private readonly ContractWordBuilder _wordBuilder;
        private readonly AnnuityContractWordBuilder _annuityWordBuilder;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly PrintTemplateRepository _printTemplateRepository;
        private readonly ContractDocumentRepository _contractDocumentRepository;
        private readonly CarRepository _carRepository;
        private readonly MachineryRepository _machineryRepository;
        private readonly RealtyRepository _realtyRepository;
        private readonly ParkingHistoryRepository _parkingHistoryRepository;
        private readonly IVerificationService _verificationService;
        private readonly IContractVerificationService _contractVerificationService;
        private readonly IDomainService _domainService;
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly IEventLog _eventLog;
        private readonly IAccountService _accountService;
        private readonly IDictionaryService<Type> _typeService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IClientSignerService _clientSignerService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        private readonly IClientModelValidateService _clientModelValidateService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IApplicationService _applicationService;
        private readonly IContractService _contractService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractKdnService _contractKdnService;
        private readonly IContractActionPartialPaymentService _contractActionPartialPaymentService;
        private readonly IPositionSubjectService _positionSubjectService;
        private readonly IPositionEstimateHistoryService _positionEstimateHistoryService;
        private readonly IContractStatusHistoryService _contractStatusHistoryService;
        private readonly ApplicationRepository _applicationRepository;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly ICreditLineService _creditLineService;
        private readonly IApplicationOnlineService _applicationOnlineService;
        private readonly IClientDefermentService _clientDefermentService;
        private readonly IAuctionRepository _auctionRepository;

        public ContractController(ContractRepository repository, UserRepository userRepository,
            ContractNumberCounterRepository counterRepository, ContractsExcelBuilder excelBuilder,
            IStorage storage,
            BranchContext branchContext, ISessionContext sessionContext, ContractWordBuilder wordBuilder,
            AnnuityContractWordBuilder annuityWordBuilder, LoanPercentRepository loanPercentRepository,
            CategoryRepository categoryRepository, PrintTemplateRepository printTemplateRepository,
            ContractDocumentRepository contractDocumentRepository, IVerificationService verificationService,
            CarRepository carRepository, MachineryRepository machineryRepository, ParkingHistoryRepository parkingHistoryRepository,
            IDomainService domainService, IContractDutyService contractDutyService,
            IAccountService accountService, IDictionaryService<Type> typeService,
            IContractPaymentScheduleService contractPaymentScheduleService, IContractVerificationService contractVerificationService,
            IClientSignerService clientSignerService, IClientQuestionnaireService clientQuestionnaireService,
            IClientModelValidateService clientModelValidateService, IInsurancePoliceRequestService insurancePoliceRequestService,
            IInsurancePremiumCalculator insurancePremiumCalculator, IApplicationService applicationService,
            IContractService contractService, IContractKdnService contractKdnService,
            IContractActionService contractActionService, RealtyRepository realtyRepository,
            IClientService clientService,
            IContractActionPartialPaymentService contractActionPartialPaymentService,
            IPositionSubjectService positionSubjectService,
            IPositionEstimateHistoryService positionEstimateHistoryService,
            ContractAdditionalInfoRepository сontractAdditionalInfoRepository,
            IContractStatusHistoryService contractStatusHistoryService,
            IEventLog eventLog,
            ApplicationRepository applicationRepository,
            IClientBlackListService clientBlackListService,
            IPaymentScheduleService paymentScheduleService,
            ICreditLineService creditLineService,
            IClientDefermentService clientDefermentService,
            IApplicationOnlineService applicationOnlineService,
            IAuctionRepository auctionRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _counterRepository = counterRepository;
            _excelBuilder = excelBuilder;
            _storage = storage;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
            _wordBuilder = wordBuilder;
            _annuityWordBuilder = annuityWordBuilder;
            _loanPercentRepository = loanPercentRepository;
            _categoryRepository = categoryRepository;
            _printTemplateRepository = printTemplateRepository;
            _contractDocumentRepository = contractDocumentRepository;
            _carRepository = carRepository;
            _machineryRepository = machineryRepository;
            _realtyRepository = realtyRepository;
            _parkingHistoryRepository = parkingHistoryRepository;
            _verificationService = verificationService;
            _contractVerificationService = contractVerificationService;
            _domainService = domainService;

            _accountService = accountService;
            _typeService = typeService;
            _contractDutyService = contractDutyService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _clientSignerService = clientSignerService;
            _clientQuestionnaireService = clientQuestionnaireService;
            _clientModelValidateService = clientModelValidateService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _applicationService = applicationService;
            _contractService = contractService;
            _contractKdnService = contractKdnService;
            _contractActionService = contractActionService;
            _clientService = clientService;
            _contractActionPartialPaymentService = contractActionPartialPaymentService;
            _positionSubjectService = positionSubjectService;
            _positionEstimateHistoryService = positionEstimateHistoryService;
            _contractAdditionalInfoRepository = сontractAdditionalInfoRepository;
            _contractStatusHistoryService = contractStatusHistoryService;
            _eventLog = eventLog;
            _applicationRepository = applicationRepository;
            _clientBlackListService = clientBlackListService;
            _paymentScheduleService = paymentScheduleService;
            _creditLineService = creditLineService;
            _applicationOnlineService = applicationOnlineService;
            _clientDefermentService = clientDefermentService;
            _auctionRepository = auctionRepository;
        }

        [HttpPost("/api/contract/list")]
        public ListModel<Contract> List([FromBody] ListQueryModel<ContractListQueryModel> listQuery)
        {
            if (listQuery == null)
                listQuery = new ListQueryModel<ContractListQueryModel>();
            if (listQuery.Model == null)
                listQuery.Model = new ContractListQueryModel();

            listQuery.Model.OwnerIds = listQuery.Model.ClientId.HasValue
                ? null
                : new int[] { _branchContext.Branch.Id };

            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<Contract>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost("/api/contract/isKdnRequired4Contract")]
        public bool IsKdnRequired4Contract([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            var contract = _repository.Get(id);
            if (contract is null)
                throw new PawnshopApplicationException($"Договор с Id {id} не найден");

            return _contractKdnService.IsKdnRequired(contract);
        }

        [HttpPost("/api/contract/getContractKdnEstimatedIncomeModels")]
        public IActionResult GetContractKdnEstimatedIncomeModels([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            var contract = _repository.Get(id);
            if (contract is null)
                throw new PawnshopApplicationException($"Договор с Id {id} не найден");

            if (contract.Client is null)
                throw new PawnshopApplicationException($"Заполните клиента для договора с Id {id}");

            if (contract.Positions is null)
                throw new PawnshopApplicationException($"Заполните позицию для договора с Id {id}");

            if (contract.ContractClass != ContractClass.Tranche && contract.Positions.Count == 0)
                throw new PawnshopApplicationException($"Заполните позицию для договора с Id {id}");

            //для кредитной линии кдн не считаем
            if (contract.ContractClass == ContractClass.CreditLine)
                return Ok();

            var user = _userRepository.Get(_sessionContext.UserId);
            return Ok(_contractKdnService.GetContractKdnEstimatedIncomeModels(contract, user));
        }

        [HttpGet("/api/contract/getonlycontract")]
        public ActionResult<Contract> GetOnlyContract(int id)
        {
            var contract = _repository.GetOnlyContract(id);
            if (contract == null)
            {
                return NotFound();
            }

            return contract;
        }

        [HttpPost("/api/contract/card")]
        public ContractModel Card([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);

            if (contract == null)
                throw new InvalidOperationException();

            if (contract.Status == ContractStatus.Draft && contract.ClientId > 0)
            {
                var cardType = CardType.Standard;

                if (contract.ContractData == null)
                    contract.ContractData = new ContractData();
                else
                    cardType = contract.ContractData.Client.CardType;

                contract.ContractData.Client = _clientService.Get(contract.ClientId);
                contract.ContractData.Client.CardType = cardType;

                if (contract.ContractClass == ContractClass.CreditLine)
                {
                    var firstTranche = _repository.GetFirstTranche(id).Result;

                    if (firstTranche != null)
                    {
                        contract.FirstTranche = new FirstTranche
                        {
                            Id = firstTranche.Id,
                            LoanCost = firstTranche.LoanCost,
                            LoanPeriod = firstTranche.LoanPeriod,
                            SettingId = firstTranche.SettingId,
                        };

                        var policyRequest = _insurancePoliceRequestService.GetLatestPoliceRequest(firstTranche.Id);

                        if (policyRequest != null)
                        {
                            contract.FirstTranche.LoanCost -= policyRequest.RequestData.InsurancePremium;
                        }
                    }
                }
            }

            // Если у текущего договора существует заявка, значит договор был создан из мобильного приложения
            if (contract.ContractClass == ContractClass.Tranche)
            {
                contract.IsFromMobileApp = !(_applicationRepository.FindByContractId(contract.CreditLineId.Value) is null);
            }
            else
            {
                contract.IsFromMobileApp = !(_applicationRepository.FindByContractId(contract.Id) is null);
            }

            var policeRequests = _insurancePoliceRequestService.List(new ListQuery(), new { ContractId = contract.Id }).List;
            var applicationDetails = _applicationService.GetApplicationDetailsByContractId(contract.Id);
            var taskFillSchedule = Task.Run(() => _contractService.CreditLineFillConsolidateSchedule(contract));

            _contractService.SetBuyoutReasonForContractAction(contract);
            contract.isPensioner = contract.Client.IsPensioner;
            _contractService.FillPositionContractNumbers(contract);
            _contractService.FillPositionSubjectsAndHasPledge(contract);

            var payTypeOperationCode = _repository.GetWaitPayTypeOperationCode(contract.Id).Result;
            int authorId = _sessionContext.UserId;
            var user = _userRepository.Get(authorId);

            // наличие аукциона по договру/КЛ
            CarAuction auction = contract.ContractClass switch
            {
                ContractClass.Credit => _auctionRepository.GetByContractId(contract.Id),
                ContractClass.Tranche => _auctionRepository.GetByContractId(contract.CreditLineId.Value),
                ContractClass.CreditLine => _auctionRepository.GetByContractId(contract.Id),
                _ => null
            };

            contract.AuctionId = auction?.AuctionId;

            if (contract.Status < ContractStatus.Signed)
            {
                //  Проверка на АСП в СУСН заемщика
                contract.Client = _clientService.SetASPStatus(contract.ClientId);

                if (contract.Subjects != null)
                {
                    foreach (var subject in contract.Subjects)
                    {
                        // Проверка на АСП в СУСН созаемщиков
                        subject.Client = _clientService.SetASPStatus(subject.ClientId);
                    }
                }
            }

            var contractKdnModel = contract.Status < ContractStatus.Signed ? _contractKdnService.FillKdnModel(contract, user) : new ContractKdnModel() { IsKdnRequired = false };

            Task.WaitAll(new[] { taskFillSchedule, _contractService.IsCanEditFirstPaymentDate(contract) });

            TrancheLimit trancheLimit = null;
            if (contract.CreatedInOnline)
            {
                trancheLimit = _applicationOnlineService.GetAddtionalLimitForInsurance(contract.Id);
            }
            else
            {
                trancheLimit = _applicationService.GetLimitByCategory(contract.Id);
            }

            var contractModel = new ContractModel()
            {
                Contract = contract,
                ApplicationDetails = applicationDetails,
                ContractKdnModel = contractKdnModel,
                PoliceRequests = policeRequests,
                PayTypeOperationCode = payTypeOperationCode,
                ApplicationAdditionalLimit = _creditLineService.GetLimitPersentForInsurance(contract.EstimatedCost).Result,
                TrancheLimit = trancheLimit
            };

            if (contract.ContractClass == ContractClass.Tranche || contract.ContractClass == ContractClass.CreditLine)
            {
                var contractSettings = _contractService.GetContractSettings(contract.Id);
                contractModel.IsInsuranceAdditionalLimitOn = contractSettings.IsInsuranceAdditionalLimitOn;
                contractModel.IsLiquidityOn = contractSettings.IsLiquidityOn;
            }

            return contractModel;
        }

        [HttpPost("/api/contract/realtyPrintingForm")]
        public RealtyPrintingFormModel RealtyPrintingForm([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);

            if (contract == null)
                throw new InvalidOperationException();


            int authorId = _sessionContext.UserId;
            var user = _userRepository.Get(authorId);
            var contractModel = new RealtyPrintingFormModel()
            {
                Contract = contract,
                User = user
            };

            return contractModel;
        }

        [HttpPost("/api/contract/getParentContract")]
        public ContractModel GetParentContract([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.GetOnlyContract(id);

            var parentContract = new ContractModel()
            {
                Contract = new Contract { Id = 0 },
                PoliceRequests = new List<InsurancePoliceRequest>()
            };

            if (contract.Locked && contract.ParentId.HasValue)
            {
                parentContract = Card(contract.ParentId.Value);
            }
            else if (contract.ContractClass == ContractClass.Tranche && contract.CreditLineId.HasValue)
            {
                parentContract = Card(contract.CreditLineId.Value);
            }

            return parentContract;
        }

        [HttpPost("/api/contract/getParentContractForApplication")]
        public ContractModel GetParentContractForApplication([FromBody] int id)
        {
            return GetParentContract(id);
        }

        [HttpPost("/api/contract/getContractHistory")]
        public List<ContractHistory> GetContractHistory([FromBody] int id)
        {
            if (id <= 0)
                return null;

            return _repository.GetContractHistory(id);
        }

        [HttpPost("/api/contract/copy"), Authorize(Permissions.ContractManage)]
        public ContractModel Copy([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);

            if (contract == null)
                throw new InvalidOperationException();

            contract.Id = 0;
            contract.Client = _clientService.Get(contract.ClientId);
            contract.ContractNumber = null;
            contract.ContractDate = DateTime.Now;
            contract.MaturityDate = contract.ContractDate.AddDays(contract.LoanPeriod - 1);
            contract.OriginalMaturityDate = contract.ContractDate.AddDays(contract.LoanPeriod - 1);
            contract.DeleteDate = null;
            contract.Status = ContractStatus.Draft;
            contract.ProlongDate = null;
            contract.Locked = false;
            contract.Actions = new List<ContractAction>();

            foreach (var position in contract.Positions)
            {
                position.Id = 0;
                position.ContractId = 0;
                position.Status = ContractPositionStatus.Active;
            }

            if (contract.ContractData != null && contract.ContractData.Client != null)
                contract.ContractData.Client.CardType = CardType.Standard;

            var conrtactModel = new ContractModel()
            {
                Contract = contract,
                PoliceRequests = new List<InsurancePoliceRequest>()
            };

            return conrtactModel;
        }

        [HttpPost("/api/contract/getInsurancePremium")]
        public IActionResult GetInsurancePremium([FromBody] InsurancePremiumModel model)
        {
            //var insuranceData = _insurancePremiumCalculator.GetInsuranceData(model.LoanCost, model.InsuranceCompanyId, model.SettingId);
            var insuranceData = _insurancePremiumCalculator.GetInsuranceDataV2(model.LoanCost, model.InsuranceCompanyId, model.SettingId);

            return Ok(new { insuranceData.InsurancePremium, InsuranceAmount = insuranceData.LoanCost });
        }

        [HttpPost("/api/contract/saveContractMarketing")]
        public async Task SaveContractMarketing([FromBody] SaveContractMarketingModel model)
        {
            var contract = await _repository.GetOnlyContractAsync(model.ContractId) ??
                                    throw new PawnshopApplicationException($"Договор {model.ContractId} не найден");

            contract.AttractionChannelId = model.AttractionChannelId;
            contract.LoanPurposeId = model.LoanPurposeId;
            contract.BusinessLoanPurposeId = model.BusinessLoanPurposeId;
            contract.OkedForIndividualsPurposeId = model.OkedForIndividualsPurposeId;
            contract.TargetPurposeId = model.TargetPurposeId;
            contract.OtherLoanPurpose = model.OtherLoanPurpose;

            await _repository.UpdateContractMarketing(contract);
        }

        [HttpPost("/api/contract/save")]
        [Event(EventCode.ContractSaved, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public async Task<ContractModel> Save([FromBody] ContractModel model)
        {
            if (!_sessionContext.Permissions.Contains(Permissions.ContractManage) && !_sessionContext.Permissions.Contains(Permissions.IsInsuranceRequiredManage))
                throw new PawnshopApplicationException("У вас не достаточно прав");

            var contract = model.Contract;
            var isFLoatingDiscrete = false;
            if (contract == null)
                throw new PawnshopApplicationException("Проверьте правильность ввода данных обязательных полей");

            _clientModelValidateService.ValidateClientModel(contract.Client);

            int authorId = _sessionContext.UserId;

            if (!contract.SettingId.HasValue)
                contract.ContractDate = DateTime.Today;

            if (!(contract.ContractData?.Client?.UserId > 0))
            {
                var user = _userRepository.SearchUserByIdentityNumber(contract.ContractData.Client.IdentityNumber);
                if (user != null)
                    contract.ContractData.Client.UserId = user.Id;
            }

            if (contract.ProductTypeId.HasValue && !contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            bool isBusinessPurpose = _contractService.IsContractBusinessPurpose(contract);
            if (!contract.LoanPurposeId.HasValue)
                throw new PawnshopApplicationException("Не выбрана цель кредита");
            else
            {

                DomainValue otherLoanPurposeTypeDomainValue = _domainService.GetDomainValue(LOAN_PURPOSE_DOMAIN, OTHER_LOAN_PURPOSE_DOMAIN_VALUE);
                if (contract.LoanPurposeId == otherLoanPurposeTypeDomainValue.Id)
                {
                    if (string.IsNullOrWhiteSpace(contract.OtherLoanPurpose))
                        throw new PawnshopApplicationException("Не заполнено поле 'Прочая цель кредита'");

                    else if (!string.IsNullOrWhiteSpace(contract.OtherLoanPurpose))
                        throw new PawnshopApplicationException("Поле 'Прочая цель кредита' не должно быть заполнено, так как цель кредита 'На прочие цели' не выбрана");

                }

                else if ((contract.ContractClass == ContractClass.Tranche || contract.ContractClass == ContractClass.CreditLine)
                    && (isBusinessPurpose && !contract.OkedForIndividualsPurposeId.HasValue || !isBusinessPurpose && contract.OkedForIndividualsPurposeId.HasValue)
                    && (isBusinessPurpose && !contract.TargetPurposeId.HasValue || !isBusinessPurpose && contract.TargetPurposeId.HasValue)
                    )
                    throw new PawnshopApplicationException("Не выбраны ОКЭД и целевое использование");

                if ((contract.ContractClass == ContractClass.Tranche || contract.ContractClass == ContractClass.CreditLine) && isBusinessPurpose && contract.OkedForIndividualsPurposeId.HasValue && contract.TargetPurposeId.HasValue)
                    _contractService.ValidateBusinessLoanPurpose(contract);
            }

            if (contract.ProductTypeId.HasValue && !contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            // могут быть разные причины чс, но тут идет проверка на чс только на создание новых контрактов
            // если клиент находится в чс но по данной категории чс можно будет создавать конракт тогда не скидываем ошибку
            IsClientInBlackList(contract.Client.Id, contract.Id);
            if (contract.Subjects != null)
            {
                foreach (var subject in contract.Subjects)
                {
                    // Проверка на черный список созаемщиков
                    IsClientInBlackList(subject.ClientId, contract.Id);
                }
            }

            await _contractVerificationService.CheckClientsAge(contract);
            await _contractVerificationService.CheckClientCoborrowerAccountAmountLimit(contract);

            if (contract.ContractClass != ContractClass.Tranche)
            {
                CheckRealtyPositionsForEstimation(contract);

                _contractService.CheckNumberOfPositionsForContractCarCollateralType(contract);

                _contractService.CheckPositionsForDuplicates(contract);

                _contractService.CheckFirstPositionCollateralTypeSameAsContractCollateralType(contract);

                await _positionEstimateHistoryService.ValidateDateAndEstimatedCost(contract);
            }

            Contract creditLineContract = null;
            if (contract.ContractClass == ContractClass.Tranche)
            {
                if (!contract.CreditLineId.HasValue || contract.CreditLineId == 0)
                    throw new PawnshopApplicationException("Для транша не указан ID Соглашения о кредитной линии");

                creditLineContract = _repository.GetContractWithSubject(contract.CreditLineId.Value);
                if (creditLineContract == null)
                    throw new PawnshopApplicationException("По данному траншу не найдено Соглашение о кредитной линии");

                var creditLineSettings = await _repository.GetCreditLineSettingsAsync(contract.CreditLineId.Value);
                if (creditLineSettings == null)
                    throw new PawnshopApplicationException("Не найдены настройки Соглашения о кредитной линии");

                if (contract.MaturityDate.AddDays(-creditLineSettings.StopIssueDaysOnMaturity) < DateTime.Now.Date)
                    throw new PawnshopApplicationException($"Нельзя создавать транш менее чем за {creditLineSettings.StopIssueDaysOnMaturity} дней до окончания срока Кредитной линии");

                if (contract.EstimatedCost == 0)
                    throw new PawnshopApplicationException("Не указана оценка");
                if (contract.MaxCreditLineCost == 0)
                    throw new PawnshopApplicationException("Не указана максимальная сумма кредитной линии");
                if (contract.LoanCost == 0)
                    throw new PawnshopApplicationException("Не указана сумма транша");

                var creditLineLimitBalance = 0M;

                if (creditLineContract.Status == ContractStatus.Signed)
                    creditLineLimitBalance = await _contractService.GetCreditLineLimit(contract.CreditLineId.Value);
                // TODO: костыль для возможности сохранения изменений первого транша.
                else if (contract.ContractNumber.Contains("-T001"))
                    creditLineLimitBalance = creditLineContract.LoanCost;

                if (contract.LoanCost >
                    Math.Min(creditLineLimitBalance, (contract.MaxCreditLineCost - creditLineContract.LoanCost + creditLineLimitBalance)))
                    throw new PawnshopApplicationException("Превышена максимальная сумма транша");

                if (creditLineContract.Status == ContractStatus.BoughtOut ||
                    creditLineContract.Status == ContractStatus.SoldOut ||
                    creditLineContract.Status == ContractStatus.Disposed)
                {
                    throw new PawnshopApplicationException("Сохранение невозможно. Кредитная линия выкуплена или реализована");
                }

                if (Math.Abs((creditLineContract.MaturityDate - contract.MaturityDate).Days) < 0)
                    throw new PawnshopApplicationException("Срок действия транша не может быть больше срока действия кредитной линии.");

                var limits = _applicationService.GetLimitByCategory(contract.Id);

                if (!creditLineContract.CreatedInOnline && creditLineContract.ContractData != null && _contractService.GetContractSettings(creditLineContract.Id).IsInsuranceAdditionalLimitOn
                    && contract.LoanCost > limits.MaxTrancheSum + limits.AdditionalLimit)
                    throw new PawnshopApplicationException("Превышен доступный остаток кредитной линии по сумме для выбранной категории аналитики");
            }

            if (contract.Id == 0)
            {
                if (contract.ContractData?.Client != null)
                {
                    ModelState.Clear();
                    TryValidateModel(contract.ContractData.Client);
                    ModelState.Validate();

                    contract.ClientId = contract.ContractData.Client.Id;
                }

                contract.OwnerId = _branchContext.Branch.Id;
                contract.BranchId = _branchContext.Branch.Id;
                contract.AuthorId = _sessionContext.UserId;
            }

            if (contract.ContractRates.Any())
            {
                contract.ContractRates.ForEach(t =>
                {
                    t.CreateDate = DateTime.Now;
                    t.Date = contract.ContractDate;
                    t.AuthorId = _sessionContext.UserId;
                });
            }

            contract.LoanPercentCost = Math.Round(contract.LoanCost * contract.LoanPercent / 100, 4, MidpointRounding.AwayFromZero);

            var insurancePoliceRequest = _insurancePoliceRequestService.GetPoliceRequest(model);

            if (contract.SettingId.HasValue)
            {
                var product = contract.Setting != null ? contract.Setting : _loanPercentRepository.Get(contract.SettingId.Value);
                isFLoatingDiscrete = product.IsFloatingDiscrete;
                if (product.IsProduct)
                {
                    if (product.LoanCostFrom > contract.LoanCost)
                        throw new PawnshopApplicationException($"Сумма договора меньше минимальной допустимой суммы по продукту {product.Name}");
                    if (product.LoanCostTo < contract.LoanCost)
                        throw new PawnshopApplicationException($"Сумма договора больше максимальной допустимой суммы по продукту {product.Name}");
                    if (product.CollateralType != contract.CollateralType)
                        throw new PawnshopApplicationException($"Продукт {product.Name} не соответствует виду залога договора");
                    if (product.ProductTypeId != contract.ProductTypeId)
                        throw new PawnshopApplicationException($"Вид продукта на договоре и в настройках отличаются");

                    _contractVerificationService.ClientAccordanceToProduct(contract);

                    if (product.CategoryId.HasValue && contract.CollateralType != CollateralType.Unsecured)
                    {
                        var category = _categoryRepository.Get(product.CategoryId.Value);
                        var categoryErrors = new List<string>();
                        foreach (var position in contract.Positions.Where(x => x.CategoryId != product.CategoryId))
                        {
                            categoryErrors.Add($"Категория аналитики позиции {position.Position.Name} недопустима в продукте {product.Name}. Поменяйте на {category.Name} или измените продукт");
                        }

                        if (categoryErrors.Count > 0)
                            throw new PawnshopApplicationException(categoryErrors.ToArray());
                    }

                    if (contract.Subjects != null && contract.Subjects.Any())
                    {
                        contract.Subjects.ForEach(x => ModelState.Validate());
                        if (contract.Subjects.Any(x => x.Client == null))
                            throw new PawnshopApplicationException($"Субъектом нужно выбрать определенного клиента");

                        var duplSubjList = contract.Subjects.GroupBy(x => x.Client.IdentityNumber)
                                                            .Where(y => y.Count() > 1)
                                                            .Select(z => new { identityNumber = z.Key, Counter = z.Count() })
                                                            .ToList();

                        if (duplSubjList != null && duplSubjList.Any())
                        {
                            StringBuilder excString = new StringBuilder(string.Empty);
                            foreach (var item in duplSubjList)
                                excString.AppendLine($"Клиент с ИИН:{item.identityNumber} дублируется в субъектах");

                            throw new PawnshopApplicationException(excString.ToString());
                        }

                        if (contract.Client != null)
                        {
                            if (contract.Subjects.Any(x => x.Client.Id == contract.Client.Id))
                                throw new PawnshopApplicationException($"Клиент дублируется в субъектах и в договоре");
                        }
                        else
                            throw new PawnshopApplicationException($"Клиент или Подписант не выбран");
                    }

                    if (product.RequiredSubjects != null && product.RequiredSubjects.Count > 0)
                    {
                        var subErrors = new List<string>();
                        if (contract.Subjects != null && contract.Subjects.Any(s => s == null))
                            subErrors.Add($"Договор содержит пустые субъекты");
                        else
                            foreach (LoanRequiredSubject requiredSubject in product.RequiredSubjects)
                            {
                                if (requiredSubject == null)
                                    subErrors.Add($"Обнаружен пустой вид обязательного субъекта");
                                else
                                {
                                    List<ContractLoanSubject> subjects = contract.Subjects?.Where(x => x.SubjectId == requiredSubject.SubjectId).ToList() ?? new List<ContractLoanSubject>();
                                    HashSet<int> uniqueSubjects = subjects.Select(s => s.ClientId).ToHashSet();
                                    if (uniqueSubjects.Contains(contract.ClientId))
                                        subErrors.Add($"Клиент договора не может быть субъектом вида {requiredSubject.Subject.Name}");
                                    else
                                    {
                                        if (uniqueSubjects.Count < subjects.Count)
                                            subErrors.Add($"Субъекты вида {requiredSubject.Subject.Name} должны быть уникальны по идентификатору клиента");
                                        else
                                        {
                                            if (subjects.Count < requiredSubject.Min)
                                                subErrors.Add($"Субъектов вида {requiredSubject.Subject.Name} должно быть не менее {requiredSubject.Min}");
                                            else if (subjects.Count > requiredSubject.Max)
                                                subErrors.Add($"Субъектов вида {requiredSubject.Subject.Name} должно быть не более {requiredSubject.Max}");
                                        }
                                    }

                                    try
                                    {
                                        _contractVerificationService.CheckGuarantorSubjects(subjects, contract.ProductType.Code);
                                        _contractVerificationService.CheckCoborrowerSubjects(subjects, contract.ProductType.Code);
                                    }
                                    catch (PawnshopApplicationException e)
                                    {
                                        subErrors.Add(e.Message);
                                    }
                                }
                            }

                        if (subErrors.Count > 0)
                            throw new PawnshopApplicationException(subErrors.ToArray());
                    }

                    if (product.InitialFeeRequired.HasValue && product.InitialFeeRequired > 0)
                    {
                        var feeErrors = new List<string>();
                        if ((contract.MinimalInitialFee < (contract.EstimatedCost * (product.InitialFeeRequired / 100))) || !contract.MinimalInitialFee.HasValue)
                            feeErrors.Add("Значение минимального первоначального взноса по договору не проходит по условиям продукта");
                        if (contract.RequiredInitialFee < contract.MinimalInitialFee)
                            feeErrors.Add("Вносимый клиентом первоначальный взнос не может быть меньше требуемого по продукту");

                        var insurancePremium = insurancePoliceRequest != null && insurancePoliceRequest.IsInsuranceRequired ? insurancePoliceRequest.RequestData.InsurancePremium : 0;
                        if (contract.EstimatedCost - contract.RequiredInitialFee != contract.LoanCost - insurancePremium)
                            feeErrors.Add("Ошибка суммы займа договора, она должна быть равна сумме оценки за вычетом первоначального взноса");

                        contract.Positions?.ForEach(position =>
                        {
                            if (position.MinimalInitialFee < ((decimal)position.EstimatedCost * (product.InitialFeeRequired / 100)))
                                feeErrors.Add($"Значение минимального первоначального взноса по позиции {position.Position.Name} не проходит по условиям продукта");
                            if (position.RequiredInitialFee < contract.MinimalInitialFee)
                                feeErrors.Add($"Вносимый клиентом первоначальный взнос по позиции {position.Position.Name} не может быть меньше требуемого по продукту");
                            if (position.EstimatedCost - position.RequiredInitialFee != position.LoanCost)
                                feeErrors.Add($"Ошибка суммы займа по позиции {position.Position.Name}, она должна быть равно сумме оценки за вычетом первоначального взноса");
                        });
                        if (feeErrors.Count > 0)
                            throw new PawnshopApplicationException(feeErrors.ToArray());
                    }

                    if (contract.ProductType.Code == Constants.PRODUCT_DAMU)
                    {
                        var clientSigner = _clientSignerService.CheckClientSigner(contract.Client, contract.ContractDate, contract.SignerId);

                        if (contract.SignerId == 0)
                            contract.SignerId = null;

                        if (clientSigner != null)
                        {
                            var filled = _clientQuestionnaireService.IsClientHasFilledQuestionnaire(clientSigner.SignerId);

                            if (!filled)
                                throw new PawnshopApplicationException($"У клиента {clientSigner.Signer.FullName} не заполнена анкета");
                        }

                        _contractVerificationService.CheckDAMUContract(contract);
                    }

                    if (contract.ProductType.Code == Constants.PRODUCT_GRANT)
                    {
                        var clientSigner = _clientSignerService.CheckClientSigner(contract.Client, contract.ContractDate, contract.SignerId);

                        if (contract.SignerId == 0)
                            contract.SignerId = null;

                        if (clientSigner != null)
                        {
                            var filled = _clientQuestionnaireService.IsClientHasFilledQuestionnaire(clientSigner.SignerId);

                            if (!filled)
                                throw new PawnshopApplicationException($"У клиента {clientSigner.Signer.FullName} не заполнена анкета");
                        }
                    }
                }
            }

            bool needChangeForPensioner = false;

            if (model.Contract.Client.IsPensioner)
            {
                if (model.Contract.SettingId.HasValue && model.Contract.Setting != null)
                {
                    if (model.Contract.Setting.IsInsuranceAvailable)
                        needChangeForPensioner = true;
                }
                else if (model.PoliceRequests.Any())
                {
                    needChangeForPensioner = true;
                }
            }

            if (needChangeForPensioner)
                model = _contractService.ChangeForPensioner(model);

            await _paymentScheduleService.CheckPayDayFromContract(contract);

            var updateCreditLineMaturityDate = false;

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var planedFirstPaymentDate = _paymentScheduleService.GetNextPaymentDateByCreditLineId(contract.CreditLineId.Value);
                var firstPaymentDate = planedFirstPaymentDate ?? contract.FirstPaymentDate ?? contract.ContractDate.AddMonths(1);
                var paymentsMonthCount = contract.LoanPeriod / 30;
                contract.MaturityDate = firstPaymentDate.AddMonths(paymentsMonthCount - 1);


                if (creditLineContract.Status != ContractStatus.Signed && contract.ContractNumber.Contains("-T001"))
                {
                    updateCreditLineMaturityDate = true;
                    var creditLinePaymentsMonthCount = creditLineContract.LoanPeriod / 30;
                    var defaultCreditLineMaturityDate = creditLineContract.ContractDate.AddMonths(creditLinePaymentsMonthCount);

                    if (defaultCreditLineMaturityDate < contract.MaturityDate)
                        creditLineContract.MaturityDate = contract.MaturityDate;
                    else
                        creditLineContract.MaturityDate = defaultCreditLineMaturityDate;
                }
                else if (creditLineContract.Status == ContractStatus.Signed && contract.MaturityDate > creditLineContract.MaturityDate)
                {
                    contract.MaturityDate = creditLineContract.MaturityDate;
                }
            }
            else if (contract.ContractClass == ContractClass.Credit)
            {
                var firstPaymentDate = contract.FirstPaymentDate ?? contract.ContractDate.AddMonths(1);
                var paymentsMonthCount = contract.LoanPeriod / 30;
                contract.MaturityDate = firstPaymentDate.AddMonths(paymentsMonthCount - 1);
            }

            if (!isFLoatingDiscrete)
            {
                if (contract.ContractClass != ContractClass.CreditLine)
                {
                    if (contract.ContractClass == ContractClass.Tranche)
                    {
                        //contract.MaturityDate = creditLineContract.MaturityDate;
                        //contract.LoanPeriod = (int)(contract.MaturityDate - contract.ContractDate).TotalDays;

                        creditLineContract.Subjects = _repository.GetContractLoanSubjects(creditLineContract.Id);

                        var newSubjects = creditLineContract.Subjects?
                            .Where(x => !contract.Subjects.Any(f => f.ClientId == x.ClientId))
                            .Select(x => new ContractLoanSubject
                            {
                                AuthorId = x.AuthorId,
                                ClientId = x.ClientId,
                                CreateDate = DateTime.Now,
                                SubjectId = x.SubjectId,
                                Subject = x.Subject,
                            });

                        contract.Subjects.ForEach(x =>
                        {
                            var subject = creditLineContract.Subjects.FirstOrDefault(s => s.ClientId == x.ClientId);

                            if (subject != null)
                                x.DeleteDate = subject.DeleteDate;
                            else
                                x.DeleteDate = DateTime.Now;
                        });

                        if (newSubjects != null && newSubjects.Any())
                            contract.Subjects.AddRange(newSubjects);
                    }
                    if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
                        contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
                        contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
                        contract.PercentPaymentType == PercentPaymentType.Product)
                    {
                        _paymentScheduleService.BuildWithContract(contract);
                        _contractService.CheckSchedule(contract);
                    }
                    else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                    {
                        _paymentScheduleService.BuildWithContract(contract);
                    }
                    else
                    {
                        contract.AnnuityType = null;
                    }
                }
                _verificationService.CheckClientQuestionnaireFilledStatus(contract.Id, contract);
            }
            else
            {
                contract.AnnuityType = null;
                _paymentScheduleService.BuildWithContract(contract);
            }

            List<Type> types = _typeService.List(new ListQuery { Page = null }).List;
            contract.PeriodTypeId = types.FirstOrDefault(x => x.TypeGroup == TypeGroup.Terms && x.Code.Equals(contract.LoanPeriod > 365 ? "TERMS_LONG" : "TERMS_SHORT")).Id;
            contract.ContractTypeId = (contract.Setting?.ContractTypeId ?? 0) == 0 ? types.FirstOrDefault(x => x.TypeGroup == TypeGroup.Contracts && x.Code.Equals(MapType(contract.CollateralType))).Id : contract.Setting.ContractTypeId;

            if (contract.Id > 0)
                _accountService.CheckAccountPlan(contract);

            ModelState.Clear();
            TryValidateModel(contract);
            ModelState.Validate();

            if (contract.CollateralType == CollateralType.Car ||
                contract.CollateralType == CollateralType.Machinery ||
                contract.CollateralType == CollateralType.Realty)
            {
                foreach (var position in contract.Positions)
                {
                    //TODO: проверка на одну КЛ
                    var exist = _repository.Find(new ContractQueryModel
                    {
                        PositionId = position.PositionId,
                        CollateralType = contract.CollateralType,
                        Status = ContractStatus.Signed
                    });

                    //убрал ограничение на несколько договоров под один залог, если договор (не позиция) является недвижимостью
                    if (contract.CollateralType != CollateralType.Realty)
                        if (exist != null)
                            throw new PawnshopApplicationException(
                                $"По выбранной позиции существует открытый договор # {exist.ContractNumber ?? "NaN"}");

                    var contractPosition = position.Position;
                    var positionSubjects = contractPosition.PositionSubjects;

                    if (contractPosition.CollateralType == CollateralType.Car)
                        position.Position = _carRepository.Get(position.PositionId);
                    else if (contractPosition.CollateralType == CollateralType.Machinery)
                        position.Position = _machineryRepository.Get(position.PositionId);
                    else if (contractPosition.CollateralType == CollateralType.Realty)
                        position.Position = _realtyRepository.Get(position.PositionId);

                    position.Position.PositionSubjects = positionSubjects;

                    if (!(contract.SettingId.HasValue && contract.ProductTypeId.HasValue && (contract.ProductType.Code == Constants.PRODUCT_DAMU || contract.ProductType.Code == Constants.PRODUCT_TMF_REALTY) && contractPosition.CollateralType == CollateralType.Realty))
                    {
                        position.Position.ClientId = contract.ClientId;
                        position.Position.Client = contract.Client;
                    }
                    else
                    {
                        position.Position.ClientId = contractPosition.ClientId;
                        position.Position.Client = contractPosition.Client;
                    }

                    if (position.Position.Client == null)
                        throw new PawnshopApplicationException(
                            "Поле залогодатель на позиции договора является обязательным для заполнения");

                    ModelState.Clear();
                    TryValidateModel(position.Position);
                    ModelState.Validate();
                }

                _contractVerificationService.CheckContractPositions(contract);

                _contractService.CheckPositionLiquidity(contract);
                _contractService.CheckMaxPossibleContractPeriod(contract);
            }

            if (contract.ContractClass != ContractClass.Tranche)
            {
                await _contractVerificationService.CheckPositionSubjects(contract);

                await _contractService.FillCollateralCostForContractPositions(contract);

                await _contractVerificationService.CheckLtvForContractEstimatedCost(contract);

                await _contractVerificationService.CheckPositionEstimateDate(contract);

                await _contractVerificationService.CheckPositionSubjectClients(contract);

                await _contractVerificationService.CheckEIfEstimatedCostPositiveForPositions(contract);
            }

            if (insurancePoliceRequest != null && insurancePoliceRequest.IsInsuranceRequired && contract.CollateralType == CollateralType.Unsecured)
                contract.EstimatedCost = (int)contract.LoanCost;

            contract.CheckEstimatedAndLoanCost();

            double loanCostDouble = Convert.ToDouble(contract.LoanCost);

            await _contractService.CalculateAPR(contract);

            if (contract.Checks != null && contract.Checks.Count > 0)
            {
                contract.Checks.ForEach(check =>
                {
                    if (!(check.Id > 0))
                    {
                        check.AuthorId = _sessionContext.UserId;
                        check.CreateDate = DateTime.Now;
                        if (check.Check.PeriodRequired)
                        {
                            check.BeginDate = contract.ContractDate;
                            check.EndDate = contract.ContractDate.AddYears(check.Check.DefaultPeriodAddedInYears ?? 0);
                        }
                    }
                });
            }

            if (contract.Subjects != null && contract.Subjects.Count > 0)
            {
                contract.Subjects.ForEach(subject =>
                {
                    if (subject.Id == 0)
                    {
                        subject.AuthorId = _sessionContext.UserId;
                        subject.CreateDate = DateTime.Now;
                    }
                });
            }

            if (contract.ContractClass != ContractClass.CreditLine)
            {
                contract.NextPaymentDate = contract.PercentPaymentType == PercentPaymentType.EndPeriod
                ? contract.MaturityDate
                : contract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);
            }

            await _contractService.CheckAndChangeStatusForRealtyContractsOnSave(contract);

            using (var transaction = _repository.BeginTransaction())
            {
                if (contract.Id > 0)
                {
                    _repository.Update(contract);

                    if (insurancePoliceRequest is null)
                        _insurancePoliceRequestService.DeletePoliceRequestsForContract(contract.Id);
                }
                else
                {
                    if (contract.ContractClass == ContractClass.Tranche)
                    {
                        var tranchesCount = await _repository.GetTranchesCount(contract.CreditLineId.Value) + 1;
                        contract.ContractNumber = creditLineContract.ContractNumber + "-T" + tranchesCount.ToString().PadLeft(3, '0');
                    }
                    else
                    {
                        contract.ContractNumber = _counterRepository.Next(
                        contract.ContractDate.Year, _branchContext.Branch.Id,
                        _branchContext.Configuration.ContractSettings.NumberCode);
                    }

                    _repository.Insert(contract);
                }

                if (updateCreditLineMaturityDate && creditLineContract != null && creditLineContract.Status != ContractStatus.Signed)
                    _repository.UpdateMaturityDate(creditLineContract);

                var tranche = await _contractService.CreateFirstTranche(contract, model.Contract.FirstTranche, authorId,
                        _insurancePremiumCalculator,
                        _insurancePoliceRequestService,
                        _contractPaymentScheduleService,
                        _contractKdnService);

                if (insurancePoliceRequest != null)
                {
                    if (insurancePoliceRequest.ContractId == 0)
                        _insurancePoliceRequestService.SetContractIdAndNumber(insurancePoliceRequest, contract);

                    _insurancePoliceRequestService.Save(insurancePoliceRequest);
                }

                foreach (var position in contract.Positions)
                    _carRepository.UpdatePosition(position.Position);

                //заполняем субъектов, привязанных к позиции для каждой позиции
                foreach (var contractPosition in contract.Positions)
                {
                    await _positionSubjectService.MigratePositionSubjectsToHistoryIfNecessary(contractPosition.PositionId);
                    if (contractPosition.Position.PositionSubjects != null)
                    {
                        contractPosition.Position.PositionSubjects = _positionSubjectService.SaveSubjectsForPosition(contractPosition.Position.PositionSubjects, contractPosition.PositionId);
                    }
                }

                if (contract.ContractClass != ContractClass.CreditLine)
                {
                    _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId);
                    //Заполняем модель ContractKdnModel для расчета КДН при сохранений
                    var user = _userRepository.Get(authorId);
                    model.ContractKdnModel = _contractKdnService.FillKdnModel(contract, user);
                }

                // если контракт не подписан и имеет цель кредита на бизнес/инвестиции/пополненине ОС
                // меняем данные при каждом сохранении контракта чтобы получать актуальные данные клиента и созаемщиков
                if (contract.ContractClass != ContractClass.CreditLine && isBusinessPurpose && contract.Status < ContractStatus.Signed)
                    _contractService.SaveContractExpertOpinionData(contract.Id);

                if (contract.ContractClass == ContractClass.Tranche || contract.ContractClass == ContractClass.CreditLine)
                {
                    var contractSettings = _contractService.GetContractSettings(contract.Id);
                    model.IsInsuranceAdditionalLimitOn = contractSettings.IsInsuranceAdditionalLimitOn;
                    model.IsLiquidityOn = contractSettings.IsLiquidityOn;
                    model.TrancheLimit = _applicationService.GetLimitByCategory(contract.Id);
                }

                //сохранение статуса договора в истории
                await _contractStatusHistoryService.SaveToStatusChangeHistory(contract.Id, contract.Status, DateTime.Now, _sessionContext.UserId);

                transaction.Commit();
            }

            var applicationDetails = _applicationService.GetApplicationDetailsByContractId(contract.Id);
            if (applicationDetails != null)
                model.ApplicationDetails = applicationDetails;

            model.PoliceRequests = _insurancePoliceRequestService.List(new ListQuery(), new { ContractId = contract.Id }).List;

            return model;
        }
        private bool CheckPensioner4Insurance(ContractModel model)
        {
            return model.PoliceRequests.Any() && model.PoliceRequests.OrderByDescending(t => t.CreateDate).FirstOrDefault(t => t.Status != InsuranceRequestStatus.Rejected && t.Id != 0).IsInsuranceRequired &&
                   model.Contract.Client.IsPensioner;
        }

        private string MapType(CollateralType collateralType)
        {
            return collateralType switch
            {
                CollateralType.Gold => "CONTRACTS_CRED_GOLD",
                CollateralType.Car => "CONTRACTS_CRED_CARS",
                CollateralType.Goods => "CONTRACTS_CRED_GOODS",
                CollateralType.Machinery => "CONTRACTS_CRED_MACHINERY",
                CollateralType.Unsecured => "CONTRACTS_CRED_BLANK",
            };
        }

        private void CheckRealtyPositionsForEstimation(Contract contract)
        {
            foreach (var pos in contract.Positions)
            {
                if (pos.Position.CollateralType == CollateralType.Realty && (pos.PositionEstimate == null || pos.PositionEstimate.CompanyId == null || pos.PositionEstimate.Number == null || pos.PositionEstimate.Date == null))
                {
                    throw new PawnshopApplicationException("Поля Оценочная компания, Номер оценки и Дата оценки обязательны для заполнения");
                }
            }
        }

        private bool IsClientInBlackList(int clientId, int contractId)
        {
            return _clientBlackListService.CheckClientIsInBlackList(clientId, ContractActionType.Sign, contractId);
        }

        [HttpPost("/api/contract/buildSchedule"), Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractBuildSchedule, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public async Task<IActionResult> BuildSchedule([FromBody] Contract contract)
        {
            await _paymentScheduleService.CheckPayDayFromContract(contract);

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditLineContract = _repository.GetOnlyContract(contract.CreditLineId.Value);
                var planedFirstPaymentDate = _paymentScheduleService.GetNextPaymentDateByCreditLineId(contract.CreditLineId.Value);
                var firstPaymentDate = planedFirstPaymentDate ?? contract.FirstPaymentDate ?? contract.ContractDate.AddMonths(1);
                var paymentsMonthCount = contract.LoanPeriod / 30;
                contract.MaturityDate = firstPaymentDate.AddMonths(paymentsMonthCount - 1);

                if (creditLineContract.Status == ContractStatus.Signed && contract.MaturityDate > creditLineContract.MaturityDate)
                {
                    contract.MaturityDate = creditLineContract.MaturityDate;
                }
            }
            else if (contract.ContractClass == ContractClass.Credit)
            {
                var firstPaymentDate = contract.FirstPaymentDate ?? contract.ContractDate.AddMonths(1);
                var paymentsMonthCount = contract.LoanPeriod / 30;
                contract.MaturityDate = firstPaymentDate.AddMonths(paymentsMonthCount - 1);
            }

            _paymentScheduleService.BuildWithContract(contract);

            return Ok(contract.PaymentSchedule);
        }

        [AllowAnonymous]
        [HttpPost("/api/contract/changeControlDateForSchedule")]
        public async Task<IActionResult> СhangeControlDateForSchedule([FromBody] ContractDutyCheckModel contact)
        {
            var date = contact.Date;
            var contractId = contact.ContractId;
            Contract contract = _contractService.Get(contractId);

            if (contract == null)
                throw new PawnshopApplicationException($"Договор c id = {contractId} не найден");

            if (contract.PaymentSchedule.Any(x => x.ActionId == null && x.ActualDate == null && x.Date < DateTime.Now.Date && x.Canceled == null))
            {
                throw new PawnshopApplicationException(
                    $"Имеется просрочка по договору или кредитной линии с id = {contract.Id}");
            }

            var defermentInformation = _clientDefermentService.GetDefermentInformation(contractId);
            if (defermentInformation != null && (defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen ||
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured))
            {
                throw new PawnshopApplicationException("Реструктуризация по контракту была проведена!");
            }

            contract.FirstPaymentDate = date;
            await _paymentScheduleService.CheckPayDayFromContract(contract);

            foreach (var item in contract.PaymentSchedule)
            {
                if (item.ActionId.HasValue)
                {
                    var act = await _contractActionService.GetAsync(item.ActionId.Value);

                    if (act != null)
                    {
                        if (act.ActionType != ContractActionType.PartialPayment)
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                        else if (act.ActionType == ContractActionType.PartialPayment && Math.Round(act.Cost) == Math.Round(item.DebtCost))
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                    }
                }
            }

            if (contract.PaymentSchedule.Any(x => x.ActionId == null && x.ActionType == null && x.Date == DateTime.Now.Date))
            {
                throw new PawnshopApplicationException($"У договора c id = {contract.Id} есть неоплаченный платеж");
            }

            _paymentScheduleService.BuildForChangeControlDate(contract, date);

            return Ok(contract.PaymentSchedule);
        }

        [HttpPost("/api/contract/saveSchedule"), Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractSaveSchedule, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public async Task<IActionResult> SaveSchedule([FromBody] ContractPaymentScheduleUpdateModel schedule)
        {
            var contract = _repository.Get(schedule.ContractId);
            int authorId = _sessionContext.UserId;

            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.AwaitForConfirmation)
            {
                throw new PawnshopApplicationException("Нельзя изменить график платежей, договор подписан");
            }

            if (contract.FirstPaymentDate != schedule.Schedule.FirstOrDefault().Date)
            {
                contract.FirstPaymentDate = schedule.Schedule.FirstOrDefault().Date;
            }

            contract.PaymentSchedule = schedule.Schedule;
            contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);

            Contract creditLineContract = null;
            var updateCreditLineMaturityDate = false;

            if (contract.SettingId.HasValue && contract.Setting != null && _paymentScheduleService.IsDefaultScheduleType(contract.PercentPaymentType, contract.Setting?.ScheduleType))
            {
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    creditLineContract = _repository.GetOnlyContract(contract.CreditLineId.Value);

                    if (creditLineContract.Status != ContractStatus.Signed && contract.ContractNumber.Contains("-T001"))
                    {
                        updateCreditLineMaturityDate = true;
                        var creditLinePaymentsMonthCount = creditLineContract.LoanPeriod / 30;
                        var defaultCreditLineMaturityDate = creditLineContract.ContractDate.AddMonths(creditLinePaymentsMonthCount);

                        if (defaultCreditLineMaturityDate < contract.MaturityDate)
                            creditLineContract.MaturityDate = contract.MaturityDate;
                        else
                            creditLineContract.MaturityDate = defaultCreditLineMaturityDate;
                    }
                    else if (creditLineContract.Status == ContractStatus.Signed && contract.MaturityDate > creditLineContract.MaturityDate)
                    {
                        contract.MaturityDate = creditLineContract.MaturityDate;
                    }
                }
                else if (contract.ContractClass == ContractClass.Credit)
                {
                    var firstPaymentDate = contract.FirstPaymentDate ?? contract.ContractDate.AddMonths(1);
                    var paymentsMonthCount = contract.LoanPeriod / 30;
                    contract.MaturityDate = firstPaymentDate.AddMonths(paymentsMonthCount - 1);
                }
            }

            _contractService.CheckSchedule(contract);

            contract.NextPaymentDate = contract.PercentPaymentType == PercentPaymentType.EndPeriod
                ? contract.MaturityDate
                : contract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);

            using (var transaction = _repository.BeginTransaction())
            {
                _repository.Update(contract);
                _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId);
                await _contractService.CalculateAPR(contract);
                _repository.Update(contract);

                if (contract.CollateralType != CollateralType.Unsecured)
                {
                    if (contract.SignDate < Constants.NEW_MAX_APR_DATE)
                    {
                        if (contract.APR > Constants.MAX_APR_OLD)
                            throw new PawnshopApplicationException(
                                $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                    }
                    else
                    {
                        if (contract.APR > Constants.MAX_APR_V2)
                            throw new PawnshopApplicationException(
                                $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                    }
                }

                if (updateCreditLineMaturityDate && creditLineContract != null && creditLineContract.Status != ContractStatus.Signed)
                    _repository.UpdateMaturityDate(creditLineContract);

                transaction.Commit();
            }

            return Ok(Card(schedule.ContractId));
        }

        [HttpPost("/api/contract/changeSchedule"), Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractSaveSchedule, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public async Task<IActionResult> ChangeSchedule([FromBody] ContractPaymentScheduleUpdateModel schedule)
        {
            int authorId = _sessionContext.UserId;
            var contract = _repository.Get(schedule.ContractId);
            var contractAdditionalInfo = _contractAdditionalInfoRepository.Get(contract.Id);
            if (contractAdditionalInfo != null && contractAdditionalInfo.ChangedControlDate != null)
            {
                throw new PawnshopApplicationException(
                    $"Контрольная Дата была уже изменена для договора с id = {contract.Id}");
            }
            var fromDateString = contract.FirstPaymentDate?.ToString("dd.MM.yyyy");
            if (contract.PaymentSchedule.Any(x => x.ActionId == null && x.ActualDate == null && x.Date < DateTime.Now.Date && x.Canceled == null))
            {
                throw new PawnshopApplicationException(
                    $"Имеется просрочка по договору или кредитной линии с id = {contract.Id}");
            }

            var incompleteExists = _contractActionService.IncopleteActionExists(contract.Id).Result;
            if (incompleteExists)
            {
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
            }

            _paymentScheduleService.CheckPayDay(schedule.ChangedControlDate.Value.Day);

            if (contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche)
            {
                var list = new List<int>();
                if (contract.ContractClass == ContractClass.CreditLine)
                {
                    list = await _repository.GetTrancheIdsByCreditLine(contract.Id);
                }
                else
                {
                    var creditLine = await _repository.GetCreditLineByTrancheId(contract.Id);
                    list = await _repository.GetTrancheIdsByCreditLine(creditLine);
                    list.Remove(contract.Id);
                }

                var contracts = new List<Contract>();
                foreach (var id in list)
                {
                    var onlyContract = _repository.GetOnlyContract(id);
                    if (onlyContract == null)
                    {
                        continue;
                    }
                    var tranche = _repository.Get(id);
                    contracts.Add(tranche);
                    if (tranche.PaymentSchedule.Any(x => x.ActionId == null && x.ActualDate == null && x.Date < DateTime.Now.Date && x.Canceled == null))
                    {
                        throw new PawnshopApplicationException($"Имеется просрочка по договору или кредитной линии с id = {id}");
                    }
                    var incompletesExists = _contractActionService.IncopleteActionExists(id).Result;
                    if (incompletesExists)
                    {
                        throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
                    }
                }

                foreach (var onlyContract in contracts)
                {
                    var id = onlyContract.Id;
                    var trancheAdditionalInfo = _contractAdditionalInfoRepository.Get(id);
                    if (trancheAdditionalInfo != null && trancheAdditionalInfo.ChangedControlDate != null)
                    {
                        throw new PawnshopApplicationException(
                            $"Контрольная Дата была уже изменена для договора с id = {contract.Id}");
                    }

                    var trancheModel = new ContractPaymentScheduleUpdateModel()
                    {
                        ContractId = id,
                        ChangedControlDate = schedule.ChangedControlDate,
                        FirstPaymentDate = schedule.FirstPaymentDate
                    };
                    using (var transaction = _repository.BeginTransaction())
                    {
                        if (onlyContract.Status != ContractStatus.BoughtOut)
                        {
                            var result = await _contractPaymentScheduleService.SaveBuilderByControlDate(trancheModel);
                            _repository.Update(result);
                            _contractPaymentScheduleService.Save(result.PaymentSchedule, result.Id, authorId, false);
                            await _contractService.CalculateAPR(result);
                            if (result.CollateralType != CollateralType.Unsecured)
                            {
                                if (result.SignDate < Constants.NEW_MAX_APR_DATE)
                                {
                                    if (result.APR > Constants.MAX_APR_OLD)
                                        throw new PawnshopApplicationException(
                                            $"Cтавка ГЭСВ ({result.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                                }
                                else
                                {
                                    if (result.APR > Constants.MAX_APR_V2)
                                        throw new PawnshopApplicationException(
                                            $"Cтавка ГЭСВ ({result.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                                }
                            }
                        }

                        if (trancheAdditionalInfo == null)
                        {
                            trancheAdditionalInfo = new ContractAdditionalInfo()
                            {
                                Id = id,
                                DateOfChangeControlDate = DateTime.Now,
                                ChangedControlDate = schedule.ChangedControlDate
                            };
                            _contractAdditionalInfoRepository.Insert(trancheAdditionalInfo);
                        }
                        else
                        {
                            trancheAdditionalInfo.DateOfChangeControlDate = DateTime.Now;
                            trancheAdditionalInfo.ChangedControlDate = schedule.ChangedControlDate;
                            _contractAdditionalInfoRepository.Update(trancheAdditionalInfo);
                        }
                        transaction.Commit();
                    }
                    _eventLog.Log(EventCode.ContractControlDateChanged, EventStatus.Success, EntityType.Contract, id, userId: authorId, branchId: onlyContract.BranchId);
                }

                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var clContract = _repository.GetOnlyContract(contract.CreditLineId.Value);
                    var actionCreditline = new ContractAction()
                    {
                        ActionType = ContractActionType.ControlDateChange,
                        Date = DateTime.Now,
                        Reason =
                            $"Изменение даты погашения {clContract.ContractNumber} от {clContract.ContractDate.ToString("dd.MM.yyyy")} с {fromDateString} на {schedule.ChangedControlDate?.ToString("dd.MM.yyyy")}",
                        TotalCost = 0,
                        Cost = 0,
                        ContractId = contract.CreditLineId.Value,
                        AuthorId = authorId,
                        CreateDate = DateTime.Now
                    };
                    _contractActionService.Save(actionCreditline);
                }
            }

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                var actionCreditline = new ContractAction()
                {
                    ActionType = ContractActionType.ControlDateChange,
                    Date = DateTime.Now,
                    Reason =
                        $"Изменение даты погашения {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} с {fromDateString} на {schedule.ChangedControlDate?.ToString("dd.MM.yyyy")}",
                    TotalCost = 0,
                    Cost = 0,
                    ContractId = contract.Id,
                    AuthorId = authorId,
                    CreateDate = DateTime.Now
                };
                _contractActionService.Save(actionCreditline);
                return Ok(Card(schedule.ContractId));
            }

            var action = new ContractAction()
            {
                ActionType = ContractActionType.ControlDateChange,
                Date = DateTime.Now,
                Reason =
                    $"Изменение даты погашения {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} с {fromDateString} на {schedule.ChangedControlDate?.ToString("dd.MM.yyyy")}",
                TotalCost = 0,
                Cost = 0,
                ContractId = contract.Id,
                AuthorId = authorId,
                CreateDate = DateTime.Now
            };
            _contractActionService.Save(action);
            var historyId = await _contractPaymentScheduleService.InsertContractPaymentScheduleHistory(contract.Id, action.Id, (int)ContractActionStatus.Canceled);
            foreach (var item in contract.PaymentSchedule)
            {
                if (item.ActionId.HasValue)
                {
                    var act = await _contractActionService.GetAsync(item.ActionId.Value);
                    if (act != null)
                    {
                        if (act.ActionType != ContractActionType.PartialPayment)
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                        else if (act.ActionType == ContractActionType.PartialPayment && Math.Round(act.Cost) == Math.Round(item.DebtCost))
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                    }
                }
                await _contractPaymentScheduleService.InsertContractPaymentScheduleHistoryItems(historyId, item);
            }

            var payments = schedule.Schedule.Where(x => x.ActionType != 40);
            var date = payments.First().Date;
            if (contract.FirstPaymentDate != date)
            {
                contract.FirstPaymentDate = date;
            }

            if (contract.PaymentSchedule.Where(x => contract.PercentPaymentType != PercentPaymentType.EndPeriod && x.Date == DateTime.Now.Date && x.ActionId == null && x.Canceled == null).Any())
            {
                throw new PawnshopApplicationException("Не возможно сделать изменить дату погашения, т.к. по договору имеется не оплаченный платеж с сегодняшней датой оплаты. Вначале сделайте погашение имеющейся задолженности по договору через функционал оплаты(кнопка 'Оплата').");
            }

            contract.PaymentSchedule = schedule.Schedule;
            contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);

            _contractService.CheckSchedule(contract, true);

            contract.NextPaymentDate = contract.PercentPaymentType == PercentPaymentType.EndPeriod
                ? contract.MaturityDate
                : contract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);

            using (var transaction = _repository.BeginTransaction())
            {
                _repository.Update(contract);
                _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId, false);
                await _contractService.CalculateAPR(contract);
                _repository.Update(contract);
                if (contract.CollateralType != CollateralType.Unsecured)
                {
                    if (contract.SignDate < Constants.NEW_MAX_APR_DATE)
                    {
                        if (contract.APR > Constants.MAX_APR_OLD)
                            throw new PawnshopApplicationException(
                                $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                    }
                    else
                    {
                        if (contract.APR > Constants.MAX_APR_V2)
                            throw new PawnshopApplicationException(
                                $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                    }
                }

                if (contractAdditionalInfo == null)
                {
                    contractAdditionalInfo = new ContractAdditionalInfo()
                    {
                        Id = contract.Id,
                        DateOfChangeControlDate = DateTime.Now,
                        ChangedControlDate = schedule.ChangedControlDate
                    };
                    _contractAdditionalInfoRepository.Insert(contractAdditionalInfo);
                }
                else
                {
                    contractAdditionalInfo.DateOfChangeControlDate = DateTime.Now;
                    contractAdditionalInfo.ChangedControlDate = schedule.ChangedControlDate;
                    _contractAdditionalInfoRepository.Update(contractAdditionalInfo);
                }
                transaction.Commit();
            }
            _eventLog.Log(EventCode.ContractControlDateChanged, EventStatus.Success, EntityType.Contract, contract.Id, userId: authorId, branchId: contract.BranchId);
            if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditLineContract = _contractService.GetOnlyContract(contract.CreditLineId.Value);
                _eventLog.Log(EventCode.ContractControlDateChanged, EventStatus.Success, EntityType.Contract, creditLineContract.Id, userId: authorId, branchId: creditLineContract.BranchId);
            }

            return Ok(Card(schedule.ContractId));
        }

        [HttpPost("/api/contract/deleteContractAndReturnToApplication"), Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractDeleted, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult DeleteContractAndReturnToApplication([FromBody] int contractId)
        {
            int applicationId;
            using (var transaction = _repository.BeginTransaction())
            {
                Delete(contractId);
                applicationId = _applicationService.ChangeStatusToNew(contractId);

                transaction.Commit();
            }

            return Ok(applicationId);
        }

        [HttpPost("/api/contract/delete"), Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractDeleted, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);
            if (contract == null)
                throw new InvalidOperationException();
            if (contract.Locked)
                throw new PawnshopApplicationException("Запрещено удалять автоматически порожденные договора.");
            if (!contract.CreatedToday && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException("Удалять можно только договоры за сегодняшний день");
            if (contract.Actions.Count > 0)
                throw new PawnshopApplicationException("Нельзя удалять пока в договоре есть активные действия");
            if (contract.Expenses.Count > 0)
                throw new PawnshopApplicationException("Нельзя удалять пока в договоре есть активные расходы");

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                List<int> trancheIds = _repository.GetTrancheIdsByCreditLine(id).Result;
                foreach (int trancheId in trancheIds)
                {
                    var tranche = _repository.Get(trancheId);
                    if (tranche.Actions.Count > 0)
                        throw new PawnshopApplicationException("Нельзя удалять пока в транше есть активные действия");

                    if (tranche.Status == ContractStatus.Draft && tranche.DeleteDate == null)
                        _repository.Delete(tranche.Id);
                }

                if (contract.Status == ContractStatus.Draft)
                    _repository.Delete(contract.Id);
            }
            else if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditLineId = _repository.GetCreditLineByTrancheId(id).Result;
                var creditLine = _repository.Get(creditLineId);

                if (creditLine.Status == ContractStatus.Draft && creditLine.DeleteDate == null)
                    _repository.Delete(creditLine.Id);

                if (contract.Status == ContractStatus.Draft && contract.DeleteDate == null)
                    _repository.Delete(contract.Id);
            }
            else
                _repository.Delete(id);
            return Ok();
        }

        [HttpPost("/api/contract/undoDelete"), Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractRecovery, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult UndoDelete([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);
            if (contract == null)
                throw new InvalidOperationException();
            if (!contract.CreatedToday)
                throw new PawnshopApplicationException("Восстанавливать можно только договоры за сегодняшний день");

            _repository.UndoDelete(id);
            return Ok();
        }

        [HttpPost("/api/contract/getContractDuty"), Authorize(Permissions.ContractView), ProducesResponseType(typeof(ContractDuty), 200)]
        [Event(EventCode.ContractDebtCheck, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult GetContractDuty([FromBody] ContractDutyCheckModel model)
        {
            ModelState.Validate();
            model.BranchId = _branchContext?.Branch?.Id;
            ContractDuty contractDuty = _contractDutyService.GetContractDuty(model);
            return Ok(contractDuty);
        }

        [HttpPost("/api/contract/export")]
        public async Task<IActionResult> Export([FromBody] List<Contract> contracts)
        {
            using (var stream = _excelBuilder.Build(contracts))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");

                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }

        [HttpPost("/api/contract/print")]
        [Event(EventCode.ReportWordDownload, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public async Task<IActionResult> Print([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);
            if (contract == null)
                throw new InvalidOperationException();
            if (contract.Status == ContractStatus.Draft && contract.ClientId > 0)
            {
                if (contract.ContractData == null)
                {
                    contract.ContractData = new ContractData();
                }
                var client = _clientService.Get(contract.ClientId);
                contract.ContractData.Client = client;
            }

            var stream = await _wordBuilder.Build(contract);
            stream.Position = 0;

            var fileName = await _storage.Save(stream, ContainerName.Temp, "contract.docx");
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

            var fileRow = new FileRow
            {
                CreateDate = DateTime.Now,
                ContentType = contentType ?? "application/octet-stream",
                FileName = fileName,
                FilePath = fileName
            };
            return Ok(fileRow);
        }

        [HttpPost("/api/contract/printAnnuity")]
        [Event(EventCode.ReportWordDownload, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public async Task<IActionResult> PrintAnnuity([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);
            if (contract == null)
                throw new InvalidOperationException();
            if (contract.Status == ContractStatus.Draft && contract.ClientId > 0)
            {
                if (contract.ContractData == null)
                {
                    contract.ContractData = new ContractData();
                }
                var client = _clientService.Get(contract.ClientId);
                contract.ContractData.Client = client;
            }

            var stream = await _annuityWordBuilder.Build(contract);
            stream.Position = 0;

            var fileName = await _storage.Save(stream, ContainerName.Temp, "contract.docx");
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

            var fileRow = new FileRow
            {
                CreateDate = DateTime.Now,
                ContentType = contentType ?? "application/octet-stream",
                FileName = fileName,
                FilePath = fileName
            };
            return Ok(fileRow);
        }

        [HttpPost("/api/contract/updateContract"), Authorize(Permissions.Support)]
        [Event(EventCode.ReportWordDownload, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public void UpdateContract([FromBody] ContractCrmModel crmModel)
        {
            _repository.UpdateCrmInfo(crmModel.ContractId, crmModel.CrmId);
        }

        [HttpPost("/api/contract/getDocumentNumber")]
        [Event(EventCode.ContractDocumentPrint, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult GetDocumentNumber([FromBody] ContractTemplateNumberQuery query)
        {
            if (query == null)
                throw new ArgumentOutOfRangeException(nameof(query));
            if (query.TemplateId <= 0)
                throw new ArgumentOutOfRangeException(nameof(query.TemplateId));
            if (query.ContractId <= 0)
                throw new ArgumentOutOfRangeException(nameof(query.ContractId));

            var document = _contractDocumentRepository.Find(query);

            if (document != null)
                return Ok(document.Number);

            document = CreateDocument(query);

            return Ok(document.Number);
        }

        [HttpPost("/api/contract/createNewTemplateNumber")]
        public IActionResult CreateNewDocumentNumber([FromBody] ContractTemplateNumberQuery query)
        {
            if (query == null)
                throw new ArgumentOutOfRangeException(nameof(query));
            if (query.TemplateId <= 0)
                throw new ArgumentOutOfRangeException(nameof(query.TemplateId));
            if (query.ContractId <= 0)
                throw new ArgumentOutOfRangeException(nameof(query.ContractId));

            var document = CreateDocument(query);

            if (query.HasCoBorrower.Value)
            {
                var coBorrowerDocument = CreateDocument(query);
                return Ok(new
                {
                    documentNumber = document.Number,
                    coBorrowerDocumentNumber = coBorrowerDocument.Number

                });
            }

            return Ok(document.Number);
        }

        [HttpPost("/api/contract/openAccounts")]
        public async Task<IActionResult> OpenAccounts([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Contract contract = await Task.Run(() => _repository.Get(id));

            List<Account> accounts = await Task.Run(() => _accountService.OpenForContract(contract));

            return Ok(accounts);
        }

        private ContractDocument CreateDocument(ContractTemplateNumberQuery query)
        {
            if (!_sessionContext.HasPermission(Permissions.ContractManage) && !_sessionContext.HasPermission(Permissions.ContractDocumentGenerateNumber))
                throw new PawnshopApplicationException("Недостаточно прав для создания документов!");

            var template = _printTemplateRepository.Get(query.TemplateId);
            var contract = _repository.Get(query.ContractId);

            var document = new ContractDocument
            {
                ContractId = contract.Id,
                AuthorId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                TemplateId = template.Id
            };

            using var transaction = _contractDocumentRepository.BeginTransaction();

            if (template.HasNumber)
            {
                var config = _printTemplateRepository.GetConfigByTemplate(query.TemplateId);

                document.GenerateNumber(config, contract, _printTemplateRepository.Next(new PrintTemplateCounterFilter
                {
                    BranchId = contract.BranchId,
                    CollateralType = contract.CollateralType,
                    ConfigId = config.Id,
                    OrganizationId = contract.Branch.OrganizationId,
                    ProductTypeId = contract.ProductTypeId,
                    ScheduleType = contract.Setting?.ScheduleType,
                    Year = DateTime.Now.Year
                }));
            }
            else
                document.Number = contract.ContractNumber;

            _contractDocumentRepository.Insert(document);

            transaction.Commit();

            return document;
        }

        [HttpPost, Authorize(Permissions.ParkingHistoryView)]
        public List<ParkingHistory> GetParkingHistory([FromBody] int id)
        {
            return _parkingHistoryRepository.List(new ListQuery { Page = null }, new { ContractId = id });
        }

        [HttpGet("/api/contract/getPreApprovedAmount")]
        public IActionResult GetPreApprovedAmount(int modelId, int releaseYear)
        {
            if (modelId == 0)
                throw new PawnshopApplicationException("Поле модель не заполнено");

            if (releaseYear == 0)
                throw new PawnshopApplicationException("Поле год не заполнено");

            return Ok(_contractService.GetPreApprovedAmount(modelId, releaseYear));
        }

        [HttpPost("/api/contract/CheckRelatedContracts")]
        public IActionResult CheckRelatedContracts([FromBody] int contractId)
        {
            return Ok(_contractService.AreAllRelatedContractsBoughtOut(contractId));
        }

        [HttpGet("/api/contract/GetKDNMessage/{contractId}")]
        public async Task<IActionResult> GetKDNMessage(int contractId)
        {
            if (contractId <= 0)
                throw new PawnshopApplicationException("Поле contractId не заполнено");
            return Ok(await _contractKdnService.GetKDNMessage(contractId));
        }

        [HttpGet("/api/contract/GetScheduleVersions/{ContractId}")]
        public async Task<IActionResult> GetScheduleVersions(int ContractId)
        {
            return Ok(await _contractPaymentScheduleService.GetScheduleVersions(ContractId));
        }

        [HttpGet("/api/contract/GetScheduleByAction/{ActionId}")]
        public async Task<IActionResult> GetScheduleByAction(int ActionId)
        {
            return Ok(await _contractPaymentScheduleService.GetScheduleByAction(ActionId));
        }

        [HttpGet("/api/contract/GetScheduleByHistory/{HistoryId}")]
        public async Task<IActionResult> GetScheduleByHistory(int HistoryId)
        {
            return Ok(await _contractPaymentScheduleService.GetScheduleByHistory(HistoryId));
        }

        [HttpGet("/api/contract/GetPartialPayments/{ContractId}")]
        public async Task<IActionResult> GetPartialPayments(int ContractId)
        {
            var contract = _contractService.GetOnlyContract(ContractId);
            var payments = await _contractActionPartialPaymentService.GetContractPartialPayments(ContractId);
            var contractAdditionalInfo = _contractAdditionalInfoRepository.Get(ContractId);
            var dateOfChangeControlDate = contractAdditionalInfo?.DateOfChangeControlDate;
            var activeDeferment = _clientDefermentService.GetActiveDeferment(ContractId);

            return Ok(new { payments, dateOfChangeControlDate, activeDeferment });
        }

        [HttpGet("/api/contract/GetParentPayments/{ContractId}")]
        public async Task<IActionResult> GetParentPayments(int ContractId)
        {
            return Ok(await _contractActionPartialPaymentService.GetContractParentPayments(ContractId));
        }

        [HttpGet("/api/contract/GetCreditLineLimit/{CreditLineContractId}")]
        public async Task<IActionResult> GetCreditLineLimit(int CreditLineContractId)
        {
            return Ok(await _contractService.GetCreditLineLimit(CreditLineContractId));
        }

        [HttpPost("/api/contract/filtered")]
        public async Task<ListModel<ContractListInfo>> GetFilteredList([FromServices] ContractQueriesRepository queriesRepository,
            [FromBody] ListQueryModel<ContractFilteredQueryModel> filter,
            [FromQuery] bool creditLinesOnly = false)
        {
            if (filter == null)
                filter = new ListQueryModel<ContractFilteredQueryModel>();

            if (filter.Model == null)
                filter.Model = new ContractFilteredQueryModel();

            filter.Model.OwnerIds = filter.Model.ClientId.HasValue
                ? null
                : new int[] { _branchContext.Branch.Id };

            if (filter.Model.EndDate.HasValue)
            {
                filter.Model.EndDate = filter.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<ContractListInfo>
            {
                List = await queriesRepository.GetFilteredList(filter, filter.Model, creditLinesOnly),
                Count = await queriesRepository.GetFilteredListCount(filter, filter.Model)
            };
        }

        [HttpGet("/api/contract/tranches/{creditLineId}")]
        public IList<ContractTrancheInfo> GetTranches(int creditLineId)
        {
            return _contractService.GetTranches(creditLineId);
        }

        [HttpGet("/api/contract/tranches/{creditLineId}/sum/")]
        public async Task<decimal> GetTranchesSum(int creditLineId)
        {
            var tranches = await _contractService.GetAllSignedTranches(creditLineId);

            var tranchesIds = tranches.Where(x => (int)x.Status >= (int)ContractStatus.Signed)
                .Select(x => x.Id)
                .ToList();

            var accountBalances = new List<decimal>();
            var overdueAccountBalances = new List<decimal>();
            List<Task> tasks = new List<Task>();

            foreach (var item in tranchesIds)
            {
                tasks.Add(Task.Run(() =>
                {
                    accountBalances.Add(_contractService.GetAccountBalance(item, DateTime.Now.Date));
                    overdueAccountBalances.Add(_contractService.GetOverdueAccountBalance(item, DateTime.Now.Date));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return accountBalances.Sum() + overdueAccountBalances.Sum();
        }

        [HttpGet("/api/contract/getPositionsByContractId/{contractId}")]
        public async Task<IActionResult> GetPositionsByContractId(int contractId)
        {
            var contract = _contractService.Get(contractId);
            var positions = await _contractService.GetPositionsByContractIdAsync(contractId);
            var updatedPositions = _contractService.FillPositionContractNumbers(contract, positions);
            return Ok(updatedPositions);
        }

        [AllowAnonymous]
        [HttpGet("/api/contract/onlycard/{id}")]
        public async Task<IActionResult> OnlyCard(int id, [FromServices] ApplicationOnlineRefinancesRepository appRefRepository)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);

            if (contract == null)
                throw new InvalidOperationException();

            if (contract.Positions != null)
                _contractService.FillPositionContractNumbers(contract);

            if (!contract.SignDate.HasValue)
                contract.SignDate = DateTime.Now;

            var policeRequests = _insurancePoliceRequestService.List(new ListQuery(), new { ContractId = contract.Id }).List;
            var refinanceContractList = await appRefRepository.GetApplicationOnlineRefinancesByContractId(id);
            var refinanceList = refinanceContractList?.Select(x =>
            {
                var contract = _repository.GetOnlyContract(x.RefinancedContractId);

                if (contract == null)
                    return null;

                return new { ContractId = contract.Id, ContractNumber = contract.ContractNumber, ContractDate = contract.ContractDate };
            });

            return Ok(new { contract, policeRequests, refinanceList });
        }

        [HttpGet("api/contract/consolidated-schedule")]
        public async Task<IActionResult> GetConsolidatedSchedule(int creditLineId)
        {
            if (creditLineId <= 0)
                throw new PawnshopApplicationException("Параметр creditLineId обязателен к заполнению.");

            var creditLine = _repository.GetOnlyContract(creditLineId);

            if (creditLine == null)
                throw new PawnshopApplicationException($"Кредитная линия не найдена {creditLineId}.");

            await _contractService.CreditLineFillConsolidateSchedule(creditLine);

            return Ok(creditLine.PaymentSchedule);
        }

        [HttpGet("api/contract/{id}/payment/amount")]
        public async Task<ActionResult<PaymentAmount>> GetPaymentAmount([FromRoute] int id, [FromServices] AccountRepository accountRepository)
        {
            var contract = _repository.GetOnlyContract(id);

            if (contract == null)
                return NotFound(new PaymentAmount { Amount = -1, Desc = $"Contract {id} not found." });

            if (contract.ContractClass == ContractClass.Tranche)
                return BadRequest(new PaymentAmount { Amount = -1, Desc = $"Contract {id} is tranche." });

            if (contract.Status == ContractStatus.BoughtOut || contract.Status == ContractStatus.SoldOut || contract.Status == ContractStatus.Disposed)
                return Ok(new PaymentAmount { Amount = 0, Desc = $"Contract {id} is closed." });

            if (contract.Status != ContractStatus.Signed)
                return Ok(new PaymentAmount { Amount = 0, Desc = $"Contract {id} is not signed." });

            return await _contractService.GetPaymentAmount(id, contract);
        }

        [HttpGet("/api/contract/{id}/loan-contract-file")]
        public async Task<IActionResult> GetLoanContractFile(
            [FromServices] IPdfService pdfService,
            [FromServices] IFileStorageService fileStorageService,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var contract = await _repository.GetOnlyContractAsync(id);

                if (contract == null || !contract.CreatedInOnline)
                    return NotFound(new LoanContractFileResponse(
                        HttpStatusCode.NotFound,
                        DRPPResponseStatusCode.ContractNotFound.ToString(),
                        DRPPResponseStatusCode.ContractNotFound));

                var applicationOnline = applicationOnlineRepository.GetByContractId(contract.Id);

                if (applicationOnline == null)
                    return NotFound(new LoanContractFileResponse(
                        HttpStatusCode.NotFound,
                        DRPPResponseStatusCode.ContractNotFound.ToString(),
                        DRPPResponseStatusCode.ContractNotFound));

                var contractAdditionalInfo = _contractAdditionalInfoRepository.Get(id);

                if (contractAdditionalInfo == null)
                {
                    return NotFound(new LoanContractFileResponse(HttpStatusCode.NotFound,
                        DRPPResponseStatusCode.FileNotFound.ToString(),
                        DRPPResponseStatusCode.FileNotFound));
                }

                if (!contractAdditionalInfo.LoanStorageFileId.HasValue)
                {
                    var fileBytes = await pdfService.GetFile(contract.Id, contract.CreditLineId, applicationOnline.SignType, cancellationToken);

                    if (fileBytes == null)
                        return NotFound(new LoanContractFileResponse(HttpStatusCode.InternalServerError,
                            DRPPResponseStatusCode.UnspecifiedProblem.ToString(),
                            DRPPResponseStatusCode.UnspecifiedProblem));

                    using (var stream = new MemoryStream(fileBytes))
                    {
                        var loanContractCode = applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByBusinessType(Constants.APPLICATION_ONLINE_FILE_BUSINESS_TYPE_LOAN_CONTRACT);
                        var response = await fileStorageService.Upload(contractAdditionalInfo.StorageListId.ToString(), stream, String.Empty, loanContractCode.BusinessType, loanContractCode.Title, cancellationToken);
                        var storageFileId = Guid.Parse(response.FileGuid);

                        contractAdditionalInfo.LoanStorageFileId = storageFileId;
                        _contractAdditionalInfoRepository.Update(contractAdditionalInfo);
                    }

                    contractAdditionalInfo = _contractAdditionalInfoRepository.Get(id);

                    return Ok(new LoanContractFileResponse(contractAdditionalInfo.LoanStorageFileId));
                }

                return Ok(new LoanContractFileResponse(contractAdditionalInfo.LoanStorageFileId));
            }
            catch (Exception ex)
            {
                return NotFound(new LoanContractFileResponse(HttpStatusCode.InternalServerError,
                    DRPPResponseStatusCode.UnspecifiedProblem.ToString(), // ex.Message,
                    DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }

        [HttpGet("/api/contract/{id}/file")]
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFile(
            [FromRoute] int id,
            [FromServices] FileStorageService service,
            CancellationToken cancellationToken)
        {
            var contract = await _repository.GetOnlyContractAsync(id);

            if (contract == null)
                return NotFound(new LoanContractFileResponse(
                    HttpStatusCode.NotFound,
                    DRPPResponseStatusCode.ContractNotFound.ToString(),
                    DRPPResponseStatusCode.ContractNotFound));

            var contractAdditionalInfo = _contractAdditionalInfoRepository.Get(id);

            if (contractAdditionalInfo == null)
            {
                return NotFound(new LoanContractFileResponse(HttpStatusCode.NotFound,
                    DRPPResponseStatusCode.FileNotFound.ToString(),
                    DRPPResponseStatusCode.FileNotFound));
            }

            try
            {
                var fileStorageFile = await service.Download(contractAdditionalInfo.LoanStorageFileId.Value, cancellationToken);

                if (fileStorageFile.ContentType == null)
                    return File(fileStorageFile.Stream, fileStorageFile.ContentType);

                return File(fileStorageFile.Stream, fileStorageFile.ContentType);
            }
            catch (ServiceUnavailableException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, exception.Message);
            }
            catch (Services.ApplicationOnlineFileStorage.Exceptions.UnexpectedResponseException exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("/api/contracts/readyForMoneySend")]
        [ProducesResponseType(typeof(ContractReadyToMoneySendListView), 200)]
        public async Task<IActionResult> GetContractsReadyForMoneySend(
            [FromQuery] ContractReadyToMoneySendListQuery query,
            [FromQuery] PageBinding pageBinding)
        {
            return Ok(await _repository.GetContractsReadyToMoneySendList(query, pageBinding.Offset, pageBinding.Limit));
        }

        [HttpGet("/api/contracts/{id}/online/buyout/available")]
        public async Task<ActionResult<ContractOnlineBuyoutAvailableView>> ContractOnlineBuyoutАvailable(
            [FromServices] ContractRepository contractRepository,
            [FromServices] CreditLineService creditLineService,
            [FromServices] IContractActionOnlineExecutionCheckerService checkerService,
            [FromRoute] int id)
        {
            var contract = await contractRepository.GetOnlyContractAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                return BadRequest();
            }

            var check = await checkerService.Check(id);
            if (check.Failed)
            {
                return StatusCode((int)check.StatusCode, new ContractOnlineBuyoutAvailableView
                {
                    CanBeProcessed = false,
                    Description = check.Message,
                    ReasonCode = check.ErrorType.ToString(),
                    TechnicalIssues = check.TechnicalIssues
                });
            }

            if (contract.ContractClass == ContractClass.Credit)
            {
                var balance = contractRepository
                    .GetBalances(new List<int> { contract.Id })
                    .FirstOrDefault();

                if (balance == null)
                    throw new PawnshopApplicationException();

                return Ok(new ContractOnlineBuyoutAvailableView
                {
                    AdvanceAccountBalance = balance.PrepaymentBalance,
                    AutoClose = true,
                    BuyoutAmount = balance.TotalRedemptionAmount,
                    CanBeProcessed = balance.CurrentDebt <= balance.PrepaymentBalance,
                    Description = "",
                    MainDebt = balance.TotalAcountAmount,
                    Percent = balance.TotalProfitAmount,
                    HowAdditionalManyMoneyNeedForBuyOut = balance.PrepaymentBalance - balance.TotalRedemptionAmount >= 0 ? 0
                        : balance.TotalRedemptionAmount - balance.PrepaymentBalance,
                    AutoBuyOutCommingSoon = balance.PrepaymentBalance >= balance.TotalRedemptionAmount,
                    CurrentDebt = balance.CurrentDebt
                });
            }

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditlineBalance = await
                    creditLineService.GetCurrentlyDebtForCreditLine(contract.CreditLineId.Value);

                var selectedtranche =
                    creditlineBalance.ContractsBalances.FirstOrDefault(contract => contract.ContractId == id);

                return Ok(new ContractOnlineBuyoutAvailableView
                {
                    AdvanceAccountBalance = creditlineBalance.SummaryPrepaymentBalance,
                    AutoClose = creditlineBalance.ContractsBalances.Count == 1,
                    BuyoutAmount = selectedtranche.TotalRedemptionAmount,
                    CanBeProcessed = creditlineBalance.SummaryCurrentDebt == 0,
                    Description = "",
                    MainDebt = selectedtranche.TotalAcountAmount,
                    Percent = selectedtranche.TotalProfitAmount,
                    HowAdditionalManyMoneyNeedForBuyOut = creditlineBalance.SummaryPrepaymentBalance - selectedtranche.TotalRedemptionAmount >= 0 ? 0
                        : selectedtranche.TotalRedemptionAmount - creditlineBalance.SummaryPrepaymentBalance,
                    AutoBuyOutCommingSoon = creditlineBalance.SummaryPrepaymentBalance >= selectedtranche.TotalRedemptionAmount,
                    CurrentDebt = creditlineBalance.SummaryCurrentDebt
                });
            }

            return StatusCode(500);
        }

        [HttpPost("/api/contracts/{id}/online/buyout")]
        public async Task<IActionResult> BuyoutContract(
            [FromServices] ContractRepository contractRepository,
            [FromServices] ICreditLinesBuyoutService creditlineBuyoutService,
            [FromServices] IContractActionOnlineExecutionCheckerService checkerService,
            [FromRoute] int id)
        {
            var contract = await contractRepository.GetOnlyContractAsync(id);
            if (contract == null)
                return NotFound();

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var check = await checkerService.Check(contract.Id);
                if (check.Failed)
                {
                    return StatusCode((int)check.StatusCode,
                        new BaseResponseDRPP(check.StatusCode, check.Message,
                            DRPPResponseStatusCode.UnspecifiedProblem));
                }

                await creditlineBuyoutService.TransferPrepaymentAndBuyOutOnline(contract.CreditLineId.Value,
                    _sessionContext.UserId,
                    contract.BranchId,
                    new List<int> { contract.Id });

                return Ok();
            }

            return BadRequest();
        }

        [HttpGet("/api/contracts/{id}/online/partialpayment/available")]
        public async Task<ActionResult<OnlinePartialPaymentAvailableView>> OnlinePartialPaymentCanBeProcessed(
            [FromRoute] int id,
            [FromServices] IContractService contractService,
            [FromServices] IContractActionOnlineExecutionCheckerService checkerService,
            [FromServices] AccountRepository accountRepository)
        {
            try
            {
                var check = await checkerService.Check(id);

                if (check.Failed)
                {
                    return StatusCode((int)check.StatusCode, new OnlinePartialPaymentAvailableView(false, check.Message, check.TechnicalIssues));
                }

                var contract = contractService.GetOnlyContract(id);

                var balance = await accountRepository.GetBalanceByContractIdAsync(id);

                return Ok(new OnlinePartialPaymentAvailableView(true, "The contract can be processed.", balance.TotalRedemptionAmount - 1000, balance.TotalProfitAmount + 1000));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new OnlinePartialPaymentAvailableView(false, ex.Message, true));
            }
        }

        [HttpGet("/api/contracts/{id}/online/partialpayment/balance")]
        public async Task<ActionResult<OnlinePartialPaymentBalanceView>> GetOnlinePartialPaymentBalance(
            [FromRoute] int id,
            [FromQuery] decimal amount,
            [FromServices] IContractService contractService,
            [FromServices] ICreditLinePartialPaymentService creditLinePartialPaymentService,
            [FromServices] AccountRepository accountRepository)
        {
            try
            {
                var contract = contractService.GetOnlyContract(id, true);

                if (contract == null)
                {
                    return NotFound(new OnlinePartialPaymentBalanceView(HttpStatusCode.NotFound, $"The contract {id} not found!"));
                }

                if (contract.ContractClass == ContractClass.CreditLine)
                {
                    return StatusCode((int)HttpStatusCode.UnprocessableEntity, new OnlinePartialPaymentBalanceView(HttpStatusCode.UnprocessableEntity, $"The contract {id} is credit line, not supported!"));
                }

                var balance = await accountRepository.GetBalanceByContractIdAsync(id);

                if (amount > balance.TotalRedemptionAmount - 1000 || amount < balance.TotalProfitAmount + 1000)
                {
                    return BadRequest(new OnlinePartialPaymentBalanceView(HttpStatusCode.BadRequest, "The amount specified is incorrect!"));
                }

                if (contract.ContractClass == ContractClass.Credit)
                {
                    var creditBalance = await accountRepository.GetBalanceByContractIdAsync(id);

                    var debtAmount = amount - creditBalance.TotalProfitAmount;
                    var debtAfterPayment = creditBalance.AccountAmount - debtAmount;
                    var paymentAmountFromClient = amount - creditBalance.PrepaymentBalance;

                    var response = new OnlinePartialPaymentBalanceView(HttpStatusCode.OK, null, creditBalance.PrepaymentBalance, debtAfterPayment, creditBalance.TotalProfitAmount, debtAmount, paymentAmountFromClient);

                    return Ok(response);
                }
                else
                {
                    var creditLineBalance = await creditLinePartialPaymentService.GetCreditLineAccountBalancesDistribution(contract.CreditLineId.Value, contract.Id, amount);
                    var payedDebt = creditLineBalance.CreditLineTransfers?.FirstOrDefault(x => x.ContractId == id)?.RefillableAccounts.FirstOrDefault(x => x.Name == "Основной долг")?.Amount ?? 0;

                    var response = new OnlinePartialPaymentBalanceView(HttpStatusCode.OK, null, creditLineBalance.SummaryPrepaymentBalance, creditLineBalance.DebtAfterPayment, creditLineBalance.CurrentProfit, payedDebt, creditLineBalance.PaymentAmountFromClient);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new OnlinePartialPaymentBalanceView(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost("/api/contracts/{id}/online/partialpayment")]
        public async Task<ActionResult<OnlinePartialPaymentView>> CreateOnlinePartialPayment(
            [FromRoute] int id,
            [FromQuery] decimal amount,
            [FromServices] ICashOrderService cashOrderService,
            [FromServices] ICollectionService collectionService,
            [FromServices] IContractActionService contractActionService,
            [FromServices] IContractPaymentService contractPaymentService,
            [FromServices] IContractService contractService,
            [FromServices] ICreditLinePartialPaymentService creditLinePartialPaymentService,
            [FromServices] ICreditLineService creditLineService,
            [FromServices] ICrmPaymentService crmPaymentService,
            [FromServices] ILegalCollectionCloseService closeLegalCaseService,
            [FromServices] IMediator mediator,
            [FromServices] IContractActionOnlineExecutionCheckerService checkerService,
            [FromServices] AccountRepository accountRepository,
            [FromServices] GroupRepository groupRepository,
            [FromServices] PayTypeRepository payTypeRepository)
        {
            try
            {
                var check = await checkerService.Check(id);

                if (check.Failed)
                {
                    return StatusCode((int)check.StatusCode, new OnlinePartialPaymentView(check.StatusCode, check.Message));
                }

                var contract = contractService.GetOnlyContract(id, true);

                if (contract == null)
                {
                    return NotFound(new OnlinePartialPaymentBalanceView(HttpStatusCode.NotFound, $"The contract {id} not found!"));
                }

                if (contract.ContractClass == ContractClass.CreditLine)
                {
                    return StatusCode((int)HttpStatusCode.UnprocessableEntity, new OnlinePartialPaymentBalanceView(HttpStatusCode.UnprocessableEntity, $"The contract {id} is credit line, not supported!"));
                }

                var balance = await accountRepository.GetBalanceByContractIdAsync(id);

                if (amount > balance.TotalRedemptionAmount - 1000 || amount < balance.TotalProfitAmount + 1000)
                {
                    return BadRequest(new OnlinePartialPaymentBalanceView(HttpStatusCode.BadRequest, "The amount specified is incorrect!"));
                }

                var actionId = 0;
                var payType = payTypeRepository.Find(new { Code = "ONLINE" });

                using (var transaction = accountRepository.BeginTransaction())
                {
                    if (contract.ContractClass == ContractClass.Tranche)
                    {
                        var creditLineBalance = await creditLineService.GetCurrentlyDebtForCreditLine(contract.CreditLineId.Value, new List<int> { contract.Id });

                        if (creditLineBalance.SummaryPrepaymentBalance < amount)
                        {
                            return StatusCode((int)HttpStatusCode.UnprocessableEntity, new OnlinePartialPaymentView(HttpStatusCode.UnprocessableEntity, $"The amount of the depo account [{creditLineBalance.SummaryPrepaymentBalance}] is less than the amount of the partial payment [{amount}]!"));
                        }

                        var result = await creditLinePartialPaymentService.PartialPaymentAndTransferByOnline(
                            contract.CreditLineId.Value,
                            _sessionContext.UserId,
                            contract.Id,
                            payType.Id,
                            contract.BranchId,
                            amount);

                        actionId = result.Value;
                    }
                    else
                    {
                        var creditBalance = await accountRepository.GetBalanceByContractIdAsync(contract.Id);

                        if (creditBalance.PrepaymentBalance < amount)
                        {
                            return StatusCode((int)HttpStatusCode.UnprocessableEntity, new OnlinePartialPaymentView(HttpStatusCode.UnprocessableEntity, $"The amount of the depo account [{creditBalance.PrepaymentBalance}] is less than the amount of the partial payment [{amount}]!"));
                        }

                        var debtAmount = amount - creditBalance.TotalProfitAmount;

                        var result = await creditLinePartialPaymentService.PartialPayment(
                            contract.Id,
                            payType.Id,
                            _sessionContext.UserId,
                            contract.BranchId,
                            debtAmount,
                            amount,
                            false);

                        actionId = result.Id;
                    }

                    var action = await contractActionService.GetAsync(actionId);
                    var orders = await cashOrderService.CheckOrdersForConfirmation(actionId);
                    var relatedActions = orders.Item2;
                    var branch = groupRepository.Get(contract.BranchId);
                    var contractIdsForCloseLegalCase = new List<int>();

                    await cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved, _sessionContext.UserId, branch, _sessionContext.ForSupport);

                    foreach (var contractActionId in relatedActions.OrderBy(x => x))
                    {
                        var relatedAction = _contractActionService.GetAsync(contractActionId).Result;
                        if (relatedAction != null && relatedAction.Status.HasValue && relatedAction.Status != ContractActionStatus.Await)
                            continue;

                        relatedAction.Status = ContractActionStatus.Approved;
                        _contractActionService.Save(relatedAction);

                        switch (relatedAction.ActionType)
                        {
                            case ContractActionType.Payment:
                                {
                                    contractPaymentService.ExecuteOnApprove(relatedAction, contract.BranchId, _sessionContext.UserId, forceExpensePrepaymentReturn: false);

                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId }).Wait();
                                    else
                                        mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId }).Wait();

                                    break;
                                }
                            case ContractActionType.PartialPayment:
                                {
                                    var contractForAction = _contractService.Get(relatedAction.ContractId);
                                    _contractPaymentScheduleService.Save(contractForAction.PaymentSchedule, contractForAction.Id, _sessionContext.UserId);

                                    await _contractPaymentScheduleService.UpdateContractPaymentScheduleHistoryStatus(relatedAction.ContractId, relatedAction.Id, 20);
                                    await _contractPaymentScheduleService.UpdateActionIdForPartialPayment(relatedAction.Id, relatedAction.Date, relatedAction.ContractId);
                                    var penaltyCost = action.Rows.Where(x => (x.PaymentType == AmountType.DebtPenalty || x.PaymentType == AmountType.LoanPenalty) && x.DebitAccountId != null).Sum(r => r.Cost);
                                    await _contractPaymentScheduleService.UpdateActionIdForPartialPaymentUnpaid(relatedAction.Id, relatedAction.Date, relatedAction.ContractId, penaltyCost, isEndPeriod: contractForAction.PercentPaymentType == PercentPaymentType.EndPeriod);

                                    var nextPaymentDate = await _contractPaymentScheduleService.GetNextPaymentSchedule(contractForAction.Id);
                                    if (nextPaymentDate != null)
                                    {
                                        contractForAction.NextPaymentDate = nextPaymentDate.Date;
                                    }
                                    if (contractForAction.PercentPaymentType == PercentPaymentType.EndPeriod)
                                    {
                                        var unpaid = await _contractPaymentScheduleService.GetUnpaidSchedule(contractForAction.Id);
                                        if (unpaid != null)
                                        {
                                            contractForAction.MaturityDate = unpaid.Date;
                                            contractForAction.NextPaymentDate = unpaid.Date;
                                        }
                                    }

                                    contractService.Save(contract);

                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        await mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId });
                                    else
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });

                                    crmPaymentService.Enqueue(contract);
                                    break;
                                }
                        }
                    }

                    transaction.Commit();

                    if (contractIdsForCloseLegalCase.Any())
                    {
                        foreach (var contractId in contractIdsForCloseLegalCase)
                        {
                            await closeLegalCaseService.CloseAsync(contractId);
                        }
                    }
                }

                return Ok(new OnlinePartialPaymentView(HttpStatusCode.OK, $"ContractActionId = {actionId}"));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new OnlinePartialPaymentView(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet("/api/contracts/readyForMoneySend/cash")]
        [ProducesResponseType(typeof(ContractReadyToMoneySendListView), 200)]
        public async Task<IActionResult> GetContractsReadyForCashMoneySend(
            [FromQuery] ContractReadyToMoneySendListQuery query,
            [FromQuery] PageBinding pageBinding)
        {
            return Ok(await _repository.GetContractsReadyToMoneySendList(query, pageBinding.Offset, pageBinding.Limit, true, _branchContext.Branch.Id));
        }

        [HttpPost("/api/contracts/{id}/registration-encumbrance")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(BaseResponse), 403)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        [ProducesResponseType(typeof(BaseResponse), 422)]
        public async Task<IActionResult> RegistrationOfEncumbrance(
            [FromRoute] int id,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ITasOnlinePermissionValidatorService permissionValidator)
        {
            if (!permissionValidator.CanEditEncumbranceRegisteredState())
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            var application = applicationOnlineRepository.GetByContractId(id);

            if (application == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка по договору {id} не найдена."));
            }

            if (application.Status != ApplicationOnlineStatus.ContractConcluded.ToString())
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity,
                    new BaseResponse(HttpStatusCode.UnprocessableEntity, $"Заявка по договору {id} должна быть в статусе \"{ApplicationOnlineStatus.ContractConcluded.GetDisplayName()}\"!"));
            }

            application.EncumbranceRegistered = true;
            application.LastChangeAuthorId = _sessionContext.UserId;
            application.UpdateDate = DateTime.Now;

            await applicationOnlineRepository.ChangeEncumbranceRegisteredState(application);

            return NoContent();
        }

        [ProducesResponseType(typeof(ContractBalanceOnlineView), 200)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 404)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 422)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 500)]
        [HttpGet("api/contracts/{id}/onlineBalance")]
        public async Task<IActionResult> GetBalanceForContractsOnline([FromRoute] int id, [FromServices] IContractBalancesService service)
        {
            var contract = await _repository.GetOnlyContractAsync(id);
            if (contract == null)
                return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"Contract with {id} not found",
                    DRPPResponseStatusCode.ContractNotFound));
            if (contract.IsOffBalance)
                return UnprocessableEntity(new BaseResponseDRPP(HttpStatusCode.UnprocessableEntity, $"Contract with {id} is off balance",
                    DRPPResponseStatusCode.ContractIsOffBalance));

            try
            {
                return Ok(await service.GetContractBalanceOnline(contract));
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }
    }
}
