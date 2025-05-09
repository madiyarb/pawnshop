using Autofac;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.TMF;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Audit;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts.LoanFinancePlans;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Dictionaries;
using Pawnshop.Services.Discounts;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Insurance.InsuranceCompanies;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Integrations.Online1C;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Migrate;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.OnlineApplications;
using Pawnshop.Services.PenaltyLimit;
using Pawnshop.Services.Positions;
using Pawnshop.Services.Realties;
using Pawnshop.Services.Refinance;
using Pawnshop.Services.Remittances;
using Pawnshop.Services.Reports;
using Pawnshop.Services.TasOnline;
using Pawnshop.Services.TMF;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.ApplicationOnlineSms;
using Pawnshop.Services.Contracts.PartialPayment;
using Pawnshop.Services.CreditLines;
using ExpenseArticleType = Pawnshop.AccountingCore.Models.ExpenseArticleType;
using Pawnshop.Services.Collection;
using Pawnshop.Services.CBBatches;
using Pawnshop.Services.CreditLines.Buyout;
using Pawnshop.Services.CreditLines.PartialPayment;
using Pawnshop.Services.CreditLines.Payment;
using Pawnshop.Services.DebtorRegisrty;
using Pawnshop.Services.DebtorRegisrty.CourtOfficer;
using Pawnshop.Services.DebtorRegisrty.CourtOfficer.Interfaces;
using Pawnshop.Services.DebtorRegisrty.Interfaces;
using Pawnshop.Services.LegalCollection;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.LoanPercent;
using Pawnshop.Services.KFM;
using Pawnshop.Services.MaximumLoanTermDetermination;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.ClientGeoPositions;
using Pawnshop.Services.DebtorRegistry;
using Pawnshop.Services.OTP;
using Pawnshop.Services.PDF;
using Pawnshop.Services.Interactions;
using Pawnshop.Services.SUSN;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Services.Parkings.History;
using Pawnshop.Services.HardCollection.Command;
using Pawnshop.Services.HardCollection.Command.Interfaces;
using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Services.HardCollection.HttpClientService.Impl;
using Pawnshop.Services.HardCollection.HttpClientService.Interfaces;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Services.HardCollection.Query;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using System.Collections.Generic;
using System.Net.Http;
using Pawnshop.Services.ApplicationOnlineFiles;
using Pawnshop.Services.HardCollection.Service.Impl;
using Pawnshop.Services.HardCollection.Service.Interfaces;
using Pawnshop.Services.Kato;
using Pawnshop.Services.TasLabBankrupt;
using Pawnshop.Services.CardCashOut;
using Pawnshop.Services.ClientExternalValidation;
using Pawnshop.Services.CollateralTypes;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Services.Regions;
using Pawnshop.Services.ApplicationsOnline.ApplicationOnlineCreditLimitVerification;
using Pawnshop.Services.CollateralTypes;
using Pawnshop.Services.Regions;
using Pawnshop.Services.Auction;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.CashOrderRemittances;
using Pawnshop.Services.ManualUpdate;
using Pawnshop.Services.Gamblers;
using Pawnshop.Services.Notifications;
using Pawnshop.Services.Contracts.ContractActionOnlineExecutionCheckerService;
using Pawnshop.Services.TasLabRecruit;
using Pawnshop.Services.ClientDeferments.Impl;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.Restructuring;
using Pawnshop.Services.LegalCollectionCalculation;
using Pawnshop.Services.LegalCollectionCalculations;
using Pawnshop.Services.TasCore;

namespace Pawnshop.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c => new HttpClient())
                .As<HttpClient>();

            // Учетное ядро(AccountingCore)
            builder.RegisterType<TypeService>().As<IDictionaryService<Type>>().AsSelf();
            builder.RegisterType<BusinessOperationService>().As<IBusinessOperationService>().As<IDictionaryWithSearchService<BusinessOperation, BusinessOperationFilter>>().AsSelf();
            builder.RegisterType<BusinessOperationSettingService>().As<IBusinessOperationSettingService>().As<IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter>>().AsSelf();
            builder.RegisterType<AccountPlanService>().As<IDictionaryService<AccountPlan>>().AsSelf();
            builder.RegisterType<ExpenseArticleTypeService>().As<IDictionaryService<ExpenseArticleType>>().AsSelf();
            builder.RegisterType<AccountSettingService>().As<IDictionaryWithSearchService<AccountSetting, AccountSettingFilter>>().AsSelf();
            builder.RegisterType<AccountPlanSettingService>().As<IAccountPlanSettingService>().As<IDictionaryWithSearchService<AccountPlanSetting, AccountPlanSettingFilter>>().AsSelf();
            builder.RegisterType<AccountBuilderService>().As<IAccountBuilderService>().AsSelf();
            builder.RegisterType<AccountService>().As<IDictionaryWithSearchService<Account, AccountFilter>>().As<IAccountService>().AsSelf();
            builder.RegisterType<CashOrderService>().As<ICashOrderService>().As<IDictionaryWithSearchService<CashOrder, CashOrderFilter>>().AsSelf();
            builder.RegisterType<AccountRecordService>().As<IAccountRecordService>().AsSelf();
            builder.RegisterType<PaymentOrderService>().As<IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter>>().AsSelf();
            builder.RegisterType<AccrualBaseService>().As<IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter>>().AsSelf();
            builder.RegisterType<RemittanceSettingService>().As<IDictionaryWithSearchService<RemittanceSetting, RemittanceSettingFilter>>().AsSelf();
            builder.RegisterType<TakeAwayToDelayService>().As<ITakeAwayToDelay>().AsSelf();
            builder.RegisterType<PenaltyAccrualService>().As<IPenaltyAccrual>().AsSelf();
            builder.RegisterType<ContractPaymentService>().As<IContractPaymentService>().AsSelf();
            builder.RegisterType<ContractActionPartialPaymentService>().As<IContractActionPartialPaymentService>().InstancePerDependency();
            //миграции
            builder.RegisterType<MigrateContractActionService>().AsSelf();
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Начисление процентов
            builder.RegisterType<InterestAccrualService>().As<IInterestAccrual>().AsSelf();
            // Логирование действий
            builder.RegisterType<EventLogService>().As<IDictionaryWithSearchService<EventLogItem, EventLogFilter>>().As<IEventLog>().AsSelf();
            // Договора
            builder.RegisterType<ContractActionService>().As<IContractActionService>().As<IDictionaryWithSearchService<ContractAction, ContractActionFilter>>().AsSelf();
            builder.RegisterType<ContractService>().As<IContractService>().AsSelf();
            builder.RegisterType<CheckService>().As<IContractActionCheckService>().AsSelf();
            builder.RegisterType<CheckService>().As<IContractCheckService>().AsSelf();
            builder.RegisterType<ContractExpenseService>().As<IContractExpenseService>();
            builder.RegisterType<PostponementService>().As<IPostponementService>().AsSelf();
            builder.RegisterType<ContractVerificationService>().As<IContractVerificationService>().InstancePerDependency();
            builder.RegisterType<ContractStatusHistoryService>().As<IContractStatusHistoryService>().AsSelf();
            //Справочники
            builder.RegisterType<BranchService>().As<IDictionaryWithSearchService<Group, BranchFilter>>().AsSelf();
            builder.RegisterType<HolidayService>().As<IHolidayService>().As<IDictionaryWithSearchService<Holiday, HolidayFilter>>().AsSelf();
            builder.RegisterType<CurrencyService>().As<IDictionaryWithSearchService<Currency, CurrencyFilter>>().AsSelf();
            builder.RegisterType<BlackoutService>().As<IDictionaryWithSearchService<Blackout, BlackoutFilter>>().AsSelf();
            //CRM
            builder.RegisterType<CrmPaymentService>().As<ICrmPaymentService>().AsSelf();
            // Расходы
            builder.RegisterType<ExpenseService>().As<IExpenseService>().AsSelf();
            builder.RegisterType<RemittanceService>().As<IRemittanceService>().AsSelf();
            builder.RegisterType<ContractExpenseOperationService>().As<IContractExpenseOperationService>().AsSelf();
            builder.RegisterType<ContractActionOperationService>().As<IContractActionOperationService>().AsSelf();
            builder.RegisterType<DeleteExpenseService>().As<IDeleteExpenseService>().AsSelf();
            
            //Исполнительная надпись
            builder.RegisterType<InscriptionService>().As<IInscriptionService>().AsSelf();
            builder.RegisterType<ContractPaymentService>().As<IContractPaymentService>().AsSelf();
            builder.RegisterType<ContractActionOperationPermisisonService>()
                .As<IContractActionOperationPermisisonService>();
            builder.RegisterType<InscriptionOffBalanceAdditionService>().As<IInscriptionOffBalanceAdditionService>().AsSelf();

            // Калькуляторы
            builder.RegisterType<ContractAmount>().As<IContractAmount>().InstancePerDependency();
            builder.RegisterType<ContractActionRowBuilder>().As<IContractActionRowBuilder>().InstancePerDependency();
            builder.RegisterType<SellingRowBuilder>().As<ISellingRowBuilder>().InstancePerDependency();
            builder.RegisterType<ContractDutyService>().As<IContractDutyService>().InstancePerDependency();
            builder.RegisterType<ContractlDiscountService>().As<IContractDiscountService>();
            builder.RegisterType<DiscountService>().As<IDiscountService>();
            builder.RegisterType<ContractPaymentScheduleService>().As<IContractPaymentScheduleService>();
            builder.RegisterType<CalculationLegalCollectionAmountsService>().As<ICalculationLegalCollectionAmountsService>().AsSelf();

            // Гэмблинг
            builder.RegisterType<FCBChecksService>().As<IFCBChecksService>();

            //Реализация
            builder.RegisterType<ContractActionSellingService>().As<IContractActionSellingService>().AsSelf();

            builder.RegisterType<BaseService<ClientSignersAllowedDocumentType>>().As<IBaseService<ClientSignersAllowedDocumentType>>().AsSelf();
            builder.RegisterType<BaseService<ContractProfile>>().As<IBaseService<ContractProfile>>().AsSelf();
            builder.RegisterType<BaseService<ClientEconomicActivity>>().As<IBaseService<ClientEconomicActivity>>().AsSelf();
            builder.RegisterType<LoanFinancePlanSerivce>().As<ILoanFinancePlanSerivce>().As<IBaseService<LoanFinancePlan>>().AsSelf();
            builder.RegisterType<DomainService>().As<IDomainService>().InstancePerDependency();
            builder.RegisterType<ClientEconomicActivityService>().As<IClientEconomicActivityService>().InstancePerDependency();
            builder.RegisterType<ClientService>().As<IClientService>().InstancePerDependency();
            builder.RegisterType<ClientModelValidateService>().As<IClientModelValidateService>().InstancePerDependency();

            //TasOnline
            builder.RegisterType<TasOnlineRequestService>().As<ITasOnlineRequestService>().As<IBaseService<TasOnlineRequest>>().AsSelf();
            builder.RegisterType<TasOnlinePaymentApi>().As<ITasOnlinePaymentApi>().AsSelf();

            //TMF
            builder.RegisterType<TMFRequestService>().As<ITMFRequestService>().As<IBaseService<TMFRequest>>().AsSelf();
            builder.RegisterType<TMFRequestApi>().As<ITMFRequestApi>().AsSelf();


            //Страхование
            builder.RegisterType<InsuranceOnlineRequestService>().As<IInsuranceOnlineRequestService>().As<IBaseService<InsuranceOnlineRequest>>().AsSelf();
            builder.RegisterType<InsurancePoliceRequestService>().As<IInsurancePoliceRequestService>().As<IBaseService<InsurancePoliceRequest>>().AsSelf();
            builder.RegisterType<InsurancePolicyService>().As<IInsurancePolicyService>().As<IBaseService<InsurancePolicy>>().AsSelf();
            builder.RegisterType<InsuranceCompanyServiceFactory>().As<IInsuranceCompanyServiceFactory>().AsSelf();
            builder.RegisterType<InsurancePremiumCalculator>().As<IInsurancePremiumCalculator>().AsSelf();
            builder.RegisterType<InsuranceCompanySettingService>().As<IInsuranceCompanySettingService>().AsSelf();
            builder.RegisterType<InsuranceReviseService>().As<IInsuranceReviseService>().AsSelf();
            builder.RegisterType<InsuranceService>().As<IInsuranceService>().AsSelf();

            //Лимиты пени
            builder.RegisterType<ContractRateService>().As<IContractRateService>().AsSelf();
            builder.RegisterType<PenaltyLimitAccrualService>().As<IPenaltyLimitAccrualService>().AsSelf();
            builder.RegisterType<ContractCloseService>().As<IContractCloseService>().AsSelf();
            builder.RegisterType<PenaltyRateService>().As<IPenaltyRateService>().AsSelf();

            //Vehcile
            builder.RegisterType<VehcileService>().As<IVehcileService>().AsSelf();
            builder.RegisterType<CarService>().As<ICarService>().AsSelf();
            builder.RegisterType<MachineryService>().As<IMachineryService>().AsSelf();
            builder.RegisterType<VehcileBlackListService>().As<IVehcileBlackListService>().AsSelf();

            //Realty
            builder.RegisterType<RealtyService>().As<IRealtyService>().AsSelf();

            builder.RegisterType<PositionSubjectService>().As<IPositionSubjectService>().AsSelf();
            builder.RegisterType<PositionService>().As<IPositionService>().AsSelf();
            builder.RegisterType<PositionEstimateHistoryService>().As<IPositionEstimateHistoryService>().AsSelf();

            builder.RegisterType<ApplicationComparer>().InstancePerDependency();

            builder.RegisterType<ClientBlackListService>().As<IClientBlackListService>().InstancePerDependency();
            builder.RegisterType<ApplicationService>().As<IApplicationService>().InstancePerDependency();

            builder.RegisterType<ProcessingService>().As<IProcessingService>().AsSelf();

            //UKassa
            builder.RegisterType<UKassaService>().As<IUKassaService>().AsSelf();
            builder.RegisterType<UKassaReportsService>().As<IUKassaReportsService>().AsSelf();

            builder.RegisterType<ProcessingService>().As<IProcessingService>().AsSelf();

            //KDN
            builder.RegisterType<ContractKdnService>().As<IContractKdnService>().AsSelf();
            builder.RegisterType<ClientIncomeService>().As<IClientIncomeService>().InstancePerDependency();
            builder.RegisterType<ClientOtherPaymentsInfoService>().As<IClientOtherPaymentsInfoService>().AsSelf();

            //Ликвидность автотранспорта
            builder.RegisterType<VehicleLiquidityService>().As<IVehicleLiquidityService>().AsSelf();
            builder.RegisterType<ContractPeriodVehicleLiquidityService>().As<IContractPeriodVehicleLiquidityService>().AsSelf();

            //Сервис для модели авто
            builder.RegisterType<VehicleModelService>().As<IVehicleModelService>().AsSelf();
            builder.RegisterType<ManualCalculationClientExpenseService>().As<IManualCalculationClientExpenseService>().AsSelf();

            //Отчеты
            builder.RegisterType<BalanceSheetReportService>().As<IBalanceSheetReportService>().AsSelf();

            builder.RegisterType<VehicleMarkService>().As<IVehicleMarkService>().AsSelf();

            builder.RegisterType<AbsOnlineService>().As<IAbsOnlineService>().AsSelf();
            builder.RegisterType<AbsOnlineContractsService>().As<IAbsOnlineContractsService>().AsSelf();
            builder.RegisterType<AbsOnlineClientsService>().As<IAbsOnlineClientsService>().AsSelf();

            builder.RegisterType<OnlineApplicationService>().As<IOnlineApplicationService>().AsSelf();
            builder.RegisterType<OnlineApplicationCarService>().As<IOnlineApplicationCarService>().AsSelf();

            // Рефинансирование
            builder.RegisterType<RefinanceService>().As<IRefinanceService>().AsSelf();
            builder.RegisterType<RefinanceBuyOutService>().As<IRefinanceBuyOutService>().AsSelf();

            //Online1C
            builder.RegisterType<Online1CService>().As<IOnline1CService>().AsSelf();

            //Действия по парковкам
            builder.RegisterType<ParkingActionService>().As<IParkingActionService>().AsSelf();
            builder.RegisterType<ParkingHistoryService>().As<IParkingHistoryService>().AsSelf();

            //Действия по коллекшн
            builder.RegisterType<CollectionService>().As<ICollectionService>().AsSelf();
            builder.RegisterType<HttpSender>().As<IHttpSender>().AsSelf();

            // CollateralType
            builder.RegisterType<CollateralTypeService>().As<ICollateralTypeService>().AsSelf();

            // действия по Legal-Collection
            builder.RegisterType<LegalCollectionDocumentTypeService>().As<ILegalCollectionDocumentTypeService>().AsSelf();
            builder.RegisterType<LegalCollectionsFilteringService>().As<ILegalCollectionsFilteringService>().AsSelf();
            builder.RegisterType<GetRegionsService>().As<IGetRegionsService>().AsSelf();
            builder.RegisterType<LegalCollectionTaskStatusService>().As<ILegalCollectionTaskStatusService>().AsSelf();
            builder.RegisterType<LegalCollectionDocumentTypeService>().As<ILegalCollectionDocumentTypeService>().AsSelf();
            builder.RegisterType<LegalCollectionUpdateService>().As<ILegalCollectionUpdateService>().AsSelf();
            builder.RegisterType<LegalCollectionCreateService>().As<ILegalCollectionCreateService>().AsSelf();
            builder.RegisterType<LegalCollectionCloseService>().As<ILegalCollectionCloseService>().AsSelf();
            builder.RegisterType<LegalCollectionActionOptionsService>().As<ILegalCollectionActionOptionsService>().AsSelf();
            builder.RegisterType<CancelCloseLegalCollectionService>().As<ICancelCloseLegalCollectionService>().AsSelf();
            builder.RegisterType<LegalCollectionDetailsService>().As<ILegalCollectionDetailsService>().AsSelf();
            builder.RegisterType<LegalCollectionsFilteringService>().As<ILegalCollectionsFilteringService>().AsSelf();
            builder.RegisterType<LegalCasesDetailConverter>().As<ILegalCasesDetailConverter>().AsSelf();
            builder.RegisterType<LegalCollectionPrintTemplateService>().As<ILegalCollectionPrintTemplateService>().AsSelf();
            builder.RegisterType<LegalCollectionDocumentsService>().As<ILegalCollectionDocumentsService>().AsSelf();
            builder.RegisterType<LegalCollectionChangeCourseService>().As<ILegalCollectionChangeCourseService>().AsSelf();
            builder.RegisterType<LegalCollectionCheckClientDeathService>().As<ILegalCollectionCheckClientDeathService>().AsSelf();
            builder.RegisterType<LegalCollectionNotificationService>().As<ILegalCollectionNotificationService>().AsSelf();
            builder.RegisterType<ContractExpensesService>().As<IContractExpensesService>().AsSelf();

            // Реестр должников
            builder.RegisterType<FilteredDebtRegistryService>().As<IFilteredDebtRegistryService>().AsSelf();
            builder.RegisterType<FilteredFilteredCourtOfficersService>().As<IFilteredCourtOfficersService>().AsSelf();
            builder.RegisterType<DebtorRegisterDetailsService>().As<IDebtorRegisterDetailsService>().AsSelf();

            // регионы
            builder.RegisterType<RegionService>().As<IRegionService>().AsSelf();
            
            // Auction
            builder.RegisterType<RegisterAuctionExpenseService>().As<IRegisterAuctionExpenseService>().AsSelf();
            builder.RegisterType<RegisterAuctionSaleService>().As<IRegisterAuctionSaleService>().AsSelf();
            builder.RegisterType<CalculationAuctionAmountsService>().As<ICalculationAuctionAmountsService>().AsSelf();
            builder.RegisterType<CreateAuctionService>().As<ICreateAuctionService>().AsSelf();
            builder.RegisterType<CarAuctionService>().As<ICarAuctionService>().AsSelf();
            builder.RegisterType<GetAuctionAmountsService>().As<IGetAuctionAmountsService>().AsSelf();
            builder.RegisterType<CreditLineBuyOutByAuctionService>().As<ICreditLineBuyOutByAuctionService>().AsSelf();
            builder.RegisterType<ContractBuyOutByAuctionService>().As<IContractBuyOutByAuctionService>().AsSelf();
            builder.RegisterType<CancelAuctionOperationService>().As<ICancelAuctionOperationService>().AsSelf();
            builder.RegisterType<GetAuctionDebtAmounts>().As<IGetAuctionDebtAmounts>().AsSelf();
            builder.RegisterType<GetAuctionAccountsService>().As<IGetAuctionAccountsService>().AsSelf();
            
            // CashOrderRemittance
            builder.RegisterType<CashOrderRemittanceService>().As<ICashOrderRemittanceService>().AsSelf();
            
            //CBBatches для ручного создания батчей
            builder.RegisterType<CBBatchesService>().As<ICBBatchesService>().AsSelf();

            //Кредитные линии
            builder.RegisterType<CreditLineService>().As<ICreditLineService>().AsSelf();
            builder.RegisterType<CreditLinePaymentService>().As<ICreditLinePaymentService>().AsSelf();
            builder.RegisterType<CreditLinesBuyoutService>().As<ICreditLinesBuyoutService>().AsSelf();
            builder.RegisterType<CreditLinePartialPaymentService>().As<ICreditLinePartialPaymentService>().AsSelf();

            builder.RegisterType<LoanPercentService>().As<ILoanPercentService>().AsSelf();

            builder.RegisterType<KFMService>().As<IKFMService>().AsSelf();

            //Сервис по определению максимального срока займа исходя из авто и выбранного продукта
            builder.RegisterType<MaximumLoanTermDeterminationService>().As<IMaximumLoanTermDeterminationService>()
                .AsSelf();

            //Файлы 
            builder.RegisterType<FileStorageService>().As<IFileStorageService>().AsSelf();
            builder.RegisterType<ApplicationOnlineFilesService>().As<IApplicationOnlineFilesService>();

            // Сервисы онлайн заявок
            builder.RegisterType<ApplicationOnlineKdnService>().As<IApplicationOnlineKdnService>().AsSelf();
            builder.RegisterType<ApplicationOnlineService>().As<IApplicationOnlineService>().AsSelf();

            // Сервис генерации OTP Кода 
            builder.RegisterType<OTPCodeGeneratorService>().As<IOTPCodeGeneratorService>().AsSelf();

            // Сервис отправки СМС ТАС онлайна
            builder.RegisterType<ApplicationOnlineSmsService>().As<IApplicationOnlineSmsService>().AsSelf();

            builder.RegisterType<PdfService>().As<IPdfService>().AsSelf();

            //Сервис определения максимального количества дней просрочки клиента
            builder.RegisterType<ClientExpiredSchedulesGetterService>()
                .As<IClientExpiredSchedulesGetterService>().AsSelf();
            //Геопозиция

            builder.RegisterType<ClientGeoPositionsService>().As<IClientGeoPositionsService>().AsSelf();

            //Рефинансирование 
            builder.RegisterType<ApplicationOnlineRefinancesService>().As<IApplicationOnlineRefinancesService>()
                .AsSelf();

            //Проверка заполнености полей заявки
            builder.RegisterType<ApplicationOnlineCheckerService>().As<IApplicationOnlineCheckerService>().AsSelf();

            //Сервис для обновления данных авто
            builder.RegisterType<ApplicationOnlineCarService>().As<IApplicationOnlineCarService>().AsSelf();

            //Взаимодействие
            builder.RegisterType<InteractionService>().As<IInteractionService>().AsSelf();

            //Сусн
            builder.RegisterType<TasLabSUSNService>().As<ITasLabSUSNService>();

            //Реестр должников
            builder.RegisterType<DebtorRegistryService>().As<IDebtorRegistryService>();

            builder.RegisterType<PaymentScheduleService>().As<IPaymentScheduleService>().AsSelf();

            //Сервис Создания Чеков 
            builder.RegisterType<ApplicationOnlineCheckCreationService>().As<IApplicationOnlineCheckCreationService>();
            //
            builder.RegisterType<ApplicationOnlineCreditLimitVerificationService>()
                .As<IApplicationOnlineCreditLimitVerificationService>();

            //Сервис ручных проверок онлайн заявок
            builder.RegisterType<ApplicationOnlineChecksService>().As<IApplicationOnlineChecksService>();


            //builder.RegisterType<UpdateCollectionToLegalHardCommand>().As<IRequest<bool>>().AsSelf();
            builder.RegisterType<UpdateCollectionToLegalHardCommandHandler>().As<IRequestHandler<UpdateCollectionToLegalHardCommand, bool>>().AsSelf();
            builder.RegisterType<UpdateNotSeizedCommandHandler>().As<IRequestHandler<UpdateNotSeizedCommand, bool>>().AsSelf();
            builder.RegisterType<UpdateNotLiveInAddressCommandHandler>().As<IRequestHandler<UpdateNotLiveInAddressCommand, bool>>().AsSelf();
            builder.RegisterType<CloseHardCollectionCommandHandler>().As<IRequestHandler<CloseHardCollectionCommand, bool>>().AsSelf();
            builder.RegisterType<AddGeoCommandHandler>().As<IRequestHandler<AddGeoCommand, int>>().AsSelf();
            builder.RegisterType<GetHistoryListQueryHandler>().As<IRequestHandler<GetHistoryListQuery, List<HCActionHistoryVM>>>().AsSelf();
            builder.RegisterType<CheckIsContractInHardCollectionQueryHandler>().As<IRequestHandler<CheckIsContractInHardCollectionQuery, bool>>().AsSelf();
            builder.RegisterType<AddCommentCommandHandler>().As<IRequestHandler<AddCommentCommand, bool>>().AsSelf();
            builder.RegisterType<UpdateFileCommandHandler>().As<IRequestHandler<UploadFileCommand, bool>>().AsSelf();
            builder.RegisterType<SendSmsCertCommandHandler>().As<IRequestHandler<SendSmsCertCommand, bool>>().AsSelf();
            builder.RegisterType<SmsVerificationCertCommandHandler>().As<IRequestHandler<SmsVerificationCertCommand, bool>>().AsSelf();
            builder.RegisterType<SmsVerficationWitnessCommandHandler>().As<IRequestHandler<SmsVerficationWitnessCommand, bool>>().AsSelf();
            builder.RegisterType<AddWitnessCommandHandler>().As<IRequestHandler<AddWitnessCommand, int>>().AsSelf();
            builder.RegisterType<AddClientAdditionalContactCommandHandler>().As<IRequestHandler<AddClientAdditionalContactCommand, int>>().AsSelf();
            builder.RegisterType<AddClientContactCommandHandler>().As<IRequestHandler<AddClientContactCommand, int>>().AsSelf();
            builder.RegisterType<AddClientAddressCommandHandler>().As<IRequestHandler<AddClientAddressCommand, int>>().AsSelf();
            builder.RegisterType<CheckInHardCollectionContractsQueryHandler>().As<IRequestHandler<CheckInHardCollectionContractsQuery, bool>>().AsSelf();
            builder.RegisterType<SendAdditionalContactCommandHandler>().As<IRequestHandler<SendAdditionalContactCommand, bool>>().AsSelf();
            builder.RegisterType<SendAddressCommandHandler>().As<IRequestHandler<SendAddressCommand, bool>>().AsSelf();
            builder.RegisterType<SendContactCommandHandler>().As<IRequestHandler<SendContactCommand, bool>>().AsSelf();
            builder.RegisterType<IsClientInHardCollectionQueryHandler>().As<IRequestHandler<IsClientInHardCollectionQuery, bool>>().AsSelf();
            builder.RegisterType<IsContractInHardCollectionQueryHandler>().As<IRequestHandler<IsContractInHardCollectionQuery, bool>>().AsSelf();
            builder.RegisterType<GetContractOnlyQueryHandler>().As<IRequestHandler<GetContractOnlyQuery, ContractDataOnly>>().AsSelf();
            builder.RegisterType<SendContractOnlyCommandHandler>().As<IRequestHandler<SendContractOnlyCommand, bool>>().AsSelf();
            builder.RegisterType<SendContractDataCommandHandler>().As<IRequestHandler<SendContractDataCommand, bool>>().AsSelf();
            builder.RegisterType<SendClosedContractCommandHandler>().As<IRequestHandler<SendClosedContractCommand, bool>>().AsSelf();
            builder.RegisterType<GetContractDataQueryHandler>().As<IRequestHandler<GetContractDataQuery, Data.Models.MobileApp.HardCollection.ViewModels.ContractData>>().AsSelf();
            builder.RegisterType<HardCollectionService>().As<IHardCollectionService>().AsSelf();
            builder.RegisterType<AddToDoMyListCommandHandler>().As<IRequestHandler<AddToDoMyListCommand, bool>>().AsSelf();
            builder.RegisterType<AddExpenceCommandHandler>().As<IRequestHandler<AddExpenceCommand, bool>>().AsSelf();

            builder.RegisterType<HistoryHandler>().As<INotificationHandler<HardCollectionNotification>>().AsSelf();
            builder.RegisterType<LogHandler>().As<INotificationHandler<HardCollectionNotification>>().AsSelf();
            builder.RegisterType<TelegramHttpSender>().As<ITelegramHttpSender>().AsSelf();
            builder.RegisterType<ManualUpdateService>().As<IManualUpdateService>().AsSelf();

            //Список банкротов
            builder.RegisterType<TasLabBankruptInfoService>().As<ITasLabBankruptInfoService>();

            //Подтверждение подписания и проведение операций
            builder.RegisterType<CardCashOutSignService>().As<ICardCashOutSignService>();

            //Проверка клиента во внешних сервисах
            builder.RegisterType<ClientExternalValidationService>().As<IClientExternalValidationService>();

            //Валидация ролей в зависимости от статуса заявки TASONLINE 
            builder.RegisterType<TasOnlinePermissionValidatorService>().As<ITasOnlinePermissionValidatorService>().AsSelf();

            //Сервис КАТО
            builder.RegisterType<KatoService>().As<IKatoService>().AsSelf();

            //Сервисы уведомлений
            builder.RegisterType<NotificationCenterService>()
                .As<INotificationCenterService>().SingleInstance();

            //Сервис проверки на выполнений действий онлайн 
            builder.RegisterType<ContractActionOnlineExecutionCheckerService>()
                .As<IContractActionOnlineExecutionCheckerService>();

            // TasCore JetPay
            builder.RegisterType<TasCoreJetPayService>().As<ITasCoreJetPayService>().AsSelf();

            // TasCore Npck
            builder.RegisterType<TasCoreNpckService>().As<ITasCoreNpckService>().AsSelf();

            //Реструктуризация
            builder.RegisterType<TasLabRecruitService>().As<ITasLabRecruitService>().AsSelf();
            builder.RegisterType<ClientDefermentService>().As<IClientDefermentService>().AsSelf();
            builder.RegisterType<ClientDefermentsTelegramService>().As<IClientDefermentsTelegramService>().AsSelf();
            builder.RegisterType<RestructuringService>().As<IRestructuringService>().AsSelf();

            //Сервис запроса балансов онлайн
            builder.RegisterType<ContractBalancesService>().As<IContractBalancesService>();
        }
    }
}
