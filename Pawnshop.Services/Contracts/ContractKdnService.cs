using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Services.PaymentSchedules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Services.Storage;
using Pawnshop.Services.Gamblers;
using Pawnshop.Services.Domains;

namespace Pawnshop.Services.Contracts
{
    public class ContractKdnService : IContractKdnService
    {
        private readonly IContractService _contractService;
        private readonly CarRepository _carRepository;
        private readonly MachineryRepository _machineryRepository;
        private readonly RealtyRepository _realtyRepository;
        private readonly ContractKdnEstimatedIncomeRepository _contractKdnEstimatedIncomeRepository;
        private readonly IClientService _clientService;
        private readonly IClientIncomeService _clientIncomeService;
        private readonly IFCBChecksService _gamblerService;
        private readonly IFcb4Kdn _fcb4Kdn;
        private readonly IEventLog _eventLog;
        private readonly IClientOtherPaymentsInfoService _clientOtherPaymentsInfoService;
        private readonly IApplicationService _applicationService;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly ContractKdnRequestRepository _contractKdnRequestRepository;
        private readonly ContractKdnCalculationLogRepository _contractKdnCalculationLogRepository;
        private readonly IContractActionService _contractActionService;
        private readonly GroupRepository _groupRepository;
        private readonly IContractCloneService _contractCloneService;
        private readonly IManualCalculationClientExpenseService _manualCalculationClientExpenseService;
        private readonly EnviromentAccessOptions _options;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly IStorage _storage;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly NotionalRateRepository _notionalRateRepository;
        private readonly IDomainService _domainService;

        public ContractKdnService(IContractService contractService,
            CarRepository carRepository,
            MachineryRepository machineryRepository,
            RealtyRepository realtyRepository,
            ContractKdnEstimatedIncomeRepository contractKdnEstimatedIncomeRepository,
            IClientService clientService,
            IClientIncomeService clientIncomeService,
            IFCBChecksService gamblerService,
            IFcb4Kdn fcb4Kdn,
            IEventLog eventLog,
            IClientOtherPaymentsInfoService clientOtherPaymentsInfoService,
            IApplicationService applicationService,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            ContractKdnRequestRepository contractKdnRequestRepository,
            ContractKdnCalculationLogRepository contractKdnCalculationLogRepository,
            IContractActionService contractActionService,
            GroupRepository groupRepository,
            IContractCloneService contractCloneService,
            IManualCalculationClientExpenseService manualCalculationClientExpenseService,
            IOptions<EnviromentAccessOptions> options,
            IPaymentScheduleService paymentScheduleService,
            IStorage storage, 
            LoanSubjectRepository loanSubjectRepository,
            IDomainService domainService,
            NotionalRateRepository notionalRateRepository)
        {
            _contractService = contractService;
            _carRepository = carRepository;
            _machineryRepository = machineryRepository;
            _realtyRepository = realtyRepository;
            _contractKdnEstimatedIncomeRepository = contractKdnEstimatedIncomeRepository;
            _clientService = clientService;
            _clientIncomeService = clientIncomeService;
            _gamblerService = gamblerService;
            _fcb4Kdn = fcb4Kdn;
            _eventLog = eventLog;
            _clientOtherPaymentsInfoService = clientOtherPaymentsInfoService;
            _applicationService = applicationService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _contractKdnRequestRepository = contractKdnRequestRepository;
            _contractKdnCalculationLogRepository = contractKdnCalculationLogRepository;
            _contractActionService = contractActionService;
            _groupRepository = groupRepository;
            _contractCloneService = contractCloneService;
            _manualCalculationClientExpenseService = manualCalculationClientExpenseService;
            _options = options.Value;
            _paymentScheduleService = paymentScheduleService;
            _storage = storage;
            _loanSubjectRepository = loanSubjectRepository;
            _domainService = domainService;
            _notionalRateRepository = notionalRateRepository;
        }

        public ContractKdnModel CheckKdn4AdditionWithCreateChild(ContractAction action, int branchId, User author, decimal? surchargeAmount, decimal additionCost, int? settingId = null, int? additionalLoanPeriod = null, int? subjectId = null, int? positionEstimatedCost = null)
        {
            action.ActionType = ContractActionType.Addition;

            var parentContract = _contractService.Get(action.ContractId);
            var parentContractUpdatedPositions = parentContract;

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            decimal childContractCost = _contractService.GetChildContractCost(parentContract.Id, action.Date, additionCost);
            decimal cashFractalPart = Math.Ceiling(childContractCost) - childContractCost;
            childContractCost += cashFractalPart;

            if (surchargeAmount.HasValue)
            {
                childContractCost += surchargeAmount.Value;
            }

            ContractKdnModel contractKdnModel;
            using (var transaction = _contractKdnCalculationLogRepository.BeginTransaction())
            {
                parentContract.Positions.ForEach(position =>
                {
                    position.Status = ContractPositionStatus.PulledOut;
                });
                parentContract.Status = ContractStatus.BoughtOut;
                _contractService.Save(parentContract);

                Contract childContract = _contractCloneService.CreateContract(parentContractUpdatedPositions, action, author.Id, branch.Id, loanCost: childContractCost, isAddition: true, settingId: settingId, additionloanPeriod: additionalLoanPeriod, subjectId: subjectId, positionEstimatedCost: positionEstimatedCost);
                action.FollowedId = childContract.Id;
                action.CreateDate = DateTime.Now;
                _contractActionService.Save(action);

                contractKdnModel = FillKdnModel(parentContract, author, true, action);

                if (positionEstimatedCost == null)
                {
                    positionEstimatedCost = parentContract.Positions.FirstOrDefault().EstimatedCost;
                }

                transaction.Rollback();
            }

            //Логируем результат расчет КДН
            using (var transaction = _contractKdnCalculationLogRepository.BeginTransaction())
            {
                int calcLogId = InsertToContractKdnCalculationLogs(parentContract.Id, contractKdnModel, author.Id, parentContract.Id, isAddition: true);

                UpdateContractKdnCalculationLogs(calcLogId, contractKdnModel, author.Id, parentContract.Id, parentContract.Id, settingId, additionalLoanPeriod, subjectId, positionEstimatedCost);

                transaction.Commit();
            }

            return contractKdnModel;
        }

        private string GetPositionDetails(Contract contract, ContractPosition contractPosition)
        {
            StringBuilder positionDetails = new StringBuilder();
            if (contract.CollateralType == CollateralType.Car)
            {
                var position = _carRepository.Get(contractPosition.PositionId);
                positionDetails.Append(position.TransportNumber).Append(" ").Append(position.Mark).Append(" ").Append(position.Model);
            }
            else if (contract.CollateralType == CollateralType.Machinery)
            {
                var position = _machineryRepository.Get(contractPosition.PositionId);
                positionDetails.Append(position.TransportNumber).Append(" ").Append(position.Mark).Append(" ").Append(position.Model);
            }
            else if (contract.CollateralType == CollateralType.Realty)
            {
                var position = _realtyRepository.Get(contractPosition.PositionId);
                positionDetails.Append(position.CadastralNumber);
            }

            return positionDetails.ToString();
        }

        private List<ContractKdnEstimatedIncomeModel> GetClientIncomeFromAuto(List<ContractKdnEstimatedIncomeModel> contractKdnEstimatedIncomeModels, Contract contract, bool isSubject)
        {
            List<ContractKdnEstimatedIncome> contractKdnEstimatedIncomeList;
            //для каждого ClientId будет свой отдельный Объект в массиве из ContractKdnEstimatedIncomeModel
            //когда вызываем данный метод уже для действующих контрактов клиента (Заемщика), сюда уже прилетатет заполненный contractKdnEstimatedIncomeModels
            //так как мы собираемся в массив contractKdnEstimatedIncomeList дополнить Позиции с действующих договоров Заемщика, мы должны найти Объект связанный с Заемщиком, 
            //чтобы вытащить его список contractKdnEstimatedIncomeList. нужную модель из массива мы ищем по ClientId. так как на действующих Договорах клиент будет такой же
            var contractKdnEstimatedIncomeModel = contractKdnEstimatedIncomeModels.Where(x => x.ClientId == contract.ClientId).FirstOrDefault();

            //Так как сумма TotalIncome будет общей как для текущей Позиции, так и длч Позиции из действующих Договоров, сохранеяем в переменной значение TotalIncome из модели
            //она как раз содержит там сумму по текущей Позиции
            //decimal totalEstimatedCost = contractKdnEstimatedIncomeModels.Where(x => x.IsSubject == isSubject).Select(x => x.TotalIncome).Sum();
            decimal totalEstimatedCost = contractKdnEstimatedIncomeModel != null ? contractKdnEstimatedIncomeModel.TotalPositionsIncome : 0;

            if (contractKdnEstimatedIncomeModel != null)
            {
                contractKdnEstimatedIncomeList = contractKdnEstimatedIncomeModel.ContractKdnEstimatedIncomes;//пишем в существующий массив ContractKdnEstimatedIncomes из Модели
            }
            else
            {
                //список contractKdnEstimatedIncomeList будет общий для данных по позиции из текущего договора, который выдется, также для позиции по действующим Договорам клиента
                contractKdnEstimatedIncomeList = new List<ContractKdnEstimatedIncome>();
                contractKdnEstimatedIncomeModel = new ContractKdnEstimatedIncomeModel()
                {
                    ClientId = contract.ClientId,
                    IIN = contract.Client.IdentityNumber,
                    IsSubject = isSubject,
                    TotalPositionsIncome = totalEstimatedCost,
                    FIO = contract.Client.FullName,
                    ContractKdnEstimatedIncomes = contractKdnEstimatedIncomeList
                };
                contractKdnEstimatedIncomeModels.Add(contractKdnEstimatedIncomeModel);
            }

            //здесь уже заполняем массив contractKdnEstimatedIncomeList данными по позиции
            if (contract.ContractClass != ContractClass.Tranche)
            {
                foreach (var position in contract.Positions)
                {
                    if (!contractKdnEstimatedIncomeList.Any(x => x.ContractPositionId == position.Id))
                    {
                        contractKdnEstimatedIncomeList.Add(new ContractKdnEstimatedIncome()
                        {
                            ContractId = contract.Id,
                            ContractPositionId = position.Id,
                            EstimatedIncome = (position.EstimatedCost / Constants.INCOME_COUNT_MONTHS),
                            PositionDetails = GetPositionDetails(contract, position),
                            PositionEstimatedIncome = position.EstimatedCost
                        });
                        totalEstimatedCost += (position.EstimatedCost / Constants.INCOME_COUNT_MONTHS);
                    }
                }
            }
            else
            {
                var creditLineContract = _contractService.Get(contract.CreditLineId.Value);
                if (creditLineContract != null)
                {
                    foreach (var position in creditLineContract.Positions)
                    {
                        if (!contractKdnEstimatedIncomeList.Any(x => x.ContractPositionId == position.Id))
                        {
                            contractKdnEstimatedIncomeList.Add(new ContractKdnEstimatedIncome()
                            {
                                ContractId = creditLineContract.Id,
                                ContractPositionId = position.Id,
                                EstimatedIncome = (contract.EstimatedCost / Constants.INCOME_COUNT_MONTHS),
                                PositionDetails = GetPositionDetails(creditLineContract, position),
                                PositionEstimatedIncome = contract.EstimatedCost
                            });
                            totalEstimatedCost += (contract.EstimatedCost / Constants.INCOME_COUNT_MONTHS);
                        }
                    }
                }
            }

            contractKdnEstimatedIncomeModel.TotalPositionsIncome = totalEstimatedCost;

            return contractKdnEstimatedIncomeModels;
        }

        private void AddOnlySubjectWithoutIncome(List<ContractKdnEstimatedIncomeModel> contractKdnEstimatedIncomeModels, ContractLoanSubject contractLoanSubject)
        {
            var subject = contractLoanSubject.Client;
            var subjectMerchantCode = _loanSubjectRepository.GetByCode("MERCHANT");
            if (subjectMerchantCode != null && contractLoanSubject.SubjectId != subjectMerchantCode.Id)
            {
                var contractKdnEstimatedIncomeList = new List<ContractKdnEstimatedIncome>();
                var contractKdnEstimatedIncomeModel = new ContractKdnEstimatedIncomeModel()
                {
                    ClientId = subject.Id,
                    IIN = subject.IdentityNumber,
                    IsSubject = true,
                    TotalPositionsIncome = 0,
                    FIO = subject.FullName,
                    ContractKdnEstimatedIncomes = contractKdnEstimatedIncomeList
                };
                contractKdnEstimatedIncomeModel.TotalPositionsIncome = 0;
                contractKdnEstimatedIncomeModels.Add(contractKdnEstimatedIncomeModel);
            }
        }

        public List<ContractKdnEstimatedIncomeModel> GetContractKdnEstimatedIncomeModels(Contract contract, User author)
        {
            var contractKdnEstimatedIncomeModels = new List<ContractKdnEstimatedIncomeModel>();

            GetClientIncomeFromAuto(contractKdnEstimatedIncomeModels, contract, false);

            //Ищем другие Договора клиента и добавляем их в contractKdnEstimatedIncomeModels
            var relatedContractsByClientId = _contractService.GetActiveContractsByClientId(contract.ClientId, contract.Id);
            relatedContractsByClientId.ForEach(x =>
            {
                GetClientIncomeFromAuto(contractKdnEstimatedIncomeModels, _contractService.FillPositions4Contract(x.Id), false);
            });

            //Ищем другие Договора созаемщика и добавляем их в contractKdnEstimatedIncomeModels
            contract.Subjects?.Where(x => x.Subject.Code == Constants.COBORROWER_CODE).ToList().ForEach(z =>
            {
                relatedContractsByClientId = _contractService.GetActiveContractsByClientId(z.ClientId);

                if (relatedContractsByClientId.Count > 0)
                {
                    relatedContractsByClientId.ForEach(x =>
                    {
                        GetClientIncomeFromAuto(contractKdnEstimatedIncomeModels, _contractService.FillPositions4Contract(x.Id), true);
                    });
                }
                else
                {
                    AddOnlySubjectWithoutIncome(contractKdnEstimatedIncomeModels, z);
                }
            });

            foreach (var item in contractKdnEstimatedIncomeModels)
            {
                var checksRessult = _gamblerService.GamblerFeatureCheck(item.ClientId, author).Result;
                item.IsGambler = checksRessult.IsGumbler;
                item.IsStopCredit = checksRessult.IsStopCredit;
            }

            return contractKdnEstimatedIncomeModels;
        }

        private Contract? GetChildContract(ContractAction? childAction)
        {
            if (childAction is null)
                throw new PawnshopApplicationException($"В метод GetChildContractId передан пустой параметр childAction");

            var childActionDb = _contractActionService.GetAsync(childAction.Id).Result;
            if (childActionDb is null)
                throw new PawnshopApplicationException($"Не найдена запись в ContractActions с Id {childAction.Id}");

            if (!childActionDb.FollowedId.HasValue)
                throw new PawnshopApplicationException($"Не заполнен FollowedId для действия с Id {childAction.Id}");

            var childContract = _contractService.Get(childActionDb.FollowedId.Value);
            if (childContract is null)
                throw new PawnshopApplicationException($"Не найден поражденный Договор с Id {childActionDb.FollowedId.Value}");

            var clientOtherPayments = childAction.ClientOtherPaymentsInfo;
            foreach (var item in clientOtherPayments)
            {
                _clientOtherPaymentsInfoService.UpdateContractId(item.Id, childContract.Id);
            }

            return childContract;
        }

        public ContractKdnModel FillKdnModel(Contract contract, User author, bool isAddition = false, ContractAction? childAction = null)
        {
            var contractKdnModel = new ContractKdnModel();
            contractKdnModel.AvgPaymentToday = _contractService.GetActiveContractsByClientId(contract.ClientId).Where(x=>x.ContractDate == DateTime.Now.Date && x.ContractClass != ContractClass.CreditLine).Select(x => (x.LoanCost + x.LoanPercentCost) / x.LoanPeriod).Sum();
            contractKdnModel.CalcKdn4Addition = isAddition;
            contractKdnModel.ChildContract = isAddition ? GetChildContract(childAction) : null;
            var processContract = !isAddition ? contract : contractKdnModel.ChildContract;

            if (IsKdnRequired(processContract))
            {
                contractKdnModel.IsKdnRequired = true;
                contractKdnModel.IsKdnPassed = false;
                contractKdnModel.Contract = processContract;

                //todo если это Добор и по нему передали SubjectId, тогда вытаскиваем Subject и добавляем его к processContract

                contractKdnModel.ContractKdnEstimatedIncomeModels = GetContractKdnEstimatedIncomeModels(processContract, author);
                foreach (var item in contractKdnModel.ContractKdnEstimatedIncomeModels)
                {
                    var checksRessult = _gamblerService.GamblerFeatureCheck(item.ClientId, author).Result;
                    item.IsGambler = checksRessult.IsGumbler;
                    item.IsStopCredit = checksRessult.IsStopCredit;
                }
                ContractCheckKdn(processContract, contractKdnModel, author, !isAddition ? (int?)null : contract.Id, isAddition);

                //Сохраняем расходы по другим договорам + на семью в анкету клиента
                SaveClientExpense(contractKdnModel);

                //здесь же можно сохранить данные в таблицу ContractKdnEstimatedIncomes
            }
            else
            {
                contractKdnModel.IsKdnRequired = false;
                contractKdnModel.IsKdnPassed = false;
            }

            //залогируем результат расчета КДН
            _eventLog.Log(EventCode.KdnCaclSucces, EventStatus.Success, EntityType.Contract, contract.Id, responseData: JsonConvert.SerializeObject(contractKdnModel), userId: author.Id);

            return contractKdnModel;
        }

        public bool IsKdnRequired(Contract contract)
        {
            var setting = _contractService.GetSettingForContract(contract);
            return setting.IsKdnRequired;
        }

        private bool CheckK1(ContractKdnModel contractKdnModel, List<string> validationErrors, List<string> kdnPassedMessage)
        {
            #region описание логики К1
            //К1 не должен быть больше 1.
            //K1 = ОРК / ОДК, где:
            //ОРК – общий расход Заемщика и Созаемщика на семью.
            //Общий расход Заемщика и Созаемщика на семью рассчитывается следующим образом:
            //ОРК = (ВПМ, если есть Созаемщик, то ВПМ * 2)+(ВПМ.детский * N, если есть созаемщик, ВПМ.детский* N детей Созаемщика), где
            //ВПМ – Величина прожиточного минимума, установленного на соответствующий финансовый год Законом РК
            //ВПМ.детский - половина величины прожиточного минимума, установленного на соответствующий финансовый год Законом РК.
            //N – общее количество несовершеннолетних членов семьи Заемщика и Созаемщика; (для уточнения обратиться к Бауржану)
            //В данном случае потребуется предусмотреть возможность ежегодной перезаписи в системе величины прожиточного минимума.
            //ОДК - общий доход Заемщика и Созаемщика. (При наличии признака Лудоман и/ или СУСН общий доход клиента не должен учитывать дополнительные доходы).
            #endregion

            contractKdnModel.ContractKdnEstimatedIncomeModels.ForEach(x =>
            {
                var client = _clientService.Get(x.ClientId);
                x.ReceivesASP = client.ReceivesASP;

                x.TotalFormalIncome = _clientIncomeService.GetTotalFormalIncome(x.ClientId);
                x.TotalInformalApprovedIncome = _clientIncomeService.GetTotalInformalApprovedIncome(x.ClientId);
                x.TotalInformalUnapprovedIncome = _clientIncomeService.GetTotalInformalUnapprovedIncome(x.ClientId);
                x.FamilyDebt = _clientIncomeService.GetFamilyDebt(x.ClientId);
            });

            decimal totalAspIncome = 0;
            foreach (var item in contractKdnModel.ContractKdnEstimatedIncomeModels)
            {
                if (item.ReceivesASP || item.IsGambler)
                {
                    totalAspIncome += item.TotalFormalIncome;
                }
                else
                {
                    totalAspIncome += item.TotalFormalIncome + item.TotalInformalApprovedIncome;
                }
            }

            var totalFamilyDebt = contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.FamilyDebt).Sum();

            bool isKdnInvalid = false;
            if (totalAspIncome <= 0)
            {
                isKdnInvalid = totalFamilyDebt > 1;
            }
            else
            {
                isKdnInvalid = totalFamilyDebt / totalAspIncome > 1;
            }

            if (isKdnInvalid)
                validationErrors.Add($"Первый шаг расчета коэффициента КДН не пройден: доход {totalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} меньше чем расход {totalFamilyDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} на содержание семьи.");
            else
                kdnPassedMessage.Add($"Первый шаг расчета коэффициента КДН пройден: доход {totalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} больше чем расход {totalFamilyDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} на содержание семьи.");

            contractKdnModel.TotalAspIncome = totalAspIncome;
            contractKdnModel.TotalFamilyDebt = totalFamilyDebt;
            contractKdnModel.K1Income = totalAspIncome + 10;
            return !isKdnInvalid;
        }

        private decimal GetDebtsFromContractSchedule(Contract contract)
        {
            if (contract is null)
                throw new NullReferenceException($"Договор не может быть NULL");

            var paymentsCount = _contractService.GetPaymentsCount(contract);

            return Math.Round((contract.PaymentSchedule.Select(x => x.DebtCost + x.PercentCost).Sum()) / paymentsCount, 2);
        }

        private decimal GetDebtsFromParentContractSchedule(List<ContractPaymentSchedule> contractPaymentSchedule)
        {
            if (contractPaymentSchedule is null)
                throw new NullReferenceException($"В методе GetDebtsFromParentContractSchedule график погашения не может быть NULL");

            var paymentsCount = contractPaymentSchedule.Count;

            return Math.Round((contractPaymentSchedule.Select(x => x.DebtCost + x.PercentCost).Sum()) / paymentsCount, 2);
        }

        private Contract GetContractClone(Contract contract)
        {
            var contractClone = contract.Clone();
            contractClone.Id = 0;
            contractClone.PaymentSchedule.Clear();

            return contractClone;
        }

        private void CreateSchedule4Contract(Contract contract)
        {
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

        private decimal GetDebtsFromContractVirtualSchedule(ContractKdnModel contractKdnModel, Contract contract)
        {
            if (contract is null)
                throw new NullReferenceException($"Договор не может быть NULL");

            var contractClone = GetContractClone(contract);
            contractClone.LoanCost = contractKdnModel.TotalAmount4Transh;

            var insuranceData = new InsuranceRequestData();
            var setting = _contractService.GetSettingForContract(contractClone);

            if (setting.IsInsuranceAvailable)
            {
                var insuranceCompany = setting.InsuranceCompanies.FirstOrDefault();
                insuranceData = _insurancePremiumCalculator.GetInsuranceDataV2(contractClone.LoanCost, insuranceCompany.InsuranceCompanyId, setting.Id);
            }
            contractClone.LoanCost = contractKdnModel.TotalAmount4Transh + insuranceData.InsurancePremium;

            CreateSchedule4Contract(contractClone);
            contractKdnModel.TotalAmount4TranshWithPremium = contractClone.LoanCost;
            contractKdnModel.VirtualPaymentSchedule = contractClone.PaymentSchedule;
            var paymentsCount = _contractService.GetPaymentsCount(contractClone);

            return Math.Round((contractClone.PaymentSchedule.Select(x => x.DebtCost + x.PercentCost).Sum()) / paymentsCount, 2);
        }

        private void LogFcbRequestAndResponse(FcbKdnRequest request, FcbKdnResponse response, int calcLogId, int clientId)
        {
            using (var transaction = _contractKdnRequestRepository.BeginTransaction())
            {
                var contractKdnRequest = new ContractKdnRequest()
                {
                    KdnCalculationId = calcLogId,
                    ClientId = clientId,
                    IncomeRequest = request.Income,
                    FCBRequestId = response.RequestId,
                    KDNScore = response.KdnScore,
                    Debt = response.Debt,
                    IncomeResponse = response.Income,
                    CorrelationId = response.CorrelationId,
                    RequestDate = request.RequestDate
                };
                _contractKdnRequestRepository.Insert(contractKdnRequest);

                transaction.Commit();
            }
        }

        private void ValidateFCBResponse(FcbKdnResponse fcbKdnResponse, List<string> validationErrors)
        {
            if (fcbKdnResponse.ErrorCode != -2004 && fcbKdnResponse.ErrorCode != 1102 && fcbKdnResponse.ErrorCode != 1 && fcbKdnResponse.ErrorCode != 2)
                validationErrors.Add($"Ошибка при обращении в ПКБ: {fcbKdnResponse.ErrorMessage}");

            if (fcbKdnResponse.ErrorCode == -2004 || fcbKdnResponse.ErrorCode == 1102 || fcbKdnResponse.ErrorCode == 2)
                fcbKdnResponse.Debt = 0;
        }

        private FcbKdnResponse? GetFcbKdnFromLocalStorage(ContractKdnEstimatedIncomeModel contractKdnModel)
        {
            var manualCalculationClientExpense = _manualCalculationClientExpenseService.GetByClientId(contractKdnModel.ClientId);
            if (manualCalculationClientExpense is null)
                return null;

            return new FcbKdnResponse()
            {
                Debt = manualCalculationClientExpense.Debt
            };
        }

        private FcbKdnResponse GetKdnFromFcb(ContractKdnEstimatedIncomeModel contractKdnModel, string author, int calcLogId, List<string> validationErrors)
        {
            //Поиск в таблице ManualCalculationClientExpenses
            var fcbKdnResponse = GetFcbKdnFromLocalStorage(contractKdnModel);
            if (fcbKdnResponse != null)
                return fcbKdnResponse;

            //Обращение к сервису ПКБ _fcb4Kdn при обращений сохраняем запрос, ответ в таблицы ContractKdnRequests + ContractKdnCalculationLogs
            var fcbKdnRequest = new FcbKdnRequest()
            {
                IIN = contractKdnModel.IIN,
                Income = contractKdnModel.TotalFormalIncome + contractKdnModel.TotalInformalApprovedIncome,
                Author = author,
                OrganizationId = Constants.OrganizationId,
                RequestDate = DateTime.Now
            };
            fcbKdnResponse = _fcb4Kdn.StorekdnReqWithIncome(fcbKdnRequest).Result;

            ValidateFCBResponse(fcbKdnResponse, validationErrors);

            //todo тут сохранеяем ответ в таблицу ContractKdnRequests
            LogFcbRequestAndResponse(fcbKdnRequest, fcbKdnResponse, calcLogId, contractKdnModel.ClientId);

            return fcbKdnResponse;
        }

        private void UpdateContractKdnCalculationLogs(int calcLogId, ContractKdnModel contractKdnModel, int authorId, int сontractId, int? parentContractId, int? settingId = null, int? additionalLoanPeriod = null, int? subjectId = null, int? positionEstimatedCost = null)
        {
            using (var transaction = _contractKdnRequestRepository.BeginTransaction())
            {
                bool IsGambler = contractKdnModel.ContractKdnEstimatedIncomeModels.Exists(item => item.IsGambler);
                bool IsStopCredit = contractKdnModel.ContractKdnEstimatedIncomeModels.Exists(item => item.IsStopCredit);

                var contractKdnCalculationLog = new ContractKdnCalculationLog()
                {
                    Id = calcLogId,
                    AuthorId = authorId,
                    IsKdnPassed = contractKdnModel.IsKdnPassed,
                    KdnError = string.Join(" ", contractKdnModel.KdnError),
                    TotalAspIncome = contractKdnModel.TotalAspIncome,
                    TotalContractDebts = contractKdnModel.TotalContractDebts,
                    TotalOtherClientPayments = contractKdnModel.TotalOtherClientPayments,
                    TotalIncome = contractKdnModel.TotalIncome,
                    TotalDebt = contractKdnModel.TotalDebt,
                    TotalFcbDebt = contractKdnModel.TotalFcbDebt,
                    TotalFamilyDebt = contractKdnModel.TotalFamilyDebt,
                    KDNCalculated = contractKdnModel.Kdn,
                    KDNK4Calculated = contractKdnModel.KdnK4,
                    ContractId = сontractId,
                    IsAddition = contractKdnModel.CalcKdn4Addition,
                    ParentContractId = parentContractId,
                    UpdateDate = DateTime.Now,
                    ChildSettingId = settingId,
                    ChildLoanPeriod = additionalLoanPeriod,
                    ChildSubjectId = subjectId,
                    PositionEstimatedCost = positionEstimatedCost,
                    IsGambler = IsGambler,
                    IsStopCredit = IsStopCredit,
                };
                _contractKdnCalculationLogRepository.UpdateKdnResultOnly(contractKdnCalculationLog);

                transaction.Commit();
            }
        }

        public ContractKdnCalculationLog GetContractKdnCalculationLog4AdditionDate(int contractId, DateTime additionDate)
        {
            var contractKdnCalculationLog = _contractKdnCalculationLogRepository.GetByParentContractId(contractId);
            if (contractKdnCalculationLog is null || contractKdnCalculationLog.UpdateDate.Date != additionDate.Date)
                throw new PawnshopApplicationException($"По Договору с Id {contractId} на дату Добора {additionDate} нет результата расчета КДН. Откройте форму Заявки на добор");

            return contractKdnCalculationLog;
        }

        public async Task<bool> IsKDNPassed(int contractId, bool isAddition)
        {
            var contractKdnCalculationLog = _contractKdnCalculationLogRepository.GetByContractId(contractId, isAddition);
            if (contractKdnCalculationLog is null)
                return false;
            if (!contractKdnCalculationLog.IsKdnPassed)
                return false;

            return true;
        }

        private int InsertToContractKdnCalculationLogs(int contractId, ContractKdnModel contractKdnModel, int authorId, int? parentContractId = null, bool isAddition = false)
        {
            var calcObj = !contractKdnModel.CalcKdn4Addition ? _contractKdnCalculationLogRepository.GetByContractId(contractId, isAddition) : _contractKdnCalculationLogRepository.GetByParentContractId(parentContractId.Value);
            int calcId;

            if (calcObj != null)
            {
                calcId = calcObj.Id;
            }
            else
            {
                using (var transaction = _contractKdnRequestRepository.BeginTransaction())
                {
                    bool IsGambler = contractKdnModel.ContractKdnEstimatedIncomeModels.Exists(item => item.IsGambler);
                    bool IsStopCredit = contractKdnModel.ContractKdnEstimatedIncomeModels.Exists(item => item.IsStopCredit);

                    var contractKdnCalculationLog = new ContractKdnCalculationLog()
                    {
                        ContractId = contractId,
                        KDNCalculated = 0,
                        CreateDate = DateTime.Now,
                        AuthorId = authorId,
                        IsAddition = contractKdnModel.CalcKdn4Addition,
                        UpdateDate = DateTime.Now,
                        IsGambler = IsGambler,
                        IsStopCredit = IsStopCredit,
                    };
                    _contractKdnCalculationLogRepository.Insert(contractKdnCalculationLog);
                    calcId = contractKdnCalculationLog.Id;

                    contractKdnModel.CalcLogId = calcId;

                    transaction.Commit();
                }
            }

            return calcId;
        }

        private bool CheckK2(ContractKdnModel contractKdnModel, Contract contract, List<string> validationErrors, string author, List<string> kdnPassedMessage, int calcLogId, int? parentContractId = null, bool isAddition = false)
        {
            #region описание логики К2
            //В настоящее время максимальное значение КДН в Финкор составляет 0,7(внутреннее правило в ТФГ)
            //По требованию законодательства максимальное значение с 01 / 04 / 2024 должно составлять 0,5(для Лудоманов - 0, 25)
            //- КДН не должен превышать коэффициент – 0,5
            //- КДН для лица, признанным Лудоманом коэффициент не должен превышать – 0,25
            //Расчет К2(КДН) состоит из двух этапов:

            //    1) Первый этап: Для начала требуется рассчитать сумму так называемого Ежемесячного платежа, которая будет участвовать в процесса расчета К2(КДН)
            //ЕП.дельта = ((ОД + %) - (ст - ть авто х 50 %)) / кол - во мес, где:
            //ЕП.дельта – ежемесячный платеж;
            //ОД – сумма основного долга по предполагаемому займу, на момент выдачи займа
            //% -сумма вознаграждения(процентов) за весь период кредитования по предполагаемому займу в соответствии с планируемым к получению займом;
            //Ст - ть авто – сумма стоимости авто, в которую оценили оценщики;
            //Кол - во мес - срок предполагаемого займа, выраженный в количестве месяцев.

            //    2) Второй этап: Расчет непосредственно самого К2(КДН)
            //КДН = (ДН + ЕП.дельта) / ОДК, где:
            //ДН – долговая нагрузка. Данную сумму получаем из отчета, который приходит из ПКБ в момент расчета КДН в текущей реализации.
            //ЕП.дельта – ежемесячный платеж, который рассчитан ранее по формуле.
            //ОДК – общий доход Заемщика и Созаемщика. (При наличии признака Лудоман и / или АСП(нужно получать из отчета СУСН) общий доход клиента не должен учитывать дополнительные доходы).
            //При этом, если ЕП.дельта будет менее нуля, то нужно оставить его равному нулю.
            //При наличии статуса Лудомана и / или статуса АСП(отчет СУСН) в качестве дохода в расчетах коэффициента может применяться только официальный доход из вкладки "Официальные доходы для расчета КДН"
            //При наличии статуса Лудоман и / или АСП(отчет СУСН) выводить сообщение в Алерте о наличии статуса
            #endregion

            var contractId4OtherPayments = contract.Id;
            var fcbErrors = new List<string>();
            foreach (var item in contractKdnModel.ContractKdnEstimatedIncomeModels)
            {
                var fcbResponse = GetKdnFromFcb(item, author, calcLogId, fcbErrors);
                if (fcbErrors.Count > 0)
                {
                    validationErrors.AddRange(fcbErrors);
                    contractKdnModel.TotalContractDebts = 0;
                    contractKdnModel.TotalOtherClientPayments = 0;
                    contractKdnModel.TotalFcbDebt = 0;
                    contractKdnModel.Kdn = 0;
                    return false;
                }

                item.FcbDebt = fcbResponse.Debt;

                item.OtherClientPayments = _clientOtherPaymentsInfoService.GetClientOtherPaymentsVal(contractId4OtherPayments, item.ClientId, fcbResponse.Debt, validationErrors);
            }

            decimal virtualTotalContractDepts;
            decimal totalContractDebts = 0;
            if (contractKdnModel.CalcKdn4Addition)
            {
                virtualTotalContractDepts = GetDebtsFromContractVirtualSchedule(contractKdnModel, contractKdnModel.ChildContract);
                totalContractDebts = !contractKdnModel.IsFirstTransh ? GetDebtsFromContractSchedule(contractKdnModel.ChildContract) : virtualTotalContractDepts;
            }
            else
            {
                virtualTotalContractDepts = GetDebtsFromContractVirtualSchedule(contractKdnModel, contract);
                totalContractDebts = !contractKdnModel.IsFirstTransh ? GetDebtsFromContractSchedule(contract) : virtualTotalContractDepts;
            }

            var totalOtherClientPayments = contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.OtherClientPayments).Sum();
            var totalFcbDebts = contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.FcbDebt).Sum();

            if (contractKdnModel.TotalAspIncome <= 0)
            {
                validationErrors.Add($"Второй шаг расчета коэффициента КДН не пройден: Общий доход {contractKdnModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} меньше или равно нулю. {Environment.NewLine}");
            }

            List<ContractPosition> positionList;
            if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditLineId = _contractService.GetCreditLineId(contract.Id).Result;
                positionList = _contractService.GetPositionsByContractId(creditLineId);
            }
            else
            {
                positionList = contract.Positions;
            }

            // https://bitrix.tascredit.kz/crm/type/134/details/669/
            // закон принят в силу
            var epDelta = totalContractDebts - ((positionList.Any() ? positionList.Select(x => x.EstimatedCost).Sum() * 0.7m : 0) / _contractService.GetPaymentsCount(contract));


            //"Изменено согласно заявки в Битриск от Коммерческого директора Анарбекова Г. Ссылка на заявку: https://bitrix.tascredit.kz/page/zapros_na_razrabotku/_zaprosy_na_razrabotku/type/134/details/537/"
            //if (epDelta < 0)
            //{
            //    epDelta = 0;
            //}

            contractKdnModel.K2Income = Math.Round(totalFcbDebts + epDelta) + contractKdnModel.AvgPaymentToday + 10;
            decimal calculatedKdn = 0;
            if (contractKdnModel.TotalAspIncome > 0)
                calculatedKdn = Math.Round((contractKdnModel.K2Income - totalOtherClientPayments) / contractKdnModel.TotalAspIncome, 2);
            else
                calculatedKdn = contractKdnModel.K2Income - totalOtherClientPayments;

            decimal kdn = _options.KDN;
            var domainValue = _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.BUSINESS_LOAN_PURPOSE);
            var kdnBusinessMessageMinMax = "";
            if (contractKdnModel.ContractKdnEstimatedIncomeModels.Any(x => x.IsGambler))
            {
                kdn = _options.KDNLowPriority;
            }
            else if (domainValue != null && contract != null && contract.LoanPurposeId == domainValue.Id)
            {
                kdn = _options.KDNBusiness;
                if (contract.LoanCost < Constants.BUSINESS_LOAN_PURPOSE_MINIMAL_SUM)
                {
                    kdnBusinessMessageMinMax = "Минимальная сумма займа должна быть - 1 000 000 тенге";
                    kdn = _options.KDN;
                }
                var mrpRateType = _domainService.GetDomainValue(Constants.NOTIONAL_RATE_TYPES, Constants.NOTIONAL_RATE_TYPES_MRP);
                if(mrpRateType != null)
                {
                    var mrpValue = _notionalRateRepository.GetByTypeOfLastYear(mrpRateType.Id);
                    if(mrpValue != null && contract.LoanCost > Constants.BUSINESS_LOAN_PURPOSE_MAXIMAL_MRP_MULTIPLIER * mrpValue.RateValue)
                    {
                        kdnBusinessMessageMinMax = $"Максимальная сумма займа на бизнес цели должна быть не более {Constants.BUSINESS_LOAN_PURPOSE_MAXIMAL_MRP_MULTIPLIER * mrpValue.RateValue} тенге";
                        kdn = _options.KDN;
                    }
                }
            }

            if (calculatedKdn > kdn)
            {
                validationErrors.Add($"Второй шаг расчета коэффициента КДН не пройден: текущее значение КДН = {calculatedKdn} > {kdn}.");
                if(kdnBusinessMessageMinMax != "")
                {
                    validationErrors.Add(kdnBusinessMessageMinMax);
                }
            }
                
            kdnPassedMessage.Add($"Второй шаг расчета коэффициента КДН пройден: текущее значение КДН = {calculatedKdn} <= {kdn}.");
            if (kdnBusinessMessageMinMax != "")
            {
                kdnPassedMessage.Add(kdnBusinessMessageMinMax);
            }

            contractKdnModel.TotalContractDebts = totalContractDebts;
            contractKdnModel.TotalOtherClientPayments = totalOtherClientPayments;
            contractKdnModel.TotalFcbDebt = totalFcbDebts;
            contractKdnModel.Kdn = calculatedKdn;
            contractKdnModel.EpDelta = epDelta;
            return calculatedKdn <= kdn;
        }

        private bool CheckK3(ContractKdnModel contractKdnModel, Contract contract, List<string> validationErrors, List<string> kdnPassedMessage)
        {
            #region Описание логики К3
            //К3 расчет, который требуется для определения общей платежеспособности Клиента
            //К3 должен быть меньше 1
            //К3 = ВРК / ОДК, где:
            //ВРК – сумма всех расходов Заемщика и Созаемщика, включая на семью(включая неработающих совершеннолетних) -рекомендую обратиться за уточнением к Бауржану, жилье и прочее;
            //ОДК – общий доход Заемщика и Созаемщика. (При наличии признака Лудоман и / или СУСН общий доход клиента не должен учитывать дополнительные доходы).
            #endregion

            contractKdnModel.ContractKdnEstimatedIncomeModels.ForEach(x =>
            {
                x.TotalFamilyDebt = _clientIncomeService.GetTotalFamilyDebt(x.ClientId);
                x.ClientExpenses = _clientIncomeService.GetClientExpenses(x.ClientId);
            });

            contractKdnModel.TotalDebt = contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.ClientExpenses + x.TotalFamilyDebt + x.FcbDebt).Sum() + contractKdnModel.TotalContractDebts - contractKdnModel.TotalOtherClientPayments;
            contractKdnModel.K3Income = Math.Round(contractKdnModel.TotalDebt + contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.TotalInformalUnapprovedIncome).Sum() + 10, 2);

            if (contractKdnModel.K3Income < contractKdnModel.TotalDebt)
                validationErrors.Add($"Третий шаг расчета коэффициента КДН не пройден: все доходы {contractKdnModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} < чем все расходы = {contractKdnModel.TotalDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))}");
            else
                kdnPassedMessage.Add($"Третий шаг расчета коэффициента КДН пройден: все доходы {contractKdnModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} >= чем все расходы = {contractKdnModel.TotalDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))}");

            return contractKdnModel.K3Income >= contractKdnModel.TotalDebt;
        }

        private bool CheckK4(ContractKdnModel contractKdnModel, Contract contract, List<string> validationErrors, List<string> kdnPassedMessage)
        {
            #region Описание логики K4
            //КДД = (СЗНЗ + СНЗ) / СДЗ <= 8
            //Расшифровка для формулы определения КДД:
            //СЗНЗ сумма задолженности по всем непогашенным банковским займам и микрокредитам заемщика.
            //СЗНЗ рассчитывается как совокупная сумма задолженности по всем непогашенным банковским займам и микрокредитам заемщика, включая суммы просроченных платежей по всем непогашенным банковским займам и микрокредитам, суммы по использованной части кредитного лимита, кредитной карте, а также по платежной карте, условиями которой предусмотрено кредитование заемщика в рамках кредитного лимита, а также 20(двадцать) процентов от неиспользованной части кредитного лимита, кредитной карты или платежной карты, условиями которых предусмотрено кредитование заемщика в рамках кредитного лимита;
            //СНЗ сумма по новой задолженности заемщика, возникающей в случаях, предусмотренных пунктом 13 Нормативов;
            //СДЗ совокупный доход заемщика, который рассчитывается как суммы заработной платы и(или) иных видов доходов заемщика, определенных на основании критериев, указанных в части второй пункта 6 Правил расчета и предельного значения коэффициента долговой нагрузки заемщика организации, осуществляющей микрофинансовую деятельность, утвержденных постановлением Правления Национального Банка Республики Казахстан от 28 ноября 2019 года № 215, зарегистрированных в Реестре государственной регистрации нормативных правовых актов под № 19670, за последние 6(шесть) месяцев, предшествующих дате обращения заемщика
            #endregion

            var hasAllValues = true;
            contractKdnModel.ContractKdnEstimatedIncomeModels.ForEach(x =>
            {
                x.ClientAllLoanExpense = _clientIncomeService.GetClientFullExpenses(x.ClientId)?.AllLoan;
            });

            foreach(var item in contractKdnModel.ContractKdnEstimatedIncomeModels)
            {
                if(item.IsSubject && !item.ClientAllLoanExpense.HasValue)
                {
                    hasAllValues = false;
                    validationErrors.Add($"Для расчета коэфицента КДД требуется заполнить в расходах клиента поле Сумма задолжности по всем непогашенным кредитам у субъекта с ИИН {item.IIN}");
                }
                else if(!item.IsSubject && !item.ClientAllLoanExpense.HasValue)
                {
                    hasAllValues = false;
                    validationErrors.Add($"Для расчета коэфицента КДД требуется заполнить в расходах клиента поле Сумма задолжности по всем непогашенным кредитам у клиента с ИИН {item.IIN}");
                }
            }

            if(!hasAllValues)
            {
                return false;
            }

            contractKdnModel.TotalAllLoan = contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.ClientAllLoanExpense.Value).Sum() + contract.LoanCost;
            contractKdnModel.K4Income = Math.Round(contractKdnModel.ContractKdnEstimatedIncomeModels.Select(x => x.TotalFormalIncome + x.TotalInformalApprovedIncome).Sum() * 12, 2);
            if (contractKdnModel.K4Income <= 0)
                contractKdnModel.KdnK4 = 10000;
            else
                contractKdnModel.KdnK4 = Math.Round(contractKdnModel.TotalAllLoan / contractKdnModel.K4Income, 2);

            if (contractKdnModel.KdnK4 <= _options.KDNK4)
                kdnPassedMessage.Add($"Четвертый шаг расчета пройден: текущее значение КДД = {contractKdnModel.KdnK4} <= {_options.KDNK4}."); 
            else
                kdnPassedMessage.Add($"Четвертый шаг расчета не пройден: текущее значение КДД = {contractKdnModel.KdnK4} > {_options.KDNK4}.");

            return true;
        }

        private void GetTranshInfo(Contract contract, ContractKdnModel contractKdnModel)
        {
            //вытаскиваем данные из ApplicationDetails и ContractPartialSign
            var transhInfoFromApplication = _applicationService.GetApplicationDetailsByContractId(contract.Id);
            if (transhInfoFromApplication != null)
            {
                contractKdnModel.IsFirstTransh = transhInfoFromApplication.IsFirstTransh;
                contractKdnModel.TotalAmount4Transh = transhInfoFromApplication.IsFirstTransh ? transhInfoFromApplication.TotalAmount4AllTransh.Value : 0;
            }
            else
            {
                var transhInfoFromPartialSign = _applicationService.GetContractPartialSign(contract.Id);
                if (transhInfoFromPartialSign != null)
                {
                    contractKdnModel.IsFirstTransh = true;
                    contractKdnModel.TotalAmount4Transh = transhInfoFromPartialSign.TotalAmount;
                }
            }
        }
            
        public void ContractCheckKdn(Contract contract, ContractKdnModel contractKdnModel, User author, int? parentContractId = null, bool isAddition = false)
        {
            var validationErrors = new List<string>();
            var kdnPassedMessage = new List<string>();

            //Создаем запись в ContractKdnCalculationLogs и передаем Id созданной записи в GetKdnFromFcb
            int calcLogId = InsertToContractKdnCalculationLogs(contract.Id, contractKdnModel, author.Id, parentContractId, isAddition: isAddition);

            //Получаем данные по траншу
            GetTranshInfo(contract, contractKdnModel);

            //Здесь проводим проверку КДН
            //1. Коэффициент достаточности дохода
            var kdnPart1 = CheckK1(contractKdnModel, validationErrors, kdnPassedMessage);

            //2. КДН (Коэффициент долговой нагрузки)
            var kdnPart2 = CheckK2(contractKdnModel, contract, validationErrors, author.Fullname, kdnPassedMessage, calcLogId, parentContractId, isAddition);

            //3. Внутренний коэффициент достаточности всех доходов
            var kdnPart3 = CheckK3(contractKdnModel, contract, validationErrors, kdnPassedMessage);

            //4. КДД(Коэфицент достаточности дохода)
            var kdnPar4 = CheckK4(contractKdnModel, contract, validationErrors, kdnPassedMessage);

            contractKdnModel.IsKdnPassed = kdnPart1 && kdnPart2 && kdnPart3 && kdnPar4;

            if (!contractKdnModel.IsKdnPassed)
            {
                string errors = string.Join(" ", validationErrors);
                validationErrors.Add($"Минимальный необходимый доход должен быть {CalculateRecomendedMinimalIncome(contractKdnModel).ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))}");
                contractKdnModel.KdnError = validationErrors;
            }
            else
            {
                string errors = string.Join(" ", kdnPassedMessage);
                contractKdnModel.KdnError = kdnPassedMessage;
            }
            string gamblerError = "";
            if (contractKdnModel.ContractKdnEstimatedIncomeModels.Any(x => x.IsGambler))
            {
                foreach (var item in contractKdnModel.ContractKdnEstimatedIncomeModels)
                {
                    if (item.IsGambler)
                    {
                        if (item.IsSubject)
                        {
                            gamblerError += "Субъект имеет статус Лудомана. ";
                        }
                        else
                        {
                            gamblerError += "Клиент имеет статус Лудомана. ";
                        }
                    }
                }
                gamblerError += $"Максимальный коэффициент КДН - {contractKdnModel.Kdn}";
                contractKdnModel.GamblerError = gamblerError;
            }

            if (contractKdnModel.ContractKdnEstimatedIncomeModels.Any(x => x.IsStopCredit))
            {
                foreach (var item in contractKdnModel.ContractKdnEstimatedIncomeModels)
                {
                    if (item.IsStopCredit)
                    {
                        if (item.IsSubject)
                        {
                            gamblerError += "Субъект имеет ограничение по получению информации в КБ. ";
                        }
                        else
                        {
                            gamblerError += "Клиент имеет ограничение по получению информации в КБ. ";
                        }
                    }

                }
                gamblerError += $"Статус \"STOP CREDIT\". ";
                contractKdnModel.GamblerError = gamblerError;
                contractKdnModel.IsKdnPassed = false;
            }

            UpdateContractKdnCalculationLogs(calcLogId, contractKdnModel, author.Id, contract.Id, parentContractId);
        }

        public bool PassContractToNextStep(ContractKdnModel contractKdnModel)
        {
            if (!contractKdnModel.IsKdnRequired)
                return true;
            if (contractKdnModel.IsKdnRequired && contractKdnModel.IsKdnPassed)
                return true;

            return false;
        }

        public void SaveClientExpense(ContractKdnModel contractKdnModel)
        {
            contractKdnModel.ContractKdnEstimatedIncomeModels.ForEach(x =>
            {
                _clientIncomeService.SaveClientExpense(x.ClientId, x.FcbDebt, x.FamilyDebt, contractKdnModel.AvgPaymentToday);
            });
        }

        public async Task<KdnCalculationLog> GetKDNMessage(int contractId)
        {
            var message = _contractKdnCalculationLogRepository.GetCalculationLogMessage(contractId);
            return new KdnCalculationLog() { KdnMessage = message };
        }

        private decimal CalculateRecomendedMinimalIncome(ContractKdnModel contractKdnModel)
        {
            decimal minK2Income = contractKdnModel.K2Income;
            if (contractKdnModel.Kdn == _options.KDN)
            {
                minK2Income = contractKdnModel.K2Income * 2;
            }
            else if (contractKdnModel.Kdn == _options.KDNLowPriority)
            {
                minK2Income = contractKdnModel.K2Income * 4;
            }
            return Math.Round(Math.Max(Math.Max(contractKdnModel.K1Income,
                minK2Income),
                contractKdnModel.K3Income), 2);
        }
    }
}
