using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models.Dictionary;
using Pawnshop.Web.Models.List;
using System;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Postponements;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Models.Clients;
using Pawnshop.Services.Expenses;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Realties;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Services.Collection;
using Pawnshop.Services.Domains;
using Pawnshop.Services.LegalCollection;
using Microsoft.Extensions.Caching.Memory;
using Pawnshop.Services.Collection.http;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseAction;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseStage;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class DictionaryController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;

        //repositories
        private readonly GroupRepository _groupRepository;
        private readonly RoleRepository _roleRepository;
        private readonly CarRepository _carRepository;
        private readonly RealtyRepository _realtyRepository;
        private readonly ClientRepository _clientRepository;
        private readonly PositionRepository _positionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly UserRepository _userRepository;
        private readonly PurityRepository _purityRepository;
        private readonly ExpenseGroupRepository _expenseGroupRepository;
        private readonly ExpenseTypeRepository _expenseTypeRepository;
        private readonly MachineryRepository _machineryRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly BlackListReasonRepository _blackListReasonRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly TransferContractRepository _transferContractRepository;
        private readonly AttractionChannelRepository _attractionChannelRepository;
        private readonly ParkingActionRepository _parkingActionRepository;
        private readonly ParkingStatusRepository _parkingStatusRepository;
        private readonly SociallyVulnerableGroupRepository _sociallyVulnerableGroupRepository;
        private readonly BlackoutRepository _blackoutRepository;
        private readonly PersonalDiscountRepository _personalDiscountRepository;
        private readonly PostponementRepository _postponementRepository;
        private readonly RequisiteTypeRepository _requisiteTypeRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        private readonly ClientDocumentTypeRepository _clientDocumentTypeRepository;
        private readonly ClientDocumentProviderRepository _clientDocumentProviderRepository;
        private readonly ClientEconomicActivityTypeRepository _clientEconomicActivityTypeRepository;
        private readonly ClientLegalFormRequiredDocumentRepository _clientLegalFormRequiredDocumentRepository;
        private readonly ClientSignersAllowedDocumentTypeRepository _clientSignersAllowedDocumentTypeRepository;
        private readonly ContractCheckRepository _contractCheckRepository;
        private readonly CountryRepository _countryRepository;
        private readonly AddressRepository _addressRepository;
        private readonly AddressTypeRepository _addressTypeRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly LoanProductTypeRepository _loanProductTypeRepository;
        private readonly CurrencyRepository _currencyRepository;
        private readonly PrintTemplateRepository _printTemplateRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly LanguagesRepository _languagesRepository;

        // Collection
        private readonly ICollectionHttpService<CollectionActions> _collectionActionHttpService;
        private readonly ICollectionHttpService<CollectionReason> _collectionReasonHttpService;
        private readonly ICollectionHttpService<CollectionStatus> _collectionStatusHttpService;

        // LegalCollection
        private readonly ILegalCollectionActionsHttpService _legalCollectionActions;
        private readonly ILegalCollectionStagesHttpService _legalCollectionStages;
        private readonly ILegalCollectionStatusesHttpService _legalCollectionStatuses;
        private readonly ILegalCollectionCoursesHttpService _legalCollectionCourses;

        private readonly VehicleMarkRepository _vehicleMarkRepository;
        private readonly VehicleModelRepository _vehicleModelRepository;
        private readonly VehicleWMIRepository _vehicleWMIRepository;
        private readonly VehicleManufacturerRepository _vehicleManufacturerRepository;
        private readonly VehicleCountryCodeRepository _vehicleCountryCodeRepository;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly HolidayRepository _holidayRepository;
        private readonly InsuranceRateRepository _insuranceRateRepository;

        //services
        private readonly IDictionaryService<AccountPlan> _accountPlanService;
        private readonly IDictionaryService<ExpenseArticleType> _expenseArticleTypeService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IDictionaryService<AccountingCore.Models.Type> _typeService;
        private readonly IDictionaryWithSearchService<Account, AccountFilter> _accountService;
        private readonly IDictionaryWithSearchService<BusinessOperation, BusinessOperationFilter> _businessOperationService;
        private readonly IAccountRecordService _accountRecordService;
        private readonly IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> _businessOperationSettingService;
        private readonly IExpenseService _expenseService;
        private readonly IRealtyService _realtyService;
        private readonly IDomainService _domainService;

        private readonly IMemoryCache _memoryCache;

        public DictionaryController(ISessionContext sessionContext, BranchContext branchContext,
            GroupRepository groupRepository, RoleRepository roleRepository, CarRepository carRepository,
            ClientRepository clientRepository, PositionRepository positionRepository,
            CategoryRepository categoryRepository,
            UserRepository userRepository, PurityRepository purityRepository,
            ExpenseGroupRepository expenseGroupRepository, ExpenseTypeRepository expenseTypeRepository,
            MachineryRepository machineryRepository, OrganizationRepository organizationRepository,
            BlackListReasonRepository blackListReasonRepository, InnerNotificationRepository innerNotificationRepository,
            TransferContractRepository transferContractRepository, AttractionChannelRepository attractionChannelRepository,
            ParkingActionRepository parkingActionRepository, ParkingStatusRepository parkingStatusRepository,
            SociallyVulnerableGroupRepository sociallyVulnerableGroupRepository, PostponementRepository postponementRepository,
            BlackoutRepository blackoutRepository, PersonalDiscountRepository personalDiscountRepository, RequisiteTypeRepository requisiteTypeRepository,
            PayTypeRepository payTypeRepository, ClientLegalFormRepository clientLegalFormRepository, ClientDocumentTypeRepository clientDocumentTypeRepository,
            ClientDocumentProviderRepository clientDocumentProviderRepository, ClientEconomicActivityTypeRepository clientEconomicActivityTypeRepository,
            ContractCheckRepository contractCheckRepository, CountryRepository countryRepository, AddressRepository addressRepository,
            AddressTypeRepository addressTypeRepository, LoanSubjectRepository loanSubjectRepository, LoanProductTypeRepository loanProductTypeRepository,
            CurrencyRepository currencyRepository, PrintTemplateRepository printTemplateRepository, LoanPercentRepository loanPercentRepository,
            VehicleMarkRepository vehicleMarkRepository, VehicleModelRepository vehicleModelRepository, VehicleWMIRepository vehicleWMIRepository,
            VehicleManufacturerRepository vehicleManufacturerRepository, VehicleCountryCodeRepository vehicleCountryCodeRepository, VehicleBlackListRepository vehicleBlackListRepository,
            HolidayRepository holidayRepository, ClientSignersAllowedDocumentTypeRepository clientSignersAllowedDocumentTypeRepository,
            ClientLegalFormRequiredDocumentRepository clientLegalFormRequiredDocumentRepository,
            InsuranceRateRepository insuranceRateRepository, RealtyRepository realtyRepository,
            LanguagesRepository languagesRepository, ICollectionHttpService<CollectionActions> collectionActionHttpService,
            ICollectionHttpService<CollectionReason> collectionReasonHttpService, ICollectionHttpService<CollectionStatus> collectionStatusHttpService,

            IDictionaryService<AccountPlan> accountPlanService,
            IDictionaryService<ExpenseArticleType> expenseArticleTypeService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IDictionaryService<AccountingCore.Models.Type> typeService,
            IDictionaryWithSearchService<Account, AccountFilter> accountService,
            IDictionaryWithSearchService<BusinessOperation, BusinessOperationFilter> businessOperationService,
            IAccountRecordService accountRecordService,
            IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> businessOperationSettingService,
            IExpenseService expenseService, IRealtyService realtyService, IDomainService domainService, 
            ILegalCollectionStatusesHttpService legalCollectionStatuses,
            ILegalCollectionStagesHttpService legalCollectionStages, 
            ILegalCollectionCoursesHttpService legalCollectionCourses, 
            ILegalCollectionActionsHttpService legalCollectionActions,
            IMemoryCache memoryCache
            )
        {
            //repositories
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _groupRepository = groupRepository;
            _roleRepository = roleRepository;
            _carRepository = carRepository;
            _clientRepository = clientRepository;
            _positionRepository = positionRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _purityRepository = purityRepository;
            _expenseGroupRepository = expenseGroupRepository;
            _expenseTypeRepository = expenseTypeRepository;
            _machineryRepository = machineryRepository;
            _organizationRepository = organizationRepository;
            _blackListReasonRepository = blackListReasonRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _transferContractRepository = transferContractRepository;
            _attractionChannelRepository = attractionChannelRepository;
            _parkingActionRepository = parkingActionRepository;
            _parkingStatusRepository = parkingStatusRepository;
            _sociallyVulnerableGroupRepository = sociallyVulnerableGroupRepository;
            _postponementRepository = postponementRepository;
            _blackoutRepository = blackoutRepository;
            _personalDiscountRepository = personalDiscountRepository;
            _requisiteTypeRepository = requisiteTypeRepository;
            _payTypeRepository = payTypeRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
            _clientDocumentTypeRepository = clientDocumentTypeRepository;
            _clientDocumentProviderRepository = clientDocumentProviderRepository;
            _clientEconomicActivityTypeRepository = clientEconomicActivityTypeRepository;
            _clientLegalFormRequiredDocumentRepository = clientLegalFormRequiredDocumentRepository;
            _clientSignersAllowedDocumentTypeRepository = clientSignersAllowedDocumentTypeRepository;
            _contractCheckRepository = contractCheckRepository;
            _countryRepository = countryRepository;
            _addressRepository = addressRepository;
            _addressTypeRepository = addressTypeRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _loanProductTypeRepository = loanProductTypeRepository;
            _currencyRepository = currencyRepository;
            _printTemplateRepository = printTemplateRepository;
            _loanPercentRepository = loanPercentRepository;
            _vehicleMarkRepository = vehicleMarkRepository;
            _vehicleModelRepository = vehicleModelRepository;
            _vehicleWMIRepository = vehicleWMIRepository;
            _vehicleManufacturerRepository = vehicleManufacturerRepository;
            _vehicleCountryCodeRepository = vehicleCountryCodeRepository;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _holidayRepository = holidayRepository;
            _insuranceRateRepository = insuranceRateRepository;
            _realtyRepository = realtyRepository;
            _languagesRepository = languagesRepository;
            _collectionActionHttpService = collectionActionHttpService;
            _collectionStatusHttpService = collectionStatusHttpService;
            _collectionReasonHttpService = collectionReasonHttpService;

            //services
            _accountPlanService = accountPlanService;
            _expenseArticleTypeService = expenseArticleTypeService;
            _accountSettingService = accountSettingService;
            _typeService = typeService;
            _accountService = accountService;
            _businessOperationService = businessOperationService;
            _accountRecordService = accountRecordService;
            _businessOperationSettingService = businessOperationSettingService;
            _expenseService = expenseService;
            _realtyService = realtyService;
            _domainService = domainService;

            _memoryCache = memoryCache;
            _legalCollectionStatuses = legalCollectionStatuses;
            _legalCollectionStages = legalCollectionStages;
            _legalCollectionCourses = legalCollectionCourses;
            _legalCollectionActions = legalCollectionActions;
        }

        [HttpPost]
        public async Task<IActionResult> Permissions()
        {
            if (!_memoryCache.TryGetValue("permissions", out List<Permission> permissions))
            {
                permissions = Core.Permissions.All.ToList();
                _memoryCache.Set("permissions", permissions,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> Groups()
        {
            if (!_memoryCache.TryGetValue("groups", out List<Group> groups))
            {
                groups = _groupRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("groups", groups,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(groups);
        }


        [HttpPost]
        public async Task<IActionResult> Users()
        {
            if (!_memoryCache.TryGetValue("users", out List<User> users))
            {
                users = _userRepository.List(new ListQuery() { Page = null }, new { organizationId = _sessionContext.OrganizationId });
                _memoryCache.Set("users", users,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Roles()
        {
            if (!_memoryCache.TryGetValue("roles", out List<Role> roles))
            {
                roles = _roleRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("roles", roles,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(roles);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Banks()
        {
            if (!_memoryCache.TryGetValue("banks", out List<Client> banks))
            {
                banks = _clientRepository.List(new ListQuery() { Page = null }, new ClientListQueryModel { IsBank = true });
                _memoryCache.Set("banks", banks,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(banks);
        }

        [HttpPost]
        public async Task<IActionResult> Clients([FromBody] ListQueryModel<ClientListQueryModel> listQuery)
        {
            var clients = new ListModel<Client>()
            {
                List = await Task.Run(() => _clientRepository.List(listQuery, listQuery.Model)),
                Count = await Task.Run(() => _clientRepository.Count(listQuery, listQuery.Model))
            };
            return Ok(clients);
        }

        [HttpPost]
        public async Task<IActionResult> EstimationCompanies([FromBody] ListQuery listQuery)
        {
            var subTypeId = _domainService.GetDomainValue(Constants.CLIENT_SUB_TYPE, Constants.ESTIMATION_COMPANY).Id;
            var estimationCompanies = new ListModel<Client>()
            {
                List = await Task.Run(() => _clientRepository.ListEstimationCompanies(listQuery, subTypeId)),
                Count = await Task.Run(() => _clientRepository.CountEstimationCompanies(listQuery, subTypeId))
            };
            return Ok(estimationCompanies);
        }

        [HttpPost]
        public async Task<IActionResult> Positions([FromBody] ListQueryModel<PositionListQueryModel> listQuery)
        {
            var subTypeId = _domainService.GetDomainValue(Constants.CLIENT_SUB_TYPE, Constants.ESTIMATION_COMPANY).Id;
            var positions = new ListModel<Position>
            {
                List = await Task.Run(() => _positionRepository.List(listQuery, listQuery?.Model)),
                Count = await Task.Run(() => _positionRepository.Count(listQuery, listQuery?.Model))
            };
            return Ok(positions);
        }

        [HttpPost]
        public async Task<IActionResult> Cars([FromBody] ListQueryModel<PositionListQueryModel> listQuery)
        {
            var cars = new ListModel<Car>
            {
                List = await Task.Run(() => _carRepository.List(listQuery, listQuery?.Model)),
                Count = await Task.Run(() => _carRepository.Count(listQuery, listQuery?.Model))
            };
            return Ok(cars);
        }

        [HttpPost]
        public async Task<IActionResult> Realties([FromBody] ListQueryModel<PositionListQueryModel> listQuery)
        {
            var realties = new ListModel<Realty>
            {
                List = await Task.Run(() => _realtyRepository.List(listQuery, listQuery?.Model)),
                Count = await Task.Run(() => _realtyRepository.Count(listQuery, listQuery?.Model))
            };
            return Ok(realties);
        }

        [HttpPost]
        public async Task<IActionResult> Machineries([FromBody] ListQueryModel<PositionListQueryModel> listQuery)
        {
            if (!_memoryCache.TryGetValue("machineries", out ListModel<Machinery> machineries))
            {
                machineries = new ListModel<Machinery>
                {
                    List = await Task.Run(() => _machineryRepository.List(listQuery, listQuery?.Model)),
                    Count = await Task.Run(() => _machineryRepository.Count(listQuery, listQuery?.Model))
                };
                _memoryCache.Set("machineries", machineries,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(machineries);
        }

        [HttpPost]
        public async Task<IActionResult> Categories([FromBody] ListQueryModel<CategoryListQueryModel> listQuery)
        {
            var categories = _categoryRepository.List(new ListQuery { Page = null }, listQuery?.Model);
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AccountPlans()
        {
            if (!_memoryCache.TryGetValue("accountPlans", out List<AccountPlan> accountPlans))
            {
                accountPlans = _accountPlanService.List(new ListQuery { Page = null }).List;
                _memoryCache.Set("accountPlans", accountPlans,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(accountPlans);
        }

        [HttpPost]
        public async Task<IActionResult> ExpenseArticleTypes()
        {
            if (!_memoryCache.TryGetValue("expenseArticleTypes", out List<ExpenseArticleType> expenseArticleTypes))
            {
                expenseArticleTypes = _expenseArticleTypeService.List(new ListQuery { Page = null }).List;
                _memoryCache.Set("expenseArticleTypes", expenseArticleTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(expenseArticleTypes);
        }

        [HttpPost]
        public async Task<IActionResult> AccountSettings()
        {
            if (!_memoryCache.TryGetValue("accountSettings", out List<AccountSetting> accountSettings))
            {
                accountSettings = _accountSettingService.List(new ListQuery { Page = null }).List;
                _memoryCache.Set("accountSettings", accountSettings,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(accountSettings);
        }

        [HttpPost]
        public async Task<IActionResult> Types()
        {
            if (!_memoryCache.TryGetValue("types", out List<AccountingCore.Models.Type> types))
            {
                types = _typeService.List(new ListQuery { Page = null }).List;
                _memoryCache.Set("types", types,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(types);
        }

        [HttpPost]
        public async Task<IActionResult> BusinessOperations([FromBody] BusinessOperationFilter filter)
        {
            var businessOperations = _businessOperationService.List(new Services.Models.List.ListQueryModel<BusinessOperationFilter>() { Page = null, Model = filter }).List;
            return Ok(businessOperations);
        }

        [HttpPost]
        public async Task<IActionResult> BusinessOperationSettings([FromBody] BusinessOperationSettingFilter filter)
        {
            var businessOperationSettings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>() { Page = null, Model = filter }).List;
            return Ok(businessOperationSettings);
        }

        [HttpPost]
        public async Task<IActionResult> Purities()
        {
            if (!_memoryCache.TryGetValue("purities", out List<Purity> purities))
            {
                purities = _purityRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("purities", purities,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3) });
            }
            return Ok(purities);
        }

        [HttpPost]
        public async Task<IActionResult> EventCodes()
        {
            if (!_memoryCache.TryGetValue("eventCodes", out List<dynamic> eventCodes))
            {
                eventCodes = new List<dynamic>();
                foreach (EventCode item in Enum.GetValues(typeof(EventCode)))
                {
                    eventCodes.Add(new
                    {
                        Id = item,
                        Name = item.GetDisplayName()
                    });
                }
                _memoryCache.Set("eventCodes", eventCodes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(eventCodes);
        }

        [HttpPost]
        public async Task<IActionResult> ExpenseGroups()
        {
            if (!_memoryCache.TryGetValue("expenseGroups", out List<ExpenseGroup> expenseGroups))
            {
                expenseGroups = _expenseGroupRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("expenseGroups", expenseGroups,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(expenseGroups);
        }

        [HttpPost]
        public async Task<IActionResult> ExpenseTypes()
        {
            if (!_memoryCache.TryGetValue("expenseTypes", out ListModel<ExpenseType> expenseTypes))
            {
                expenseTypes = new ListModel<ExpenseType>
                {
                    List = await Task.Run(() => _expenseTypeRepository.List(new ListQuery() { Page = null })),
                    Count = await Task.Run(() => _expenseTypeRepository.Count(new ListQuery() { Page = null }))
                };
                _memoryCache.Set("expenseTypes", expenseTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(expenseTypes);
        }

        [HttpPost]
        public async Task<IActionResult> Organizations()
        {
            if (!_memoryCache.TryGetValue("organizations", out ListModel<Organization> organizations))
            {
                organizations = new ListModel<Organization>
                {
                    List = await Task.Run(() => _organizationRepository.List(new ListQuery() { Page = null })),
                    Count = await Task.Run(() => _organizationRepository.Count(new ListQuery() { Page = null }))
                };
                _memoryCache.Set("organizations", organizations,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(organizations);
        }

        [HttpPost]
        public async Task<IActionResult> BlackListReasons([FromBody] ListQueryModel<ReasonTypeListQueryModel> listQuery)
        {
            var blackListReasons = new ListModel<BlackListReason>
            {
                List = await Task.Run(() => _blackListReasonRepository.List(new ListQuery() { Page = null }, listQuery?.Model)),
                Count = await Task.Run(() => _blackListReasonRepository.Count(new ListQuery() { Page = null }, listQuery?.Model))
            };
            return Ok(blackListReasons);
        }

        [HttpPost]
        public async Task<IActionResult> Messages()
        {
            var messages = new ListModel<InnerNotification>
            {
                List = await Task.Run(() => _innerNotificationRepository.List(new ListQuery() { Page = null }, new { _sessionContext.UserId, BranchId = _branchContext.Branch.Id })),
                Count = await Task.Run(() => _innerNotificationRepository.Count(new ListQuery() { Page = null }, new { _sessionContext.UserId, BranchId = _branchContext.Branch.Id }))
            };
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> ClientExpenses()
        {
            if (!_memoryCache.TryGetValue("clientExpenses", out ListModel<Expense> clientExpenses))
            {
                clientExpenses = new ListModel<Expense>
                {
                    List = await Task.Run(() => _expenseService.GetList(new ListQuery() { Page = null })),
                    Count = await Task.Run(() => _expenseService.Count(new ListQuery() { Page = null }))
                };
                _memoryCache.Set("clientExpenses", clientExpenses,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(clientExpenses);
        }

        [HttpPost]
        public async Task<IActionResult> GetContractPool()
        {
            if (!_memoryCache.TryGetValue("contractPool", out ListModel<int> contractPool))
            {
                contractPool = new ListModel<int>
                {
                    List = await Task.Run(() => _transferContractRepository.PoolList()),
                    Count = await Task.Run(() => _transferContractRepository.PoolCount())
                };
                _memoryCache.Set("contractPool", contractPool,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }
            return Ok(contractPool);
        }

        [HttpPost]
        public async Task<IActionResult> AttractionChannels()
        {
            if (!_memoryCache.TryGetValue("attractionChannels", out List<AttractionChannel> attractionChannels))
            {
                attractionChannels = _attractionChannelRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("attractionChannels", attractionChannels,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(attractionChannels);
        }

        [HttpPost]
        public async Task<IActionResult> ParkingActions()
        {
            if (!_memoryCache.TryGetValue("parkingActions", out List<ParkingAction> parkingActions))
            {
                parkingActions = _parkingActionRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("parkingActions", parkingActions,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(parkingActions);
        }

        [HttpPost]
        public async Task<IActionResult> ParkingStatuses()
        {
            if (!_memoryCache.TryGetValue("parkingStatuses", out List<ParkingStatus> parkingStatuses))
            {
                parkingStatuses = _parkingStatusRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("parkingStatuses", parkingStatuses,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(parkingStatuses);
        }

        [HttpPost]
        public async Task<IActionResult> SociallyVulnerableGroups()
        {
            if (!_memoryCache.TryGetValue("sociallyVulnerableGroups", out List<SociallyVulnerableGroup> sociallyVulnerableGroups))
            {
                sociallyVulnerableGroups = _sociallyVulnerableGroupRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("sociallyVulnerableGroups", sociallyVulnerableGroups,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(sociallyVulnerableGroups);
        }

        [HttpPost]
        public async Task<IActionResult> Postponements()
        {
            if (!_memoryCache.TryGetValue("postponements", out List<Postponement> postponements))
            {
                postponements = _postponementRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("postponements", postponements,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(postponements);
        }

        [HttpPost]
        public async Task<IActionResult> Blackouts()
        {
            if (!_memoryCache.TryGetValue("blackouts", out List<Blackout> blackouts))
            {
                blackouts = _blackoutRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("blackouts", blackouts,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(blackouts);
        }

        [HttpPost]
        public async Task<IActionResult> PersonalDiscounts()
        {
            if (!_memoryCache.TryGetValue("personalDiscounts", out List<PersonalDiscount> personalDiscounts))
            {
                personalDiscounts = _personalDiscountRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("personalDiscounts", personalDiscounts,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(personalDiscounts);
        }

        [HttpPost]
        public async Task<IActionResult> RequisiteTypes()
        {
            if (!_memoryCache.TryGetValue("requisiteTypes", out List<RequisiteType> requisiteTypes))
            {
                requisiteTypes = _requisiteTypeRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("requisiteTypes", requisiteTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(requisiteTypes);
        }

        [HttpPost]
        public async Task<IActionResult> PayTypes([FromBody] ListQueryModel<PayTypeQueryModel> listQuery)
        {
            var payTypes = _payTypeRepository.List(new ListQuery() { Page = null }, listQuery?.Model);
            return Ok(payTypes);
        }

        [HttpPost]
        public async Task<IActionResult> ClientLegalForms()
        {
            if (!_memoryCache.TryGetValue("clientLegalForms", out List<ClientLegalForm> clientLegalForms))
            {
                clientLegalForms = _clientLegalFormRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("clientLegalForms", clientLegalForms,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(clientLegalForms);
        }

        [HttpPost]
        public async Task<IActionResult> ClientSignersAllowedDocumentTypes([FromBody] int? companyLegalFormId)
        {
            if (!_memoryCache.TryGetValue("clientSignersAllowedDocumentTypes", out List<ClientSignersAllowedDocumentType> clientSignersAllowedDocumentTypes))
            {
                clientSignersAllowedDocumentTypes = _clientSignersAllowedDocumentTypeRepository.List(new ListQuery() { Page = null }, new { CompanyLegalFormId = companyLegalFormId });
                _memoryCache.Set("clientSignersAllowedDocumentTypes", clientSignersAllowedDocumentTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(clientSignersAllowedDocumentTypes);
        }

        [HttpPost]
        public async Task<IActionResult> ClientDocumentTypes()
        {
            if (!_memoryCache.TryGetValue("clientDocumentTypes", out List<ClientDocumentType> clientDocumentTypes))
            {
                clientDocumentTypes = _clientDocumentTypeRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("clientDocumentTypes", clientDocumentTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(clientDocumentTypes);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyDocumentTypes()
        {
            if (!_memoryCache.TryGetValue("realtyDocumentTypes", out List<ClientDocumentType> realtyDocumentTypes))
            {
                realtyDocumentTypes = _clientDocumentTypeRepository.ListRealtyDocumentTypes();
                _memoryCache.Set("realtyDocumentTypes", realtyDocumentTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyDocumentTypes);
        }

        [HttpPost]
        public async Task<IActionResult> ClientDocumentProviders()
        {
            if (!_memoryCache.TryGetValue("clientDocumentProviders", out List<ClientDocumentProvider> clientDocumentProviders))
            {
                clientDocumentProviders = _clientDocumentProviderRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("clientDocumentProviders", clientDocumentProviders,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(clientDocumentProviders);
        }

        [HttpPost]
        public async Task<IActionResult> ClientLegalFormRequiredDocuments()
        {
            if (!_memoryCache.TryGetValue("clientLegalFormRequiredDocuments", out List<ClientLegalFormRequiredDocument> clientLegalFormRequiredDocuments))
            {
                clientLegalFormRequiredDocuments = _clientLegalFormRequiredDocumentRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("clientLegalFormRequiredDocuments", clientLegalFormRequiredDocuments,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(clientLegalFormRequiredDocuments);
        }

        [HttpPost]
        public async Task<IActionResult> Countries()
        {
            if (!_memoryCache.TryGetValue("countries", out List<Country> countries))
            {
                countries = _countryRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("countries", countries,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(countries);
        }

        [HttpPost]
        public async Task<IActionResult> ContractChecks()
        {
            if (!_memoryCache.TryGetValue("contractChecks", out List<ContractCheck> contractChecks))
            {
                contractChecks = _contractCheckRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("contractChecks", contractChecks,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(contractChecks);
        }

        [HttpPost]
        public async Task<IActionResult> Addresses([FromBody] int? parentId)
        {
            if (!_memoryCache.TryGetValue($"addresses_{parentId}", out List<Address> addresses))
            {
                addresses = _addressRepository.Find(new { ParentId = parentId });
                _memoryCache.Set($"addresses_{parentId}", addresses,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> AddressTypes()
        {
            if (!_memoryCache.TryGetValue("addressTypes", out List<AddressType> addressTypes))
            {
                addressTypes = _addressTypeRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("addressTypes", addressTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(addressTypes);
        }

        [HttpPost]
        public async Task<IActionResult> ClientEconomicActivityTypes([FromBody] int? parentId)
        {
            if (!_memoryCache.TryGetValue($"clientEconomicActivityTypes_{parentId}", out List<ClientEconomicActivityType> clientEconomicActivityTypes))
            {
                clientEconomicActivityTypes = _clientEconomicActivityTypeRepository.Find(new { ParentId = parentId });
                _memoryCache.Set($"clientEconomicActivityTypes_{parentId}", clientEconomicActivityTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(clientEconomicActivityTypes);
        }

        [HttpPost]
        public async Task<IActionResult> LoanSubjects()
        {
            if (!_memoryCache.TryGetValue("loanSubjects", out List<LoanSubject> loanSubjects))
            {
                loanSubjects = _loanSubjectRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("loanSubjects", loanSubjects,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(loanSubjects);
        }

        [HttpPost]
        public async Task<IActionResult> LoanProductTypes()
        {
            if (!_memoryCache.TryGetValue("loanProductTypes", out List<LoanProductType> loanProductTypes))
            {
                loanProductTypes = _loanProductTypeRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("loanProductTypes", loanProductTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(loanProductTypes);
        }

        [HttpGet]
        public async Task<IActionResult> CreditLineLoanProductTypes()
        {
            if (!_memoryCache.TryGetValue("creditLineLoanProductTypes", out List<LoanProductType> creditLineLoanProductTypes))
            {
                creditLineLoanProductTypes = _loanProductTypeRepository.GetProductsByContractClass(Data.Models.Contracts.ContractClass.CreditLine);
                _memoryCache.Set("creditLineLoanProductTypes", creditLineLoanProductTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(creditLineLoanProductTypes);
        }

        [HttpPost]
        public async Task<IActionResult> Currencies()
        {
            if (!_memoryCache.TryGetValue("currencies", out List<Currency> currencies))
            {
                currencies = _currencyRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("currencies", currencies,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(currencies);
        }

        [HttpPost]
        public async Task<IActionResult> PrintTemplates()
        {
            if (!_memoryCache.TryGetValue("printTemplates", out List<PrintTemplate> printTemplates))
            {
                printTemplates = _printTemplateRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("printTemplates", printTemplates,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(printTemplates);
        }

        [HttpPost]
        public async Task<IActionResult> Products()
        {
            if (!_memoryCache.TryGetValue("products", out List<LoanPercentSetting> products))
            {
                products = _loanPercentRepository.List(new ListQuery() { Page = null },
                    new LoanPercentQueryModel
                    {
                        IsProduct = true,
                        OrganizationId = _sessionContext.OrganizationId,
                        BranchId = _branchContext.Branch.Id,
                        IsActual = true
                    }).Where(x => IsNeedViewProduct(x)).ToList();

                _memoryCache.Set("products", products,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreditLineProducts()
        {
            if (!_memoryCache.TryGetValue("creditLineProducts", out List<LoanPercentSetting> creditLineProducts))
            {
                creditLineProducts = _loanPercentRepository.List(new ListQuery() { Page = null },
                    new LoanPercentQueryModel
                    {
                        IsProduct = true,
                        OrganizationId = _sessionContext.OrganizationId,
                        BranchId = _branchContext.Branch.Id,
                        IsActual = true,
                        ContractClass = Data.Models.Contracts.ContractClass.CreditLine
                    }).Where(x => IsNeedViewProduct(x)).ToList();

                _memoryCache.Set("creditLineProducts", creditLineProducts,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(creditLineProducts);
        }

        [HttpGet]
        public async Task<IActionResult> NonCreditLineProducts()
        {
            if (!_memoryCache.TryGetValue("nonCreditLineProducts", out List<LoanPercentSetting> nonCreditLineProducts))
            {
                nonCreditLineProducts = _loanPercentRepository.List(new ListQuery() { Page = null },
                    new LoanPercentQueryModel
                    {
                        IsProduct = true,
                        OrganizationId = _sessionContext.OrganizationId,
                        BranchId = _branchContext.Branch.Id,
                        IsActual = true,
                        ContractClass = Data.Models.Contracts.ContractClass.Credit
                    }).Where(x => IsNeedViewProduct(x)).ToList();

                _memoryCache.Set("nonCreditLineProducts", nonCreditLineProducts,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(nonCreditLineProducts);
        }

        private bool IsNeedViewProduct(LoanPercentSetting product)
        {
            var exceptProductTypes = new List<string> { "TSO_MIGRATION", "TAS_ONLINE" };

            if (product.ProductType == null)
                return true;

            return !exceptProductTypes.Contains(product.ProductType.Code);
        }

        [HttpPost]
        public async Task<IActionResult> ContractActionTypes()
        {
            if (!_memoryCache.TryGetValue("contractActionTypes", out List<ContractActionTypeInfo> contractActionTypes))
            {
                contractActionTypes = new ContractActionTypes().All();
                _memoryCache.Set("contractActionTypes", contractActionTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(contractActionTypes);
        }

        [HttpPost]
        public async Task<IActionResult> VehicleMarks()
        {
            if (!_memoryCache.TryGetValue("vehicleMarks", out List<VehicleMark> vehicleMarks))
            {
                vehicleMarks = _vehicleMarkRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("vehicleMarks", vehicleMarks,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(vehicleMarks);
        }

        [HttpPost]
        public async Task<IActionResult> VehicleModels([FromBody] int? vehicleMarkId)
        {
            if (vehicleMarkId == null)
            {
                if (!_memoryCache.TryGetValue("vehicleModels", out List<VehicleModel> vehicleModels))
                {
                    vehicleModels = _vehicleModelRepository.List(new ListQuery { Page = null });
                    _memoryCache.Set("vehicleModels", vehicleModels,
                        new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
                }
                return Ok(vehicleModels);
            }
            else
            {
                var vehicleModels = _vehicleModelRepository.List(new ListQuery { Page = null }, new { VehicleMarkId = vehicleMarkId });
                return Ok(vehicleModels);
            }
        }

        [HttpPost]
        public async Task<IActionResult> VehicleWMIs()
        {
            if (!_memoryCache.TryGetValue("vehicleWMIs", out List<VehicleWMI> vehicleWMIs))
            {
                vehicleWMIs = _vehicleWMIRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("vehicleWMIs", vehicleWMIs,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(vehicleWMIs);
        }

        [HttpPost]
        public async Task<IActionResult> VehicleManufacturers()
        {
            if (!_memoryCache.TryGetValue("vehicleManufacturers", out List<VehicleManufacturer> vehicleManufacturers))
            {
                vehicleManufacturers = _vehicleManufacturerRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("vehicleManufacturers", vehicleManufacturers,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(vehicleManufacturers);
        }

        [HttpPost]
        public async Task<IActionResult> VehicleCountryCodes()
        {
            if (!_memoryCache.TryGetValue("vehicleCountryCodes", out List<VehicleCountryCode> vehicleCountryCodes))
            {
                vehicleCountryCodes = _vehicleCountryCodeRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("vehicleCountryCodes", vehicleCountryCodes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(vehicleCountryCodes);
        }

        [HttpPost]
        public async Task<IActionResult> CarColors()
        {
            if (!_memoryCache.TryGetValue("carColors", out List<string> carColors))
            {
                carColors = _carRepository.Colors();
                _memoryCache.Set("carColors", carColors,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(carColors);
        }

        [HttpPost]
        public async Task<IActionResult> MachineryColors()
        {
            if (!_memoryCache.TryGetValue("machineryColors", out List<string> machineryColors))
            {
                machineryColors = _machineryRepository.Colors();
                _memoryCache.Set("machineryColors", machineryColors,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(machineryColors);
        }

        [HttpPost]
        public async Task<IActionResult> Accounts([FromBody] AccountFilter model)
        {
            var accounts = _accountService.List(new Services.Models.List.ListQueryModel<AccountFilter>() { Page = null, Model = model }).List;
            return Ok(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> AccountRecords([FromBody] ListQueryModel<AccountRecordFilter> model)
        {
            var accountRecords = _accountRecordService.List(model);
            return Ok(accountRecords);
        }

        [HttpPost]
        public async Task<IActionResult> VehiclesBlackList()
        {
            if (!_memoryCache.TryGetValue("vehiclesBlackList", out List<VehiclesBlackListItem> vehiclesBlackList))
            {
                vehiclesBlackList = _vehicleBlackListRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("vehiclesBlackList", vehiclesBlackList,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(vehiclesBlackList);
        }

        [HttpPost]
        public async Task<IActionResult> AmountTypes()
        {
            if (!_memoryCache.TryGetValue("amountTypes", out List<AmountTypeInfo> amountTypes))
            {
                amountTypes = new AmountTypes().All();
                _memoryCache.Set("amountTypes", amountTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(amountTypes);
        }

        [HttpPost]
        public async Task<IActionResult> OrderTypes()
        {
            if (!_memoryCache.TryGetValue("orderTypes", out List<OrderTypeInfo> orderTypes))
            {
                orderTypes = new OrderTypes().All();
                _memoryCache.Set("orderTypes", orderTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(orderTypes);
        }

        [HttpPost]
        public async Task<IActionResult> CollateralTypes()
        {
            if (!_memoryCache.TryGetValue("collateralTypes", out List<CollateralTypesInfo> collateralTypes))
            {
                collateralTypes = new CollateralTypes().All();
                _memoryCache.Set("collateralTypes", collateralTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(collateralTypes);
        }

        [HttpPost]
        public async Task<IActionResult> Holidays()
        {
            if (!_memoryCache.TryGetValue("holidays", out List<Holiday> holidays))
            {
                holidays = _holidayRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("holidays", holidays,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(holidays);
        }

        [HttpPost]
        public async Task<IActionResult> InsuranceRates()
        {
            if (!_memoryCache.TryGetValue("insuranceRates", out List<InsuranceRate> insuranceRates))
            {
                insuranceRates = _insuranceRateRepository.List(new ListQuery() { Page = null });
                _memoryCache.Set("insuranceRates", insuranceRates,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return Ok(insuranceRates);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyTypes()
        {
            if (!_memoryCache.TryGetValue("realtyTypes", out List<DomainValue> realtyTypes))
            {
                realtyTypes = _realtyService.GetRealtyTypes();
                _memoryCache.Set("realtyTypes", realtyTypes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyTypes);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyPurposes()
        {
            if (!_memoryCache.TryGetValue("realtyPurposes", out List<DomainValue> realtyPurposes))
            {
                realtyPurposes = _realtyService.GetRealtyPurpose();
                _memoryCache.Set("realtyPurposes", realtyPurposes,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyPurposes);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyWallMaterials()
        {
            if (!_memoryCache.TryGetValue("realtyWallMaterials", out List<DomainValue> realtyWallMaterials))
            {
                realtyWallMaterials = _realtyService.GetRealtyWallMaterial();
                _memoryCache.Set("realtyWallMaterials", realtyWallMaterials,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyWallMaterials);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyLightning()
        {
            if (!_memoryCache.TryGetValue("realtyLightning", out List<DomainValue> realtyLightning))
            {
                realtyLightning = _realtyService.GetRealtyLightning();
                _memoryCache.Set("realtyLightning", realtyLightning,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyLightning);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyColdWaterSupply()
        {
            if (!_memoryCache.TryGetValue("realtyColdWaterSupply", out List<DomainValue> realtyColdWaterSupply))
            {
                realtyColdWaterSupply = _realtyService.GetRealtyColdWaterSupply();
                _memoryCache.Set("realtyColdWaterSupply", realtyColdWaterSupply,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyColdWaterSupply);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyGasSupply()
        {
            if (!_memoryCache.TryGetValue("realtyGasSupply", out List<DomainValue> realtyGasSupply))
            {
                realtyGasSupply = _realtyService.GetRealtyGasSupply();
                _memoryCache.Set("realtyGasSupply", realtyGasSupply,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyGasSupply);
        }

        [HttpPost]
        public async Task<IActionResult> RealtySanitation()
        {
            if (!_memoryCache.TryGetValue("realtySanitation", out List<DomainValue> realtySanitation))
            {
                realtySanitation = _realtyService.GetRealtySanitation();
                _memoryCache.Set("realtySanitation", realtySanitation,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtySanitation);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyHotWaterSupply()
        {
            if (!_memoryCache.TryGetValue("realtyHotWaterSupply", out List<DomainValue> realtyHotWaterSupply))
            {
                realtyHotWaterSupply = _realtyService.GetRealtyHotWaterSupply();
                _memoryCache.Set("realtyHotWaterSupply", realtyHotWaterSupply,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyHotWaterSupply);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyHeating()
        {
            if (!_memoryCache.TryGetValue("realtyHeating", out List<DomainValue> realtyHeating))
            {
                realtyHeating = _realtyService.GetRealtyHeating();
                _memoryCache.Set("realtyHeating", realtyHeating,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyHeating);
        }

        [HttpPost]
        public async Task<IActionResult> RealtyPhoneConnection()
        {
            if (!_memoryCache.TryGetValue("realtyPhoneConnection", out List<DomainValue> realtyPhoneConnection))
            {
                realtyPhoneConnection = _realtyService.GetRealtyPhoneConnection();
                _memoryCache.Set("realtyPhoneConnection", realtyPhoneConnection,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(realtyPhoneConnection);
        }

        [HttpPost]
        public async Task<IActionResult> GetVpm()
        {
            if (!_memoryCache.TryGetValue("vpm", out NotionalRate vpm))
            {
                vpm = _realtyService.GetVpm();
                _memoryCache.Set("vpm", vpm,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(vpm);
        }

        [HttpPost]
        public async Task<IActionResult> Languages()
        {
            if (!_memoryCache.TryGetValue("languages", out List<Language> languages))
            {
                languages = _languagesRepository.List(new ListQuery());
                _memoryCache.Set("languages", languages,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(languages);
        }

        [HttpPost]
        public async Task<IActionResult> Actions()
        {
            if (!_memoryCache.TryGetValue("collectionActions", out List<CollectionActions> actions))
            {
                actions = await _collectionActionHttpService.List();
                _memoryCache.Set("collectionActions", actions,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(actions);
        }

        [HttpPost]
        public async Task<IActionResult> Statuses()
        {
            if (!_memoryCache.TryGetValue("collectionStatuses", out List<CollectionStatus> statuses))
            {
                statuses = await _collectionStatusHttpService.List();
                _memoryCache.Set("collectionStatuses", statuses,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(statuses);
        }

        [HttpPost]
        public async Task<IActionResult> Reasons()
        {
            if (!_memoryCache.TryGetValue("reasons", out List<CollectionReason> reasons))
            {
                reasons = await _collectionReasonHttpService.List();
                _memoryCache.Set("collectionReasons", reasons,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(reasons);
        }
        
        [HttpPost]
        public async Task<List<LegalCaseStatusDto>> LegalCollectionStatuses() =>
            await Task.Run(() => _legalCollectionStatuses.List());
        
        [HttpPost]
        public async Task<LegalCaseStagesList> LegalCollectionStages() =>
            await Task.Run(() => _legalCollectionStages.List());
        
        [HttpPost]
        public async Task<LegalCaseCourseList> LegalCollectionCourses() =>
            await Task.Run(() => _legalCollectionCourses.List());
        
        [HttpPost]
        public async Task<LegalCaseActionsList> LegalCollectionActions() =>
            await Task.Run(() => _legalCollectionActions.List());

        [HttpPost]
        public IActionResult GroupsOnline()
        {
            if (!_memoryCache.TryGetValue("groups", out List<Group> groupsOnline))
            {
                groupsOnline = _groupRepository.ListOnline();

                _memoryCache.Set("groupsOnline", groupsOnline,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }
            return Ok(groupsOnline);
        }
    }
}