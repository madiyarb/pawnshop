using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Bankruptcy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Services.ClientDeferments.Interfaces;

namespace Pawnshop.Services.Applications
{
    public class ApplicationService : IApplicationService
    {
        private const int DISCOUNT_FOR_CHANGE_CATEGORY = 8;

        private readonly ClientDocumentProviderRepository _clientDocumentProviderRepository;
        private readonly VehicleModelRepository _vehicleModelRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        private readonly CarRepository _carRepository;
        private readonly CountryRepository _countryRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ApplicationRepository _applicationRepository;
        private readonly ApplicationDetailsRepository _applicationDetailsRepository;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly BlackListReasonRepository _blackListReasonRepository;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly VehicleMarkRepository _vehicleMarkRepository;
        private readonly ContractRepository _contractRepository;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractAmount _contractAmount;
        private readonly IContractService _contractService;
        private readonly IVehcileService _vehcileService;
        private readonly IClientModelValidateService _clientModelValidateService;
        private readonly IEventLog _eventLog;
        private readonly ApplicationComparer _applicationComparer;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly PositionRepository _positionRepository;
        private readonly IInsurancePolicyService _insurancePolicyService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IContractDiscountService _contractDiscountService;
        private readonly ISessionContext _sessionContext;
        private readonly ContractCheckRepository _contractCheckRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly ContractPartialSignRepository _contractPartialSignRepository;
        private readonly IContractPeriodVehicleLiquidityService _contractPeriodVehicleLiquidityService;
        private readonly ApplicationMerchantRepository _applicationMerchantRepository;
        private readonly ContractLoanSubjectRepository _contractLoanSubjectRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly UserRepository _userRepository;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly ContractCreditLineAdditionalLimitsRepository _contractCreditLineAdditionalLimitsRepository;
        private readonly BankruptcyService _bankruptcyService;
        private readonly IClientDefermentService _clientDefermentService;

        public ApplicationService(ClientDocumentProviderRepository clientDocumentProviderRepository,
            VehicleModelRepository vehicleModelRepository, ClientLegalFormRepository clientLegalFormRepository,
            CarRepository carRepository, CountryRepository countryRepository, ClientRepository clientRepository,
            ApplicationRepository applicationRepository, VehicleBlackListRepository vehicleBlackListRepository,
            BlackListReasonRepository blackListReasonRepository, IClientBlackListService clientBlackListService,
            VehicleMarkRepository vehicleMarkRepository, ContractRepository contractRepository,
            IContractDutyService contractDutyService, IContractAmount contractAmount, IContractService contractService, IVehcileService vehcileService,
            IClientModelValidateService clientModelValidateService, IEventLog eventLog, ApplicationDetailsRepository applicationDetailsRepository,
            ApplicationComparer applicationComparer, LoanPercentRepository loanPercentRepository, IInsurancePremiumCalculator insurancePremiumCalculator,
            PositionRepository positionRepository, IInsurancePolicyService insurancePolicyService, IInsurancePoliceRequestService insurancePoliceRequestService,
            IContractDiscountService contractDiscountService, ISessionContext sessionContext, ContractCheckRepository contractCheckRepository,
            CashOrderRepository cashOrderRepository, ContractPartialSignRepository contractPartialSignRepository,
            IContractPeriodVehicleLiquidityService contractPeriodVehicleLiquidityService,
            ApplicationMerchantRepository applicationMerchantRepository,
            ContractLoanSubjectRepository contractLoanSubjectRepository,
            LoanSubjectRepository loanSubjectRepository,
            UserRepository userRepository,
            IContractActionRowBuilder contractActionRowBuilder,
            ContractCreditLineAdditionalLimitsRepository contractCreditLineAdditionalLimitsRepository,
            BankruptcyService bankruptcyService,
            IClientDefermentService clientDefermentService)
        {
            _clientDocumentProviderRepository = clientDocumentProviderRepository;
            _vehicleModelRepository = vehicleModelRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
            _carRepository = carRepository;
            _countryRepository = countryRepository;
            _clientRepository = clientRepository;
            _applicationRepository = applicationRepository;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _blackListReasonRepository = blackListReasonRepository;
            _clientBlackListService = clientBlackListService;
            _vehicleMarkRepository = vehicleMarkRepository;
            _contractRepository = contractRepository;
            _contractDutyService = contractDutyService;
            _contractAmount = contractAmount;
            _contractService = contractService;
            _vehcileService = vehcileService;
            _clientModelValidateService = clientModelValidateService;
            _eventLog = eventLog;
            _applicationDetailsRepository = applicationDetailsRepository;
            _applicationComparer = applicationComparer;
            _loanPercentRepository = loanPercentRepository;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _positionRepository = positionRepository;
            _insurancePolicyService = insurancePolicyService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _contractDiscountService = contractDiscountService;
            _sessionContext = sessionContext;
            _contractCheckRepository = contractCheckRepository;
            _cashOrderRepository = cashOrderRepository;
            _contractPartialSignRepository = contractPartialSignRepository;
            _contractPeriodVehicleLiquidityService = contractPeriodVehicleLiquidityService;
            _applicationMerchantRepository = applicationMerchantRepository;
            _contractLoanSubjectRepository = contractLoanSubjectRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _userRepository = userRepository;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractCreditLineAdditionalLimitsRepository = contractCreditLineAdditionalLimitsRepository;
            _bankruptcyService = bankruptcyService;
            _clientDefermentService = clientDefermentService;
        }

        public void Validate(ApprovedContract model)
        {
            _vehcileService.BodyNumberValidate(model.BodyNumber);
            _vehcileService.TechPassportNumberValidate(model.TechPassportNumber);
            _vehcileService.TechPassportDateValidate(model.TechPassportDate);
            _vehcileService.TransportNumberValidate(model.TransportNumber);
            _vehcileService.ReleaseYearValidate(model.ReleaseYear);
        }

        public bool Save(ApprovedContract model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var vehicleModel = _vehicleModelRepository.Get(model.ModelId);

            if (vehicleModel is null)
                throw new PawnshopApplicationException("Модель не найдена");

            var providerType = _clientDocumentProviderRepository.Find(new { Code = model.DocumentProviderCode });

            if (providerType is null)
                throw new PawnshopApplicationException("Орган выдачи не найден");

            var documentType = providerType.PossibleDocumentTypes.FirstOrDefault(t => t.Code == model.DocumentTypeCode);

            if (documentType is null)
                throw new PawnshopApplicationException("Тип документа не найден");

            if (model.IsAddition && model.ParentContractId == null)
                throw new PawnshopApplicationException("Заявка на добор не содержит Id родительского Договора");

            var clientLegalForm = _clientLegalFormRepository.Find(new { Code = "INDIVIDUAL" });

            var client = _clientRepository.FindByIdentityNumber(model.IdentityNumber);

            if (client == null)
            {
                client = GetClient(model);
                _clientModelValidateService.ValidateMobileAppClientModel(client);
            }
            else
            {
                var deferments = _clientDefermentService.GetActiveDeferments(client.Id);
                if(deferments.Any())
                    throw new PawnshopApplicationException("Клиент имеет отсрочку по действующему контракту/действующим контрактам, выдача запрещена");
            }

            var car = _carRepository.Find(new { model.TechPassportNumber }) ?? new Car()
            {
                Name = model.TransportNumber,
                Mark = vehicleModel.VehicleMark.Name.ToUpper(),
                Model = vehicleModel.Name.ToUpper(),
                ReleaseYear = model.ReleaseYear,
                TransportNumber = model.TransportNumber,
                BodyNumber = model.BodyNumber,
                TechPassportNumber = model.TechPassportNumber,
                Color = model.Color,
                TechPassportDate = model.TechPassportDate,
                VehicleMarkId = vehicleModel.VehicleMark.Id,
                VehicleModelId = vehicleModel.Id
            };

            var position = car.Id > 0 ? _positionRepository.Get(car.Id) : null;
            if (car.Id > 0 && position == null)
                throw new NullReferenceException($"Для автомобиля с Id {car.Id} не найден соответствующий Position");

            if (!(model.MotorCost > 0))
                throw new PawnshopApplicationException("Одобренная сумма \"Без права вождения\" должна быть больше нуля");

            if (!model.WithoutDriving && (model.LightCost > model.MotorCost || model.TurboCost > model.MotorCost))
                throw new PawnshopApplicationException("Одобренная сумма \"С правом вождения\" не должна быть больше чем \"Без права вождения\"");

            if (model.ParentContractId.HasValue && _contractService.GetOnlyContract(model.ParentContractId.Value).ContractClass == ContractClass.CreditLine)
            {
                var parentApplication = _applicationRepository.FindByContractId(model.ParentContractId.Value);
                if (parentApplication != null && (model.MotorCost > parentApplication.MotorCost || 
                    model.LightCost > parentApplication.MotorCost || model.TurboCost > parentApplication.MotorCost))
                {
                    throw new NullReferenceException($"Оценка автомобиля в заявке больше чем в заявке на кредитную линию");
                }
            }

            if (model.IsAddition)
            {
                var contract = _contractService.Get((int)model.ParentContractId);
                if (contract == null)
                    throw new NullReferenceException($"Договор с Id {model.ParentContractId} не найден");

                if (client.Id == 0)
                    throw new PawnshopApplicationException($"Не найден клиент для ИИН {model.IdentityNumber} из Заявки при доборе");

                if (car.Id == 0)
                    throw new PawnshopApplicationException($"Не найден автомобиль для техпаспорта {model.TechPassportNumber} из Заявки при доборе");

                if (client.Id > 0 && contract.ClientId != client.Id)
                    throw new PawnshopApplicationException($"Клиент на Договоре с ClientId {contract.ClientId} не соотвтетствет клиенту из Заявки с ИИН {model.IdentityNumber}");

                if (car.Id > 0 && position.ClientId != client.Id)
                    throw new PawnshopApplicationException($"Клиент на Позиции с ClientId {contract.ClientId} не соотвтетствет клиенту из Заявки с ИИН {model.IdentityNumber}");

                if (contract.SettingId.HasValue)
                {
                    var productSettings = _contractService.GetProductSettings(contract.SettingId.Value);

                    if (productSettings != null && productSettings.IsProduct && !productSettings.AdditionAvailable)
                    {
                        _eventLog.Log(EventCode.ContractAddition, EventStatus.Failed, EntityType.Contract, contract.Id, null, null);
                        throw new PawnshopApplicationException($"По Договору {contract.Id} запрешен Добор согласно настройкам продукта");
                    }
                }

                if (!contract.Positions.Any())
                    throw new PawnshopApplicationException($"Для родительского Договора с Id {contract.Id} массив позиции пуст");

                if (contract.Positions.First().PositionId != car.Id)
                    throw new PawnshopApplicationException($"Для родительского Договора с Id {contract.Id} не совпадают позиции из Заявки car.Id={car.Id}");
            }

            var application = _applicationRepository.FindByAppId(model.AppId);
            bool isNewApplication = application is null;

            if (isNewApplication)
                application = new Application();

            application.AuthorId = model.AuthorId;
            application.Client = client;
            application.Position = car;
            application.AppId = model.AppId;
            application.ApplicationDate = model.ApplicationDate;
            application.DebtorsRegisterSum = model.DebtorsRegisterSum;
            application.EstimatedCost = model.EstimatedCost;
            application.PrePayment = model.PrePayment;
            application.LightCost = model.LightCost;
            application.LimitSum = model.LimitSum;
            application.MotorCost = model.MotorCost;
            application.RequestedSum = (int)(model.PrePayment > 0 ? model.EstimatedCost - model.PrePayment : model.RequestedSum);
            application.TurboCost = model.TurboCost;
            application.Status = ApplicationStatus.New;
            application.ManagerGuarentee = model.ManagerGuarentee;
            application.WithoutDriving = model.WithoutDriving;
            application.IsAddition = model.IsAddition;
            application.ParentContractId = model.ParentContractId;
            application.BitrixId = model.BitrixId;
            application.IsAutocredit = model.IsAutocredit;
            application.ContractClass = model.ContractClass;

            ApplicationMerchant applicationMerchant = _applicationMerchantRepository.FindByIdentityNumber(model.MerchantIdentityNumber);
            bool isMerchantNew = applicationMerchant is null;

            if (application.IsAutocredit == 1)
            {
                if (isMerchantNew)
                    applicationMerchant = new ApplicationMerchant();

                applicationMerchant.Name = model.MerchantName;
                applicationMerchant.Surname = model.MerchantSurname;
                applicationMerchant.MiddleName = model.MerchantMiddleName;
                applicationMerchant.BirthDay = model.MerchantBirthDay;
                applicationMerchant.DocumentTypeCode = model.MerchantDocumentTypeCode;
                applicationMerchant.BirthOfPlace = model.BirthPlace;
                applicationMerchant.IdentityNumber = model.MerchantIdentityNumber;
                applicationMerchant.LicenseNumber = model.MerchantLicenseNumber;
                applicationMerchant.LicenseDateOfIssue = model.MerchantLicenseDate;
                applicationMerchant.LicenseDateOfEnd = model.MerchantLicenseDateExpire;
                applicationMerchant.LicenseIssuer = model.MerchantLicenseProvider;
                applicationMerchant.DefinitionLegalPerson = model.DefinitionLegalPerson;
                applicationMerchant.IsAutocredit = model.IsAutocredit;

                application.ApplicationMerchant = applicationMerchant;
            }

            using (var transaction = _applicationRepository.BeginTransaction())
            {
                if (client.Id == 0)
                {
                    _clientRepository.Insert(client);
                    _eventLog.Log(EventCode.ClientSaved, EventStatus.Success, EntityType.Client, client.Id, null, null);
                }


                if (car.Id == 0)
                {
                    car.ClientId = client.Id;
                    _carRepository.Insert(car);
                    _eventLog.Log(EventCode.DictCarSaved, EventStatus.Success, EntityType.Position, car.Id, null, null);
                }

                if (application.IsAutocredit == 1)
                {
                    if (isMerchantNew)
                    {
                        _applicationMerchantRepository.Insert(applicationMerchant);
                        _eventLog.Log(EventCode.ApplicationMerchantSaved, EventStatus.Success, EntityType.Application, applicationMerchant.Id, null, null);
                    }
                    else
                    {
                        _applicationMerchantRepository.Update(applicationMerchant);
                        _eventLog.Log(EventCode.ApplicationMerchantUpdated, EventStatus.Success, EntityType.Application, applicationMerchant.Id, null, null);
                    }
                }

                application.ClientId = client.Id;
                application.PositionId = car.Id;

                if (application.IsAutocredit == 1)
                    application.ApplicationMerchantId = applicationMerchant.Id;

                if (isNewApplication)
                {
                    _applicationRepository.Insert(application);
                    _eventLog.Log(EventCode.ApplicationSaved, EventStatus.Success, EntityType.Application, application.Id, null, null);
                }
                else
                {
                    _applicationRepository.UpdateByAppId(application);
                    _eventLog.Log(EventCode.ApplicaitonUpdatedByApId, EventStatus.Success, EntityType.Application, application.Id, null, null);
                }

                transaction.Commit();
            }
            _bankruptcyService.CheckIndividualClient(client.IdentityNumber).Wait();

            return application.Id > 0;
        }

        private ClientDocument GetDocument(IApplicationModel model, int providerTypeId, ClientDocumentType documentType)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            if (!model.DocumentDate.HasValue)
                throw new PawnshopApplicationException("Не указана дата выдачи документа");

            if (!model.DocumentDateExpire.HasValue)
                throw new PawnshopApplicationException("Не указан срок окончания действия документа");

            return new ClientDocument()
            {
                Date = model.DocumentDate.Value,
                DateExpire = model.DocumentDateExpire.Value,
                Number = model.DocumentNumber,
                ProviderId = providerTypeId,
                TypeId = documentType.Id,
                DocumentType = documentType,
                BirthPlace = model.BirthPlace,
                CreateDate = DateTime.Now,
                AuthorId = model.AuthorId
            };
        }

        private Client GetClient(IApplicationModel model)
        {
            var providerType = _clientDocumentProviderRepository.Find(new { Code = model.DocumentProviderCode });

            if (providerType is null)
                throw new PawnshopApplicationException("Орган выдачи не найден");

            var documentType = providerType.PossibleDocumentTypes.FirstOrDefault(t => t.Code == model.DocumentTypeCode);

            if (documentType is null)
                throw new PawnshopApplicationException("Тип документа не найден");

            var clientLegalForm = _clientLegalFormRepository.Find(new { Code = "INDIVIDUAL" });

            var result = int.TryParse(model.IdentityNumber.Substring(6, 1), out int isMaleNumber);

            if (!result)
                throw new PawnshopApplicationException("ИИН указан не корректно, не удалось определить пол");

            var isResident = documentType.Code == "IDENTITYCARD" || documentType.Code == "PASSPORTKZ";
            var country = _countryRepository.Find(new { Code = isResident ? "KAZ" : model.CountryCode });

            if (country is null)
                throw new PawnshopApplicationException("Страна гражданства не найдена");

            if (!model.BirthDay.HasValue)
                throw new PawnshopApplicationException("Не указана дата рождения");

            return new Client()
            {
                CardType = CardType.Standard,
                IdentityNumber = model.IdentityNumber,
                FullName = $"{model.SurName} {model.Name} {model.Patronymic}",
                LegalFormId = clientLegalForm.Id,
                LegalForm = clientLegalForm,
                Name = model.Name,
                Patronymic = model.Patronymic,
                Surname = model.SurName,
                BirthDay = model.BirthDay,
                CreateDate = DateTime.Now,
                AuthorId = model.AuthorId,
                CitizenshipId = country.Id,
                Citizenship = country,
                IsResident = isResident,
                IsMale = isMaleNumber == 3 || isMaleNumber == 5,
                Documents = new List<ClientDocument>()
                {
                    GetDocument(model, providerType.Id, documentType)
                }
            };
        }

        public SameContractList GetSameContracts(MobileAppModel mobileAppModel)
        {
            return _applicationRepository.GetSameContracts(mobileAppModel);
        }

        public bool SendCarOrClientToBlackList(ModelForBlackList model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            VehiclesBlackListItem carInBlackList = new VehiclesBlackListItem();
            ClientsBlackList clientInBlackList = new ClientsBlackList();

            if (model.BlackListSendType == BlackListSendType.Both || model.BlackListSendType == BlackListSendType.Car)
            {
                var reason = _blackListReasonRepository.Find(new { Name = model.CarReason });
                if (reason is null)
                    throw new PawnshopApplicationException("Не указана причина");

                carInBlackList = _vehicleBlackListRepository.Find(model);

                if (carInBlackList != null)
                    carInBlackList.ReasonId = reason.Id;
                else
                {
                    carInBlackList = new VehiclesBlackListItem()
                    {
                        CreateDate = DateTime.Now,
                        AuthorId = model.AuthorId,
                        BodyNumber = model.BodyNumber,
                        Number = model.TransportNumber,
                        ReasonId = reason.Id
                    };

                }

                using (var transaction = _vehicleBlackListRepository.BeginTransaction())
                {
                    if (carInBlackList.Id > 0)
                        _vehicleBlackListRepository.Update(carInBlackList);
                    else
                        _vehicleBlackListRepository.Insert(carInBlackList);

                    transaction.Commit();
                }

            }
            if (model.BlackListSendType == BlackListSendType.Both || model.BlackListSendType == BlackListSendType.Client)
            {
                foreach (var clientReason in model.ClientReasonList)
                {
                    var reason = _blackListReasonRepository.Find(new { Name = clientReason });
                    if (reason is null)
                        throw new PawnshopApplicationException("Не указана причина");

                    var client = _clientRepository.FindByIdentityNumber(model.IdentityNumber);

                    if (client is null)
                        client = GetClient(model);

                    clientInBlackList = _clientBlackListService.Find(new { ClientId = client.Id, ReasonId = reason.Id });

                    using (var transaction = _clientRepository.BeginTransaction())
                    {
                        if (client.Id == 0)
                            _clientRepository.Insert(client);

                        if (clientInBlackList is null)
                        {
                            clientInBlackList = new ClientsBlackList()
                            {
                                ClientId = client.Id,
                                ReasonId = reason.Id,
                                AddReason = reason.Name,
                                AddedBy = model.AuthorId,
                                AddedAt = DateTime.Now
                            };

                            _clientBlackListService.Save(clientInBlackList);
                        }

                        transaction.Commit();
                    }
                }
            }

            return
                model.BlackListSendType == BlackListSendType.Both ?
                    carInBlackList.Id > 0 && clientInBlackList.Id > 0 : model.BlackListSendType == BlackListSendType.Car ?
                        carInBlackList.Id > 0 : clientInBlackList.Id > 0;
        }

        public List<string> CheckForBlackList(MobileAppModel mobileAppModel)
        {
            var blackListReasons = new List<string>();

            if (mobileAppModel is null)
                throw new ArgumentNullException(nameof(mobileAppModel));

            if (string.IsNullOrWhiteSpace(mobileAppModel.BodyNumber) && string.IsNullOrWhiteSpace(mobileAppModel.IdentityNumber))
                throw new PawnshopApplicationException("Укажите вин-код или ИИН");

            if (!string.IsNullOrWhiteSpace(mobileAppModel.BodyNumber))
            {
                var carInBlackList = _vehicleBlackListRepository.Find(mobileAppModel);
                if (carInBlackList != null)
                    blackListReasons.Add(carInBlackList.BlackListReason.Name);
            }

            if (!string.IsNullOrWhiteSpace(mobileAppModel.IdentityNumber))
            {
                var client = _clientRepository.FindByIdentityNumber(mobileAppModel.IdentityNumber);

                if (client is null)
                    throw new PawnshopApplicationException("Клиент не найден");

                var clientInBlackList = _clientBlackListService.GetClientsBlackListsByClientId(client.Id);

                if (clientInBlackList.Any())
                    clientInBlackList.ForEach(t => blackListReasons.Add(t.BlackListReason.Name));
            }

            return blackListReasons;
        }

        public List<VehicleMark> GetVehicleMarks() => _vehicleMarkRepository.GetVehicleMarks();

        public List<VehicleModel> GetVehicleModelsByMarkId(int markId) => _vehicleModelRepository.GetVehicleModelsByMarkId(markId);

        public async Task<List<ContractDataForAddition>> GetContractDataForAdditionAsync(string bodyNumber, string identityNumber, string techPassportNumber)
        {
            if (string.IsNullOrWhiteSpace(bodyNumber))
                throw new PawnshopApplicationException("Номер вин-кода пустой");

            var contractPosition = _contractRepository.FindContractPositionByBodyNumber(bodyNumber);

            ValidateContractDataForAddition(contractPosition, identityNumber, techPassportNumber, bodyNumber, true);

            var contract = contractPosition.Contract;
            var car = _carRepository.Get(contractPosition.PositionId);

            (IEnumerable<ContractActionRow> rows, decimal buyoutCost) = CalculateBuyoutCost(contract);

            List<ContractDataForAddition> contracts = new List<ContractDataForAddition>
            {
                CreateContractDataForAddition(contract, car, rows, buyoutCost)
            };

            var ContractTranches = (await _contractService.GetAllTranchesAsync(contract.Id)).ToList()
                ?? throw new PawnshopApplicationException("По текущему договору не найден транш");

            foreach (var contractTranche in ContractTranches)
            {
                if (contractTranche.Status == ContractStatus.BoughtOut)
                    continue;

                var tranche = _contractRepository.Get(contractTranche.Id);
                var trancheCar = _carRepository.Get(contractPosition.PositionId);

                (IEnumerable<ContractActionRow> trancheRows, decimal trancheBuyoutCost) = CalculateBuyoutCost(tranche);

                contracts.Add(CreateContractDataForAddition(tranche, trancheCar, trancheRows, trancheBuyoutCost));
            }

            return contracts;
        }

        private (IEnumerable<ContractActionRow>, decimal) CalculateBuyoutCost(Contract contract)
        {
            var contractDuty = _contractDutyService.GetContractDuty(new ContractDutyCheckModel()
            {
                ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                ContractId = contract.Id,
                Date = DateTime.Now
            });

            decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id, DateTime.Now);
            IEnumerable<ContractActionRow> rows = contractDuty.Rows.Where(t => t.DebitAccountId.HasValue);
            decimal buyoutCost = rows.Any() ? rows.Sum(t => t.Cost) + contractDuty.ExtraExpensesCost - depoBalance : 0;

            return (rows, buyoutCost);
        }

        private ContractDataForAddition CreateContractDataForAddition(Contract contract, Car car, IEnumerable<ContractActionRow> rows, decimal buyoutCost)
        {
            return new ContractDataForAddition()
            {
                ContractNumber = contract.ContractNumber,
                LoanCost = contract.LoanCost,
                PercentCost = rows.Any() ? rows.Where(t => t.PaymentType == AmountType.Loan || t.PaymentType == AmountType.OverdueLoan).Sum(t => t.Cost) : 0,
                PenyCost = rows.Any() ? rows.Where(t => t.PaymentType == AmountType.DebtPenalty || t.PaymentType == AmountType.LoanPenalty).Sum(t => t.Cost) : 0,
                DebtLeftCost = rows.Any() ? rows.Where(t => t.PaymentType == AmountType.Debt || t.PaymentType == AmountType.OverdueDebt).Sum(t => t.Cost) : 0,
                BuyoutCost = buyoutCost > 0 ? buyoutCost : 0,
                DelayDayCount = _contractAmount.CalculatePenaltyDays(contract).Item1,
                Car = $"{car.VehicleMark.Name} {car.VehicleModel.Name} {car.TransportNumber}",
                EstimatedCost = contract.EstimatedCost,
                ParentContractId = contract.Id
            };
        }

        public async Task<List<ContractDataForTranche>> GetContractDataForTrancheAsync(string bodyNumber, string identityNumber, string techPassportNumber)
        {
            if (string.IsNullOrWhiteSpace(bodyNumber))
                throw new PawnshopApplicationException("Номер вин-кода пустой");

            var contractPosition = _contractRepository.FindContractPositionByBodyNumber(bodyNumber, techPassportNumber, true);

            if (contractPosition is null)
                throw new PawnshopApplicationException($"По вин-коду {bodyNumber} отсутствует действующий договор");

            ValidateContractDataForAddition(contractPosition, identityNumber, techPassportNumber, bodyNumber, true);

            var contract = contractPosition.Contract;
            var car = _carRepository.Get(contractPosition.PositionId);

            decimal MainDebt = 0;
            decimal BuyoutAmount = 0;
            int DelayDayCount = 0;

            var ContractTranchesInfoList = _contractService.GetTranches(contract.Id);

            if (ContractTranchesInfoList is null)
                throw new PawnshopApplicationException("По текущему договору не найден транш");

            foreach (var tranche in ContractTranchesInfoList)
            {
                BuyoutAmount += tranche.TotalRedemptionAmount;
                MainDebt += tranche.UrgentDebt.PrincipalDebt;
            }

            var TranchesList = (await _contractService.GetAllTranchesAsync(contract.Id)).ToList();

            if (TranchesList is null)
                throw new PawnshopApplicationException("По текущему договору не найден транш");

            foreach (var tranche in TranchesList)
            {
                var contractTranche = _contractRepository.Get(tranche.Id);
                int PenaltyDays = _contractAmount.CalculatePenaltyDays(contractTranche).Item1;
                DelayDayCount = PenaltyDays > DelayDayCount ? PenaltyDays : DelayDayCount;
            }

            return new List<ContractDataForTranche>()
            {
                new ContractDataForTranche()
                {
                    ParentContractId = contract.Id,
                    Car = $"{car.VehicleMark.Name} {car.VehicleModel.Name} {car.TransportNumber}",
                    ContractNumber = contract.ContractNumber,
                    LoanCost = contract.LoanCost,
                    DebtLeftCost = contract.LoanCost - MainDebt,
                    MainDebt = MainDebt,
                    BuyoutAmount = BuyoutAmount,
                    DelayDayCount = DelayDayCount,
                    EstimatedCost = contract.EstimatedCost,
                }
            };
        }

        public ContractDataForAddition GetContractDataForAddition(string bodyNumber)
        {
            if (string.IsNullOrWhiteSpace(bodyNumber))
                throw new PawnshopApplicationException("Номер вин-кода пустой");

            var contractPosition = _contractRepository.FindContractPositionByBodyNumber(bodyNumber);

            if (contractPosition is null)
                throw new PawnshopApplicationException("Действующие договора по данному вин-коду не найдены");

            var contract = contractPosition.Contract;
            var car = _carRepository.Get(contractPosition.PositionId);

            var contractDuty = _contractDutyService.GetContractDuty(new ContractDutyCheckModel()
            {
                ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                ContractId = contract.Id,
                Date = DateTime.Now
            });

            decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id, DateTime.Now);
            var rows = contractDuty.Rows.Where(t => t.DebitAccountId.HasValue);
            decimal buyoutCost = rows.Any() ? rows.Sum(t => t.Cost) + contractDuty.ExtraExpensesCost - depoBalance : 0;

            return new ContractDataForAddition()
            {
                ContractNumber = contract.ContractNumber,
                LoanCost = contract.LoanCost,
                PercentCost = rows.Any() ? rows.Where(t => t.PaymentType == AmountType.Loan || t.PaymentType == AmountType.OverdueLoan).Sum(t => t.Cost) : 0,
                PenyCost = rows.Any() ? rows.Where(t => t.PaymentType == AmountType.DebtPenalty || t.PaymentType == AmountType.LoanPenalty).Sum(t => t.Cost) : 0,
                DebtLeftCost = rows.Any() ? rows.Where(t => t.PaymentType == AmountType.Debt || t.PaymentType == AmountType.OverdueDebt).Sum(t => t.Cost) : 0,
                BuyoutCost = buyoutCost > 0 ? buyoutCost : 0,
                DelayDayCount = _contractAmount.CalculatePenaltyDays(contract).Item1,
                Car = $"{car.VehicleMark.Name} {car.VehicleModel.Name} {car.TransportNumber}",
                EstimatedCost = contract.EstimatedCost,
                ParentContractId = contract.Id
            };
        }

        private void ValidateContractDataForAddition(ContractPosition contractPosition, string identityNumber, string techPassportNumber, string bodyNumber, bool isTrancheInCreditLine = false)
        {
            if (contractPosition is null)
                throw new PawnshopApplicationException($"По вин-коду {bodyNumber} отсутствует действующий договор");

            var contract = contractPosition.Contract;
            var car = _carRepository.Get(contractPosition.PositionId);
            contract.Client = _clientRepository.GetOnlyClient(contract.ClientId);

            if (isTrancheInCreditLine == false)
            {
                if (contract.SettingId.HasValue)
                {
                    var productSettings = _contractService.GetProductSettings(contract.SettingId.Value);

                    if (productSettings != null && productSettings.IsProduct && !productSettings.AdditionAvailable)
                    {
                        throw new PawnshopApplicationException($"По Договору {contract.ContractNumber} запрещен Добор согласно настройкам продукта");
                    }
                }
            }

            var error = String.Empty;

            if (!contract.Client.IdentityNumber.Equals(identityNumber))
            {
                error += $"Клиент по ИИН {identityNumber} не совпадает с ИИН-ом клиента на договоре {contract.ContractNumber}. ";
            }

            if (!car.TechPassportNumber.Equals(techPassportNumber))
            {
                error += $"Автотранспорт по номеру техпаспорта {techPassportNumber} не совпадает с автотранспортом на договоре {contract.ContractNumber}. ";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new PawnshopApplicationException(error);
            }
        }

        public ListModel<Application> ListWithCount(ListQueryModel<ApplicationListQueryModel> listQuery)
        {
            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<Application>
            {
                List = _applicationRepository.List(listQuery, listQuery.Model),
                Count = _applicationRepository.Count(listQuery, listQuery.Model)
            };
        }

        private void ValidateParentContractForAddition(Contract parentContract, Application application, int? barnchId)
        {
            if (parentContract.SettingId.HasValue && parentContract.Setting is null)
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} из Заявки на Добор с Id {application.Id} не заполнен Setting");

            if (parentContract.SettingId.HasValue && !parentContract.Setting.AdditionAvailable)
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} из Заявки на Добор с Id {application.Id} добор запрещен");

            if (!parentContract.Positions.Any())
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} из Заявки на Добор с Id {application.Id} массив позиции пуст");

            if (parentContract.Positions.First().PositionId != application.PositionId)
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} не совпадают позиции из Заявки на Добор с Id {application.Id}");

            if (parentContract.ClientId != application.ClientId)
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} не совпадают ClientId из Заявки на Добор с Id {application.Id}");

            if (!_applicationRepository.GetBranchCodeByUserId(application.AuthorId).Contains(parentContract.BranchId))
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} не совпадает филиал с филиалом менеджера из Заявки на Добор с Id {application.Id}");

            if (barnchId.HasValue && barnchId.Value != parentContract.BranchId)
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} не совпадает филиал с филиалом менеджера создающий Договор");
        }

        private Contract FindParentContractForAddition(Application application, int? barnchId = null)
        {
            var parentContract = _contractService.Get(application.ParentContractId.Value);

            ValidateParentContractForAddition(parentContract, application, barnchId);

            return parentContract;
        }

        private bool IsCategoryWithoutDriving(Contract parentContract)
        {
            if (!parentContract.Positions.Any())
                return false;

            if (parentContract.Positions.First().CategoryId == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY)
                return true;

            return false;
        }
        private List<LimitAmount> AddLimitAmount(List<LimitAmount> maxLimitAmounts, ProdKind prodKind, Application application)
        {
            decimal maxAmount = 0;
            decimal debtCost = _contractService.GetAccountBalance(application.ParentContractId.Value, DateTime.Now);
            decimal debtOverdueCost = _contractService.GetOverdueAccountBalance(application.ParentContractId.Value, DateTime.Now);

            switch (prodKind)
            {
                case ProdKind.Light:
                    maxAmount = application.LightCost.Value - (debtCost + debtOverdueCost);
                    break;
                case ProdKind.Turbo:
                    maxAmount = application.TurboCost.Value - (debtCost + debtOverdueCost);
                    break;
                case ProdKind.Motor:
                    maxAmount = application.MotorCost.Value - (debtCost + debtOverdueCost);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ProdKind));
            }

            maxLimitAmounts.Add(new LimitAmount { ProdKind = prodKind, MaxLimitAmount = maxAmount });

            return maxLimitAmounts;
        }

        private List<LimitAmount> GetMaxLimitAmountsFromParentContract(Contract parentContract, Application application)
        {
            var maxLimitAmounts = new List<LimitAmount>();

            if (IsCategoryWithoutDriving(parentContract))
            {
                AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
                return maxLimitAmounts;
            }

            if (parentContract.SettingId.HasValue)
            {
                switch (parentContract.Setting.ScheduleType)
                {
                    case ScheduleType.Annuity:
                        AddLimitAmount(maxLimitAmounts, ProdKind.Light, application);
                        AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
                        break;
                    case ScheduleType.Discrete:
                        AddLimitAmount(maxLimitAmounts, ProdKind.Turbo, application);
                        AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ScheduleType));
                }
            }
            else
            {
                if (parentContract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
                    parentContract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
                    parentContract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix)
                {
                    AddLimitAmount(maxLimitAmounts, ProdKind.Light, application);
                    AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
                }
                else if (parentContract.PercentPaymentType == PercentPaymentType.EndPeriod)
                {
                    AddLimitAmount(maxLimitAmounts, ProdKind.Turbo, application);
                    AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
                }
                else if (parentContract.PercentPaymentType == PercentPaymentType.Product)
                {
                    AddLimitAmount(maxLimitAmounts, ProdKind.Turbo, application);
                    AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
                    AddLimitAmount(maxLimitAmounts, ProdKind.Light, application);
                }
            }

            return maxLimitAmounts;
        }

        private List<LimitAmount> GetMaxLimitAmounts(Contract parentContract, Application application)
        {
            var maxLimitAmounts = new List<LimitAmount>();

            if (application.WithoutDriving)
            {
                return AddLimitAmount(maxLimitAmounts, ProdKind.Motor, application);
            }

            var maxLimitAmountsFromParent = GetMaxLimitAmountsFromParentContract(parentContract, application);

            if (maxLimitAmountsFromParent.Any())
                maxLimitAmounts.AddRange(maxLimitAmountsFromParent);

            if (!maxLimitAmounts.Any())
                throw new PawnshopApplicationException($"Для родительского Договора с Id {parentContract.Id} не заполнен maxLimitAmounts для Заявки на Добор с Id {application.Id}");

            return maxLimitAmounts;
        }

        public ApplicationModel Get(int id)
        {
            var application = _applicationRepository.Get(id);
            ApplicationDetails applicationDetails;

            if (application.Status != ApplicationStatus.Done)
            {
                applicationDetails = new ApplicationDetails()
                {
                    ApplicationId = application.Id,
                    OverIssueAmount = Constants.APPLICATION_OVER_ISSUE_AMOUNT
                };
            }
            else
            {
                applicationDetails = _applicationDetailsRepository.Get(id);
            }

            if (application.IsAddition && !application.ParentContractId.HasValue)
                throw new PawnshopApplicationException($"В Заявке на добор с Id {application.Id} не заполнен ParentContractId");

            if (application.IsAddition && application.ParentContractId.HasValue)
            {
                var parentContract = FindParentContractForAddition(application);
                applicationDetails.MaxLimitAmounts = GetMaxLimitAmounts(parentContract, application);
                applicationDetails.ParentCategoryId = GetCategory(parentContract);
            }

            var applicationAdditionalLimit = _contractCreditLineAdditionalLimitsRepository.GetLimitBySum((decimal)application.EstimatedCost).Result;

            var applicationModel = new ApplicationModel()
            {
                Application = application,
                ApplicationDetails = applicationDetails,
                ApplicationAdditionalLimit = applicationAdditionalLimit,
                CreditLineMaturityDate = application.ParentContractId.HasValue && !application.IsAddition ? _contractService.GetOnlyContract(application.ParentContractId.Value).MaturityDate : DateTime.MinValue
            };

            return applicationModel;
        }

        private int GetCategory(Contract parentContract)
        {
            return parentContract.Positions.First().CategoryId;
        }

        private decimal GetApplicationLimitCost(bool isAddition, decimal applicationLimit)
        {
            return isAddition ? applicationLimit : applicationLimit + Constants.APPLICATION_OVER_ISSUE_AMOUNT;
        }

        public void ValidateLoanAmounts(decimal validatedAmount, ApplicationModel applicationModel, ContractClass contractClass)
        {
            var application = applicationModel.Application;
            var applicationDetails = applicationModel.ApplicationDetails;

            switch (applicationDetails.ProdKind)
            {
                case ProdKind.Light:
                    decimal applicationLimit = application.IsAutocredit == 1 ? application.RequestedSum : application.LightCost.Value;

                    if (validatedAmount > GetApplicationLimitCost(application.IsAddition, applicationLimit) && contractClass != ContractClass.CreditLine)
                        throw new PawnshopApplicationException($"Ссуда (получит клиент) {validatedAmount} не может быть больше суммы по категории аналитики \"Аннуитет\" ({application.LightCost}) утвержденной Кредитным комитетом для Заявки с Id {application.Id}");

                    break;
                case ProdKind.Turbo:
                    if (validatedAmount > GetApplicationLimitCost(application.IsAddition, application.TurboCost.Value))
                        throw new PawnshopApplicationException($"Ссуда (получит клиент) {validatedAmount} не может быть больше суммы по категории аналитики \"Дискрет\" ({application.TurboCost}) утвержденной Кредитным комитетом для Заявки с Id {application.Id}");

                    break;
                case ProdKind.Motor:
                    var limit = application.EstimatedCost * (applicationModel.ApplicationAdditionalLimit / 100);
                    if (limit > Constants.MAX_ADDITIONAL_SUM)
                        limit = Constants.MAX_ADDITIONAL_SUM;

                    if ((validatedAmount > GetApplicationLimitCost(application.IsAddition, application.MotorCost.Value) && contractClass != ContractClass.CreditLine)
                        || (validatedAmount > GetApplicationLimitCost(application.IsAddition, limit + application.MotorCost.Value) && contractClass == ContractClass.CreditLine))
                        throw new PawnshopApplicationException($"Ссуда (получит клиент) {validatedAmount} не может быть больше суммы по категории аналитики \"Без Права вождения\" ({application.MotorCost}) утвержденной Кредитным комитетом для Заявки с Id {application.Id}");

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ProdKind));
            }
        }

        public ContractPartialSign GetContractPartialSign(int contractId)
        {
            return _contractPartialSignRepository.GetByContractId(contractId);
        }

        public void SavePartialSign(Contract contract, ApplicationDetails applicationDetails)
        {
            if (applicationDetails.IsFirstTransh)
            {
                var exisistPartialSign = _contractPartialSignRepository.GetByContractId(contract.Id);
                if (exisistPartialSign != null)
                    _contractPartialSignRepository.Delete(exisistPartialSign.Id);

                var contractPartialSign = new ContractPartialSign()
                {
                    Id = contract.Id,
                    TotalAmount = applicationDetails.TotalAmount4AllTransh.Value,
                    AuthorId = contract.AuthorId,
                    CreateDate = DateTime.Now
                };
                _contractPartialSignRepository.Insert(contractPartialSign);
            }
        }

        private void ValidateAdditionDetails(AdditionDetails model)
        {
            if (!model.ApplicationId.HasValue)
                throw new PawnshopApplicationException($"В запросе на Добор не заполнен ApplicationId");

            if (model.ApplicationId.HasValue && model.Application is null)
                throw new PawnshopApplicationException($"В запросе на Добор не заполнена модель Application");

            if (!model.AdditionAmount.HasValue)
                throw new PawnshopApplicationException($"В запросе на Добор не заполнен AdditionAmount");

            if (!model.ProdKind.HasValue)
                throw new PawnshopApplicationException($"Не заполнен ProdKind");

            if (model.ProdKind.HasValue && !model.ProdKind.Equals(ProdKind.Motor) && model.IsChangeCategory)
                throw new PawnshopApplicationException($"Категория изменяется только для ProdKind Motor");

            if (model.IsFirstTransh && !model.TotalAmount4AllTransh.HasValue)
                throw new PawnshopApplicationException($"Не заполнена Общая сумма добора при транше");
        }

        private void ValidateAdditionApplication(AdditionDetails model, Application application)
        {
            if (!_applicationComparer.Equals(model.Application, application))
                throw new PawnshopApplicationException($"model.AdditionDetails.Application не соответствует Заявке с Id {application.Id}");

            if (!model.ApplicationId.HasValue)
                throw new PawnshopApplicationException($"В запросе на Добор не заполнен ApplicationId");

            if (!model.AdditionAmount.HasValue)
                throw new PawnshopApplicationException($"В запросе на Добор не заполнен AdditionAmount");

            if (application.IsAddition && !application.ParentContractId.HasValue)
                throw new PawnshopApplicationException($"В Заявке на добор с Id {application.Id} не заполнен ParentContractId");

            if (application.ParentContractId.HasValue && !application.IsAddition)
                throw new PawnshopApplicationException($"В Заявке на добор с Id {application.Id} не заполнен IsAddition");
        }

        private void ValidateAdditionLoanAmounts(AdditionDetails model, Application application, Contract parentContract)
        {
            if (_insurancePoliceRequestService.GetApprovedPoliceRequest(parentContract.Id) != null)
            {
                throw new PawnshopApplicationException("По договору имеется не отозванная заявка в Страховую компанию. Отзовите заявку через Service Desk");
            }

            var maxLimitAmounts = GetMaxLimitAmounts(parentContract, application);
            if (!maxLimitAmounts.Any())
                throw new PawnshopApplicationException($"В Заявке на добор с Id {application.Id} не заполнен MaxLimitAmounts");

            var maxAmount = maxLimitAmounts.Where(x => x.ProdKind.Equals(model.ProdKind)).FirstOrDefault();
            var MaxLimitAmountValue = maxAmount != null ? maxAmount.MaxLimitAmount : 0;

            if (model.AdditionAmount > MaxLimitAmountValue)
                throw new PawnshopApplicationException($"Сумма добора не может быть больше чем {MaxLimitAmountValue} для выбранного ProdKind {model.ProdKind}");
        }

        public bool IsCreateAdditionWithInsurance(AdditionDetails model, int branchId, int userId)
        {
            ValidateAdditionDetails(model);
            var application = _applicationRepository.Get(model.ApplicationId.Value);
            bool insuranceRequired = false;

            ValidateAdditionApplication(model, application);

            var parentContract = FindParentContractForAddition(application, branchId);

            ValidateAdditionLoanAmounts(model, application, parentContract);

            using (var transaction = _applicationDetailsRepository.BeginTransaction())
            {
                var applicationDetails = new ApplicationDetails
                {
                    ApplicationId = application.Id,
                    ContractId = parentContract.Id,
                    ProdKind = model.ProdKind.Value,
                    AdditionAmount = model.AdditionAmount.HasValue ? model.AdditionAmount.Value : default,
                    IsFirstTransh = model.IsFirstTransh,
                    TotalAmount4AllTransh = model.IsFirstTransh ? model.TotalAmount4AllTransh : 0
                };

                try
                {
                    _insurancePolicyService.GetInsurancePolicyForAddition(parentContract, applicationDetails.AdditionAmount, application.ApplicationDate);
                }
                catch (Exception e)
                {
                    insuranceRequired = true;
                }

                applicationDetails.InsuranceRequired = insuranceRequired;
                _applicationDetailsRepository.Insert(applicationDetails);

                ChangeApplicationStatus(application.Id, ApplicationStatus.Processing);

                //Добавляем новую запись в таблицу ContractPartialSign
                if (applicationDetails.IsFirstTransh)
                    SavePartialSign(parentContract, applicationDetails);

                transaction.Commit();
            }

            //return insuranceRequired;
            return true;//теперь всегда идем через форму "Заявка на добор"
        }

        private ContractDiscount CreateContractDiscount(AdditionDetails model, int userId)
        {
            var application = model.Application;
            var contractDiscount = new ContractDiscount
            {
                PersonalDiscountId = DISCOUNT_FOR_CHANGE_CATEGORY,
                PersonalDiscount = _contractDiscountService.GetPersonalDiscountById(DISCOUNT_FOR_CHANGE_CATEGORY),
                IsTypical = true,
                ContractId = application.ParentContractId.Value,
                CreateDate = application.ApplicationDate.Date,
                BeginDate = application.ApplicationDate.Date,
                EndDate = application.ApplicationDate.Date,
                AuthorId = userId,
                Status = ContractDiscountStatus.Accepted
            };

            return contractDiscount;
        }

        private ContractDiscount? GetPersonalDiscountForApplication(int applicationId)
        {
            try
            {
                int personalDiscountId = _applicationDetailsRepository.GetCreatedPersonalDiscountId(applicationId);

                var personalDiscount = _contractDiscountService.Get(personalDiscountId);
                if (!personalDiscount.Status.Equals(ContractDiscountStatus.Accepted))
                    return null;

                return personalDiscount;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void DeletePersonalDiscountForChangeCategory(int discountId)
        {
            _contractDiscountService.Delete(discountId);
        }

        public void RejectApplication(int id)
        {
            ChangeApplicationStatus(id, ApplicationStatus.Rejected);
        }

        public void CompleteApplication(int id)
        {
            ChangeApplicationStatus(id, ApplicationStatus.Done);
        }

        public void CancelApplicationByContractId(int contractId)
        {
            var applicationDetails = GetApplicationDetailsByContractId(contractId);
            if (applicationDetails is null)
                return;

            ChangeApplicationStatus(applicationDetails.ApplicationId, ApplicationStatus.New);
        }

        public Application GetApplicationByParentContractId(int parentContractId)
        {
            return _applicationRepository.Find(new { ParentContractId = parentContractId, ApplicationStatuses = new List<int>() { (int)ApplicationStatus.New, (int)ApplicationStatus.Processing } });
        }

        public ApplicationDetails GetApplicationDetailsByContractId(int contractId)
        {
            return _applicationDetailsRepository.Find(new { ContractId = contractId });
        }

        public ApplicationDetails GetApplicationDetailsByApplicationId(int applicationId)
        {
            var applicationDetails = _applicationDetailsRepository.Get(applicationId);
            if (applicationDetails is null)
                throw new PawnshopApplicationException($"Не найдена Заявка с Id {applicationId}");

            return applicationDetails;
        }

        public void UpdateApplicationDetailsChildContractId(ApplicationDetails applicationDetails)
        {
            _applicationDetailsRepository.Update(applicationDetails);
        }

        private void ChangeApplicationStatus(int id, ApplicationStatus applicationStatus)
        {
            var application = _applicationRepository.Get(id);
            application.Status = applicationStatus;
            _applicationRepository.Update(application);
        }

        public void ValidateAndCompleteApplication(int? parentContractId, int? childContractId, Contract contract, InsurancePoliceRequest latestPoliceRequest)
        {
            UpdateContractIdForAdditionApplication(parentContractId, childContractId);
            ValidateLoanCostWithApplicationLimitAndChangeApplicationStatus(contract, latestPoliceRequest);
        }

        private void UpdateContractIdForAdditionApplication(int? parentContractId, int? childContractId)
        {
            if (!parentContractId.HasValue)
                return;

            var application = GetApplicationByParentContractId(parentContractId.Value);
            if (application is null)
                return;

            if (!childContractId.HasValue)
                throw new PawnshopApplicationException($"Не заполнен childContractId.Id при доборе");

            var applicationDetails = GetApplicationDetailsByApplicationId(application.Id);
            applicationDetails.ContractId = childContractId.Value;
            UpdateApplicationDetailsChildContractId(applicationDetails);
        }

        private void ValidateLoanCostWithApplicationLimitAndChangeApplicationStatus(Contract contract, InsurancePoliceRequest latestPoliceRequest)
        {
            InsurancePolicy insurancePolicy = null;
            var applicationDetails = GetApplicationDetailsByContractId(contract.Id);

            if (applicationDetails is null)
                return;

            var applicationModel = Get(applicationDetails.ApplicationId);
            applicationModel.ApplicationDetails = applicationDetails;

            decimal validatedAmount = contract.LoanCost;
            if (latestPoliceRequest != null && latestPoliceRequest.IsInsuranceRequired && latestPoliceRequest.RequestData.InsurancePremium > 0)
            {
                validatedAmount -= latestPoliceRequest.RequestData.InsurancePremium;
            }

            ValidateLoanAmounts(validatedAmount, applicationModel, contract.ContractClass);

            CompleteApplication(applicationModel.Application.Id);
        }

        public Application IsAdditionFromApplication(Contract parentContract)
        {
            try
            {
                var application = GetApplicationByParentContractId(parentContract.Id);
                if (application != null && application.Status.Equals(ApplicationStatus.Processing))
                {
                    var applicationDetails = GetApplicationDetailsByApplicationId(application.Id);
                    if (applicationDetails.CreatedDate.Date == DateTime.Today.Date)
                        return application;
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ContractPosition[] ChangePositionEstimatedCostFromApplication(Application application, Contract parentContract, bool isAddition = false, int? positionEstimatedCost = null)
        {
            var pulledPositions = parentContract.Positions.Where(p => p.Status == ContractPositionStatus.PulledOut).ToList();
            if (application != null)
            {
                pulledPositions.ForEach(x =>
                {
                    x.EstimatedCost = application.EstimatedCost;
                });
            }
            else if (application == null && isAddition && positionEstimatedCost != null)
            {
                pulledPositions.ForEach(x =>
                {
                    x.EstimatedCost = positionEstimatedCost.Value;
                });
            }
            return pulledPositions.ToArray();
        }

        public List<Application> GetApplicationsForReject()
        {
            return _applicationRepository.GetApplicationsForReject(new { ApplicationStatuses = new List<int>() { (int)ApplicationStatus.Processing, (int)ApplicationStatus.New } });
        }

        private bool CheckCashOrdersForContract(Contract contract)
        {
            var cashOderId = _cashOrderRepository.GetCashOrderByContractId(contract.Id);

            return cashOderId != null ? true : false;
        }

        public void RejectApplicationAndDeleteContract(Application application, DateTime date)
        {
            var applicationDetails = _applicationDetailsRepository.GetAll(application.Id);
            applicationDetails.ForEach(x =>
            {
                if (x.ContractId != 0)
                {
                    var contract = _contractService.GetOnlyContract(x.ContractId);
                    if (contract.Status.Equals(ContractStatus.Draft) && !CheckCashOrdersForContract(contract))
                        _contractService.Delete(x.ContractId);
                }
            });
            ChangeApplicationStatus(application.Id, ApplicationStatus.Rejected);
        }

        public void ValidateApplicationForAdditionCost(int contractId, decimal additionCost)
        {
            bool isContractCreateFromButton = _sessionContext
                    .Permissions.Where(x => x.Equals(Permissions.ContractCreateFromButton))
                    .FirstOrDefault() == Permissions.ContractCreateFromButton;

            if (!isContractCreateFromButton)
            {
                var application = GetApplicationByParentContractId(contractId);
                var applicationDetails = GetApplicationDetailsByApplicationId(application.Id);

                if (!application.ApplicationDate.Date.Equals(DateTime.Today.Date))
                    throw new PawnshopApplicationException($"Заявка с id {application.Id} не соответствует текущей дате");
            }
        }

        public ApplicationModel GetApplicationModelForAddition(int? parentContractId)
        {
            if (!parentContractId.HasValue)
                throw new PawnshopApplicationException($"Входящий параметр parentContractId не заполнен");

            var application = GetApplicationByParentContractId(parentContractId.Value);
            if (application is null)
                throw new PawnshopApplicationException($"Активная Заявка связанная с parentContractId {parentContractId} не найдена");

            var applicationDetails = GetApplicationDetailsByApplicationId(application.Id);
            if (applicationDetails.CreatedDate.Date != DateTime.Today.Date)
                throw new PawnshopApplicationException($"Заявка c id {application.Id} не актуальна");

            return new ApplicationModel()
            {
                Application = application,
                ApplicationDetails = applicationDetails
            };
        }

        public int ChangeStatusToNew(int contractId)
        {
            var applicationDetails = GetApplicationDetailsByContractId(contractId);
            ChangeApplicationStatus(applicationDetails.ApplicationId, ApplicationStatus.New);

            return applicationDetails.ApplicationId;
        }

        public string CheckIdentityDocument(string identityNumber, string documentNumber)
        {
            var client = _clientRepository.FindByIdentityNumber(identityNumber);
            var document = _clientRepository.GetClientDocumentByNumber(documentNumber);

            if (document == null)
            {
                return "Отсутствует данный документ";
            }

            if (client == null || !client.Documents.Any(x => x.Number == document.Number))
            {
                throw new PawnshopApplicationException("Документ должен быть уникальным");
            }

            return "Данные совпадают";
        }

        public TrancheLimit GetLimitByCategory(int contractId)
        {
            var contract = _contractService.GetOnlyContract(contractId);
            if ((contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche) && contract.CollateralType == CollateralType.Car)
            {
                var creditLineId = contract.ContractClass == ContractClass.CreditLine ? contractId : contract.CreditLineId.Value;
                var positions = _contractService.GetPositionsByContractId(creditLineId);
                var contractSettings = _contractService.GetContractSettings(contractId);

                if (contractSettings.IsInsuranceAdditionalLimitOn && positions != null)
                {
                    return getLimits(positions.FirstOrDefault(), contractId);
                }
                else
                {
                    if (contractId != creditLineId)
                    {
                        contract = _contractService.GetOnlyContract(creditLineId);
                    }
                    var leftLoanCost = _contractService.GetDebtInfoByCreditLine(creditLineId).PrincipalDebt;
                    return new TrancheLimit()
                    {
                        MaxAvailableAmount = contract.LoanCost - leftLoanCost
                    };
                }
            }
            return new TrancheLimit();
        }   
        private TrancheLimit getLimits(ContractPosition position, int contractId)
        {
            // процент добавления суммы от оценки
            var applicationAdditionalLimit = _contractCreditLineAdditionalLimitsRepository.GetLimitBySum(position.EstimatedCost).Result;

            // итоговая сумма лимита
            var additionalLimitCost = Math.Min(position.EstimatedCost * applicationAdditionalLimit / 100, Constants.MAX_ADDITIONAL_SUM);
            decimal maxSumWithLimit = getMaxSumWithLimit(position, contractId);

            var leftLoanCost = _contractService.GetDebtInfoByCreditLine(position.ContractId).PrincipalDebt;

            var sum = position.MotorCost.HasValue ? position.MotorCost.Value : position.LoanCost;
            return new TrancheLimit()
            {
                MaxSumWithLimit = maxSumWithLimit + additionalLimitCost,
                MaxTrancheSum = maxSumWithLimit - leftLoanCost,
                AdditionalLimit = additionalLimitCost,
                MaxAvailableAmount = sum - leftLoanCost + additionalLimitCost
            };
        }

        private decimal getMaxSumWithLimit(ContractPosition position, int contractId)
        {
            var contract = _contractService.GetOnlyContract(contractId);
            var tranches = _contractRepository.GetAllSignedTranches(position.ContractId).Result;
            var creditLineApplicationDetails = GetApplicationDetailsByContractId(position.ContractId);

            if (creditLineApplicationDetails == null)
            {
                return 0;
            }

            ApplicationDetails applicationDetails = null;
            Application application = null;
            bool IsFirstTranche = false;
            if (tranches.Count > 0)
            {
                applicationDetails = GetApplicationDetailsByContractId(contractId);
                if (applicationDetails == null)
                {
                    IsFirstTranche = true;
                    application = _applicationRepository.FindByContractId(position.ContractId);
                }
                else
                    application = _applicationRepository.FindByContractId(applicationDetails.ContractId);
            }
            else
            {
                IsFirstTranche = true;
                application = _applicationRepository.FindByContractId(position.ContractId);
            }

            bool isMotor = false;
            if (contract.ContractClass == ContractClass.Tranche)
                isMotor = (IsFirstTranche ? creditLineApplicationDetails.ProdKind : applicationDetails.ProdKind) == ProdKind.Motor;
            else
                isMotor = position.Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE;

            decimal maxSumWithLimit = 0;
            // при первом транше берем суммы и выбранную категорию из позиции
            if (IsFirstTranche)
                maxSumWithLimit = isMotor ? position.MotorCost.Value : position.TurboCost.Value;
            // при вторых траншах, из заявок на эти транши
            else if (contract.ContractClass == ContractClass.CreditLine || !IsFirstTranche)
                maxSumWithLimit = isMotor ? application.MotorCost.Value : application.LightCost.Value;


            return maxSumWithLimit;
        }

        public int GetBitrixId(int contractId)
        {
            return _applicationRepository.GetBitrixId(contractId);
        }
    }
}
