using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Crm;
using Pawnshop.Web.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Insurance;
using Type = Pawnshop.Data.Models.AccountingCore.Type;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Services.Cars;

namespace Pawnshop.Web.Engine.Services
{
    public class ContractActionAdditionService : IContractActionAdditionService
    {
        private readonly IContractActionCheckService _contractActionCheckService;
        private readonly IContractService _contractService;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionSignService _contractActionSignService;
        private readonly IContractCloneService _contractCloneService;
        private readonly IEventLog _eventLog;
        private readonly IVerificationService _verificationService;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IExpenseService _expenseService;
        private readonly IContractActionOperationPermisisonService _contractActionOperationPermisisonService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;

        private readonly ContractRepository _contractRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly GroupRepository _groupRepository;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly CarRepository _carRepository;
        private readonly UserRepository _userRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly TypeRepository _typeRepository;
        private readonly IInsurancePolicyService _insurancePolicyService;
        private readonly IDomainService _domainService;
        private readonly IParkingActionService _parkingActionService;

        private readonly IContractKdnService _contractKdnService;

        private string _badDateError = "Дата не может быть меньше даты последнего действия по договору";
        public ContractActionAdditionService(
            IContractActionCheckService contractActionCheckService,
            ContractRepository contractRepository,
            IContractService contractService,
            LoanPercentRepository loanPercentRepository,
            ICrmPaymentService crmPaymentService,
            PayTypeRepository payTypeRepository,
            GroupRepository groupRepository,
            IContractActionOperationService contractActionOperationService,
            VehicleBlackListRepository vehicleBlackListRepository,
            CarRepository carRepository, IContractActionService contractActionService,
            IContractActionSignService contractActionSignService,
            IContractCloneService contractCloneService,
            UserRepository userRepository,
            MintosContractRepository mintosContractRepository,
            IEventLog eventLog, MintosContractActionRepository mintosContractActionRepository,
            IVerificationService verificationService,
            IContractExpenseService contractExpenseService,
            IExpenseService expenseService,
            IContractActionOperationPermisisonService contractActionOperationPermisisonService,
            TypeRepository typeRepository, IContractPaymentScheduleService contractPaymentScheduleService,
            IInsurancePolicyService insurancePolicyService, IDomainService domainService,
            IContractKdnService contractKdnService,
            IParkingActionService parkingActionService
            )
        {
            _contractActionCheckService = contractActionCheckService;
            _contractRepository = contractRepository;
            _contractService = contractService;
            _loanPercentRepository = loanPercentRepository;
            _crmPaymentService = crmPaymentService;
            _payTypeRepository = payTypeRepository;
            _groupRepository = groupRepository;
            _contractActionOperationService = contractActionOperationService;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _carRepository = carRepository;
            _contractActionService = contractActionService;
            _contractActionSignService = contractActionSignService;
            _contractCloneService = contractCloneService;
            _userRepository = userRepository;
            _mintosContractRepository = mintosContractRepository;
            _eventLog = eventLog;
            _mintosContractActionRepository = mintosContractActionRepository;
            _verificationService = verificationService;
            _contractExpenseService = contractExpenseService;
            _expenseService = expenseService;
            _contractActionOperationPermisisonService = contractActionOperationPermisisonService;
            _typeRepository = typeRepository;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _insurancePolicyService = insurancePolicyService;
            _domainService = domainService;
            _contractKdnService = contractKdnService;
            _parkingActionService = parkingActionService;
        }


        public void Exec(ContractAction action, int authorId, int branchId, bool unsecuredContractSignNotallowed, bool forceExpensePrepaymentReturn)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.Cost <= 0)
                throw new PawnshopApplicationException("Укажите сумму добора");

            _verificationService.CheckVerification(action.ContractId);

            action.ActionType = ContractActionType.Addition;
            _contractActionCheckService.ContractActionCheck(action);
            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            decimal expensesCostinDB = _contractService.GetExtraExpensesCost(action.ContractId, action.Date);
            decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;
            if (expensesCostinDB != extraExpensesCost)
                throw new PawnshopApplicationException("Не сходятся суммы доп расходов с базой");

            var parentContract = _contractService.Get(action.ContractId);

            if (parentContract.Status != ContractStatus.Signed && parentContract.Status != ContractStatus.InsuranceApproved)
                throw new PawnshopApplicationException($"Договор {action.ContractId} должен быть подписан");

            if (parentContract.Actions.Any() && action.Date.Date < parentContract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            if (parentContract.ContractTransfers?.FirstOrDefault(t => t.BackTransferDate is null) != null)
                throw new PawnshopApplicationException("Данный функционал не предусмотрен для переданных договоров");

            if (parentContract.SettingId.HasValue)
            {
                var product = _loanPercentRepository.Get(parentContract.SettingId.Value);
                if (!product.AdditionAvailable)
                {
                    throw new PawnshopApplicationException($"В продукте {product.Name} добор невозможен из-за настроек продукта");
                }
            }

            if (action.ExtraExpensesIds.Any())
                CheckAllowedExtraExpenses(action.ExtraExpensesIds);

            
            if (action.PayTypeId.HasValue)
            {
                var payType = _payTypeRepository.Get(action.PayTypeId.Value);

                if (payType.OperationCode == Constants.PAY_OPERATION_IBAN)
                    throw new PawnshopApplicationException("Невозможно выдать добор на расчетный счет");
            }

            decimal contractDue = 0;
            var profitDue = _contractService.GetProfitBalance(action.ContractId, action.Date);
            contractDue += profitDue;
            contractDue += _contractService.GetOverdueProfitBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetPenyAccountBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetPenyProfitBalance(action.ContractId, action.Date);
            contractDue += extraExpensesCost;

            if (action.Cost < contractDue)
                throw new PawnshopApplicationException($"Сумма добора должна быть больше общей суммы задолженности по договору ({contractDue})");

            decimal childContractCost = _contractService.GetChildContractCost(parentContract.Id, action.Date, action.Cost);
            InsurancePolicy insurancePolicy = _insurancePolicyService.GetInsurancePolicyForAddition(parentContract, action.Cost, action.Date, childContractCost, action.ChildSettingId );
            
            using (var transaction = _contractRepository.BeginTransaction())
            {
                _crmPaymentService.Enqueue(parentContract);

                action.AuthorId = authorId;

                _contractActionOperationService.Register(parentContract, action, authorId, branchId: branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: Data.Models.CashOrders.OrderStatus.Approved);
                parentContract.Positions.ForEach(position =>
                {
                    position.Status = ContractPositionStatus.PulledOut;
                });

                parentContract.Status = ContractStatus.BoughtOut;
                parentContract.BuyoutDate = action.Date;
                parentContract.BuyoutReasonId = _domainService.GetDomainValue(Constants.BUYOUT_REASON_CODE, Constants.BUYOUT_ADDITION).Id;
                _contractRepository.Update(parentContract);

                var parentContractUpdatedPositions = parentContract;

                foreach (ContractPosition position in parentContractUpdatedPositions.Positions)
                {
                    var actionPosition = action.Positions.Find(x => x.PositionId == position.PositionId);
                    position.EstimatedCost = actionPosition.EstimatedCost;

                    if (position.Position.CollateralType == CollateralType.Car)
                    {
                        var car = action.Cars.Find(x => x.Id == position.Position.Id);

                        _vehicleBlackListRepository.SearchVehicleBlackListByBodyNumber(car, position.CategoryId, action.ActionType);

                        position.Position = car;
                        position.PositionId = actionPosition.PositionId;

                        _carRepository.Update(car);
                    }

                    if (position.Position.CollateralType == CollateralType.Machinery)
                    {
                        var machinery = parentContract.Positions.FirstOrDefault().Position as Machinery;

                        _vehicleBlackListRepository.SearchVehicleBlackListByBodyNumber(machinery, position.CategoryId, action.ActionType);
                    }
                }
                if (parentContractUpdatedPositions.ContractData.PrepaymentCost > 0 && parentContractUpdatedPositions.ContractData.PrepaymentCost > (action.ExtraExpensesCost ?? 0))
                {
                    action.Data.MigratedPrepaymentCost += parentContractUpdatedPositions.ContractData.PrepaymentCost - (action.ExtraExpensesCost ?? 0);
                }
                List<ContractActionRow> signActionRows = new List<ContractActionRow>();

                decimal cashFractalPart = Math.Ceiling(childContractCost) - childContractCost;

                ContractActionRow signActionRow = new ContractActionRow
                {
                    PaymentType = AmountType.Debt,
                    Cost = Math.Ceiling(childContractCost),
                };
                signActionRows.Add(signActionRow);
                childContractCost += cashFractalPart;

                if (insurancePolicy != null)
                {
                    ContractActionRow insurancePremiumActionRow = new ContractActionRow
                    {
                        PaymentType = AmountType.InsurancePremium,
                        Cost = insurancePolicy.SurchargeAmount,
                    };
                    signActionRows.Add(insurancePremiumActionRow);

                    childContractCost += insurancePolicy.SurchargeAmount;
                }

                if (parentContract.UsePenaltyLimit)
                {
                    ContractActionRow penaltyLimitActionRow = new ContractActionRow
                    {
                        PaymentType = AmountType.PenaltyLimit,
                        Cost = Math.Round(childContractCost * 0.1M, 2),
                    };
                    signActionRows.Add(penaltyLimitActionRow);
                }

                Contract childContract = _contractCloneService.CreateContract(parentContractUpdatedPositions, action, author.Id, branch.Id, loanCost: childContractCost, isAddition: true, settingId: action.ChildSettingId, additionloanPeriod: action.ChildLoanPeriod, subjectId: action.ChildSubjectId);

                if (action.ActionType == ContractActionType.Addition
                    && action.CategoryChanged.HasValue
                    && action.CategoryChanged.Value)
                {
                    if (action.Data != null)
                        action.Data.CategoryChanged = action.CategoryChanged.Value;
                    else
                    {
                        ContractActionData data = new ContractActionData();
                        data.CategoryChanged = action.CategoryChanged.Value;
                        action.Data = data;
                    }
                }

                action.FollowedId = childContract.Id;
                action.CreateDate = DateTime.Now;
                
                _contractActionService.Save(action);
                parentContractUpdatedPositions.ContractData.PrepaymentCost -= action.Data.MigratedPrepaymentCost;

                foreach (var schedule in parentContractUpdatedPositions.PaymentSchedule.Where(x => x.ActionId == null))
                {
                    schedule.Canceled = DateTime.Now;
                }

                _contractRepository.Update(parentContractUpdatedPositions);

                if (insurancePolicy != null)
                {
                    insurancePolicy.ContractId = childContract.Id;
                    _insurancePolicyService.Save(insurancePolicy);
                }

                _contractPaymentScheduleService.Save(parentContractUpdatedPositions.PaymentSchedule, parentContractUpdatedPositions.Id, authorId);
                PayType payType = new PayType();
                if (action.PayTypeId.HasValue)
                {
                    payType = _payTypeRepository.Get(action.PayTypeId.Value);
                }

                ContractAction signAction = new ContractAction()
                {
                    ActionType = ContractActionType.Sign,
                    Date = action.Date,
                    Reason = $"Договор займа {childContract.ContractNumber} от {childContract.ContractDate.ToString("dd.MM.yyyy")}",
                    TotalCost = childContract.LoanCost,
                    ContractId = childContract.Id,
                    ParentActionId = action.Id,
                    ParentAction = action,
                    PayTypeId = action.PayTypeId ?? null,
                    RequisiteId = action.RequisiteId ?? null,
                    RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                    Checks = action.Checks,
                    Files = action.Files,
                };

                signAction.Rows = signActionRows.ToArray();
                _contractActionSignService.Exec(signAction, authorId, branchId, unsecuredContractSignNotallowed, false, false, parentContract.Id);
                action.ChildActionId = signAction.Id;

                _contractActionService.Save(action);

                CheckKdn4Addition(parentContract, action, authorId);

                transaction.Commit();
            }

            ScheduleMintosPaymentUpload(_contractService.Get(action.ContractId), action);
        }

        private void CheckKdn4Addition(Contract contract, ContractAction action, int authorId)
        {
            var user = _userRepository.Get(authorId);
            var contractKdnModel = _contractKdnService.FillKdnModel(contract, user, true, action);

            if (contractKdnModel.IsKdnRequired && !contractKdnModel.IsKdnPassed)
                throw new PawnshopApplicationException($"Ошибка расчета КДН при Доборе: {contractKdnModel.KdnError}");
        }

        private void CheckAllowedExtraExpenses(IEnumerable<int> extraExpensesIds)
        {
            if (extraExpensesIds == null || !extraExpensesIds.Any())
                throw new ArgumentNullException($"Не передан список доп расходов для проверки  {nameof(extraExpensesIds)}");

            foreach (int extraExpensesId in extraExpensesIds)
            {
                ContractExpense contractExpense = _contractExpenseService.GetAsync(extraExpensesId).Result;
                if (contractExpense == null)
                    throw new PawnshopApplicationException($"Не найден расход c идентифкатором {extraExpensesId}");

                Expense expense = _expenseService.Get(contractExpense.ExpenseId);
                if (expense == null)
                    throw new PawnshopApplicationException($"Не найден тип расхода {contractExpense.ExpenseId}");

                if (expense.Type == null)
                {
                    if (!expense.TypeId.HasValue)
                        throw new PawnshopApplicationException($"На расходе {expense.Name} не настроен тип иерархии ");
                    expense.Type = _typeRepository.Get(expense.TypeId.Value);
                }

                bool restrictedExtraExpense = _contractActionOperationPermisisonService.RestrictedExtraExpensesForAddition(expense.Type.Code);
                if (restrictedExtraExpense)
                    throw new PawnshopApplicationException($"В операции Добор запрещена оплата расхода {expense.Name}. Воспользуйтесь операцией ОПЛАТА ");
            }
        }

        private void ScheduleMintosPaymentUpload(Contract contract, ContractAction action)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var mintosContracts = _mintosContractRepository.GetByContractId(contract.Id);
            MintosContract mintosContract = null;
            if (mintosContracts.Count > 1)
            {
                mintosContract = mintosContracts.Where(x => x.MintosStatus.Contains("active")).FirstOrDefault();
                if (mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() > 1)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(mintosContracts),
                        $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos {mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count()} раз(а)");
                }
            }
            else if (mintosContracts.Count == 1)
            {
                mintosContract = mintosContracts.FirstOrDefault();
                if (mintosContract != null &&
                    mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() == 0)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(mintosContracts),
                        $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos, но не проверен/утвержден модерацией Mintos)");
                }
            }
            else
            {
                return;
            }

            if (mintosContracts == null || mintosContract == null) return;
            if (!mintosContract.MintosStatus.Contains("active")) return;

            try
            {
                MintosContractAction mintosAction = new MintosContractAction(action);

                using (var transaction = _mintosContractRepository.BeginTransaction())
                {
                    mintosAction.MintosContractId = mintosContract.Id;

                    _mintosContractActionRepository.Insert(mintosAction);

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    responseData: e.Message);
            }
        }
    }
}
