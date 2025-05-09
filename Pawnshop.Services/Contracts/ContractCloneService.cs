using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Contracts.LoanFinancePlans;
using Pawnshop.Services.Models.Insurance;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Pawnshop.Services.Models.Calculation.APR;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.PaymentSchedules;

namespace Pawnshop.Services.Contracts
{
    public class ContractCloneService : IContractCloneService
    {
        private readonly ClientRepository _clientRepository;
        private readonly UserRepository _userRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IContractService _contractService;
        private readonly ContractNumberCounterRepository _contractCounterRepository;
        private readonly IEventLog _eventLog;
        private readonly MintosBlackListRepository _mintosBlackListRepository;
        private readonly InsuranceRepository _insuranceRepository;
        private readonly IAccountService _accountService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly ILoanFinancePlanSerivce _loanFinancePlanSerivce;
        private readonly IBaseService<ContractProfile> _contractProfileService;
        private readonly IContractRateService _contractRateService;
        private readonly IApplicationService _applicationService;
        private readonly IPaymentScheduleService _paymentScheduleService;

        public ContractCloneService(
            ClientRepository clientRepository,
            UserRepository userRepository,
            LoanPercentRepository loanPercentRepository,
            GroupRepository groupRepository,
            IContractService contractService,
            ContractNumberCounterRepository contractCounterRepository,
            IEventLog eventLog,
            MintosBlackListRepository mintosBlackListRepository,
            InsuranceRepository insuranceRepository,
            IAccountService accountService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            ILoanFinancePlanSerivce loanFinancePlanSerivce,
            IBaseService<ContractProfile> contractProfileService,
            IContractRateService contractRateService,
            IApplicationService applicationService,
            IPaymentScheduleService paymentScheduleService)
        {
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _loanPercentRepository = loanPercentRepository;
            _groupRepository = groupRepository;
            _contractService = contractService;
            _contractCounterRepository = contractCounterRepository;
            _eventLog = eventLog;
            _mintosBlackListRepository = mintosBlackListRepository;
            _insuranceRepository = insuranceRepository;
            //_emailSender = emailSender;
            _accountService = accountService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _loanFinancePlanSerivce = loanFinancePlanSerivce;
            _contractProfileService = contractProfileService;
            _contractRateService = contractRateService;
            _applicationService = applicationService;
            _paymentScheduleService = paymentScheduleService;
        }

        private List<ContractLoanSubject> AddSubjectById(int subjectId, User author)
        {
            var subject = _clientRepository.Get(subjectId);

            return new List<ContractLoanSubject>() { new ContractLoanSubject(){
                    Author = author,
                    AuthorId = author.Id,
                    Client = subject,
                    ClientId = subjectId,
                    SubjectId = 2,
                    CreateDate = DateTime.Now,
                    Subject = new LoanSubject()
                    {
                        Author = author,
                        AuthorId = author.Id,
                        CBId = 2,
                        Code = "COBORROWER",
                        CreateDate = DateTime.Now,
                        Name = "Созаемщик",
                        NameAlt = "Созаемщик"
                    }
                }
            };
        }

        private List<ContractLoanSubject> AddSubjectsById(List<ContractLoanSubject>? subjects, int? subjectId, User author)
        {
            var subjectList = subjects?.Select(s => new ContractLoanSubject
            {
                CreateDate = DateTime.Now,
                AuthorId = author.Id,
                ClientId = s.ClientId,
                Client = s.Client,
                SubjectId = s.SubjectId,
                Subject = s.Subject
            }).ToList();

            if (subjectList is null)
                subjectList = new List<ContractLoanSubject>();

            if (subjectId.HasValue)
            {
                var subject = _clientRepository.Get(subjectId.Value);

                subjectList.Add(new ContractLoanSubject()
                {
                    Author = author,
                    AuthorId = author.Id,
                    Client = subject,
                    ClientId = subject.Id,
                    SubjectId = 2,
                    CreateDate = DateTime.Now,
                    Subject = new LoanSubject()
                    {
                        Author = author,
                        AuthorId = author.Id,
                        CBId = 2,
                        Code = "COBORROWER",
                        CreateDate = DateTime.Now,
                        Name = "Созаемщик",
                        NameAlt = "Созаемщик"
                    }
                });
            }

            return subjectList;
        }

        public Contract CreateContract(Contract contract, ContractAction action, int authorId, int branchId, decimal? loanCost = null,
                                       bool isAddition = false, ContractRefinanceConfig refConfig = null, int? settingId = null, int? additionloanPeriod = null, int? subjectId = null, int? positionEstimatedCost = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь(автор) {authorId} не найден");

            //var pulledPositions = contract.Positions.Where(p => p.Status == ContractPositionStatus.PulledOut)
            //    .ToArray();
            var application = _applicationService.IsAdditionFromApplication(contract);
            var pulledPositions = _applicationService.ChangePositionEstimatedCostFromApplication(application, contract, isAddition, positionEstimatedCost);

            int? contractSettingId = settingId.HasValue ? settingId : contract.SettingId;

            var client = _clientRepository.Get(contract.ClientId);

            var PaymentOrderSchema = Constants.DEFAULT_PAYMENT_ORDER_SCHEMA;
            if (contractSettingId.HasValue)
            {
                var product = _loanPercentRepository.Get(contractSettingId.Value);
                PaymentOrderSchema = product.PaymentOrderSchema;

                contract.Setting = product;

                if (product.IsProduct)
                {
                    contract.PercentPaymentType = PercentPaymentType.Product;
                }
            }

            var clone = new Contract
            {
                ContractDate = action.Date,
                ClientId = contract.ClientId,
                Client = client,
                CollateralType = contract.CollateralType,
                PercentPaymentType = refConfig == null ? contract.PercentPaymentType : refConfig.PercentPaymentType,
                LoanPeriod = refConfig == null ? (additionloanPeriod.HasValue ? additionloanPeriod.Value : contract.LoanPeriod) : refConfig.LoanPeriod,
                EstimatedCost = contract.CollateralType == CollateralType.Unsecured
                    ? contract.EstimatedCost
                    : pulledPositions.Sum(p => p.EstimatedCost),
                LoanCost = loanCost ?? pulledPositions.Sum(p => p.LoanCost),
                LoanPercent = refConfig == null ? contract.LoanPercent : refConfig.LoanPercent,
                Note = contract.Note,
                ContractData = new ContractData()
                {
                    AttractionChannelInfo = contract.ContractData.AttractionChannelInfo,
                    Client = client,
                    PrepaymentCost = action.Data != null ? action.Data.MigratedPrepaymentCost : 0,
                },
                ContractSpecific = contract.ContractSpecific,
                Files = contract.Files,
                Positions = pulledPositions.Select(p => new ContractPosition
                {
                    PositionId = p.PositionId,
                    Position = p.Position,
                    PositionCount = p.PositionCount,
                    LoanCost = loanCost.HasValue
                        ? loanCost.Value / pulledPositions.Count()
                        : p.LoanCost,
                    EstimatedCost = p.EstimatedCost,
                    CategoryId = p.CategoryId,
                    Note = p.Note,
                    PositionSpecific = p.PositionSpecific,
                    Status = ContractPositionStatus.Active,
                    RequiredInitialFee = p.RequiredInitialFee,
                    MinimalInitialFee = p.MinimalInitialFee
                }).ToList(),
                Status = ContractStatus.Draft,
                Locked = true,
                OwnerId = branch.Id,
                BranchId = branch.Id,
                AuthorId = author.Id,
                PaymentSchedule = refConfig == null ? contract.PaymentSchedule : refConfig.Schedule,
                AttractionChannelId = contract.AttractionChannelId,
                ParentId = action.ActionType == ContractActionType.Addition
                    ? (int?)null
                    : contract.ParentId ?? contract.Id,
                ClosedParentId = contract.ClosedParentId ?? contract.Id,
                Subjects = AddSubjectsById(contract.Subjects, subjectId, author),
                SettingId = contractSettingId,
                Setting = contract.Setting,
                ProductTypeId = contract.ProductTypeId,
                ProductType = contract.ProductType,
                RequiredInitialFee = contract.RequiredInitialFee,
                PayedInitialFee = contract.PayedInitialFee,
                MinimalInitialFee = contract.MinimalInitialFee,
                CrmId = contract.CrmId,
                CrmPaymentId = contract.CrmPaymentId,
                LoanPurposeId = contract.LoanPurposeId,
                OtherLoanPurpose = contract.OtherLoanPurpose,
                ContractTypeId = contract.ContractTypeId,
                PeriodTypeId = contract.PeriodTypeId,
                PaymentOrderSchema = PaymentOrderSchema,
                UsePenaltyLimit = contract.UsePenaltyLimit,
                ContractClass = contract.ContractClass
            };

            if (contract.ContractRates is null || contract.ContractRates.Count == 0)
                throw new PawnshopApplicationException("На договоре не заполнены ставки по пене");

            if (!isAddition)
            {
                var contractRates = new List<ContractRate>();
                var contractRateSettings = contract.ContractRates.Select(t => t.RateSettingId).Distinct();

                foreach (var rateSettingId in contractRateSettings)
                {
                    var maxDate = contract.ContractRates.Where(t => t.RateSettingId == rateSettingId).Max(t => t.Date).Date;
                    var rate = contract.ContractRates.FirstOrDefault(t => t.Date.Date == maxDate && t.Rate != Constants.NBRK_PENALTY_RATE && t.RateSettingId == rateSettingId);

                    if (rate is null)
                        rate = contract.ContractRates.FirstOrDefault(t => t.Date.Date == maxDate && t.Rate == Constants.NBRK_PENALTY_RATE && t.RateSettingId == rateSettingId);

                    var contractRate = new ContractRate()
                    {
                        ActionId = action.Id,
                        AuthorId = authorId,
                        CreateDate = DateTime.Now,
                        Rate = rate.Rate,
                        RateSettingId = rate.RateSettingId,
                        Date = clone.ContractDate
                    };

                    contractRates.Add(contractRate);
                }

                clone.ContractRates = contractRates;
            }

            if (isAddition)
            {
                var percentSettings = contract.Setting;

                clone.LoanPercent = percentSettings.LoanPercent;

                var rates = percentSettings.LoanSettingRates.Select(t => new { t.Rate, t.RateSettingId });

                if (rates is null || rates.Count() == 0)
                    throw new PawnshopApplicationException("Не заполнены ставки по пене на настройках продукта");

                var contractRates = new List<ContractRate>();
                foreach (var rate in rates)
                {
                    var contractRate = new ContractRate()
                    {
                        ActionId = action.Id,
                        AuthorId = authorId,
                        CreateDate = DateTime.Now,
                        Rate = rate.Rate,
                        RateSettingId = rate.RateSettingId,
                        Date = clone.ContractDate
                    };

                    contractRates.Add(contractRate);
                }

                clone.ContractRates = contractRates;

                if (action.ContractChecks != null && action.ContractChecks.Any())
                {
                    var checks = action.ContractChecks;
                    checks.ForEach(check =>
                    {
                        if (!(check.Id > 0))
                        {
                            check.AuthorId = author.Id;
                            check.CreateDate = DateTime.Now;
                            if (check.Check.PeriodRequired)
                            {
                                check.BeginDate = clone.ContractDate;
                                check.EndDate =
                                    clone.ContractDate.AddYears(check.Check.DefaultPeriodAddedInYears ?? 0);
                            }
                        }
                    });
                    clone.Checks = checks;
                }
            }

            //корректировка процентов, согласно использованной скидке
            if (refConfig == null)
                UseDiscount(clone, action, authorId);

            clone.LoanPercentCost = clone.LoanCost * (clone.LoanPercent / 100);
            clone.ContractData.Client.CardType = CardType.Standard;
            clone.MaturityDate = refConfig == null
                ? isAddition
                    ? clone.ContractDate.AddDays(clone.LoanPeriod)
                    : clone.PercentPaymentType != PercentPaymentType.EndPeriod
                        ? contract.MaturityDate
                        : clone.ContractDate.AddDays(clone.LoanPeriod)
                : refConfig.MaturityDate;
            clone.OriginalMaturityDate = isAddition
                ? clone.ContractDate.AddDays(clone.LoanPeriod)
                : clone.PercentPaymentType != PercentPaymentType.EndPeriod
                    ? contract.MaturityDate
                    : clone.ContractDate.AddDays(clone.LoanPeriod);

            if (action.ActionType == ContractActionType.PartialPayment ||
                action.ActionType == ContractActionType.Refinance)
            {
                clone.PartialPaymentParentId = contract.Id;
            }

            if (action.ActionType == ContractActionType.Addition || action.ActionType == ContractActionType.PartialPayment)
            {
                _paymentScheduleService.BuildWithContract(clone);
                clone.FirstPaymentDate = clone.PaymentSchedule.Min(x => x.Date);
                clone.MaturityDate = clone.PaymentSchedule.Max(x => x.Date);
            }

            clone.NextPaymentDate = clone.PercentPaymentType == PercentPaymentType.EndPeriod
                ? clone.MaturityDate
                : clone.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);

            clone.ContractNumber = _contractCounterRepository.Next(
                clone.ContractDate.Year, branch.Id,
                branch.Configuration.ContractSettings.NumberCode);

            if (action.ActionType == ContractActionType.PartialPayment)
            {
                clone.Checks = contract.Checks;
            }

            double loanCostDouble = Convert.ToDouble(clone.LoanCost);
            var calculator = new APRCalculator(loanCostDouble);

            if (clone.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                calculator.AddInstalment(
                    (double)(clone.LoanCost + (clone.LoanPercentCost * clone.LoanPeriod)),
                    (clone.MaturityDate - clone.ContractDate).Days + (clone.Locked ? 1 : 0));
                clone.APR = (decimal)calculator.Calculate();
            }
            else
            {
                clone.PaymentSchedule.ToList().ForEach(payment =>
                {
                    calculator.AddInstalment((double)(payment.DebtCost + payment.PercentCost),
                        (payment.Date - clone.ContractDate).Days);
                });

                clone.APR = (decimal)calculator.Calculate();
            }

            if (contract.ProductType != null && contract.ProductType.Code == Constants.PRODUCT_DAMU)
            {
                clone.SignerId = contract.SignerId;
                clone.LCDate = contract.LCDate;
                clone.LCDecisionNumber = contract.LCDecisionNumber;
            }

            //проверка максимального срока займа от ликвидности авто
            _contractService.CheckPositionLiquidity(clone);
            _contractService.CheckMaxPossibleContractPeriod(clone);

            using (IDbTransaction transaction = _contractCounterRepository.BeginTransaction())
            {
                _contractService.Save(clone);
                _contractPaymentScheduleService.Save(clone.PaymentSchedule, clone.Id, author.Id);
                List<Account> accounts = new List<Account>();
                accounts = _accountService.OpenForContract(clone);

                _eventLog.Log(EventCode.ContractSaved, EventStatus.Success, EntityType.Contract, clone.Id, null, null, userId: authorId);
                _mintosBlackListRepository.Insert(new MintosBlackList()
                { ContractId = clone.Id, LockUntilDate = DateTime.Now.AddDays(1) });

                var insurance = _insuranceRepository.Find(new InsuranceQueryModel { ContractId = contract.Id });
                if (insurance != null)
                {
                    var loanPeriod = (action.Date - contract.ContractDate).Days;
                    var cashbackCost = insurance.InsuranceCost -
                                       (insurance.InsuranceCost / insurance.InsurancePeriod * loanPeriod);

                    insurance.EndDate = action.Date;
                    insurance.CashbackCost = cashbackCost;
                    insurance.Status = InsuranceStatus.Closed;
                    _insuranceRepository.Update(insurance);

                    var cloneInsurance = new Data.Models.Insurances.Insurance
                    {
                        ContractId = clone.Id,
                        InsuranceNumber = insurance.InsuranceNumber,
                        InsuranceCost = cashbackCost,
                        InsurancePeriod = insurance.InsurancePeriod - loanPeriod,
                        BeginDate = action.Date.Date.AddDays(1),
                        PrevInsuranceId = insurance.Id,
                        BranchId = branch.Id,
                        UserId = author.Id,
                        OwnerId = branch.Id
                    };

                    _insuranceRepository.Insert(cloneInsurance);
                    DateTime lastNskInsuranceDate = new DateTime(2019, 1, 21);
                    if (insurance.BeginDate < lastNskInsuranceDate)
                    {
                        //_emailSender.InsuranceCloseSend(insurance, contract);
                    }
                }

                if (contract.ProductType != null && contract.ProductType.Code == Constants.PRODUCT_DAMU)
                {
                    //Save FinancePlan
                    var loanFinancePlans = _loanFinancePlanSerivce.GetList(contract.Id);
                    if (loanFinancePlans != null && loanFinancePlans.Count != 0)
                    {
                        loanFinancePlans.ForEach(f =>
                        {
                            var cloneFinancePlan = new LoanFinancePlan
                            {
                                ContractId = clone.Id,
                                LoanPurposeId = f.LoanPurposeId,
                                Description = f.Description,
                                OwnFunds = f.OwnFunds,
                                DebtFunds = f.DebtFunds,
                                CreateDate = action.Date,
                                AuthorId = author.Id
                            };
                            _loanFinancePlanSerivce.Save(cloneFinancePlan);
                        });
                    }

                    //Save ContractProfiles
                    var contractProfile = _contractProfileService.Find(new { ContractId = contract.Id });
                    if (contractProfile != null)
                    {
                        var cloneContractProfile = new ContractProfile
                        {
                            ContractId = clone.Id,
                            BusinessTypeId = contractProfile.BusinessTypeId,
                            NewEmploymentNumberPlanned = contractProfile.NewEmploymentNumberPlanned,
                            IsStartingBorrower = contractProfile.IsStartingBorrower,
                            CreateDate = action.Date,
                            AuthorId = author.Id,
                            NewEmploymentNumberActual = contractProfile.NewEmploymentNumberActual,
                            ATEId = contractProfile.ATEId
                        };
                        _contractProfileService.Save(cloneContractProfile);
                    }
                }

                transaction.Commit();
                return clone;
            }
        }

        public void UseDiscount(Contract contract, ContractAction contractAction, int authorId)
        {
            var discounts = contractAction.Discount.Discounts.Where(x => x.ContractDiscountId.HasValue).Where(x =>
                x.ContractDiscount.IsTypical &&
                x.ContractDiscount.PersonalDiscount.ActionType == contractAction.ActionType &&
                (x.TotalLoanPercentAdjustment != 0 || x.TotalPenaltyPercentAdjustment != 0/* || x.CategoryHasGhanges*/));

            foreach (var discount in discounts)
            {
                if (discount.TotalLoanPercentAdjustment != 0)
                {
                    contract.LoanPercent -= discount.TotalLoanPercentAdjustment;

                    if (contract.LoanPercent < 0)
                        contract.LoanPercent = 0;
                }

                if (discount.TotalPenaltyPercentAdjustment != 0)
                {
                    var penaltyRate = _contractRateService.GetPenyAccountRateWithoutBankRate(contract.Id, contract.ContractDate);
                    var profitPenaltyRate = _contractRateService.GetPenyProfitRateWithoutBankRate(contract.Id, contract.ContractDate);

                    penaltyRate.Rate -= discount.TotalPenaltyPercentAdjustment;
                    profitPenaltyRate.Rate -= discount.TotalPenaltyPercentAdjustment;

                    var contractRate = new ContractRate()
                    {
                        ActionId = contractAction.Id,
                        Date = contractAction.Date,
                        Rate = penaltyRate.Rate < 0 ? 0 : penaltyRate.Rate,
                        RateSettingId = penaltyRate.RateSettingId,
                        AuthorId = authorId,
                        CreateDate = DateTime.Now
                    };

                    contract.ContractRates.Add(contractRate);

                    contractRate.Rate = profitPenaltyRate.Rate < 0 ? 0 : profitPenaltyRate.Rate;
                    contractRate.RateSettingId = profitPenaltyRate.RateSettingId;

                    contract.ContractRates.Add(contractRate);
                }
            }
        }
    }
}
