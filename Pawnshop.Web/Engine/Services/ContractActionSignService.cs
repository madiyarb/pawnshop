using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Parking;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Web.Engine.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Web.Engine.Services
{
    public class ContractActionSignService : IContractActionSignService
    {
        private readonly IContractService _contractService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractActionCheckService _contractActionCheckService;
        private readonly IVerificationService _verificationService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        private readonly ContractRepository _contractRepository;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly MachineryRepository _machineryRepository;
        private readonly CrmUploadContractRepository _crmRepository;
        private readonly ParkingHistoryRepository _parkingHistoryRepository;
        private readonly CarRepository _carRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IInterestAccrual _interestAccrualService;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly IClientSignerService _clientSignerService;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly IContractVerificationService _contractVerificationService;
        private readonly IClientModelValidateService _clientModelValidateService;
        private readonly IInsurancePolicyService _insurancePolicyService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IContractActionService _contractActionService;
        private readonly IApplicationService _applicationService;
        private readonly IBusinessOperationService _businessOperationService;

        public ContractActionSignService(IContractService contractService,
            IContractActionOperationService contractActionOperationService,
            IContractActionCheckService contractActionCheckService,
            ContractRepository contractRepository,
            VehicleBlackListRepository vehicleBlackListRepository,
            CategoryRepository categoryRepository,
            MachineryRepository machineryRepository,
            CrmUploadContractRepository crmRepository,
            ParkingHistoryRepository parkingHistoryRepository,
            CarRepository carRepository,
            GroupRepository groupRepository,
            IClientQuestionnaireService clientQuestionnaireService,
            IInterestAccrual interestAccrualService,
            IClientBlackListService clientBlackListService,
            IClientSignerService clientSignerService,
            LoanPercentRepository loanPercentRepository,
            PayTypeRepository payTypeRepository,
            IContractVerificationService contractVerificationService,
            IClientModelValidateService clientModelValidateService,
            IInsurancePolicyService insurancePolicyService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            IContractActionService contractActionService,
            IApplicationService applicationService,
            IBusinessOperationService businessOperationService,
            IVerificationService verificationService
        )
        {
            _contractService = contractService;
            _contractActionOperationService = contractActionOperationService;
            _contractActionCheckService = contractActionCheckService;
            _contractRepository = contractRepository;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _categoryRepository = categoryRepository;
            _machineryRepository = machineryRepository;
            _crmRepository = crmRepository;
            _parkingHistoryRepository = parkingHistoryRepository;
            _carRepository = carRepository;
            _groupRepository = groupRepository;
            _clientQuestionnaireService = clientQuestionnaireService;
            _interestAccrualService = interestAccrualService;
            _clientBlackListService = clientBlackListService;
            _clientSignerService = clientSignerService;
            _payTypeRepository = payTypeRepository;
            _contractVerificationService = contractVerificationService;
            _clientModelValidateService = clientModelValidateService;
            _insurancePolicyService = insurancePolicyService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _contractActionService = contractActionService;
            _applicationService = applicationService;
            _businessOperationService = businessOperationService;
            _verificationService = verificationService;
        }

        public void Exec(ContractAction action,
            int authorId, int branchId,
            bool unsecuredContractSignNotAllowed,
            bool ignoreVerification = false,
            bool ignoreCheckQuestionnaireFilledStatus = false,
            int? parentContractId = null,
            OrderStatus? orderStatus = null,
            int? cashIssueBranchId = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.ActionType != ContractActionType.Sign)
                throw new ArgumentException($"Поле {nameof(action.ActionType)} должен иметь значение {ContractActionType.Sign}",
                    nameof(action));

            Contract contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");
            if (contract.CollateralType == CollateralType.Realty && contract.ParentId == null)
            {
                if (contract.SignDate == null)
                    throw new PawnshopApplicationException("Дата выдачи не указана");
                if (contract.SignDate.Value.Date != DateTime.Today)
                    throw new PawnshopApplicationException("Дата выдачи не совпадает с датой подписания");
            }

            contract.SignDate = contract.SignDate == null ? action.Date : contract.SignDate;

            Group branch = _groupRepository.Get(cashIssueBranchId ?? branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {cashIssueBranchId ?? branchId} не найден");

            //Проверка заполнения обязательных полей по клиенту
            _clientModelValidateService.ValidateClientModel(contract.Client, contract.CreatedInOnline);

            if (!ignoreVerification)
                _verificationService.CheckVerification(action.ContractId);

            if (!ignoreCheckQuestionnaireFilledStatus)
                _verificationService.CheckClientQuestionnaireFilledStatus(action.ContractId);

            _contractActionCheckService.ContractActionCheck(action);

            InsurancePoliceRequest latestPoliceRequest = null;

            if (!contract.PartialPaymentParentId.HasValue)
            {
                var setting = _contractService.GetSettingForContract(contract);

                if (setting.IsInsuranceAvailable)
                {
                    latestPoliceRequest = _insurancePoliceRequestService.GetLatestPoliceRequestAllStatus(contract.Id);

                    if (latestPoliceRequest is null)
                    {
                        throw new PawnshopApplicationException("Запрос на создание страхового полиса не найден. Подписание не доступно");
                    }
                }
            }

            contract.CheckEstimatedAndLoanCost();

            if (!action.ParentActionId.HasValue || (action.ParentAction != null &&
                                                    action.ParentAction.ActionType !=
                                                    ContractActionType.PartialPayment))
            {
                if (contract.CollateralType != CollateralType.Unsecured)
                {
                    if (contract.SignDate < Constants.NEW_MAX_APR_DATE)
                    {
                        if (contract.APR > Constants.MAX_APR_OLD)
                            throw new PawnshopApplicationException(
                                $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, свяжитесь с региональным менеджером!");
                    }
                    else
                    {
                        if (contract.APR > Constants.MAX_APR_V2)
                            throw new PawnshopApplicationException(
                                $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, свяжитесь с региональным менеджером!");
                    }
                }
            }

            if (contract.CollateralType == CollateralType.Unsecured)
            {
                if (!(action.ParentAction != null &&
                      action.ParentAction.ActionType == ContractActionType.PartialPayment) && unsecuredContractSignNotAllowed)
                {
                    throw new PawnshopApplicationException(
                        "У вас нет прав для подписания данного договора. Обратитесь к администратору.");
                }

                //if (!(contract.ContractData.Client.UserId > 0))
                //    throw new PawnshopApplicationException(
                //        "ИИН клиента не найден в базе сотрудников. Сначала внесите в список сотрудников.");
                if ((contract.AuthorId == contract.ContractData.Client.UserId) ||
                    (contract.ContractData.Client.UserId == authorId))
                    throw new PawnshopApplicationException("Клиент и Автор не могут быть одинаковые");

                if (contract.Positions != null && contract.Positions.Count > 0)
                {
                    if (contract.Positions.Any(x => x.EstimatedCost <= 0))
                        throw new PawnshopApplicationException("Оценка позиции не может быть меньше или равной 0");
                    if (contract.Positions.Any(x => x.LoanCost <= 0))
                        throw new PawnshopApplicationException("Ссуда позиции не может быть меньше или равной 0");
                }
            }

            if (contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR &&
                contract.Status != ContractStatus.AwaitForSign && !contract.Locked)
            {
                var errorStatus = ContractStatus.AwaitForSign.GetDisplayName();

                string.Format("Для данного вида продуктов действие подписание доступно только из статуса {0}", errorStatus);
            }

            _clientBlackListService.CheckClientIsInBlackList(contract.ClientId, action.ParentActionId > 0 ? action.ParentAction.ActionType : action.ActionType, contract.Id);

            if (contract.ProductTypeId.HasValue && contract.SettingId.HasValue)
            {
                if (!contract.Locked && contract.Setting.InitialFeeRequired.HasValue)
                {
                    decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, action.Date);
                    if (depoMerchantBalance < contract.RequiredInitialFee)
                    {
                        throw new PawnshopApplicationException(
                            $"Сумма первоначального взноса менее требуемой по договору, требуется внести ещё {contract.RequiredInitialFee - depoMerchantBalance}");
                    }
                }
            }

            Category category = null;
            if (contract.ContractClass != ContractClass.Tranche && (contract.CollateralType == CollateralType.Car || contract.CollateralType == CollateralType.Machinery))
            {
                if (action.ParentAction?.ActionType == ContractActionType.Addition || !action.ParentActionId.HasValue)
                {
                    _vehicleBlackListRepository.SearchVehicleBlackListByBodyNumber(
                        (IBodyNumber)contract.Positions.FirstOrDefault().Position,
                        contract.Positions.FirstOrDefault().CategoryId,
                        action.ParentActionId > 0 ? action.ParentAction.ActionType : action.ActionType);
                }

                category = _categoryRepository.Get(contract.Positions.FirstOrDefault().CategoryId);
            }

            if (!contract.Locked && contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR)
            {
                var initialFeeAccountBalance = _contractService.GetContractAccountBalance(contract.Id,
                    Constants.ACCOUNT_SETTING_DEPO_MERCHANT, action.Date);


                if (initialFeeAccountBalance == 0)
                    throw new PawnshopApplicationException(
                        $"Остаток на счете задолженности перед продавцом с кодом {Constants.ACCOUNT_SETTING_DEPO_MERCHANT} равен 0, от покупателя не был получен первоначальный взнос");

                var merchant = contract.Subjects?.FirstOrDefault(x => x.Subject.Code == "MERCHANT");
                if (merchant == null)
                    throw new PawnshopApplicationException($"Не найден субъект кредитования \"Мерчант\"(MERCHANT)");

                action.Data ??= new ContractActionData();

                action.Data.PrepaymentUsed += contract.RequiredInitialFee.Value;
                contract.PayedInitialFee = action.Data.PrepaymentUsed;

                action.Date = DateTime.Now.Date;
            }

            _contractVerificationService.CheckContractPositions(contract);

            if (contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_DAMU)
            {
                var payType = _payTypeRepository.Get(action.PayTypeId.Value);

                if (action.ParentActionId is null && ((payType != null && payType.OperationCode != Constants.PAY_OPERATION_IBAN) || payType is null))
                    throw new PawnshopApplicationException($"Обязательный вид платежа - \"Перечисление на расчетный счет\"");

                var clientSigner = _clientSignerService.CheckClientSigner(contract.Client, contract.ContractDate, contract.SignerId);

                if (clientSigner != null)
                {
                    var filled = _clientQuestionnaireService.IsClientHasFilledQuestionnaire(clientSigner.SignerId);

                    if (!filled)
                        throw new PawnshopApplicationException($"У клиента {clientSigner.Signer.FullName} не заполнена анкета");
                }

                var contractGuarantorSubjects = _contractRepository.GetContractWithSubject(contract.Id).Subjects;
                _contractVerificationService.CheckGuarantorSubjects(contractGuarantorSubjects);

                if (action.ParentActionId is null) _contractVerificationService.CheckDAMUContract(contract);
                _contractVerificationService.CheckDAMUContractMaturityDate(contract);
            }

            if (contract.ContractClass != ContractClass.CreditLine)
            {
                contract.NextPaymentDate = contract.PercentPaymentType == PercentPaymentType.EndPeriod
                    ? contract.MaturityDate
                    : contract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);
            }
            Contract creditLineContract = null;
            if (contract.ContractClass == ContractClass.Tranche)
            {
                creditLineContract = _contractRepository.Get(contract.CreditLineId.Value);
                if (!(creditLineContract.Status == ContractStatus.Signed || creditLineContract.Status == ContractStatus.AwaitForOrderApprove))
                {
                    throw new PawnshopApplicationException($"СОКЛ не подписано. Нельзя создавать транш.");
                }
                if (creditLineContract.Status == ContractStatus.BoughtOut ||
                    creditLineContract.Status == ContractStatus.SoldOut ||
                    creditLineContract.Status == ContractStatus.Disposed)
                {
                    throw new PawnshopApplicationException("Подписание невозможно. Кредитная линия выкуплена или реализована");
                }
            }

            using (var transaction = _contractRepository.BeginTransaction())
            {
                action.AuthorId = authorId;
                action.CreateDate = DateTime.Now;
                if (!contract.Locked && contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR)
                {
                    _contractRepository.Update(contract);
                }

                if (latestPoliceRequest != null)
                    _insurancePoliceRequestService.Save(latestPoliceRequest);

                _contractActionOperationService.Register(contract, action, authorId, branchId: branch.Id, orderStatus: orderStatus ?? OrderStatus.WaitingForApprove);
                // если у action нет платежной операции, 
                // то не меняем статус договора на Signed
                // TODO: absMigration - возможно потребуется удалить провреку orderStatus
                if (!action.PayOperationId.HasValue && !orderStatus.HasValue)
                    contract.Status = ContractStatus.AwaitForOrderApprove;

                _contractRepository.Update(contract);

                if (!contract.Locked)
                {
                    CrmUploadContract crmContract = new CrmUploadContract()
                    {
                        ContractId = contract.Id,
                        CreateDate = DateTime.Now,
                        UserId = authorId,
                        ClientCrmId = contract.Client.CrmId,
                        ContractCrmId = contract.CrmId,
                        BitrixId = _applicationService.GetBitrixId(contract.Id)
                    };
                    _crmRepository.Insert(crmContract);
                }

                if (category != null && (contract.CollateralType == CollateralType.Car ||
                                         contract.CollateralType == CollateralType.Machinery))
                {
                    string note = "";
                    if ((action.ActionType == ContractActionType.Addition || action.ActionType == ContractActionType.Sign))
                    {
                        if (category.Name == "без права вождения")
                            note = "Постановка автотранспорта на стоянку при заключении договора";
                    }

                    ParkingHistory parkingHistory = new ParkingHistory()
                    {
                        ContractId = contract.Id,
                        PositionId = contract.Positions.FirstOrDefault().PositionId,
                        StatusBeforeId = null,
                        StatusAfterId = (int)category.DefaultParkingStatusId,
                        UserId = authorId,
                        DelayDays = 0,
                        CreateDate = DateTime.Now,
                        Note = note,
                        Date = DateTime.Now.Date,
                        ParkingActionId = null,
                        ActionId = action.Id
                    };
                    _parkingHistoryRepository.Insert(parkingHistory);

                    if (contract.CollateralType == CollateralType.Car)
                    {
                        var position = contract.Positions.FirstOrDefault().Position as Car;
                        position.ParkingStatusId = (int)category.DefaultParkingStatusId;

                        _carRepository.Update(position);

                    }

                    if (contract.CollateralType == CollateralType.Machinery)
                    {
                        var position = contract.Positions.FirstOrDefault().Position as Machinery;
                        position.ParkingStatusId = (int)category.DefaultParkingStatusId;

                        _machineryRepository.Update(position);
                    }
                }

                if (contract.PercentPaymentType == PercentPaymentType.EndPeriod && !contract.Locked)
                {
                    var interestAccrualAction = _interestAccrualService.OnAnyDateAccrual(contract, authorId, contract.ContractDate);

                    action.ParentActionId = interestAccrualAction.Id;
                    _contractActionService.Save(action);

                    interestAccrualAction.ChildActionId = action.Id;
                    _contractActionService.Save(interestAccrualAction);
                }

                _applicationService.ValidateAndCompleteApplication(parentContractId, contract.Id, contract, latestPoliceRequest);

                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var amountDict = new Dictionary<AmountType, decimal> { { AmountType.CreditLineLimit, contract.LoanCost } };
                    _businessOperationService.Register(creditLineContract, action.Date, "TRANCHE_SIGN", branch, authorId, amountDict, action: action);
                }

                transaction.Commit();
            }
        }
    }
}
