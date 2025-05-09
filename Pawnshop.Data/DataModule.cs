using Autofac;
using Microsoft.AspNetCore.Http;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Access.Auction;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Access.Reports;
using Pawnshop.Data.Access.TMF;
using Pawnshop.Data.Models._1c;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.ApplicationOnlineApprovedOtherPayment;
using Pawnshop.Data.Models.ApplicationOnlineFcbKdnPayment;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnline.Kdn;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CallPurpose;
using Pawnshop.Data.Models.CardCashOutTransaction;
using Pawnshop.Data.Models.CardTopUpTransaction;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Comments;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Data.Models.Contracts.Postponements;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Interaction;
using Pawnshop.Data.Models.InteractionResults;
using Pawnshop.Data.Models.Investments;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Mail;
using Pawnshop.Data.Models.ManualUpdate;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Data.Models.Mintos.UploadModels;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Data.Models.OnlinePayments;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Data.Models.Parking;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Data.Models.Positions;
using Pawnshop.Data.Models.Postponements;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Data.Models.Sms;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.TMF;
using Pawnshop.Data.Models.Transfers;
using Pawnshop.Data.Models.Transfers.TransferContracts;
using Pawnshop.Data.Models.UKassa;
using Pawnshop.Data.Models.Verifications;
using Pawnshop.Services.SUSNStatuses;
using Pawnshop.Data.Access.Auction.Mapping.Interfaces;
using Pawnshop.Data.Access.Auction.Mapping;
using Pawnshop.Data.Models.ApplicationOnlineNpck;

namespace Pawnshop.Data
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            CustomTypesInitializer.Init();


            //
            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            //AccountingCore
            builder.RegisterType<TypeRepository>().As<IRepository<Type>>().AsSelf();
            builder.RegisterType<BusinessOperationRepository>().As<IRepository<BusinessOperation>>().AsSelf();
            builder.RegisterType<BusinessOperationSettingRepository>().As<IRepository<BusinessOperationSetting>>().AsSelf();
            builder.RegisterType<AccountSettingRepository>().As<IRepository<AccountSetting>>().AsSelf();
            builder.RegisterType<AccountPlanRepository>().As<IRepository<AccountPlan>>().AsSelf();
            builder.RegisterType<ExpenseArticleTypeRepository>().As<IRepository<ExpenseArticleType>>().AsSelf();
            builder.RegisterType<AccountPlanSettingRepository>().As<IRepository<AccountPlanSetting>>().AsSelf();
            builder.RegisterType<AccountRecordRepository>().As<IRepository<AccountRecord>>().AsSelf();
            builder.RegisterType<AccountRepository>().As<IRepository<Account>>().AsSelf();
            builder.RegisterType<PaymentOrderRepository>().As<IRepository<PaymentOrder>>().AsSelf();
            builder.RegisterType<AccrualBaseRepository>().As<IRepository<AccrualBase>>().AsSelf();
            builder.RegisterType<PenaltyAccrualRepository>().AsSelf();

            //Migrations
            builder.RegisterType<MigrationRepository>().AsSelf();

            //--------------------------------------------------------------
            builder.RegisterType<OrganizationRepository>().As<IRepository<Organization>>().AsSelf();
            builder.RegisterType<UserRepository>().As<IRepository<User>>().AsSelf();
            builder.RegisterType<MemberRepository>().AsSelf();
            builder.RegisterType<RoleRepository>().As<IRepository<Role>>().AsSelf();
            builder.RegisterType<GroupRepository>().As<IRepository<Group>>().AsSelf();
            builder.RegisterType<CategoryRepository>().As<IRepository<Category>>().AsSelf();
            builder.RegisterType<GoldRepository>().As<IRepository<Position>>().AsSelf();
            builder.RegisterType<GoodsRepository>().As<IRepository<Position>>().AsSelf();
            builder.RegisterType<CarRepository>().As<IRepository<Car>>().AsSelf();
            builder.RegisterType<FileRepository>().As<IRepository<FileRow>>().AsSelf();
            builder.RegisterType<ContractFileRowRepository>().As<IRepository<ContractFileRow>>().AsSelf();
            builder.RegisterType<ContractRepository>().As<IRepository<Contract>>().AsSelf();
            builder.RegisterType<ContractQueriesRepository>().AsSelf();
            builder.RegisterType<ClientRepository>().As<IRepository<Client>>().AsSelf();
            builder.RegisterType<ClientFileRowRepository>().As<IRepository<ClientFileRow>>().AsSelf();
            builder.RegisterType<PositionRepository>().As<IRepository<Position>>().AsSelf();
            builder.RegisterType<ContractNumberCounterRepository>().As<IRepository<ContractNumberCounter>>().AsSelf();
            builder.RegisterType<CashOrderRepository>().As<IRepository<CashOrder>>().AsSelf();
            builder.RegisterType<CashOrderQueriesRepository>().AsSelf();
            builder.RegisterType<CashOrderNumberCounterRepository>().As<IRepository<CashOrderNumberCounter>>().AsSelf();
            builder.RegisterType<ContractActionRepository>().As<IRepository<ContractAction>>().AsSelf();
            builder.RegisterType<LoanPercentRepository>().As<IRepository<LoanPercentSetting>>().AsSelf();
            builder.RegisterType<PurityRepository>().As<IRepository<Purity>>().AsSelf();
            builder.RegisterType<SellingRepository>().As<IRepository<Selling>>().AsSelf();
            builder.RegisterType<EventLogRepository>().As<IRepository<EventLogItem>>().AsSelf();
            builder.RegisterType<ContractMonitoringRepository>().AsSelf();
            builder.RegisterType<ContractNoteRepository>().As<IRepository<ContractNote>>().AsSelf();
            builder.RegisterType<AccountAnalysisRepository>().AsSelf();
            builder.RegisterType<ExpenseGroupRepository>().As<IRepository<ExpenseGroup>>().AsSelf();
            builder.RegisterType<ExpenseTypeRepository>().As<IRepository<ExpenseType>>().AsSelf();
            builder.RegisterType<NotificationRepository>().As<IRepository<Notification>>().AsSelf();
            builder.RegisterType<NotificationReceiverRepository>().As<IRepository<NotificationReceiver>>().AsSelf();
            builder.RegisterType<NotificationLogRepository>().As<IRepository<NotificationLog>>().AsSelf();
            builder.RegisterType<InvestmentRepository>().As<IRepository<Investment>>().AsSelf();
            builder.RegisterType<RemittanceRepository>().As<IRepository<Remittance>>().AsSelf();
            builder.RegisterType<RemittanceSettingRepository>().AsSelf();
            builder.RegisterType<AssetRepository>().As<IRepository<Asset>>().AsSelf();
            builder.RegisterType<MachineryRepository>().As<IRepository<Machinery>>().AsSelf();
            builder.RegisterType<InnerNotificationRepository>().As<IRepository<InnerNotification>>().AsSelf();
            builder.RegisterType<BlackListReasonRepository>().As<IRepository<BlackListReason>>().AsSelf();
            builder.RegisterType<OnlinePaymentRepository>().As<IRepository<OnlinePayment>>().AsSelf();
            builder.RegisterType<OnlinePaymentReviseRepository>().As<IRepository<OnlinePaymentRevise>>().AsSelf();
            builder.RegisterType<NotificationTemplateRepository>().As<IRepository<NotificationTemplate>>().AsSelf();
            builder.RegisterType<ExpenseRepository>().As<IRepository<Expense>>().AsSelf();
            builder.RegisterType<ContractExpenseRepository>().As<IRepository<ContractExpense>>().AsSelf();
            builder.RegisterType<TransferContractRepository>().As<IRepository<TransferContract>>().AsSelf();
            builder.RegisterType<TransferRepository>().As<IRepository<Transfer>>().AsSelf();
            builder.RegisterType<AnnuitySettingRepository>().As<IRepository<AnnuitySetting>>().AsSelf();
            builder.RegisterType<InscriptionRepository>().As<IRepository<Inscription>>().AsSelf();
            builder.RegisterType<AttractionChannelRepository>().As<IRepository<AttractionChannel>>().AsSelf();
            builder.RegisterType<SociallyVulnerableGroupRepository>().As<IRepository<SociallyVulnerableGroup>>().AsSelf();
            builder.RegisterType<JobLogRepository>().As<IRepository<JobLogItem>>().AsSelf();
            builder.RegisterType<CrmUploadContractRepository>().As<IRepository<CrmUploadContract>>().AsSelf();
            builder.RegisterType<ReportDataRepository>().AsSelf();
            builder.RegisterType<ReportDataRowRepository>().AsSelf();
            builder.RegisterType<CurrencyRepository>().As<IRepository<Currency>>().AsSelf();
            builder.RegisterType<MintosConfigRepository>().As<IRepository<MintosConfig>>().AsSelf();
            builder.RegisterType<MintosUploadQueueRepository>().As<IRepository<MintosUploadQueue>>().AsSelf();
            builder.RegisterType<MintosContractRepository>().As<IRepository<MintosContract>>().AsSelf();
            builder.RegisterType<MintosContractActionRepository>().As<IRepository<MintosContractAction>>().AsSelf();
            builder.RegisterType<MintosBlackListRepository>().As<IRepository<MintosBlackList>>().AsSelf();
            builder.RegisterType<ParkingActionRepository>().As<IRepository<ParkingAction>>().AsSelf();
            builder.RegisterType<ParkingStatusRepository>().As<IRepository<ParkingStatus>>().AsSelf();
            builder.RegisterType<ParkingHistoryRepository>().As<IRepository<ParkingHistory>>().AsSelf();
            builder.RegisterType<BlackoutRepository>().As<IRepository<Blackout>>().AsSelf();
            builder.RegisterType<PostponementRepository>().As<IRepository<Postponement>>().AsSelf();
            builder.RegisterType<ContractPostponementRepository>().As<IRepository<ContractPostponement>>().AsSelf();
            builder.RegisterType<PersonalDiscountRepository>().As<IRepository<PersonalDiscount>>().AsSelf();
            builder.RegisterType<ContractDiscountRepository>().As<IRepository<ContractDiscount>>().AsSelf();
            builder.RegisterType<RequisiteTypeRepository>().As<IRepository<RequisiteType>>().AsSelf();
            builder.RegisterType<PayTypeRepository>().As<IRepository<PayType>>().AsSelf();
            builder.RegisterType<ContractActionCheckRepository>().As<IRepository<ContractActionCheck>>().AsSelf();
            builder.RegisterType<PayOperationRepository>().As<IRepository<PayOperation>>().AsSelf();
            builder.RegisterType<PayOperationNumberCounterRepository>().As<IRepository<PayOperationNumberCounter>>().AsSelf();
            builder.RegisterType<PayOperationActionRepository>().As<IRepository<PayOperationAction>>().AsSelf();
            builder.RegisterType<PayOperationQueryRepository>().As<IRepository<PayOperationQuery>>().AsSelf();
            builder.RegisterType<ClientLegalFormRepository>().As<IRepository<ClientLegalForm>>().AsSelf();
            builder.RegisterType<ClientDocumentProviderRepository>().As<IRepository<ClientDocumentProvider>>().AsSelf();
            builder.RegisterType<ClientDocumentTypeRepository>().As<IRepository<ClientDocumentType>>().AsSelf();
            builder.RegisterType<ContractCheckRepository>().As<IRepository<ContractCheck>>().AsSelf();
            builder.RegisterType<CountryRepository>().As<IRepository<Country>>().AsSelf();
            builder.RegisterType<AddressATETypeRepository>().As<IRepository<AddressATEType>>().AsSelf();
            builder.RegisterType<AddressBuildingTypeRepository>().As<IRepository<AddressBuildingType>>().AsSelf();
            builder.RegisterType<AddressGeonimTypeRepository>().As<IRepository<AddressGeonimType>>().AsSelf();
            builder.RegisterType<AddressRoomTypeRepository>().As<IRepository<AddressRoomType>>().AsSelf();
            builder.RegisterType<AddressATERepository>().As<IRepository<AddressATE>>().AsSelf();
            builder.RegisterType<AddressBuildingRepository>().As<IRepository<AddressBuilding>>().AsSelf();
            builder.RegisterType<AddressGeonimRepository>().As<IRepository<AddressGeonim>>().AsSelf();
            builder.RegisterType<AddressRoomRepository>().As<IRepository<AddressRoom>>().AsSelf();
            builder.RegisterType<AddressRepository>().As<IRepository<Address>>().AsSelf();
            builder.RegisterType<AddressTypeRepository>().As<IRepository<AddressType>>().AsSelf();
            builder.RegisterType<CBBatchRepository>().As<IRepository<CBBatch>>().AsSelf();
            builder.RegisterType<CBContractRepository>().As<IRepository<CBContract>>().AsSelf();
            builder.RegisterType<CBCollateralRepository>().As<IRepository<CBCollateral>>().AsSelf();
            builder.RegisterType<CBInstallmentRepository>().As<IRepository<CBInstallment>>().AsSelf();
            builder.RegisterType<CBSubjectRepository>().As<IRepository<ICBSubject>>().AsSelf();
            builder.RegisterType<HolidayRepository>().As<IRepository<Holiday>>().AsSelf();
            builder.RegisterType<CrmPaymentRepository>().As<IRepository<CrmUploadPayment>>().AsSelf();
            builder.RegisterType<CrmSyncContactQueueRepository>().As<IRepository<CrmSyncContact>>().AsSelf();
            builder.RegisterType<CrmSyncContactRepository>().As<IRepository<CrmSyncContact>>().AsSelf();
            builder.RegisterType<CrmStatusesRepository>().AsSelf();
            builder.RegisterType<LoanSubjectRepository>().As<IRepository<LoanSubject>>().AsSelf();
            builder.RegisterType<LoanProductTypeRepository>().As<IRepository<LoanProductType>>().AsSelf();
            builder.RegisterType<PrintTemplateRepository>().As<IRepository<PrintTemplate>>().AsSelf();
            builder.RegisterType<ContractDocumentRepository>().As<IRepository<ContractDocument>>().AsSelf();
            builder.RegisterType<VehicleMarkRepository>().As<IRepository<VehicleMark>>().AsSelf();
            builder.RegisterType<VehicleModelRepository>().As<IRepository<VehicleModel>>().AsSelf();
            builder.RegisterType<VehicleWMIRepository>().As<IRepository<VehicleWMI>>().AsSelf();
            builder.RegisterType<VehicleManufacturerRepository>().As<IRepository<VehicleManufacturer>>().AsSelf();
            builder.RegisterType<VehicleCountryCodeRepository>().As<IRepository<VehicleCountryCode>>().AsSelf();
            builder.RegisterType<DomainRepository>().AsSelf();
            builder.RegisterType<DomainValueRepository>().As<IRepository<DomainValue>>().AsSelf();
            builder.RegisterType<ClientContactRepository>().As<IRepository<ClientContact>>().AsSelf();
            builder.RegisterType<ClientAddressRepository>().As<IRepository<ClientAddress>>().AsSelf();
            builder.RegisterType<VerificationRepository>().As<IRepository<Verification>>().AsSelf();
            builder.RegisterType<VehicleBlackListRepository>().As<IRepository<VehiclesBlackListItem>>().AsSelf();
            builder.RegisterType<ContractTransferRepository>().As<IRepository<ContractTransfer>>().AsSelf();
            builder.RegisterType<RevisionRepository>().AsSelf();
            builder.RegisterType<ApplicationRepository>().AsSelf();
            builder.RegisterType<ApplicationMerchantRepository>().AsSelf();
            builder.RegisterType<ClientEconomicActivityTypeRepository>().As<IRepository<ClientEconomicActivityType>>().AsSelf();
            builder.RegisterType<ClientEconomicActivityRepository>().As<IRepository<ClientEconomicActivity>>().AsSelf();
            builder.RegisterType<ClientSignerRepository>().As<IRepository<ClientSigner>>().AsSelf();
            builder.RegisterType<ClientSignersAllowedDocumentTypeRepository>().As<IRepository<ClientSignersAllowedDocumentType>>().AsSelf();
            builder.RegisterType<LoanFinancePlanRepository>().As<IRepository<LoanFinancePlan>>().AsSelf();
            builder.RegisterType<ContractProfileRepository>().As<IRepository<ContractProfile>>().AsSelf();
            builder.RegisterType<ClientLegalFormRequiredDocumentRepository>().As<IRepository<ClientLegalFormRequiredDocument>>().AsSelf();
            builder.RegisterType<ClientLegalFormValidationFieldRepository>().As<IRepository<ClientLegalFormValidationField>>().AsSelf();
            builder.RegisterType<PensionAgesRepository>().AsSelf();
            builder.RegisterType<RealtyRepository>().As<IRepository<Realty>>().AsSelf();
            builder.RegisterType<RealtyAddressRepository>().As<IRepository<RealtyAddress>>().AsSelf();
            builder.RegisterType<PositionEstimatesRepository>().As<IRepository<PositionEstimate>>().AsSelf();
            builder.RegisterType<RealtyDocumentsRepository>().As<IRepository<RealtyDocument>>().AsSelf();
            builder.RegisterType<LanguagesRepository>().As<IRepository<Language>>().AsSelf();
            builder.RegisterType<PositionSubjectsRepository>().As<IRepository<PositionSubject>>().AsSelf();
            builder.RegisterType<LoanSettingProductTypeLTVRepository>().As<IRepository<LoanSettingProductTypeLTV>>().AsSelf();
            builder.RegisterType<PositionEstimateHistoryRepository>().As<IRepository<PositionEstimateHistory>>().AsSelf();
            builder.RegisterType<ContractStatusHistoryRepository>().As<IRepository<ContractStatusHistory>>().AsSelf();
            builder.RegisterType<CollectionStatusRepository>().As<IRepository<CollectionContractStatus>>().AsSelf();
            builder.RegisterType<HCContractStatusRepository>().As<IRepository<HCContractStatus>>().AsSelf();
            builder.RegisterType<HCActionHistoryRepository>().As<IRepository<HCActionHistory>>().AsSelf();
            builder.RegisterType<HCGeoDataRepository>().As<IRepository<HCGeoData>>().AsSelf();
            builder.RegisterType<PositionSubjectHistoryRepository>().As<IRepository<PositionSubjectHistory>>().AsSelf();
            builder.RegisterType<OuterServiceSettingRepository>().As<IRepository<OuterServiceSetting>>().AsSelf();
            builder.RegisterType<ManualUpdateExecuteRepository>().AsSelf();
            builder.RegisterType<ManualUpdateHistoryRepository>().AsSelf();
            builder.RegisterType<ReportLogsRepository>().AsSelf();
            builder.RegisterType<ReportsRepository>().AsSelf();

            //Insurance
            builder.RegisterType<InsuranceRepository>().As<IRepository<Insurance>>().AsSelf();
            builder.RegisterType<InsuranceActionRepository>().As<IRepository<InsuranceAction>>().AsSelf();
            builder.RegisterType<InsuranceOnlineRequestRepository>().As<IRepository<InsuranceOnlineRequest>>().AsSelf();
            builder.RegisterType<InsurancePoliceRequestRepository>().As<IRepository<InsurancePoliceRequest>>().AsSelf();
            builder.RegisterType<InsurancePolicyRepository>().As<IRepository<InsurancePolicy>>().AsSelf();
            builder.RegisterType<InsuranceRateRepository>().As<IRepository<InsuranceRate>>().AsSelf();
            builder.RegisterType<LoanPercentSettingInsuranceCompanyRepository>().As<IRepository<LoanPercentSettingInsuranceCompany>>().AsSelf();
            builder.RegisterType<InsuranceReviseRepository>().As<IRepository<InsuranceRevise>>().AsSelf();

            // регистрируем сервисы анкеты клиента
            builder.RegisterType<ClientExpenseRepository>().AsSelf();
            builder.RegisterType<ClientProfileRepository>().AsSelf();
            builder.RegisterType<ClientEmploymentRepository>().AsSelf();
            builder.RegisterType<ClientAdditionalIncomeRepository>().AsSelf();
            builder.RegisterType<ClientAdditionalContactRepository>().AsSelf();
            builder.RegisterType<ClientAssetRepository>().AsSelf();
            builder.RegisterType<ContractExpenseRowRepository>().AsSelf();
            builder.RegisterType<ContractExpenseRowOrderRepository>().AsSelf();
            builder.RegisterType<ContractActionCheckValueRepository>().AsSelf();
            builder.RegisterType<ContractCheckValueRepository>().AsSelf();
            builder.RegisterType<ContractActionRowRepository>().AsSelf();
            builder.RegisterType<DiscountRepository>().AsSelf();
            builder.RegisterType<DiscountRowRepository>().AsSelf();
            builder.RegisterType<ContractLoanSubjectRepository>().AsSelf();
            builder.RegisterType<ContractPaymentScheduleRepository>().AsSelf();
            builder.RegisterType<ClientsBlackListRepository>().AsSelf();

            //TasOnline
            builder.RegisterType<TasOnlinePaymentRepository>().As<IRepository<TasOnlinePayment>>().AsSelf();
            builder.RegisterType<TasOnlineRequestRepository>().As<IRepository<TasOnlineRequest>>().AsSelf();

            //TMF
            builder.RegisterType<TMFPaymentRepository>().As<IRepository<TMFPayment>>().AsSelf();
            builder.RegisterType<TMFRequestRepository>().As<IRepository<TMFRequest>>().AsSelf();

            builder.RegisterType<MailingRepository>().As<IRepository<Mailing>>().AsSelf();

            builder.RegisterType<ContractRateRepository>().As<IRepository<ContractRate>>().AsSelf();
            builder.RegisterType<LoanSettingRateRepository>().As<IRepository<LoanSettingRate>>().AsSelf();

            builder.RegisterType<ApplicationDetailsRepository>().AsSelf();

            //UKassa
            builder.RegisterType<UKassaRepository>().As<IRepository<UKassaRequest>>().AsSelf();
            builder.RegisterType<UKassaAccountSettingsRepository>().As<IRepository<UKassaAccountSettings>>().AsSelf();
            builder.RegisterType<UKassaBOSettingsRepository>().As<IRepository<UKassaBOSettings>>().AsSelf();
            builder.RegisterType<UKassaKassasRepository>().As<IRepository<UKassaKassa>>().AsSelf();
            builder.RegisterType<UKassaSectionsRepository>().As<IRepository<UKassaSection>>().AsSelf();

            //Kdn
            builder.RegisterType<ContractKdnEstimatedIncomeRepository>().As<IRepository<ContractKdnEstimatedIncome>>().AsSelf();
            builder.RegisterType<NotionalRateRepository>().As<IRepository<NotionalRate>>().AsSelf();
            builder.RegisterType<ClientIncomeRepository>().As<IRepository>().AsSelf();
            builder.RegisterType<ClientIncomeCalculationSettingRepository>().As<IRepository<ClientIncomeCalculationSetting>>().AsSelf();
            builder.RegisterType<ContractPartialSignRepository>().As<IRepository<ContractPartialSign>>().AsSelf();
            builder.RegisterType<ContractKdnDetailRepository>().As<IRepository<ContractKdnDetail>>().AsSelf();
            builder.RegisterType<FcbReportRepository>().As<IRepository<FCBReport>>().AsSelf();
            builder.RegisterType<ContractKdnRequestRepository>().As<IRepository<ContractKdnRequest>>().AsSelf();
            builder.RegisterType<ContractKdnCalculationLogRepository>().As<IRepository<ContractKdnCalculationLog>>().AsSelf();
            builder.RegisterType<KatoNewRepository>().As<IRepository<KatoNew>>().AsSelf();

            //VehicleLiquidity
            builder.RegisterType<VehicleLiquidityRepository>().As<IRepository<VehicleLiquidity>>().AsSelf();
            builder.RegisterType<ContractPeriodVehicleLiquidityRepository>().As<IRepository<ContractPeriodVehicleLiquidity>>().AsSelf();
            builder.RegisterType<ManualCalculationClientExpensesRepository>().As<IRepository<ManualCalculationClientExpense>>().AsSelf();

            //Отчеты
            builder.RegisterType<BalanceSheetReportRepository>().AsSelf();

            builder.RegisterType<OnlineApplicationRepository>().As<IRepository<OnlineApplication>>().AsSelf();
            builder.RegisterType<OnlineApplicationPositionRepository>().As<IRepository<OnlineApplicationPosition>>().AsSelf();
            builder.RegisterType<OnlineApplicationCarRepository>().As<IRepository<OnlineApplicationCar>>().AsSelf();
            builder.RegisterType<OnlineApplicationRetryInsuranceRepository>().As<IRepository<OnlineApplicationRetryInsurance>>().AsSelf();

            // Вывод на карту 
            builder.RegisterType<CardCashOutTransactionRepository>().As<IRepository<CardCashOutTransaction>>().AsSelf();
            builder.RegisterType<CardCashOutJetPayTransactionRepository>().AsSelf();

            builder.RegisterType<JetPayCardPayoutInformationRepository>().AsSelf();

            // Пополнение с карты
            builder.RegisterType<CardTopUpTransactionRepository>().As<IRepository<CardTopUpTransaction>>().AsSelf();

            builder.RegisterType<ContractAdditionalInfoRepository>().As<IRepository<ContractAdditionalInfo>>().AsSelf();

            //Online1C
            builder.RegisterType<Online1CRepository>().As<IRepository>().AsSelf();

            //Сообщения статусов батчей от ГКБ
            builder.RegisterType<CBBatchMessagesSCBRepository>().As<IRepository>().AsSelf();
            builder.RegisterType<CBBatchContractsUploadRepository>().As<IRepository>().AsSelf();

            //credit lines
            builder.RegisterType<ContractCreditLineAdditionalLimitsRepository>().AsSelf();
            builder.RegisterType<CreditLineRepository>().AsSelf();

            // legal-collection
            builder.RegisterType<LegalCollectionRepository>().As<ILegalCollectionRepository>().AsSelf();
            builder.RegisterType<LegalCaseContractsStatusRepository>().As<ILegalCaseContractsStatusRepository>().AsSelf();
            
            // Auction
            builder.RegisterType<AuctionPaymentRepository>().As<IAuctionPaymentRepository>().AsSelf();
            builder.RegisterType<AuctionRepository>().As<IAuctionRepository>().AsSelf();
            builder.RegisterType<AuctionContractExpenseRepository>().As<IAuctionContractExpenseRepository>().AsSelf();
            builder.RegisterType<AuctionMappingRepository>().As<IAuctionMappingRepository>().AsSelf(); // todo удалить после успешного маппинга

            // CashOrderRemittance
            builder.RegisterType<CashOrderRemittanceRepository>().As<ICashOrderRemittanceRepository>().AsSelf();

            // ApplicationOnline 
            builder.RegisterType<ApplicationOnlineRepository>().AsSelf();

            // ApplicationOnlineCar 
            builder.RegisterType<ApplicationOnlineCarRepository>().AsSelf();

            //ApplicationOnlinePosition
            builder.RegisterType<ApplicationOnlinePositionRepository>().AsSelf();

            //ApplicationOnlineEstimation
            builder.RegisterType<ApplicationsOnlineEstimationRepository>().AsSelf();

            //AplicationOnlineFileCodes 
            builder.RegisterType<ApplicationOnlineFileCodesRepository>().AsSelf();

            //ApplicationOnlineFile
            builder.RegisterType<ApplicationOnlineFileRepository>().AsSelf();

            //ApplicationOnlineKdnLog
            builder.RegisterType<ApplicationOnlineKdnLogRepository>().As<IRepository<ApplicationOnlineKdnLog>>().AsSelf();

            //ApplicationOnlineKdnPosition
            builder.RegisterType<ApplicationOnlineKdnPositionRepository>().As<IRepository<ApplicationOnlineKdnPosition>>().AsSelf();

            //ApplicationOnlineTemplateChecksRepository
            builder.RegisterType<ApplicationOnlineTemplateChecksRepository>().As<IRepository<ApplicationOnlineTemplateCheck>>().AsSelf();

            //ApplicationOnlineChecksRepository
            builder.RegisterType<ApplicationOnlineChecksRepository>().As<IRepository<ApplicationOnlineCheck>>().AsSelf();

            builder.RegisterType<BranchesPartnerCodesRepository>().AsSelf();

            //ApplicationOnlineNpckFile
            builder.RegisterType<ApplicationOnlineNpckFileRepository>().As<IRepository<ApplicationOnlineNpckFile>>().AsSelf();

            //ApplicationOnlineNpckSign
            builder.RegisterType<ApplicationOnlineNpckSignRepository>().As<IRepository<ApplicationOnlineNpckSign>>().AsSelf();

            //ApplicationOnlineNpckSignFile
            builder.RegisterType<ApplicationOnlineNpckSignFileRepository>().As<IRepository<ApplicationOnlineNpckSignFile>>().AsSelf();

            //Client Requisites 

            builder.RegisterType<ClientRequisitesRepository>().AsSelf();

            // GeoPositions
            builder.RegisterType<ClientsGeoPositionsRepository>().AsSelf();

            //Client mobile contacts 
            builder.RegisterType<ClientsMobilePhoneContactsRepository>().AsSelf();

            // KFM 
            builder.RegisterType<KFMPersonRepository>().AsSelf();

            // Comments
            builder.RegisterType<CommentsRepository>().As<IRepository<Comment>>().AsSelf();

            // Localizations
            builder.RegisterType<LocalizationRepository>().AsSelf();

            // Calls
            builder.RegisterType<CallsRepository>().AsSelf();
            builder.RegisterType<CallBlackListRepository>().AsSelf();

            // Application online verification 
            builder.RegisterType<ApplicationOnlineSignOtpVerificationRepository>().AsSelf();

            //Online tasks
            builder.RegisterType<OnlineTasksRepository>().AsSelf();

            //Status changes histories 
            builder.RegisterType<ApplicationOnlineStatusChangeHistoryRepository>().AsSelf();

            //Application Online Insurance (Страховка)
            builder.RegisterType<ApplicationOnlineInsuranceRepository>().AsSelf();

            //Client address repository
            builder.RegisterType<ClientAddressesRepository>().AsSelf();

            //Application online rejection reasons причины отказа 
            builder.RegisterType<ApplicationOnlineRejectionReasonsRepository>().AsSelf();

            //Application online approved other payments (другие подтвержденные платежи в ФИ)
            builder.RegisterType<ApplicationOnlineApprovedOtherPaymentRepository>().As<IRepository<ApplicationOnlineApprovedOtherPayment>>().AsSelf();

            //Application online fcb kdn payments (Список запросов в ПКБ КДН)
            builder.RegisterType<ApplicationOnlineFcbKdnPaymentRepository>().As<IRepository<ApplicationOnlineFcbKdnPayment>>().AsSelf();

            //Логирование изменений для ТАС ОНЛАЙН
            builder.RegisterType<ApplicationOnlineHistoryLoggerService>().As<IApplicationOnlineHistoryLoggerService>();
            builder.RegisterType<ApplicationOnlineLogItemsRepository>().AsSelf();
            builder.RegisterType<ClientLogItemsRepository>().AsSelf();
            builder.RegisterType<ClientDocumentLogItemsRepository>().AsSelf();
            builder.RegisterType<ApplicationOnlineCarLogItemRepository>().AsSelf();
            builder.RegisterType<ClientAddressLogItemsRepository>().AsSelf();
            builder.RegisterType<ClientRequisiteLogItemsRepository>().AsSelf();
            builder.RegisterType<ApplicationOnlineFileLogItemsRepository>().AsSelf();
            builder.RegisterType<ClientAdditionalContactLogItemsRepository>().AsSelf();

            //СМС
            builder.RegisterType<SmsMessageTypeRepository>().As<IRepository<SmsMessageType>>().AsSelf();
            builder.RegisterType<SmsMessageAttributeRepository>().As<IRepository<SmsMessageAttribute>>().AsSelf();
            builder.RegisterType<SmsTemplateRepository>().As<IRepository<SmsTemplate>>().AsSelf();

            //Рефинасирование
            builder.RegisterType<ApplicationOnlineRefinancesRepository>().AsSelf();

            builder.RegisterType<CallPurposesRepository>().As<IRepository<CallPurpose>>().AsSelf();
            builder.RegisterType<InteractionResultsRepository>().As<IRepository<InteractionResult>>().AsSelf();
            builder.RegisterType<InteractionsRepository>().As<IRepository<Interaction>>().AsSelf();
            builder.RegisterType<UserBranchSignerRepository>().As<IUserBranchSignerRepository>().AsSelf();
            builder.RegisterType<RegionRepository>().As<IRegionRepository>().AsSelf();

            // отмена/отклонение автокредитов которые без ПВ больше чем один день (один день пока что не решен, и статус тоже пока что не решен)
            builder.RegisterType<AutocreditContractCancelRepository>().AsSelf();
            
            
            //Статусы СУСН
            builder.RegisterType<SUSNStatusesRepository>().AsSelf();
            builder.RegisterType<ClientSUSNStatusesRepository>().AsSelf();
            builder.RegisterType<SUSNRequestsRepository>().AsSelf();


            //Реестр должников
            builder.RegisterType<ClientDebtorRegistryRequestsRepository>().AsSelf();
            builder.RegisterType<ClientDebtorRegistryDataRepository>().AsSelf();

            //Лиды
            builder.RegisterType<LeadsRepository>().AsSelf();

            builder.RegisterType<FunctionSettingRepository>().AsSelf();

            //Запись о неудачных запросах во внешние системы
            builder.RegisterType<ClientExternalValidationDataRepository>().AsSelf();

            //Позиции договоров
            builder.RegisterType<ContractPositionRepository>().AsSelf();

            //Реструктуризация
            builder.RegisterType<TasLabRecruitRequestsRepository>().AsSelf();
            builder.RegisterType<ClientDefermentRepository>().AsSelf();
            builder.RegisterType<RestructuredContractPaymentScheduleRepository>().AsSelf();
        }
    }
}