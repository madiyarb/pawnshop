using System.Collections.Generic;

namespace Pawnshop.Core
{
    public static class Permissions
    {
        public static readonly IEnumerable<Permission> All = new List<Permission>();

        public const string UserView = "UserView";
        public const string UserManage = "UserManage";
        public const string GroupView = "GroupView";
        public const string GroupManage = "GroupManage";
        public const string RoleView = "RoleView";
        public const string RoleManage = "RoleManage";
        public const string ClientView = "ClientView";
        public const string ClientManage = "ClientManage";
        public const string ClientFcbRequest = "ClientFcbRequest";
        public const string ClientContactActualizedAdd = "ClientContactActualizedAdd";
        public const string ClientContactActualizedManage = "ClientContactActualizedManage";
        public const string ClientDocumentsManage = "ClientDocumentsManage";
        public const string CategoryView = "CategoryView";
        public const string CategoryManage = "CategoryManager";
        public const string GoldView = "GoldView";
        public const string GoldManage = "GoldManager";
        public const string GoodsView = "GoodsView";
        public const string GoodsManage = "GoodsManager";
        public const string CarView = "CarView";
        public const string CarManage = "CarManager";
        public const string RequisiteTypeView = "RequisiteTypeView";
        public const string RequisiteTypeManage = "RequisiteTypeManage";
        public const string PayTypeView = "PayTypeView";
        public const string PayTypeManage = "PayTypeManage";
        public const string ContractActionCheckView = "ContractActionCheckView";
        public const string ContractActionCheckManage = "ContractActionCheckManage";
        public const string ClientLegalFormView = "ClientLegalFormView";
        public const string ClientLegalFormManage = "ClientLegalFormManage";
        public const string ClientDocumentTypeView = "ClientDocumentTypeView";
        public const string ClientDocumentTypeManage = "ClientDocumentTypeManage";
        public const string ClientDocumentProviderView = "ClientDocumentProviderView";
        public const string ClientDocumentProviderManage = "ClientDocumentProviderManage";
        public const string ClientEconomicActivityView = "ClientEconomicActivityView";
        public const string ClientEconomicActivityManage = "ClientEconomicActivityManage";
        public const string ClientSignersAllowedDocumentTypeView = "ClientSignersAllowedDocumentTypeView";
        public const string ClientSignersAllowedDocumentTypeManage = "ClientSignersAllowedDocumentTypeManage";
        public const string ContractCheckView = "ContractCheckView";
        public const string ContractCheckManage = "ContractCheckManage";
        public const string CountryView = "CountryView";
        public const string CountryManage = "CountryManage";
        public const string HolidayView = "HolidayView";
        public const string HolidayManage = "HolidayManage";
        public const string LoanSubjectView = "LoanSubjectView";
        public const string LoanSubjectManage = "LoanSubjectManage";
        public const string LoanProductTypeView = "LoanProductTypeView";
        public const string LoanProductTypeManage = "LoanProductTypeManage";
        public const string PrintTemplateView = "PrintTemplateView";
        public const string PrintTemplateManage = "PrintTemplateManage";

        public const string ContractView = "ContractView";
        public const string ContractManage = "ContractManager";
        public const string ContractDiscount = "ContractDiscount";
        public const string ContractPersonalDiscount = "ContractPersonalDiscount";
        public const string ContractTransfer = "ContractTransfer";
        public const string UnsecuredContractSign = "UnsecuredContractSign";
        public const string Support = "Support";
        public const string ContractPostponement = "ContractPostponement";
        public const string ContractBuyout = "ContractBuyout";
        public const string ContractAccountingActions = "ContractAccountingActions";
        public const string EncumbranceView = "EncumbranceView";
        public const string ContractCreateFromButton = "ContractCreateFromButton";
        public const string ContractAdditionAndPartialPaymentActionsCancel = "ContractAdditionAndPartialPaymentActionsCancel";
        public const string ContractDocumentGenerateNumber = "ContractDocumentGenerateNumber";
        public const string RealtyContractConfirm = "RealtyContractConfirm";

        public const string AccountView = "AccountView";
        public const string AccountManage = "AccountManager";
        public const string CashOrderView = "CashOrderView";
        public const string CashOrderManage = "CashOrderManager";
        public const string CashOrderApprove = "CashOrderApprove";
        public const string CashOrderConfirm = "CashOrderConfirm";
        public const string CashOrderCashTransaction = "CashOrderCashTransaction";
        public const string CashOrderReasonEdit = "CashOrderReasonEdit";
        public const string CashOrderNoteManage = "CashOrderNoteManage";
        public const string CashOrderCounterpartyEdit = "CashOrderCounterpartyEdit";
        public const string CashRegisterTransferManage = "CashRegisterTransferManage";
        public const string ReconciliationAccountingRegisterView = "ReconciliationAccountingRegisterView";
        public const string MachineryView = "MachineryView";
        public const string MachineryManage = "MachineryManage";
        public const string BlackListReasonView = "BlackListReasonView";
        public const string BlackListReasonManage = "BlackListReasonManage";
        public const string AttractionChannelView = "AttractionChannelView";
        public const string AttractionChannelManage = "AttractionChannelManage";
        public const string ParkingView = "ParkingView";
        public const string ParkingManage = "ParkingManage";
        public const string BlackoutView = "BlackoutView";
        public const string BlackoutManage = "BlackoutManage";
        public const string PersonalDiscountView = "PersonalDiscountView";
        public const string PersonalDiscountManage = "PersonalDiscountManage";
        public const string SociallyVulnerableGroupView = "SociallyVulnerableGroupView";
        public const string SociallyVulnerableGroupManage = "SociallyVulnerableGroupManage";
        public const string PostponementView = "PostponementView";
        public const string PostponementManage = "PostponementManage";
        public const string OnlinePaymentsManage = "OnlinePaymentsManage";
        public const string PayOperationView = "PayOperationView";
        public const string PayOperationManage = "PayOperationManage";
        public const string RevisionLoad = "RevisionLoad";
        public const string FillCBBatchesManually = "FillCBBatchesManually";

        public const string OrganizationConfigurationManage = "OrganizationConfigurationManage";
        public const string BranchConfigurationManage = "BranchConfigurationManage";

        public const string ClientCardTypeManage = "ClientCardTypeManage";
        public const string PositionCategoryManage = "PositionCategoryManage";

        public const string LoanPercentSettingView = "LoanPercentSettingView";
        public const string LoanPercentSettingManage = "LoanPercentSettingManage";

        public const string InsuranceCompanySettingsView = "InsuranceCompanySettingsView";
        public const string InsuranceCompanySettingsManage = "InsuranceCompanySettingsManage";
        public const string InsuranceRevise = "InsuranceRevise";

        public const string AnnuitySettingView = "AnnuitySettingView";
        public const string AnnuitySettingManage = "AnnuitySettingManage";

        public const string PurityView = "PurityView";
        public const string PurityManage = "PurityManage";

        public const string SellingView = "SellingView";
        public const string SellingManage = "SellingManage";

        public const string EventLogView = "EventLogView";
        public const string EventLogFullView = "EventLogFullView";

        public const string ExpenseGroupView = "ExpenseGroupView";
        public const string ExpenseGroupManage = "ExpenseGroupManage";
        public const string ExpenseTypeView = "ExpenseTypeView";
        public const string ExpenseTypeManage = "ExpenseTypeManage";
        public const string ExpenseView = "ExpenseView";
        public const string ExpenseManage = "ExpenseManage";

        public const string NotificationView = "NotificationView";
        public const string NotificationManage = "NotificationManage";

        public const string InnerNotificationView = "InnerNotificationView";
        public const string InnerNotificationManage = "InnerNotificationManage";

        public const string InvestmentView = "InvestmentView";
        public const string InvestmentManage = "InvestmentManage";

        public const string InsuranceView = "InsuranceView";
        public const string InsuranceManage = "InsuranceManage";
        public const string InsuranceActionManage = "InsuranceActionManage";

        public const string InscriptionView = "InscriptionView";
        public const string InscriptionManage = "InscriptionManage";

        public const string AssetView = "AssetView";
        public const string AssetManage = "AssetManage";

        public const string CashBookView = "CashBookView";
        public const string CashBalanceView = "CashBalanceView";
        public const string ContractMonitoringView = "ContractMonitoringView";
        public const string CashReportView = "CashReportView";
        public const string SellingReportView = "SellingReportView";
        public const string DelayReportView = "DelayReportView";
        public const string DailyReportView = "DailyReportView";
        public const string ConsolidateReportView = "ConsolidateReportView";
        public const string CollateralReportView = "CollateralReportView";

        public const string CollectionReportView = "CollectionReportView";

        public const string TransferredContractsReportView = "TransferredContractsReportView";
        public const string DiscountReportView = "DiscountReportView";
        public const string AccountCardView = "AccountCardView";
        public const string OrderRegisterView = "OrderRegisterView";
        public const string AccountAnalysisView = "AccountAnalysisView";
        public const string AccountCycleView = "AccountCycleView";
        public const string GoldPriceView = "GoldPriceView";
        public const string ExpenseMonthReportView = "ExpenseMonthReportView";
        public const string ExpenseYearReportView = "ExpenseYearReportView";
        public const string OperationalReportView = "OperationalReportView";
        public const string ProfitReportView = "ProfitReportView";
        public const string SplitProfitReportView = "SplitProfitReportView";
        public const string ReconciliationReportView = "ReconciliationReportView";
        public const string PaymentReportView = "PaymentReportView";
        public const string IssuanceReportView = "IssuanceReportView";
        public const string СlientExpensesReportView = "СlientExpensesReportView";
        public const string AccountMonitoringReportView = "AccountMonitoringReportView";
        public const string ConsolidateIssuanceReportView = "ConsolidateIssuanceReportView";
        public const string AnalysisIssuanceReportView = "AnalysisIssuanceReportView";
        public const string AttractionChannelReportView = "AttractionChannelReportView";
        public const string EmloyeeContractReportView = "EmloyeeContractReportView";
        public const string PosTerminalBookReportView = "PosTerminalBookReportView";
        public const string CarParkingStatusReportView = "CarParkingStatusReportView";
        public const string CashInTransitReportView = "CashInTransitReportView";
        public const string WithoutDriveLicenseReportView = "WithoutDriveLicenseReportView";
        public const string BuyoutContractsWithInscriptionReportView = "BuyoutContractsWithInscriptionReportView";
        public const string InsurancePolicyReportView = "InsurancePolicyReportView";
        public const string InsurancePolicyActReportView = "InsurancePolicyActReportView";
        public const string PrepaymentMonitoringReportView = "PrepaymentMonitoringReportView";
        public const string BuyoutContractsReportView = "BuyoutContractsReportView";
        public const string BuyoutGprReportView = "BuyoutGprReportView";
        public const string RefinanceGrpReportView = "RefinanceGrpReportView";
        public const string KdnFailureStatisticsReportView = "KdnFailureStatisticsReportView";
        public const string RequestsToPkbReportView = "RequestsToPkbReportView";

        public const string ContractOuterView = "ContractOuterView";
        public const string ReinforceAndWithdrawReportView = "ReinforceAndWithdrawReportView";
        public const string AccountableReportView = "AccountableReportView";
        public const string NotificationReportView = "NotificationReportView";
        public const string RegionMonitoringReportView = "RegionMonitoringReportView";
        public const string StatisticsReportView = "StatisticsReportView";
        public const string PrepaymentUsedReportView = "PrepaymentUsedReportView";
        public const string AccountMfoReportView = "AccountMfoReportView";
        public const string SoftCollectionReportView = "SoftCollectionReportView";
        public const string CarParkingStatusReportForCARTASView = "CarParkingStatusReportForCARTASView";
        public const string UserPermissionsReportView = "UserPermissionsReportView";
        public const string PhotoReportView = "PhotoReportView";
        public const string OnlinePaymentManageReportView = "OnlinePaymentManageReportView";
        public const string SMSSenderReportView = "SMSSenderReportView";
        public const string PaymentsMoreThanTenReportView = "PaymentsMoreThanTenReportView";
        public const string AuditOnPledgesReportView = "AuditOnPledgesReportView";
        public const string ContractDebtPrintManage = "ContractDebtPrintManage";
        public const string ContractDiscountReportView = "ContractDiscountReportView";
        public const string ArrestedAndDeadReportView = "ArrestedAndDeadReportView";
        public const string IsInsuranceRequiredManage = "IsInsuranceRequiredManage";
        public const string DelayReportForSoftCollectionView = "DelayReportForSoftCollectionView";
        public const string ReportLogsView = "ReportLogsView";

        public const string OnlinePayment = "OnlinePayment";
        public const string PrepaymentWithNotification = "PrepaymentWithNotification";
        public const string ChooseCounterpartyForContractExpense = "ChooseCounterpartyForContractExpense";
        public const string ExpenseForContractWithInscription = "ExpenseForContractWithInscription";
        public const string DisablePaymentActionsOnContracts = "DisablePaymentActionsOnContracts";

        public const string VehicleMarkView = "VehicleMarkView";
        public const string VehicleMarkManage = "VehicleMarkManage";
        public const string VehicleModelView = "VehicleModelView";
        public const string VehicleModelManage = "VehicleModelManage";
        public const string VehicleLiquidityView = "VehicleLiquidityView";
        public const string VehicleLiquidityManage = "VehicleLiquidityManage";
        public const string VehicleWMIView = "VehicleWMIView";
        public const string VehicleWMIManage = "VehicleWMIManage";
        public const string VehicleManufacturerView = "VehicleManufacturerView";
        public const string VehicleManufacturerManage = "VehicleManufacturerManage";
        public const string VehicleCountryCodeView = "VehicleCountryCodeView";
        public const string VehicleCountryCodeManage = "VehicleCountryCodeManage";
        public const string ParkingHistoryView = "ParkingHistoryView";
        public const string VehiclesBlackListView = "VehiclesBlackListView";
        public const string VehiclesBlackListManage = "VehiclesBlackListManage";
        public const string ClientBlackListView = "ClientBlackListView";
        public const string ClientBlackListManage = "ClientBlackListManage";
        public const string ManualCalculationClientExpensesView = "ManualCalculationClientExpensesView";
        public const string ManualCalculationClientExpensesManage = "ManualCalculationClientExpensesManage";
        public const string ClientEstimationCompanyView = "ClientEstimationCompanyView";
        public const string ClientEstimationCompanyManage = "ClientEstimationCompanyManage";

        public const string InscriptionDutyManage = "InscriptionDutyManage";

        public const string DomainView = "DomainView";
        public const string DomainManage = "DomainManage";
        public const string ClientCodeWordManage = "ClientCodeWordManage";
        public const string MobileAppAccess = "MobileAppAccess";
        public const string MobileAppAccessForManager = "MobileAppAccessForManager";
        public const string MobileAppAccessForAppraiser = "MobileAppAccessForAppraiser";
        public const string InsuranceRateView = "InsuranceRateView";
        public const string InsuranceRateManage = "InsuranceRateManage";
        public const string PensionAgesView = "PensionAgesView";
        public const string PensionAgesManage = "PensionAgesManage";

        //Права доступа для настройки учетного ядра(AccountingCorePermissions)
        public const string TypeView = "TypeView";
        public const string TypeManage = "TypeManage";
        public const string BusinessOperationView = "BusinessOperationView";
        public const string BusinessOperationManage = "BusinessOperationManage";
        public const string AccountPlanView = "AccountPlanView";
        public const string AccountPlanManage = "AccountPlanManage";
        public const string AccountPlanSettingView = "AccountPlanSettingView";
        public const string AccountPlanSettingManage = "AccountPlanSettingManage";
        public const string AccountSettingView = "AccountSettingView";
        public const string AccountSettingManage = "AccountSettingManage";
        public const string PaymentOrderView = "PaymentOrderView";
        public const string PaymentOrderManage = "PaymentOrderManage";
        public const string AccrualBaseView = "AccrualBaseView";
        public const string AccrualBaseManage = "AccrualBaseManage";

        //Права доступа для статьи расхода(AccountingCorePermissions)
        public const string ExpenseArticleTypeView = "ExpenseArticleTypeView";
        public const string ExpenseArticleTypeManage = "ExpenseArticleTypeManage";
        // Права доступа к справочникам Collections
        public const string CollectionDictionariesView = "CollectionDictionariesView";
        public const string CollectionDictionariesManage = "CollectionDictionariesManage";

        // Права доступа для передачи в Hard и Legal Collections
        public const string TransferToHardCollectionManage = "TransferToHardCollectionManage";
        public const string TransferToLegalCollectionManage = "TransferToLegalCollectionManage";

        // Права доступа к функциям учетного ядра
        public const string AccountingActionsRemove = "AccountingActionsRemove";
        public const string StornoContractActions = "StornoContractActions";
        public const string StornoManualOrders = "StornoManualOrders";

        //Права для PowerBi
        public const string ReportDataManage = "ReportDataManage";

        public const string TasOnlinePaymentView = "TasOnlinePaymentView";
        public const string TasOnlineReportView = "TasOnlineReportView";
        public const string TmfReportView = "TmfReportView";
        public const string VintageAnalysisView = "VintageAnalysisView";

        public const string TmfPaymentView = "TmfPaymentView";

        public const string PrepaymentMoveManage = "PrepaymentMoveManage";

        public const string ContractFromApplicationView = "ContractFromApplicationView";
        public const string ContractFromApplicationAllView = "ContractFromApplicationAllView";

        //Недвижимость
        public const string RealtyView = "RealtyView";
        public const string RealtyManage = "RealtyManage";
        public const string RealtyUpdate = "RealtyUpdate";
        public const string RealtyContractPrintInWordFormat = "RealtyContractPrintInWordFormat";

        //Разное
        public const string OldPrintButtonView = "OldPrintButtonView";

        //Отчет Оборотно-сальдовая ведомость по счету
        public const string BalanceSheetReportView = "BalanceSheetReportView";

        //Отчеты ТасОнлайн
        public const string LoanPortfolioReportView = "LoanPortfolioReportView";
        public const string CurrentAccountDebtAndBalanceReportView = "CurrentAccountDebtAndBalanceReportView";
        public const string CallingCustomersWithCurrentPaymentDateReportView = "CallingCustomersWithCurrentPaymentDateReportView";
        public const string SecurityServiceReportView = "SecurityServiceReportView";
        public const string WithdrawalFromPledgeReportView = "WithdrawalFromPledgeReportView";
        public const string SecurityServiceEffectivenessReportView = "SecurityServiceEffectivenessReportView";
        public const string DelayReportTasOnlineView = "DelayReportTasOnlineView";

        //Отчеты Online
        public const string OnlineReportsView = "OnlineReportsView";

        //ТасОнлайн
        public const string DisabledForTasOnline = "DisabledForTasOnline";

        //1C
        public const string ExportDataTo1C = "ExportDataTo1C";

        //LegalCollection
        public const string LegalCollectionView = "LegalCollectionView";
        public const string LegalCollectionManage = "LegalCollectionManage";
        public const string LegalCollectionManageAccrual = "LegalCollectionManageAccrual";
        public const string LegalCollectionDeleteDocument = "LegalCollectionDeleteDocument";
        public const string LegalCollectionChangeCourse = "LegalCollectionChangeCourse";
        public const string LegalCollectionAmountsView = "LegalCollectionAmountsView";

        /// <summary>
        /// доступ на управление действиями по Аукциону
        /// </summary>
        public const string AuctionView = "AuctionView";
        public const string AuctionManage = "AuctionManage";

        //Hard Collection
        public const string HardCollectionManager = "HardCollectionManager";
        public const string HardCollectionRegionDirector = "HardCollectionRegionDirector";
        public const string HardCollectionMainDirector = "HardCollectionMainDirector";

        //TasOnline
        public const string TasOnlineManager = "TasOnlineManager";
        public const string TasOnlineVerificator = "TasOnlineVerificator";
        public const string TasOnlineAdministrator = "TasOnlineAdministrator";
        public const string TasOnlineCreditAdministrator = "TasOnlineCreditAdministrator";

        //Manual Update
        public const string ManualUpdateAllowed = "ManualUpdateManager";

        //Реструктуризация
        public const string RestructuringPageView = "RestructuringPageView";
        public const string RestructuringRecruitManage = "RestructuringRecruitManage";
        public const string RestructuringContractManage = "RestructuringContractManage";

        static Permissions()
        {
            //Бизнес
            Register(UserView, "Просмотр списка пользователей", "Бизнес");
            Register(UserManage, "Управление пользователями", "Бизнес");
            Register(GroupView, "Просмотр списка групп", "Бизнес");
            Register(GroupManage, "Управление группами", "Бизнес");
            Register(RoleView, "Просмотр списка ролей", "Бизнес");
            Register(RoleManage, "Управление ролями", "Бизнес");
            Register(ClientView, "Просмотр списка клиентов", "Бизнес");
            Register(ClientManage, "Управление клиентами", "Бизнес");
            Register(ClientFcbRequest, "Запрос кредитной истории клиента", "Бизнес");
            Register(ClientContactActualizedAdd, "Добавление актуализированных контактов");
            Register(ClientContactActualizedManage, "Управление актуализированными контактами");
            Register(ClientDocumentsManage, "Управление документами клиентов", "Бизнес");
            Register(CategoryView, "Просмотр списка категорий аналитики", "Бизнес");
            Register(CategoryManage, "Управление категориями аналитики", "Бизнес");
            Register(GoldView, "Просмотр списка позиций золота", "Бизнес");
            Register(GoldManage, "Управление позициями золота", "Бизнес");
            Register(GoodsView, "Просмотр списка позиций товара", "Бизнес");
            Register(GoodsManage, "Управление позициями товара", "Бизнес");
            Register(CarView, "Просмотр списка позиций автотранспорта", "Бизнес");
            Register(CarManage, "Управление позициями автотранспорта", "Бизнес");
            Register(ContractView, "Просмотр списка договоров", "Бизнес");
            Register(ContractManage, "Управление договорами", "Бизнес");
            Register(Support, "Администрирование и поддержка", "Бизнес");
            Register(ContractDiscount, "Управление скидками договора", "Бизнес");
            Register(ContractPersonalDiscount, "Управление стандартными(персональными) скидками договора", "Бизнес");
            Register(ContractTransfer, "Управление передачей договора", "Бизнес");
            Register(ContractPostponement, "Управление отсрочками договора", "Бизнес");
            Register(UnsecuredContractSign, "Подписание беззалогового договора", "Бизнес");
            Register(InscriptionView, "Просмотр исполнительной надписи", "Бизнес");
            Register(InscriptionManage, "Управление исполнительной надписью", "Бизнес");
            Register(PayOperationView, "Просмотр списка платежных операций", "Бизнес");
            Register(PayOperationManage, "Управление списом платежных операций", "Бизнес");
            Register(ChooseCounterpartyForContractExpense, "Право на добавление сотрудника для дополнительных раснодов на договоре", "Бизнес");
            Register(ExpenseForContractWithInscription, "Право на добавление расходов для договоров переданных в ЧСИ", "Бизнес");
            Register(DisablePaymentActionsOnContracts, "Право на отключения платёжныx действий по договору", "Бизнес");
            Register(ParkingHistoryView, "Просмотр истории стоянок", "Бизнес");
            Register(RevisionLoad, "Ревизии залога", "Бизнес");
            Register(InscriptionDutyManage, "Редактирование поля \"Госпошлина\" при передачи ЧСИ и утверждении исполнительной надписи", "Бизнес");
            Register(ClientCodeWordManage, "Редактирование поля \"Кодовое слово\" в карте клиента", "Бизнес");
            Register(ContractDebtPrintManage, "Печать справок о задолженности", "Бизнес");
            Register(ContractBuyout, "Действие выкуп на договоре", "Бизнес");
            Register(ContractAccountingActions, "Действия \"Начисление процентов\", \"Вынос на просрочку\", \"Начисление штрафов\" на договоре", "Бизнес");
            Register(EncumbranceView, "Отображение признака постановки залога в арест в списке договоров в карте клиента", "Бизнес");
            Register(MobileAppAccess, "Доступ для мобильного приложения", "Бизнес");
            Register(MobileAppAccessForManager, "Доступ в мобильное приложение для менеджера", "Бизнес");
            Register(MobileAppAccessForAppraiser, "Доступ в мобильное приложение для оценщика", "Бизнес");
            Register(PrepaymentMoveManage, "Перенос аванса на другой договор", "Бизнес");
            Register(IsInsuranceRequiredManage, "Редактирование чекбокса \"Обязательное страхование\"", "Бизнес");
            Register(ContractCreateFromButton, "Создание договора или добора из кнопок Фронта\"", "Бизнес");
            Register(ContractAdditionAndPartialPaymentActionsCancel, "Отмена Добора и ЧДП в журнале действий", "Бизнес");
            Register(RealtyView, "Просмотр списка позиций недвижимости", "Бизнес");
            Register(RealtyManage, "Управление позициями недвижимости", "Бизнес");
            Register(RealtyUpdate, "Обновления позиций недвижимости", "Бизнес");
            Register(ContractDocumentGenerateNumber, "Создание номеров для документов договора", "Бизнес");
            Register(RealtyContractPrintInWordFormat, "Выгрузка Договора залога недвижимости в Word формате", "Бизнес");
            Register(RealtyContractConfirm, "Разрешение на согласование договора недвижимости", "Бизнес");
            Register(TransferToHardCollectionManage, "Передача в Hard Collection", "Бизнес");
            Register(TransferToLegalCollectionManage, "Передача в Legal Collection", "Бизнес");
            Register(FillCBBatchesManually, "Ручное создание батчей по договорам", "Бизнес");
            Register(InsuranceRevise, "Сверка отчетов со страховой компанией", "Бизнес");
            Register(RestructuringPageView, "Просмотр истории реструктуризаций", "Бизнес");
            Register(RestructuringRecruitManage, "Получение данных о воинской службе ИИН", "Бизнес");
            Register(RestructuringContractManage, "Реструктуризация контракта", "Бизнес");

            //Оплаты онлайн и р/с
            Register(OnlinePayment, "Проведение онлайн оплат (для поставщиков онлайн услуг)", "Оплаты онлайн и на р/с");
            Register(OnlinePaymentsManage, "Управление онлайн оплатами (сверка, настройка)", "Оплаты онлайн и на р/с");
            Register(PrepaymentWithNotification, "Начисление аванса с уведомлением филиалов", "Оплаты онлайн и на р/с");

            //ТасОнлайн
            Register(TasOnlinePaymentView, "Просмотр платежей ТасОнлайн ", "ТасОнлайн");
            Register(DisabledForTasOnline, "Отключении функции для ТасОнлайн", "ТасОнлайн");
            //ТМФ
            Register(TmfPaymentView, "Просмотр платежей ТМФ ", "ТМФ");

            //Учетное ядро(AccountingCore)
            Register(TypeView, "Просмотр списка типов и их иерархий", "Учетное ядро");
            Register(TypeManage, "Управление типами и их иерархией", "Учетное ядро");
            Register(BusinessOperationView, "Просмотр списка бизнес-операций", "Учетное ядро");
            Register(BusinessOperationManage, "Управление бизнес-операциями", "Учетное ядро");
            Register(AccountPlanView, "Просмотр списка плана счетов", "Учетное ядро");
            Register(AccountPlanManage, "Управление планами счетов", "Учетное ядро");
            Register(AccountPlanSettingView, "Просмотр соответствия плана счетов настройкам счетов", "Учетное ядро");
            Register(AccountPlanSettingManage, "Управление соответствием плана счетов настройкам счетов", "Учетное ядро");
            Register(AccountSettingView, "Просмотр списка настроек счетов", "Учетное ядро");
            Register(AccountSettingManage, "Управление настройками счетов", "Учетное ядро");
            Register(PaymentOrderView, "Просмотр настроек порядка погашения", "Учетное ядро");
            Register(PaymentOrderManage, "Управление настройками порядка погашения", "Учетное ядро");
            Register(AccrualBaseView, "Просмотр настроек баз начисления", "Учетное ядро");
            Register(AccrualBaseManage, "Управление настройками баз начислений", "Учетное ядро");
            Register(AccountingActionsRemove, "Отмена операций автоматического начисления", "Учетное ядро");
            Register(StornoContractActions, "Сторнирование действий договора", "Учетное ядро");
            Register(StornoManualOrders, "Сторнирование ручных проводок", "Учетное ядро");

            //Справочники
            Register(ExpenseArticleTypeView, "Просмотр списка статьи расхода", "Справочники");
            Register(ExpenseArticleTypeManage, "Управление статьями расхода", "Справочники");
            Register(AccountView, "Просмотр списка счетов", "Справочники");
            Register(AccountManage, "Управление счетами", "Справочники");
            Register(CashOrderView, "Просмотр списка кассовых ордеров", "Справочники");
            Register(CashOrderManage, "Управление кассовыми ордерами", "Справочники");
            Register(CashOrderApprove, "Подтверждение кассового ордера", "Справочники");
            Register(CashOrderConfirm, "Согласование кассового ордера", "Справочники");
            Register(CashOrderCashTransaction, "Согласование наличных операций кассового ордера", "Справочники");
            Register(CashOrderReasonEdit, "Редактирование поля \"Основание\" в форме кассового ордера", "Справочники");
            Register(CashOrderNoteManage, "Просмотр и редактирование поля примечание у ордеров");
            Register(CashOrderCounterpartyEdit, "Редактирование поля \"Контрагент\" у ордеров");
            Register(CashRegisterTransferManage, "Просмотр и управление передачы кассы", "Справочники");
            Register(ReconciliationAccountingRegisterView, "Просмотр сверки с Учет Кассой", "Справочники");
            Register(OrganizationConfigurationManage, "Управление конфигурацией организации", "Справочники");
            Register(BranchConfigurationManage, "Управление конфигурацией филиала", "Справочники");
            Register(ClientCardTypeManage, "Изменение типа карты клиента", "Справочники");
            Register(PositionCategoryManage, "Изменение категории аналитики позиций", "Справочники");
            Register(LoanPercentSettingView, "Просмотр настроек процентов кредита", "Справочники");
            Register(LoanPercentSettingManage, "Управление настройками процентов кредита", "Справочники");
            Register(InsuranceCompanySettingsView, "Просмотр настроек страховой компании", "Справочники");
            Register(InsuranceCompanySettingsManage, "Управление настройками страховой компании", "Справочники");
            Register(AnnuitySettingView, "Просмотр настроек периодов аннуитета", "Справочники");
            Register(AnnuitySettingManage, "Управление настройками периодов аннуитета", "Справочники");
            Register(PurityView, "Просмотр списка проб", "Справочники");
            Register(PurityManage, "Управление пробами", "Справочники");
            Register(SellingView, "Просмотр списка реализации", "Справочники");
            Register(SellingManage, "Управление реализацией", "Справочники");
            Register(EventLogView, "Просмотр журнала событий по филиалу", "Справочники");
            Register(EventLogFullView, "Просмотр журнала событий по системе", "Справочники");
            Register(ExpenseGroupView, "Просмотр групп расходов компании", "Справочники");
            Register(ExpenseGroupManage, "Управление группами расходов компании", "Справочники");
            Register(ExpenseTypeView, "Просмотр видов расходов компании", "Справочники");
            Register(ExpenseTypeManage, "Управление видами расходов компании", "Справочники");
            Register(ExpenseView, "Просмотр видов расходов клиента", "Справочники");
            Register(ExpenseManage, "Управление видами расходов клиента", "Справочники");
            Register(NotificationView, "Просмотр уведомлений клиентов", "Справочники");
            Register(NotificationManage, "Управление уведомлениями клиентов", "Справочники");
            Register(NotificationView, "Просмотр уведомлений для сотрудников", "Справочники");
            Register(NotificationManage, "Управление уведомлениями для сотрудников", "Справочники");
            Register(InvestmentView, "Просмотр инвестиций", "Справочники");
            Register(InvestmentManage, "Управление инвестициями", "Справочники");
            Register(InsuranceView, "Просмотр страховых договоров", "Справочники");
            Register(InsuranceManage, "Управление страховыми договорами", "Справочники");
            Register(InsuranceActionManage, "Управление действиями страховых договоров", "Справочники");
            Register(AssetView, "Просмотр основных средств", "Справочники");
            Register(AssetManage, "Управление основными средствами", "Справочники");
            Register(MachineryView, "Просмотр списка позиций спецтехники", "Справочники");
            Register(MachineryManage, "Управление позициями спецтехники", "Справочники");
            Register(BlackListReasonView, "Просмотр списка причин добавления в черный список клиента", "Справочники");
            Register(BlackListReasonManage, "Управление списком причин добавления в черный список клиента", "Справочники");
            Register(AttractionChannelView, "Просмотр списка каналов привлечения клиентов", "Справочники");
            Register(AttractionChannelManage, "Управление списком каналов привлечения клиентов", "Справочники");
            Register(ParkingView, "Просмотр действий/статусов для постановки автотранспорта на стоянку", "Справочники");
            Register(ParkingManage, "Управление действиями/статусами для постановки автотранспорта на стоянку", "Справочники");
            Register(BlackoutView, "Просмотр настроек отключения начисления процентов/штрафов", "Справочники");
            Register(BlackoutManage, "Управление настройками отключения начисления процентов/штрафов", "Справочники");
            Register(SociallyVulnerableGroupView, "Просмотр списка социально уязвимых слоев населения", "Справочники");
            Register(SociallyVulnerableGroupManage, "Управление списком социально уязвимых слоев населения", "Справочники");
            Register(PersonalDiscountView, "Просмотр шаблонов персональных скидок", "Справочники");
            Register(PersonalDiscountManage, "Управление шаблонами персональных скидок", "Справочники");
            Register(PostponementView, "Просмотр видов отсрочек", "Справочники");
            Register(PostponementManage, "Управление видами отсрочек", "Справочники");
            Register(RequisiteTypeView, "Просмотр видов реквизитов", "Справочники");
            Register(RequisiteTypeManage, "Управление видами реквизитов", "Справочники");
            Register(PayTypeView, "Просмотр видов реквизитов", "Справочники");
            Register(PayTypeManage, "Управление видами реквизитов", "Справочники");
            Register(ContractActionCheckView, "Просмотр списка подтверждений по действиям", "Справочники");
            Register(ContractActionCheckManage, "Управление списом подтверждений по действиям", "Справочники");
            Register(ClientLegalFormView, "Просмотр видов правовых форм клиентов", "Справочники");
            Register(ClientLegalFormManage, "Управление видами правовых форм клиентов", "Справочники");
            Register(ClientDocumentTypeView, "Просмотр видов документов клиентов", "Справочники");
            Register(ClientDocumentTypeManage, "Управление видами документов клиентов", "Справочники");
            Register(ClientDocumentProviderView, "Просмотр органов выдачи документов клиентов", "Справочники");
            Register(ClientDocumentProviderManage, "Управление органами выдачи документов клиентов", "Справочники");
            Register(ClientEconomicActivityView, "Просмотр переченя ОКЭД клиента", "Справочники");
            Register(ClientEconomicActivityManage, "Управление переченем ОКЭД клиента", "Справочники");
            Register(ClientSignersAllowedDocumentTypeView, "Просмотр списка доступных документов для подписантов юр лиц", "Справочники");
            Register(ClientSignersAllowedDocumentTypeManage, "Управление списком доступных документов для подписантов юр лиц", "Справочники");
            Register(ContractCheckView, "Просмотр проверок по договору", "Справочники");
            Register(ContractCheckManage, "Управление проверками по договору", "Справочники");
            Register(CountryView, "Просмотр стран", "Справочники");
            Register(CountryManage, "Управление странами", "Справочники");
            Register(HolidayView, "Просмотр выходных и праздников", "Справочники");
            Register(HolidayManage, "Управление выходными и праздниками", "Справочники");
            Register(LoanSubjectView, "Просмотр видов субъектов договора", "Справочники");
            Register(LoanSubjectManage, "Управление видами субъектов договора", "Справочники");
            Register(LoanProductTypeView, "Просмотр видов субъектов договора", "Справочники");
            Register(LoanProductTypeManage, "Управление видами субъектов договора", "Справочники");
            Register(PrintTemplateView, "Просмотр шаблонов распечатки документов", "Справочники");
            Register(PrintTemplateManage, "Управление шаблонами распечатки документов", "Справочники");
            Register(VehicleMarkView, "Просмотр марок автотранспорта", "Справочники");
            Register(VehicleMarkManage, "Управление марками автотранспорта", "Справочники");
            Register(VehicleModelView, "Просмотр моделей автотранспорта", "Справочники");
            Register(VehicleModelManage, "Управление моделями автотранспорта", "Справочники");
            Register(VehicleLiquidityView, "Просмотр ликвидности автотранспорта", "Справочники");
            Register(VehicleLiquidityManage, "Управление ликвидностями автотранспорта", "Справочники");
            Register(VehicleWMIView, "Просмотр индексов изготовителя автотранспорта", "Справочники");
            Register(VehicleWMIManage, "Управление индексами изготовителя автотранспорта", "Справочники");
            Register(VehicleManufacturerView, "Просмотр изготовителей автотранспорта", "Справочники");
            Register(VehicleManufacturerManage, "Управление изготовителями автотранспорта", "Справочники");
            Register(VehicleCountryCodeView, "Просмотр кодов стран-изготовителей автотранспорта", "Справочники");
            Register(VehicleCountryCodeManage, "Управление кодами стран-изготовителей автотранспорта", "Справочники");
            Register(VehiclesBlackListView, "Просмотр черного списка автотранспортов", "Справочники");
            Register(VehiclesBlackListManage, "Управление черным списком автотранспортов", "Справочники");
            Register(ClientBlackListView, "Просмотр включения и исключения из черного списка клиентов", "Справочники");
            Register(ClientBlackListManage, "Управление включением и исключением из черного списка клиентов", "Справочники");
            Register(InsuranceRateView, "Просмотр ставок страхования", "Справочники");
            Register(InsuranceRateManage, "Управление ставками страхования", "Справочники");
            Register(PensionAgesView, "Просмотр пенсионного возраста", "Справочники");
            Register(PensionAgesManage, "Управление возрастом пенсионеров", "Справочники");
            Register(ManualCalculationClientExpensesView, "Просмотр расходов клиентов по прочим платежам (расчет ручной)", "Справочники");
            Register(ManualCalculationClientExpensesManage, "Управление расходами клиентов по прочим платежам (расчет ручной)", "Справочники");
            Register(ClientEstimationCompanyView, "Просмотр списка оценочных компаний", "Справочники");
            Register(ClientEstimationCompanyManage, "Управление списком оценочных компаний", "Справочники");

            Register(CollectionDictionariesView, "Просмотр справочников Collection (Статусы, Действия, Причины, Сценарии)", "Справочники");
            Register(CollectionDictionariesManage, "Управление справочниками Collection (Статусы, Действия, Причины, Сценарии)", "Справочники");

            //Отчеты
            Register(CashBookView, "Просмотр отчета \"Кассовая книга\"", "Отчёт");
            Register(CashBalanceView, "Просмотр отчета \"Остаток в кассе\"", "Отчёт");
            Register(ContractMonitoringView, "Просмотр отчета \"Мониторинг билетов\"", "Отчёт");
            Register(CashReportView, "Просмотр отчета \"Кассовый отчет\"", "Отчёт");
            Register(SellingReportView, "Просмотр отчета \"Отчет по реализации\"", "Отчёт");
            Register(CollateralReportView, "Просмотр отчета \"Отчет по недвижимости\"", "Отчёт");
            Register(CollectionReportView, "Просмотр отчета \"Отчет по collection\"", "Отчёт");
            Register(TransferredContractsReportView, "Просмотр отчета \"Отчет по договорам ЧСИ с авансом\"", "Отчёт");
            Register(DelayReportView, "Просмотр отчета \"Просрочки\"", "Отчёт");
            Register(DailyReportView, "Просмотр отчета \"Ежедневная сводка\"", "Отчёт");
            Register(ConsolidateReportView, "Просмотр отчета \"Сводный отчет\"", "Отчёт");
            Register(DiscountReportView, "Просмотр отчета о произведенных скидках", "Отчёт");
            Register(AccountCardView, "Просмотр отчета \"Карточка счета\"", "Отчёт");
            Register(OrderRegisterView, "Просмотр отчета \"Журнал ордер\"", "Отчёт");
            Register(AccountAnalysisView, "Просмотр отчета \"Анализ счета\"", "Отчёт");
            Register(AccountCycleView, "Просмотр отчета \"Обортная ведомость по счетам\"", "Отчёт");
            Register(GoldPriceView, "Просмотр отчета \"Контроль оценки золота\"", "Отчёт");
            Register(ExpenseMonthReportView, "Просмотр отчета \"Отслеживание расходов за месяц\"", "Отчёт");
            Register(ExpenseYearReportView, "Просмотр отчета \"Отслеживание расходов за год\"", "Отчёт");
            Register(OperationalReportView, "Просмотр оперативного отчета", "Отчёт");
            Register(ProfitReportView, "Просмотр отчета \"Начисленные доходы\"", "Отчёт");
            Register(SplitProfitReportView, "Просмотр отчета по фактическому разделению доходов", "Отчёт");
            Register(ReconciliationReportView, "Просмотр акта сверки", "Отчёт");
            Register(PaymentReportView, "Просмотр отчета о предстоящей оплате", "Отчёт");
            Register(IssuanceReportView, "Просмотр отчета \"Форма для выдачи свыше 3 000 000\"", "Отчёт");
            Register(ReinforceAndWithdrawReportView, "Просмотр отчета подкрепления и снятия", "Отчёт");
            Register(AccountableReportView, "Просмотр отчета \"Подотчетные платежи\"", "Отчёт");
            Register(NotificationReportView, "Просмотр отчета \"Отчёт по уведомлениям\"", "Отчёт");
            Register(RegionMonitoringReportView, "Просмотр отчета \"Региональный мониторинг по дате\"", "Отчёт");
            Register(StatisticsReportView, "Просмотр отчета \"Анализ статистики по филиалам\"", "Отчёт");
            Register(PrepaymentUsedReportView, "Просмотр отчета \"Авансов получено от заёмщиков\"", "Отчёт");
            Register(ContractOuterView, "Внешний просмотр информации о договоре", "Отчёт");
            Register(AccountMfoReportView, "Отчёт по погашениям СФК", "Отчёт");
            Register(SoftCollectionReportView, "Отчёты \"Система мотивации SOFT\"", "Отчёт");
            Register(СlientExpensesReportView, "Отчёт \"Расходы клиента\"", "Отчёт");
            Register(AccountMonitoringReportView, "Отчёт \"Мониторинг по номеру счета\"", "Отчёт");
            Register(ConsolidateIssuanceReportView, "Отчёт \"Сводный по выдачам\"", "Отчёт");
            Register(AnalysisIssuanceReportView, "Отчёт \"Анализ по выдачам\"", "Отчёт");
            Register(AttractionChannelReportView, "Отчёт \"Каналы привлечения клиента\"", "Отчёт");
            Register(EmloyeeContractReportView, "Отчёт \"Отчет по кредитам, выданным сотрудникам\"", "Отчёт");
            Register(CarParkingStatusReportView, "Отчёт \"Отчет статуса стоянки по машинам\"", "Отчёт");
            Register(CarParkingStatusReportForCARTASView, "Отчёт \"Отчет по статусам машин для CARTAS\"", "Отчёт");
            Register(UserPermissionsReportView, "Отчёт \"Отчет по правам пользователей\"", "Отчёт");
            Register(PosTerminalBookReportView, "Просмотр отчета \"Отчет по POS-терминалу\"", "Отчёт");
            Register(CashInTransitReportView, "Просмотр отчета \"Отчет о денежных средствах в пути\"", "Отчёт");
            Register(PaymentsMoreThanTenReportView, "Просмотр отчета \"Форма по погашению свыше 10 000 000\"", "Отчёт");
            Register(AuditOnPledgesReportView, "Просмотр отчета \"Отчет для ревизии по залогам\"", "Отчёт");
            Register(ContractDiscountReportView, "Просмотр отчета \"Отчет по скидкам\"", "Отчёт");
            Register(ArrestedAndDeadReportView, "Просмотр отчета \"Умершие и осужденные\"", "Отчёт");
            Register(TasOnlineReportView, "Просмотр отчета \"Отчет для сверки с Тасонлайн\"", "Отчёт");
            Register(TmfReportView, "Просмотр отчета \"Отчет для сверки с TMF\"", "Отчёт");
            Register(VintageAnalysisView, "Просмотр анализа \"Винтажный анализ\"", "Аналитика");


            Register(InsurancePolicyReportView, "Просмотр отчета \"Отчет по Договорам страхования жизним\"", "Отчёт");
            Register(InsurancePolicyActReportView, "Просмотр отчета \"Акт сверки с СК\"", "Отчёт");
            Register(PrepaymentMonitoringReportView, "Просмотр отчета \"Мониторинг по освоению аванса\"", "Отчёт");
            Register(DelayReportForSoftCollectionView, "Отчёт \"Отчет по просрочкам для SC\"", "Отчёт");
            Register(BuyoutContractsReportView, "Просмотр отчета \"Отчет по выкупленным договорам\"", "Отчёт");
            Register(BuyoutGprReportView, "Отчёт \"Отчет по выкупленным договорам Гепард\"", "Отчёт");
            Register(RefinanceGrpReportView, "Отчёт \"Отчет рефинансирования по договорам Гепард\"", "Отчёт");
            Register(KdnFailureStatisticsReportView, "Просмотр отчета \"Статистика отказов из Заявок по причине высокого КДН\"", "Отчёт");
            Register(RequestsToPkbReportView, "Просмотр отчета \"Запросы в ПКБ\"", "Отчёт");
            Register(BalanceSheetReportView, "Отчёт \"Оборотно-сальдовая ведомость по счету\"", "Отчёт");
            Register(ReportLogsView, "Просмотр истории выгрузки отчетов", "Отчёт");

            // ТасОнлайн отчеты
            Register(LoanPortfolioReportView, "Отчёт \"Кредитный портфель\"", "Отчёт");
            Register(CurrentAccountDebtAndBalanceReportView, "Отчёт \"О задолженности и остатке на текущем счете\"", "Отчёт");
            Register(CallingCustomersWithCurrentPaymentDateReportView, "\"Отчёт для обзвона клиентов с текущей датой платежа\"", "Отчёт");
            Register(SecurityServiceReportView, "\"Отчёт для СБ\"", "Отчёт");
            Register(WithdrawalFromPledgeReportView, "Отчёт \"Реестр постановки/снятия с залога\"", "Отчёт");
            Register(SecurityServiceEffectivenessReportView, "Отчёт \"Эффективность СБ\"", "Отчёт");
            Register(DelayReportTasOnlineView, "Просмотр отчета \"Просрочки TasOnline\"", "Отчёт");

            // Online отчеты
            Register(OnlineReportsView, "Просмотр отчетов \"Online\"", "Отчёт");

            // домены и его значения
            Register(DomainView, "Просмотр доменов и их значений", "Домены");
            Register(DomainManage, "Управление доменами и его значениями", "Домены");
            Register(WithoutDriveLicenseReportView, "Просмотр отчета \"Отчет по залогам договоров Без права вождения\"", "Отчёт");
            Register(BuyoutContractsWithInscriptionReportView, "Просмотр отчета \"Отчет по выкупленным договорам через исполнительную надпись\"", "Отчёт");
            Register(PhotoReportView, "Просмотр отчета \"Отчет для проверки фотоотчета\"", "Отчёт");
            Register(OnlinePaymentManageReportView, "Просмотр отчета \"Консолидация по проверке онлайн оплат\"", "Отчёт");
            Register(SMSSenderReportView, "Просмотр отчета \"Отчет по смс-отправкам\"", "Отчёт");

            // PowerBi
            Register(ReportDataManage, "Управление для PowerBi", "Оплаты онлайн и на р/с");

            //Договора из Заявок МП
            Register(ContractFromApplicationView, "Просмотр списка заявок МП для создания договоров", "Обработка заявок МП");
            Register(ContractFromApplicationAllView, "Просмотр списка всех заявок МП", "Обработка заявок МП");

            //Разное
            Register(OldPrintButtonView, "Отображение старой кнопки печати", "Разное");

            //1C
            Register(ExportDataTo1C, "Экспорт данных в 1С", "1C");

            //Legal Collection
            Register(LegalCollectionView, "Просмотр в Легал Коллекшн", "Легал Коллекшн");
            Register(LegalCollectionManage, "Управление в Легал Коллекшн", "Легал Коллекшн");
            Register(LegalCollectionManageAccrual, "Управление начислениями в Легал Коллекшн", "Легал Коллекшн");
            Register(LegalCollectionDeleteDocument, "Удаление документов в Легал Коллекшн", "Легал Коллекшн");
            Register(LegalCollectionChangeCourse, "Смена направления в Legal collection", "Легал Коллекшн");
            Register(LegalCollectionAmountsView, "Просмотр задолженности в калькуляторе договора по Legal collection", "Легал Коллекшн");

            //Auction
            Register(AuctionView, "Просмотр кнопки Выкупа в Аукцион", "Аукцион");
            Register(AuctionManage, "Управление кнопки Выкупа в Аукцион", "Аукцион");

            //TasOnline
            Register(TasOnlineManager, "Менеджер Tas Online", "Tas Online Application");
            Register(TasOnlineVerificator, "Верификатор Tas Online", "Tas Online Application");
            Register(TasOnlineAdministrator, "Администратор Tas Online", "Tas Online Application");
            Register(TasOnlineCreditAdministrator, "Кредитный Администратор Tas Online", "Tas Online Application");

            //Hard Collection
            Register(HardCollectionManager, "Менеджер Хард Коллекшн", "Хард Коллекшн");
            Register(HardCollectionRegionDirector, "Региональный директор Хард Коллекшн", "Хард Коллекшн");
            Register(HardCollectionMainDirector, "Главный директор Хард Коллекшн", "Хард Коллекшн");

            // Manual Update
            Register(ManualUpdateAllowed, "Менеджер по ручному добавлению", "Администрирование");
        }

        private static void Register(string name, string displayName, string description = null)
        {
            var all = (List<Permission>)All;
            all.Add(new Permission
            {
                Name = name,
                DisplayName = displayName,
                Description = description
            });
        }
    }
}