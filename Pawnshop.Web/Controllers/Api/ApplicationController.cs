using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Web.Engine;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.CreditLines;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractFromApplicationView)]
    public class ApplicationController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly ApplicationRepository _applicationRepository;
        private readonly ContractRepository _contractRepository;
        private readonly ContractService _contractService;
        private readonly ClientRepository _clientRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ContractCheckRepository _contractCheckRepository;
        private readonly ApplicationMerchantRepository _applicationMerchantRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        private readonly UserRepository _userRepository;
        private readonly ApplicationDetailsRepository _applicationDetailsRepository;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IContractKdnService _contractKdnService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractActionService _contractActionService;
        private readonly IFcb4Kdn _fcb;
        private readonly ICreditLineService _creditLineService;
        private readonly AttractionChannelRepository _attractionChannelRepository;

        public ApplicationController(
            IApplicationService applicationService,
            ISessionContext sessionContext,
            BranchContext branchContext,
            ApplicationRepository applicationRepository,
            ContractRepository contractRepository,
            ContractService contractService,
            ClientRepository clientRepository,
            LoanPercentRepository loanPercentRepository,
            ContractCheckRepository contractCheckRepository,
            ApplicationMerchantRepository applicationMerchantRepository,
            LoanSubjectRepository loanSubjectRepository,
            ClientLegalFormRepository clientLegalFormRepository,
            UserRepository userRepository,
            ApplicationDetailsRepository applicationDetailsRepository,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IContractKdnService contractKdnService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractActionService contractActionService,
            IFcb4Kdn fcb,
            ICreditLineService creditLineService,
            AttractionChannelRepository attractionChannelRepository
        )
        {
            _applicationService = applicationService;
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _applicationRepository = applicationRepository;
            _contractRepository = contractRepository;
            _contractService = contractService;
            _clientRepository = clientRepository;
            _loanPercentRepository = loanPercentRepository;
            _contractCheckRepository = contractCheckRepository;
            _applicationMerchantRepository = applicationMerchantRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
            _userRepository = userRepository;
            _applicationDetailsRepository = applicationDetailsRepository;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _contractKdnService = contractKdnService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractActionService = contractActionService;
            _fcb = fcb;
            _creditLineService = creditLineService;
            _attractionChannelRepository = attractionChannelRepository;
        }


        [HttpPost]
        public ListModel<Application> List([FromBody] ListQueryModel<ApplicationListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<ApplicationListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new ApplicationListQueryModel();

            if (_sessionContext.HasPermission(Permissions.ContractFromApplicationAllView))
                listQuery.Model.ShowAllApplications = true;

            return _applicationService.ListWithCount(listQuery);
        }

        [HttpPost, Authorize(Permissions.ContractManage), ProducesResponseType(typeof(ApplicationModel), 200)]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            return Ok(_applicationService.Get(id));
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractSaved, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public async Task<IActionResult> CreateContract([FromBody] ContractModel model)
        {
            return Ok(await CreateContractFromApplication(model, _branchContext.Branch.Id, _branchContext.Configuration.ContractSettings.NumberCode, _sessionContext.UserId));
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractSaved, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public IActionResult IsCreateAdditionWithInsurance([FromBody] AdditionDetails model)
        {
            return Ok(_applicationService.IsCreateAdditionWithInsurance(model, _branchContext.Branch.Id, _sessionContext.UserId));
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        public void CancelApplication([FromBody] int id)
        {
            _applicationService.RejectApplication(id);
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractSaved, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public IActionResult CardByParentContractId([FromBody] int? parentContractId)
        {
            return Ok(_applicationService.GetApplicationModelForAddition(parentContractId));
        }

        [HttpPost]
        public IActionResult GetLimitByCategory([FromBody] int trancheId)
        {
            if (trancheId <= 0)
                return null;
            return Ok(_applicationService.GetLimitByCategory(trancheId));
        }

        private void ValidateContractModel(ContractModel model)
        {
            //проверяем Contract
            if (model.Contract == null)
                throw new NullReferenceException($"Для создания Договора из Заявки Contract не может быть пустым");

            var contract = model.Contract;

            if (model.ApplicationDetails == null)
                throw new NullReferenceException($"Для создания Договора из Заявки ApplicationDetails не может быть пустым");

            if (model.ApplicationDetails.ApplicationId == 0)
                throw new NullReferenceException($"Для создания Договора из Заявки ApplicationDetails.ApplicationId не может быть пустым");

            if (contract.ClientId == 0)
                throw new NullReferenceException($"В модели Contract не заполнен ClientId");

            if (!contract.Positions.Any())
                throw new PawnshopApplicationException($"В модели Contract массив Позиции не заполнен");

            if (contract.Positions.FirstOrDefault().PositionId == 0)
                throw new PawnshopApplicationException($"В модели Contract для Позиции не заполнен PositionId");

            if (contract.ProductTypeId.HasValue && !contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            if (model.ApplicationDetails.IsFirstTransh && !model.ApplicationDetails.TotalAmount4AllTransh.HasValue)
                throw new PawnshopApplicationException("Заполните общую сумму Договора с учетом первого транша");
        }

        private void ValidateApplication(Contract contract, ApplicationDetails applicationDetails, Application application)
        {
            if (applicationDetails.ProdKind == 0)
                throw new NullReferenceException($"Для создания Договора из Заявки ApplicationModel.ApplicationDetails не может быть пустым");

            if (DateTime.Compare(application.ApplicationDate, DateTime.Today) < 0)
                throw new PawnshopApplicationException($"Невозможно создать Договор. Дата Заявки с Id {application.Id} меньше текущей даты");

            if (application.ClientId == 0)
                throw new NullReferenceException($"В Заявке с Id {application.Id} не заполнен ClientId");

            if (application.PositionId == 0)
                throw new NullReferenceException($"В Заявке с Id {application.Id} не заполнен PositionId");

            if (contract.ClientId != application.ClientId)
                throw new PawnshopApplicationException($"Contract.ClientId {contract.ClientId} не соответствует Application.ClientId {application.ClientId} для Заявки с Id {application.Id}");

            if (contract.Positions.FirstOrDefault().PositionId != application.PositionId)
                throw new PawnshopApplicationException($"PositionId из модели Contract не соответствует Application.PositionId {application.PositionId} для Заявки с Id {application.Id}");

            if (application.IsAddition && !application.ParentContractId.HasValue)
                throw new PawnshopApplicationException($"В Заявке на добор с Id {application.Id} не заполнен ParentContractId");

            if (application.WithoutDriving && !applicationDetails.ProdKind.Equals(ProdKind.Motor))
                throw new PawnshopApplicationException($"Выбранный ProdKind не соответствует значению WithoutDriving = {application.WithoutDriving} для Заявки с Id {application.Id} ");

            if (contract.Setting.ContractClass != ContractClass.Tranche && IsExistActiveContractForPosition(contract, application))
                throw new PawnshopApplicationException($"По выбранной позиции существует открытый Договор {contract.ContractNumber ?? string.Empty}");

            if ((application.LightCost > application.MotorCost || application.TurboCost > application.MotorCost)
                && contract.CollateralType == CollateralType.Car)
                throw new PawnshopApplicationException("сумма \"С правом вождения\" превышает сумму \"Без права вождения\", уточните у оценщика");
        }

        private bool IsExistActiveContractForPosition(Contract contract, Application application)
        {
            var existContract = _contractService.Find(new ContractFilter
            {
                PositionId = application.PositionId,
                CollateralType = contract.CollateralType,
                Status = ContractStatus.Signed
            });

            if (existContract != null)
                return true;

            return false;
        }

        public async Task<int> CreateContractFromApplication(ContractModel contractModel, int branchId, string numberCode, int userId)
        {
            ValidateContractModel(contractModel);

            var contract = contractModel.Contract;
            if (contract.AttractionChannelId == null || contract.AttractionChannelId == 0)
            {
                contract.AttractionChannelId = _attractionChannelRepository.GetIdByCode(Constants.DEFAULT_ATTRACTION_CHANNEL_CODE);
            }
            var applicationModel = CreateApplicationModel(contractModel);

            ValidateApplication(contract, applicationModel.ApplicationDetails, applicationModel.Application);
            _applicationService.ValidateLoanAmounts(contract.LoanCost, applicationModel, contract.Setting.ContractClass);
            _contractService.CheckMaxPossibleContractPeriod(contract);

            if (await _fcb.ValidateIsOverdueClient(contract.Client.Id))
            {
                applicationModel.Application.Status = ApplicationStatus.Rejected;
                _applicationRepository.Update(applicationModel.Application);
                throw new PawnshopApplicationException("Клиент имеет просрочку свыше 90 дней. Заявка отклонена");
            }
            
            bool isTranche = contract.Setting.ContractClass == ContractClass.Tranche && contract.ParentId != null;
            ContractAction changeCategoryAction = new ContractAction();

            if (isTranche)
            {
                if (!contract.ParentId.HasValue)
                    throw new PawnshopApplicationException("Нельзя создать транш без кредитной линии!");

                contract.ParentContract = _contractService.Get(contract.ParentId.Value);

                if (contract.ParentContract.ContractClass != ContractClass.CreditLine )
                    throw new PawnshopApplicationException("Создать транш можно только в рамках Кредитной линии! Обратитесь в службу тех. поддержки");

                if (contract.ParentContract.MaturityDate < contract.MaturityDate)
                    throw new PawnshopApplicationException("Нельзя создать транш с датой возврата позднее чем дата возврата Кредитной линии");

                int tranchesCount = await _contractRepository.GetTranchesCount(contract.ParentId.Value) + 1;
                contract.ContractNumber = $"{contract.ParentContract.ContractNumber}-T{tranchesCount:D3}";
                contract.CreditLineId = contract.ParentContract.Id;

                var trancheSetting = _loanPercentRepository.Get(contract.SettingId.Value);
                if (trancheSetting.IsInsuranceAdditionalLimitOn)
                {
                    CalcLeftLoanCostForTranche(contract);
                    InitTrancheWithInsuranceAdditionalLimit(contract, applicationModel, changeCategoryAction, userId);
                }
                else
                {
                    CalcLeftLoanCostForTranche(contract);
                }
            }
            else
            {
                contract.ContractNumber = _contractService.GenerateContractNumber(contract.ContractDate, branchId, numberCode);
            }

            InitContract(contract, applicationModel.Application, branchId, userId);
            AddChecks(contract);

            bool IsCreditLine = contract.Setting.ContractClass == ContractClass.CreditLine;
            if (IsCreditLine)
            {
                var product = _loanPercentRepository.Get(contract.SettingId.Value);
                contract.Positions[0].LoanCost = contract.LoanCost;
                contract.MaturityDate = DateTime.Now.Date.AddMonths(product.ContractPeriodTo.Value);
                contract.LoanPeriod = product.ContractPeriodTo.Value * (int)product.ContractPeriodToType;
            }

            using (var transaction = _applicationRepository.BeginTransaction())
            {
                _contractService.Save(contract);
                applicationModel.Application.Status = ApplicationStatus.Processing;
                _applicationRepository.Update(applicationModel.Application);
                applicationModel.ApplicationDetails.ContractId = contract.Id;
                _applicationDetailsRepository.Insert(applicationModel.ApplicationDetails);

                if (IsCreditLine)
                {
                    await _contractService.CreateFirstTranche(contract, contract.FirstTranche, contract.AuthorId,
                        _insurancePremiumCalculator,
                        _insurancePoliceRequestService,
                        _contractPaymentScheduleService,
                        _contractKdnService,
                        true);
                }
                else if (isTranche && changeCategoryAction.ActionType == ContractActionType.ChangeCreditLineCategory)
                {
                    changeCategoryAction.ContractId = contract.Id;
                    _contractActionService.Save(changeCategoryAction);
                }

                _contractService.Save(contract);

                //Добавляем новую запись в таблицу ContractPartialSign
                if (applicationModel.ApplicationDetails.IsFirstTransh)
                {
                    _applicationService.SavePartialSign(contract, applicationModel.ApplicationDetails);
                }

                transaction.Commit();
            }

            return IsCreditLine ? contract.FirstTranche.Id.Value : contract.Id;
        }

        private ApplicationModel CreateApplicationModel(ContractModel contractModel)
        {
            var application = _applicationRepository.Get(contractModel.ApplicationDetails.ApplicationId);
            DateTime? creditLineMaturityDate = DateTime.MinValue;
            if (application.ParentContractId.HasValue && !application.IsAddition)
                creditLineMaturityDate = _contractService.GetOnlyContract(application.ParentContractId.Value).MaturityDate;

            return new ApplicationModel()
            {
                Application = application,
                ApplicationDetails = contractModel.ApplicationDetails,
                ApplicationAdditionalLimit = _creditLineService.GetLimitPersentForInsurance(application.EstimatedCost).Result,
                CreditLineMaturityDate = creditLineMaturityDate.Value
            };
        }

        private void InitContract(Contract contract, Application application, int branchId, int userId)
        {
            LoanPercentSetting loanPercentSetting = null;
            contract.ContractDate = DateTime.Today;
            contract.EstimatedCost = application.EstimatedCost;
            contract.ClientId = application.ClientId;
            contract.Client = _clientRepository.Get(application.ClientId);

            if (contract.SettingId.HasValue)
            {
                loanPercentSetting = _loanPercentRepository.Get(contract.SettingId.Value);
            }
            else
            {
                LoanPercentQueryModel query = new LoanPercentQueryModel()
                {
                    CollateralType = contract.CollateralType,
                    CardType = contract.Client.CardType,
                    LoanCost = contract.LoanCost,
                    LoanPeriod = contract.LoanPeriod,
                    BranchId = branchId
                };

                loanPercentSetting = _loanPercentRepository.Find(query);
            }

            contract.ContractTypeId = loanPercentSetting.ContractTypeId;
            contract.ProductTypeId = loanPercentSetting.ProductTypeId;
            contract.CollateralType = loanPercentSetting.CollateralType;
            contract.PeriodTypeId = loanPercentSetting.PeriodTypeId;
            contract.LoanPercent = loanPercentSetting.LoanPercent;
            contract.LoanPercentCost = contract.LoanCost * (contract.LoanPercent / 100);
            contract.UsePenaltyLimit = loanPercentSetting.UsePenaltyLimit;
            contract.OwnerId = branchId;
            contract.BranchId = branchId;
            contract.AuthorId = userId;
            contract.ContractClass = loanPercentSetting.ContractClass;
            if (contract.ContractData == null)
                contract.ContractData = new ContractData() { Client = application.Client, PrepaymentCost = 0 };
            else
            {
                contract.ContractData.Client = application.Client;
                contract.ContractData.PrepaymentCost = 0;
            }

            contract.ContractRates = new List<ContractRate>();

            loanPercentSetting.LoanSettingRates.ForEach(x =>
            {
                contract.ContractRates.Add(new ContractRate
                {
                    ContractId = contract.Id,
                    RateSettingId = x.RateSettingId,
                    Date = contract.ContractDate,
                    Rate = x.Rate,
                    CreateDate = DateTime.Now,
                    AuthorId = userId
                });
            });

            // При признаке автокредита, добавляется субъект к контракту 
            if (application.IsAutocredit == 1)
            {
                InitAutocredit(contract, application, loanPercentSetting.Name);
            }
        }

        private void CalcLeftLoanCostForTranche(Contract contract)
        {
            contract.LeftLoanCost += _contractService.GetDebtInfoByCreditLine(contract.ParentId.Value).PrincipalDebt;

            var trancheProduct = _loanPercentRepository.Get(contract.SettingId.Value);
            if (trancheProduct.IsInsuranceAvailable)
            {
                var insuranceCompanyId = trancheProduct.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId;
                var insurancePremium = _insurancePremiumCalculator.GetInsuranceDataV2(contract.LoanCost, insuranceCompanyId, contract.SettingId.Value).InsurancePremium;
                contract.LeftLoanCost += insurancePremium;
            }

            contract.LeftLoanCost += contract.LoanCost;
            contract.MaxCreditLineCost = contract.ParentContract.LoanCost;
        }

        private void InitTrancheWithInsuranceAdditionalLimit(Contract contract, ApplicationModel applicationModel, ContractAction changeCategoryAction, int userId)
        {
            var parentApp = _applicationRepository.FindByContractId(contract.ParentId.Value);
            var application = applicationModel.Application;
            var applicationDetails = applicationModel.ApplicationDetails;

            if (application.MotorCost > parentApp.MotorCost)
                throw new PawnshopApplicationException("Нельзя создать транш, у которого сумма займа больше, чем одобренная сумма на момент создания кредитной линии для категории \"Без права вождения\"");
            if (application.LightCost > parentApp.MotorCost || application.TurboCost > parentApp.MotorCost)
                throw new PawnshopApplicationException("Нельзя создать транш, у которого сумма займа больше, чем одобренная сумма на момент создания кредитной линии для категории \"С правом вождения\"");

            var creditLineCategory = contract.ParentContract.Positions.FirstOrDefault().Category.Code;
            bool isCategoryMotor = creditLineCategory == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE;

            var changeCategoryData = _contractService.ChangeCategoryForCredilLineData(application, isCategoryMotor, applicationDetails.ProdKind, applicationModel.ApplicationAdditionalLimit, contract.LeftLoanCost, contract.SettingId.Value);

            if (changeCategoryData.isCategoryChange && (changeCategoryData.checkLeftLoanCostForMotor || changeCategoryData.checkLeftLoanCostForLight))
            {
                changeCategoryAction.ActionType = ContractActionType.ChangeCreditLineCategory;
                changeCategoryAction.Date = DateTime.Now;
                // сумма контракта по категории аналитики
                changeCategoryAction.Cost = changeCategoryData.maxSumByAnaliticsCategory;
                changeCategoryAction.Data = new ContractActionData() { CategoryChanged = false };
                changeCategoryAction.CreateDate = DateTime.Now;
                changeCategoryAction.Status = ContractActionStatus.Await;
                changeCategoryAction.AuthorId = userId;
                changeCategoryAction.Reason = $"Cмена категории аналитики кредитнорй линии по договору {contract.ParentContract.ContractNumber} от {DateTime.Now.Date.Date}";
                // костыль чтоб не было исключения
                changeCategoryAction.PayTypeId = 1;
            }
            else if (changeCategoryData.isCategoryChange && !changeCategoryData.checkLeftLoanCostForLight && !application.WithoutDriving)
            {
                throw new PawnshopApplicationException("Нельзя сменить категорию аналитики, есть неоплаченный долг по Кредитной линии");
            }
        }

        private void AddChecks(Contract contract)
        {
            var contractChecks = _contractCheckRepository.List(new ListQuery() { Page = null });

            contract.Checks.AddRange(contractChecks.Select(c => new ContractCheckValue()
            {
                AuthorId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                BeginDate = c.PeriodRequired ? contract.ContractDate : default,
                EndDate = c.PeriodRequired ? contract.ContractDate.AddYears(c.DefaultPeriodAddedInYears ?? 0) : default,
                Value = true,
                CheckId = c.Id,
                Check = c
            }).ToList());
        }

        private void InitAutocredit(Contract contract, Application application, string productName)
        {
            ApplicationMerchant applicationMerchant = _applicationMerchantRepository.Get((int)application.ApplicationMerchantId);
            LoanSubject loanSubject = _loanSubjectRepository.GetByCode("MERCHANT");
            ClientLegalForm clientLegalForm = _clientLegalFormRepository.Find(new { Code = applicationMerchant.DefinitionLegalPerson });
            User Author = _userRepository.Get(application.AuthorId);

            var merchant = _clientRepository.FindByIdentityNumber(applicationMerchant.IdentityNumber);
            bool isMerchantNew = merchant is null;

            if (isMerchantNew)
                merchant = new Client();

            merchant.CardType = CardType.Standard;
            merchant.IdentityNumber = applicationMerchant.IdentityNumber;
            merchant.Name = applicationMerchant.Name;
            merchant.Surname = applicationMerchant.Surname;
            merchant.Patronymic = applicationMerchant.MiddleName;
            merchant.FullName = GetFullName(applicationMerchant);
            merchant.BirthDay = applicationMerchant.BirthDay;
            merchant.CreateDate = DateTime.Now;
            merchant.DocumentNumber = applicationMerchant.LicenseNumber;
            merchant.Author = Author;
            merchant.AuthorId = Author.Id;
            merchant.LegalForm = clientLegalForm;
            merchant.LegalFormId = clientLegalForm.Id;
            merchant.DeleteDate = null;
            merchant.IsSeller = true;

            if (isMerchantNew)
                _clientRepository.Insert(merchant);
            else
                _clientRepository.Update(merchant);

            ContractLoanSubject contractLoanSubject = new ContractLoanSubject()
            {
                SubjectId = loanSubject.Id,
                Subject = loanSubject,
                ContractId = contract.Id,
                ClientId = merchant.Id,
                Client = merchant,
                AuthorId = Author.Id,
                Author = Author,
                CreateDate = DateTime.Now,
                DeleteDate = null
            };

            contract.Subjects = new List<ContractLoanSubject>() { contractLoanSubject };
            decimal percent = decimal.Parse(Regex.Replace(productName, "[^0-9]", "")) / 100;

            if (percent > 0 && application.EstimatedCost > 0)
                contract.MinimalInitialFee = application.EstimatedCost * percent;

            contract.RequiredInitialFee = application.PrePayment;
            contract.Positions[0].MinimalInitialFee = (decimal)contract.MinimalInitialFee;
            contract.Positions[0].RequiredInitialFee = (decimal)contract.RequiredInitialFee;
        }

        private string GetFullName(ApplicationMerchant applicationMerchant)
        {
            string fullName = $"{applicationMerchant.Surname} {applicationMerchant.Name} {applicationMerchant.MiddleName}";

            if (string.IsNullOrEmpty(applicationMerchant.Surname) || applicationMerchant.Surname == "Не указан")
                fullName = $"{applicationMerchant.Name} {applicationMerchant.MiddleName}";

            return fullName;
        }
    }
}
