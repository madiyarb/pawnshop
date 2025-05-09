using System;
using System.Net.NetworkInformation;

namespace Pawnshop.Core
{
    public static class Constants
    {
        #region Регулярные выражения для проверки правильности заполнения полей

        public const string CLIENT_NAME_REGEX = "^[A-ZА-ЯЁӘІҢҒҮҰҚӨҺ][ -]?([a-zA-Zа-яёәіңғүұқөһА-ЯЁӘІҢҒҮҰҚӨҺ]+[ -])*[a-zA-Zа-яёәіңғүұқөһА-ЯЁӘІҢҒҮҰҚӨҺ]*$";
        public const string CLIENT_NAME_CYRILLIC_REGEX = "^[а-яёәіңғүұқөһА-ЯЁӘІҢҒҮҰҚӨҺ -]*$";
        public const string CLIENT_NAME_LATIN_REGEX = "^[a-zA-Z- ]*$";
        public const string CLIENT_FULLNAME_REGEX = "^[a-zA-Zа-яёәіңғүұқөһА-ЯЁӘІҢҒҮҰҚӨҺ -]*$";
        public const string KAZAKHSTAN_PHONE_REGEX = @"^77\d{9}$";
        public const string PHONE_REGEX = @"\d{11}$";
        public const string IIN_REGEX = @"^\d{12}$";
        public const string PASSWORD_REGEX = @"(?=.*[0-9])(?=.*[!@#$%^&*()_+|\-=\\{}\[\]:"";'<>?,.])(?=.*[a-z])(?=.*[A-Z])[0-9a-zA-Z!@#$%^&*()_+|\-=\\{}\[\]:"";'<>?,.]{8,}";
        public const string CYRILLIC_REGEX = "[А-Яа-яЁё]";
        public const string VIN_REGEX = @"^[0-9A-HJK-NPR-Z][-0-9A-HJK-NPR-Z]+$";
        public const string TECH_PASSPORT_REGEX = @"^[A-Z]{2}\d{8}$";
        public const string TRANSPORT_NUMBER_REGEX = @"^[0-9A-Z]+$";
        public const string REGEX_NOT_MATCH = @"([A-HJK-NPR-Z0-9-]+)";
        public const string RCA_REGEX = @"^\d{16,16}$";
        public const string CADASTRAL_NUMBER_REGEX = @"^[а-яА-ЯЁё0-9\/:]+$";
        #endregion

        #region константы по кодом доменов и их значений

        public const string CONTACT_OWNER_TYPE_DOMAIN = "CONTACT_OWNER_TYPE";
        public const string INCOME_TYPE_DOMAIN = "INCOME_TYPE";
        public const string FORMAL_INCOME_DOCTYPES = "FORMAL_INCOME_DOCTYPES";
        public const string INFORMAL_INCOME_DOCTYPES = "INFORMAL_INCOME_DOCTYPES";
        public const string ASSET_TYPE_DOMAIN = "ASSET_TYPE";
        public const string DOMAIN_CONTACT_TYPE_CODE = "CONTACT_TYPE";
        public const string DOMAIN_CONTACT_CATEGORY = "CLIENT_CONTACT_CATEGORY";
        public const string DOMAIN_CONTACT_SOURCE = "CLIENT_CONTACT_SOURCE";
        public const string DOMAIN_VALUE_CONTACT_CONTRACT = "CONTRACT_CONTACTS";
        public const string DOMAIN_VALUE_CONTACT_ACTUALIZED = "ACTUALIZED_CONTACTS";
        public const string DOMAIN_VALUE_FROM_CLIENT = "FROM_CLIENT";
        public const string DOMAIN_VALUE_FROM_THIRD_PARTIES = "FROM_THIRD_PARTIES";
        public const string DOMAIN_VALUE_OPEN_SOURCE = "OPEN_SOURCE";
        public const string DOMAIN_VALUE_CONTRACT_CLIENT_PROFILE = "CONTRACT_CLIENT_PROFILE";
        public const string DOMAIN_VALUE_MOBILE_PHONE_CODE = "MOBILE_PHONE";
        public const string DOMAIN_VALUE_WORK_PHONE_CODE = "WORK_PHONE";
        public const string DOMAIN_VALUE_HOME_PHONE_CODE = "HOME_PHONE";
        public const string DOMAIN_VALUE_EMAIL_CODE = "EMAIL";
        public const string WORK_EXPERIENCE_DOMAIN = "WORK_EXPERIENCE";
        public const string BUSINESS_SCOPE_DOMAIN = "BUSINESS_SCOPE";
        public const string EMPLOYEE_COUNT_DOMAIN = "EMPLOYEE_COUNT";
        public const string POSITION_TYPE_DOMAIN = "POSITION_TYPE";
        public const string RESIDENCE_ADDRESS_TYPE_DOMAIN = "RESIDENCE_ADDRESS_TYPE";
        public const string MARITAL_STATUS_DOMAIN = "MARITAL_STATUS";
        public const string EDUCATION_TYPE_DOMAIN = "EDUCATION_TYPE";
        public const string TOTAL_WORK_EXPERIENCE_DOMAIN = "TOTAL_WORK_EXPERIENCE";
        public const string EXPENSES_CONFIGURATION_DOMAIN = "EXPENSES_CONFIGURATION";
        public const string MARRIED_MARITAL_STATUS_DOMAIN_VALUE = "MARRIED";
        public const string INVESTMENTS_AND_CURRENT_ASSETS = "INVESTMENTS_AND_CURRENT_ASSETS";
        public const string OKED_KINDS_DOMAIN_VALUE = "OKED_KINDS";
        public const string OKED_KINDS_MAIN_CODE = "MAJOR";
        public const string SIGNER_POSITIONS_DOMAIN_VALUE = "SIGNER_POSITIONS";
        public const string LOAN_PURPOSE_DOMAIN_VALUE = "LOAN_PURPOSE";
        public const string BUYOUT_ADDITION = "ADDITION";
        public const string BUYOUT_AUTOMATIC_BUYOUT = "AUTOMATIC_BUYOUT";
        public const string BUYOUT_PARTIAL_PAYMENT = "PARTIAL_PAYMENT";
        public const string REALTY_TYPE = "REALTY_TYPE";
        public const string REALTY_PURPOSE = "REALTY_PURPOSE";
        public const string REALTY_PHONE_CONNECTION = "REALTY_PHONE_CONNECTION";
        public const string REALTY_LIGHTNING = "REALTY_LIGHTNING";
        public const string REALTY_COLD_WATER_SUPPLY = "REALTY_COLD_WATER_SUPPLY";
        public const string REALTY_GAS_SUPPLY = "REALTY_GAS_SUPPLY";
        public const string REALTY_HEATING = "REALTY_HEATING";
        public const string REALTY_HOT_WATER_SUPPLY = "REALTY_HOT_WATER_SUPPLY";
        public const string REALTY_SANITATION = "REALTY_SANITATION";
        public const string REALTY_WALL_MATERIAL = "REALTY_WALL_MATERIAL";
        public const string REALTY_COMMERCIAL_PROPERTY = "COMMERCIAL_PROPERTY";
        public const string REALTY_APPARMENT = "APPARMENT";
        public const string REALTY_HOUSE = "HOUSE";
        public const string REALTY_COMMERCIAL_LAND = "COMMERCIAL_LAND";
        public const string REALTY_PARKING = "PARKING";
        public const string EVALUATE_MOBILE_APP = "EVALUATE_MOBILE_APP";

        public const string CLIENT_DOCUMENT_SUB_TYPE = "CLIENT_DOCUMENT_SUB_TYPE";
        public const string REALTY_DOCUMENT = "REALTY_DOCUMENT";

        public const string CLIENT_SUB_TYPE = "CLIENT_SUB_TYPE";
        public const string ESTIMATION_COMPANY = "ESTIMATION_COMPANY";

        public const string TAS_ONLINE_INTEGRATION_SETTINGS_CODE = "TAS_ONLINE";
        public const string PAY_OPERATION_INTEGRATION_SETTINGS_CODE = "PAY_OPERATION";
        public const string FIRST_CREDIT_BUREAU_INTEGRATION_SETTINGS_CODE = "FIRST_CREDIT_BUREAU";
        public const string STATE_CREDIT_BUREAU_INTEGRATION_SETTINGS_CODE = "STATE_CREDIT_BUREAU";
        public const string TELEGRAM_BATCH_LOGS_SETTINGS_CODE = "TELEGRAM_BATCH_LOGS";
        public const string TELEGRAM_RESTRUCTURING_SETTINGS_CODE = "TELEGRAM_RESTRUCTURING";
        public const string OUTER_SERVICE_TYPES_DOMAIN_VALUE = "OUTER_SERVICE_TYPES";
        public const string BUYOUT_REASON_CODE = "BUYOUT_REASON";

        public static string NOTIONAL_RATE_TYPES = "NOTIONAL_RATE_TYPES";
        public static string NOTIONAL_RATE_TYPES_VPM = "NOTIONAL_RATE_TYPES_VPM";
        public static string NOTIONAL_RATE_TYPES_MRP = "NOTIONAL_RATE_TYPES_MRP";
        public const string TMF_INTEGRATION_SETTINGS_CODE = "TMF_PAYMENT";
        public const string ABS_ONLINE_INTEGRATION_SETTINGS_CODE = "ABS_ONLINE";
        public const string ABS_ONLINE_PDF_INTEGRATION_SETTINGS_CODE = "ABS_ONLINE_PDF";
        public const string SMS_INTEGRATION_SERVICE_SETTING_CODE = "SMS_INTEGRATION_SERVICE";

        public const string PROCESSING_KZ_ENDPOINT = "PROCESSING_KZ_ENDPOINT";
        public const string PROCESSING_KZ_CARD = "PROCESSING_KZ_CARD";
        public const string FINCORE_URL = "FINCORE_URL";
        public const string DEBTOR_REGISTRY = "DEBTOR_REGISTRY";
        public const string SUSN = "SUSN";
        public const string BANKRUPT_SHORT = "BANKRUPT_SHORT";
        public const string BANKRUPT_ONLINE = "BANKRUPT_ONLINE";

        public const string API_GATEWAY_TASLAB = "API_GATEWAY_TASLAB";
        public const string ONLINE_1C_INTEGRATION_SETTINGS_CODE = "ONLINE_1C";

        public const string BITRIX_TRIGGER = "BITRIX_TRIGGER";
        public const string BITRIX_REST_URL = "BITRIX_REST_URL";
        public const string CRM_CONTACT_ADD = "CRM_CONTACT_ADD";
        public const string CRM_CONTACT_UPDATE = "CRM_CONTACT_UPDATE";
        public const string CRM_CONTACT_LIST = "CRM_CONTACT_LIST";
        public const string CRM_CONTACT_GET = "CRM_CONTACT_GET";
        public const string CRM_DEAL_ADD = "CRM_DEAL_ADD";
        public const string CRM_DEAL_UPDATE = "CRM_DEAL_UPDATE";
        public const string CRM_DEAL_LIST = "CRM_DEAL_LIST";
        public const string CRM_DEAL_GET = "CRM_DEAL_GET";

        public const int BITRIX_CATEGORY = 87;

        public const string SETTINGS_DOMAIN = "SETTINGS";
        public const string FCB_NEGATIVE_REPORT_CHECK_DOMAIN_VALUE = "FCB_NEGATIVE_REPORT_CHECK";
        public const string SUSN_ASP_STATUS_CODE = "26000";

        public const string KFM_SETTING = "KFM_SETTING";

        public const string BUSINESS_LOAN_PURPOSE = "BUSINESS";
        public const string INVESTMENTS_LOAN_PURPOSE = "INVESTMENTS";
        public const string CURRENT_ASSETS_LOAN_PURPOSE = "CURRENT_ASSETS";
        public const string INVESTMENTS_AND_CURRENT_ASSETS_LOAN_PURPOSE = "INVESTMENTS_AND_CURRENT_ASSETS";
        public const string DOMAIN_CREDIT_BUREAUS_CODE = "CREDIT_BUREAUS";
        public const string DOMAIN_VALUE_CREDIT_BUREAU_FCB_CODE = "CREDIT_BUREAU_FCB";
        public const string DOMAIN_VALUE_CREDIT_BUREAU_MKB_CODE = "CREDIT_BUREAU_MKB";
        public const decimal BUSINESS_LOAN_PURPOSE_MINIMAL_SUM = 1000000;
        public const int BUSINESS_LOAN_PURPOSE_MAXIMAL_MRP_MULTIPLIER = 2710;

        #endregion

        #region Названия типов субъектов

        public const string LOAN_SUBJECT_MERCHANT = "MERCHANT";
        public const string COBORROWER_CODE = "COBORROWER";
        public const string GUARANTOR_CODE = "GUARANTOR";
        public const string PLEDGER_CODE = "PLEDGER";
        public const string MAIN_PLEDGER_CODE = "MAIN_PLEDGER";
        #endregion

        #region Категории

        public const int WITH_DRIVE_RIGHT_CATEGORY = 1;
        public const int WITHOUT_DRIVE_RIGHT_CATEGORY = 12;
        public const string WITH_DRIVE_RIGHT_CATEGORY_CODE = "WITH_DRIVE_RIGHT";
        public const string WITHOUT_DRIVE_RIGHT_CATEGORY_CODE = "WITHOUT_DRIVE_RIGHT";
        #endregion

        #region Роли

        /// <summary>
        /// Идентификатор пользователя Администратор
        /// </summary>
        public const int ADMINISTRATOR_IDENTITY = 1;
        #endregion

        #region Начисление процентов

        /// Количество дней начисления процентов при реализации на дискретных договорах
        /// </summary>
        public const int SELLING_DISCRETE_LOAN_PERIOD = 30;
        /// <summary>
        /// Количество дней начисления процентов при реализации на аннуитетных договорах
        /// </summary>
        public const int SELLING_ANNUITY_LOAN_PERIOD = 60;
        /// <summary>
        /// Количество месяцев начисления процентов при реализации на аннуитетных договорах
        /// </summary>
        public const int SELLING_ANNUITY_LOAN_MONTH = 2;
        #endregion

        #region Бизнес-операции

        #region Константы по названиям бизнес операциям

        public const string BUSINESS_OPERATION_EXPENSE_CREATION = "EXPENSE_CREATION";
        public const string BUSINESS_OPERATION_EXPENSE_CANCEL = "EXPENSE_CANCEL";
        public const string BUSINESS_OPERATION_EXPENSE_PAYMENT = "EXPENSE_PAYMENT";
        public const string BUSINESS_OPERATION_EXPENSE_PREPAYMENT_RETURN = "EXPENSE_PREPAYMENT_RETURN";
        public const string BUSINESS_OPERATION_REMITTANCE_IN = "REMITTANCE_IN";
        public const string BUSINESS_OPERATION_REMITTANCE_OUT = "REMITTANCE_OUT";
        public const string BO_INTEREST_ACCRUAL = "PROFIT.ACCOUNT";
        public const string BO_INTEREST_ACCRUAL_OFFBALANCE = "PROFIT.ACCOUNT.OFFBALANCE";
        public const string BO_DISCOUNT = "DISCOUNT";
        public const string BO_DISCOUNT_CORRECTION = "DISCOUNT_CORRECTION";
        public const string BO_RESTORE_ON_BALANCE = "RESTORE_ON_BALANCE";
        public const string BO_INSCRIPTION_WRITEOFF = "INSCRIPTION_WRITEOFF";
        public const string BO_INSCRIPTION_OFFBALANCE_WRITEOFF = "INSCRIPTION_OFFBALANCE_WRITEOFF";
        public const string BO_REMITTANCE_IN = "REMITTANCE_IN";
        public const string BO_REMITTANCE_OUT = "REMITTANCE_OUT";

        public const string BO_TASONLINE_PAYMENT = "PAYMENT4TASONLINE";
        public const string BO_PREPAYMENT_MOVE_TO_TRANSIT = "PREPAYMENT_MOVE_TO_TRANSIT";
        public const string BO_PREPAYMENT_MOVE_FROM_TRANSIT = "PREPAYMENT_MOVE_FROM_TRANSIT";
        public const string BO_PENALTY_LIMIT_ACCRUAL = "PENALTY_LIMIT_ACCRUAL";
        public const string BO_PENALTY_LIMIT_WRITEOFF = "PENALTY_LIMIT_WRITEOFF";
        public const string BO_INTEREST_ACCRUAL_OVERDUEDEBT = "PROFIT.OVERDUE_ACCOUNT";
        public const string BO_TMF_PAYMENT = "PAYMENT4TMF";
        public const string BO_CREDITLINE_CLOSE = "CLOSE";
        /// <summary>
        /// Префикс настройки бизнес операции по начислению на вне балансе
        /// </summary>
        public const string BO_OFFBALANCE_POSTFIX = ".OFFBALANCE";
        public const string BO_REFINANCE = "REFINANCE";
        #endregion

        #region Константы по кодам настроек БО

        public const string BO_SETTING_PAYMENT_OVERDUE_PROFIT = "PAYMENT_OVERDUE_PROFIT";
        public const string BO_SETTING_PAYMENT_PROFIT = "PAYMENT_PROFIT ";
        public const string BO_SETTING_PAYMENT_ACCOUNT = "PAYMENT_ACCOUNT";
        public const string BO_SETTING_MERCHANT_CASH_OUT = "MERCHANT_CASH_OUT";
        public const string BO_SETTING_MERCHANT_CURRENT_ACCOUNT_OUT = "MERCHANT_CURRENT_ACCOUNT_OUT";
        public const string BO_DISCOUNT_OVERDUE_PROFIT = "DISCOUNT_OVERDUE_PROFIT";
        public const string BO_DISCOUNT_PROFIT = "DISCOUNT_PROFIT";
        public const string BO_SETTING_IMPRESTS = "IMPRESTS";
        public const string BO_SETTING_INTEREST_ACCRUAL = "INTEREST_ACCRUAL";
        public const string BO_SETTING_INTEREST_ACCRUAL_MIGRATION = "INTEREST_ACCRUAL_MIGRATION";
        public const string BO_SETTING_INTEREST_ACCRUAL_OVERDUEDEBT = "INTEREST_ACCRUAL_OVERDUEDEBT";
        public const string INTEREST_PAID_OFFBALANCE = "INTEREST_PAID_OFFBALANCE";
        /// <summary>
        /// Префикс настройки бизнес операции по начислению процентов в выходные дни
        /// </summary>
        public const string BO_SETTING_INTEREST_ACCRUAL_ON_HOLIDAYS = "INTEREST_ACCRUAL_ON_HOLIDAYS";
        /// <summary>
        /// Префикс настройки бизнес операции по начислению штрафов
        /// </summary>
        public const string BO_SETTING_PENALTYACCRUAL_PREFIX = "PENALTYACCRUAL.";
        public const string BO_SETTING_CORR_PENALTYACCRUAL_PREFIX = "CORR.PENALTYACCRUAL.";
        /// <summary>
        /// Префикс настройки бизнес операции по начислению на вне балансе
        /// </summary>
        public const string BO_SETTING_OFFBALANCE_POSTFIX = "_OFFBALANCE";

        #endregion

        #endregion

        #region Названия настроек аккаунта

        public const string ACCOUNT_SETTING_DEPO = "DEPO";
        public const string ACCOUNT_SETTING_DEPO_MERCHANT = "DEPO_MERCHANT";
        public const string ACCOUNT_SETTING_EXPENSE = "EXPENSE";
        public const string ACCOUNT_SETTING_ACCOUNT = "ACCOUNT";
        public const string ACCOUNT_SETTING_OVERDUE_ACCOUNT = "OVERDUE_ACCOUNT";
        public const string ACCOUNT_SETTING_PROFIT = "PROFIT";
        public const string ACCOUNT_SETTING_OVERDUE_PROFIT = "OVERDUE_PROFIT";
        public const string ACCOUNT_SETTING_PENY_ACCOUNT = "PENY_ACCOUNT";
        public const string ACCOUNT_SETTING_PENY_PROFIT = "PENY_PROFIT";
        public const string ACCOUNT_SETTING_PENALTY_LIMIT = "PENALTY_LIMIT";
        public const string ACCOUNT_SETTING_RECEIVABLE_ONLINEPAYMENT = "RECEIVABLE_ONLINEPAYMENT";
        public const string ACCOUNT_SETTING_CREDIT_LINE_LIMIT = "CREDIT_LINE_LIMIT";
        
        public const string PROFIT_OFFBALANCE = "PROFIT_OFFBALANCE"; // Начисленные проценты на внебалансе
        public const string OVERDUE_PROFIT_OFFBALANCE = "OVERDUE_PROFIT_OFFBALANCE"; // Просроченные проценты на внебалансе
        public const string PENY_ACCOUNT_OFFBALANCE = "PENY_ACCOUNT_OFFBALANCE"; // Пеня на просроченный основной долг на внебалансе
        public const string PENY_PROFIT_OFFBALANCE = "PENY_PROFIT_OFFBALANCE"; // Пеня на просроченные проценты на внебалансе
        public const string PROVISIONS = "PROVISIONS"; // Провизии
        public const string ACCOUNT_SETTING_PROFIT_OFFBALANCE = "PROFIT_OFFBALANCE";
        public const string ACCOUNT_SETTING_OVERDUE_PROFIT_OFFBALANCE = "OVERDUE_PROFIT_OFFBALANCE";
        public const string ACCOUNT_SETTING_PENY_ACCOUNT_OFFBALANCE = "PENY_ACCOUNT_OFFBALANCE";
        public const string ACCOUNT_SETTING_PENY_PROFIT_OFFBALANCE = "PENY_PROFIT_OFFBALANCE";
        #endregion

        #region Названия платежных операций

        public const string PAY_OPERATION_IBAN = "IBAN";
        public const string PAY_TYPE_ONLINE = "ONLINE";
        public const string PAY_TYPE_CASH = "CASH";
        #endregion

        #region Шаблоны текста

        public const string REASON_AUTO_END_PERIOD_CONTRACTS_PROLONGATION = "Автоматическое продление договора {0} от {1} по {2}";
        public const string REASON_AUTO_PAYMENT = "Автоматическая оплата договора {0} от {1}";
        public const string REASON_AUTO_BUYOUT = "Автоматический выкуп договора {0} от {1}";
        public const string PROCESSING_NOTE_KAZ = "Құрметті тұтынушылар! Сағат 22:30-ден кейін (Нұр-Сұлтан қ. уақыты бойынша) жүргізілген төлем келесі күнімен игеріледі";
        public const string PROCESSING_NOTE_RUS = "Уважаемые клиенты! Оплата, произведенная после 22:30 (по времени г. Нур-Султан), будет зачислена на следующий день";

        #endregion

        #region Коды продуктов

        public const string PRODUCT_BUYCAR = "BUYCAR";
        public const string PRODUCT_DAMU = "DAMU";
        public const string PRODUCT_TMF_REALTY = "TMF_REALTY";
        public const string PRODUCT_GRANT = "GRANT";
        #endregion

        #region Код по умолчанию для схемы погашения
        public const int DEFAULT_PAYMENT_ORDER_SCHEMA = 10;
        #endregion

        #region Типы иерархий

        public const string TH_EXPENSES_INSCRIPTIONS = "EXPENSES_INSCRIPTIONS";
        public const string TH_EXPENSES_STATE_DUTY = "EXPENSES_STATE_DUTY";

        public const string TYPE_HIERARCHY_CONTRACTS_ALL = "CONTRACTS_ALL";
        public const string TYPE_HIERARCHY_TERMS_ALL = "TERMS_ALL";
        #endregion

        #region Типы документов
        /// <summary>
        /// Доверенность
        /// </summary>
        public const string POWER_OF_ATTORNEY = "POWER_OF_ATTORNEY";
        /// <summary>
        /// Приказ
        /// </summary>
        public const string ORDER = "ORDER";
        /// <summary>
        /// Устав
        /// </summary>
        public const string CHARTER = "CHARTER";
        /// <summary>
        /// Удостоверение личности
        /// </summary>
        public const string IDENTITYCARD = "IDENTITYCARD";
        /// <summary>
        /// Паспорт РК
        /// </summary>
        public const string PASSPORTKZ = "PASSPORTKZ";
        /// <summary>
        /// Вид на жительство
        /// </summary>
        public const string RESIDENCE = "RESIDENCE";
        /// <summary>
        /// Номер паспорта РФ
        /// </summary>
        public const string PASSPORTRU = "PASSPORTRU";
        /// <summary>
        /// Другой документ
        /// </summary>
        public const string ANOTHER = "ANOTHER";
        #endregion

        #region Типы правовых форм

        public const string TOO_CODE = "LIMITED_LIABILITY_PARTNERSHIP";
        public const string INDIVIDUAL = "INDIVIDUAL";
        public const string SOLE_PROPRIETOR = "SOLE_PROPRIETOR";
        public const string MICRO_ENTREPRENEUR_NOT_REGISTERED = "MICRO_ENTREPRENEUR_NOT_REGISTERED";
        #endregion

        #region Коды Филиалов

        public const string AKU = "AKU";
        public const string AST = "AST";
        public const string ATY = "ATY";
        public const string KRG = "KRG";
        public const string TAL = "TAL";
        public const string PAV = "PAV";
        public const string AKT = "AKT";
        public const string KSK = "KSK";
        public const string SEM = "SEM";
        public const string ABA = "ABA";
        public const string OSK = "OSK";
        public const string TRZ = "TRZ";
        public const string SHM = "SHM";
        public const string KKS = "KKS";
        public const string KZO = "KZO";
        public const string TKS = "TKS";
        public const string BZK = "BZK";
        public const string SAR = "SAR";
        public const string ALA = "ALA";
        public const string SRM = "SRM";
        public const string ZHK = "ZHK";
        public const string URL = "URL";
        public const string NUR = "NUR";
        public const string SHY = "SHY";
        public const string DOS = "DOS";
        public const string KSN = "KSN";
        public const string PET = "PET";
        public const string TLG = "TLG";
        public const string ZHA = "ZHA";
        public const string ZHN = "ZHN";
        public const string NBO = "NBO";
        public const string OKA = "OKA";
        public const string KBU = "KBU";
        public const string BKS = "BKS";
        public const string TSA = "TSA";
        public const string TSO = "TSO";
        public const string KRD = "KRD";
        public const string SKO = "SKO";
        public const string ESB = "ESB";
        public const string STA = "STA";
        #endregion

        #region Коды полей клиента

        public const string Surname = "Surname";
        public const string Name = "Name";
        public const string Patronymic = "Patronymic";
        public const string MaidenName = "MaidenName";
        public const string FullName = "FullName";
        public const string CitizenshipId = "CitizenshipId";
        public const string BirthDay = "BirthDay";
        public const string CheifId = "CheifId";
        public const string IdentityNumber = "IdentityNumber";

        #endregion

        #region Коды полей недвижимости
        public const string CadastralNumber = "CadastralNumber";
        public const string Rca = "Rca";
        #endregion

        #region Коды статусов парковки
        public const string AT_CLIENT = "AT_CLIENT";
        public const string INPARKING_WAITING = "INPARKING_WAITING";
        public const string INPARKING_LAWYER = "INPARKING_LAWYER";
        public const string INPARKING_SELLING = "INPARKING_SELLING";
        public const string TRANSFERRED_CARTAS = "TRANSFERRED_CARTAS";
        public const string INPARKING_AUCTION = "INPARKING_AUCTION";
        public const string TRANSFERRED_BUYER = "TRANSFERRED_BUYER";
        public const string INPARKING_LEGAL = "INPARKING_LEGAL";
        public const string TAKENOUT_PARKING = "TAKENOUT_PARKING";
        #endregion

        #region Коды адресов

        public static string REGISTRATION = "REGISTRATION";
        public static string LEGALPLACE = "LEGALPLACE";

        #endregion

        #region Коды коллекшн
        public const string COLLECTION_INTEGRATION_SETTING_CODE = "COLLECTION";

        public const string NOCOLLECTION_STATUS = "NOT_INCOLLECTION";
        public const string SOFTCOLLECTION_STATUS = "SOFT_COLLECTION";
        public const string HARDCOLLECTION_STATUS = "HARD_COLLECTION";
        public const string LEGALCOLLECTION_STATUS = "LEGAL_COLLECTION";
        public const string LEGALHARDCOLLECTION_STATUS = "LEGALHARD_COLLECTION";

        public const string SEND_SOFTCOLLECTION_ACTION = "SEND_SOFTCOLLECTION";
        public const string SEND_HARDCOLLECTION_ACTION = "SEND_HARDCOLLECTION";
        public const string SEND_LEGALCOLLECTION_ACTION = "SEND_LEGALCOLLECTION";
        public const string SEND_LEGALHARDCOLLECTION_ACTION = "SEND_LEGALHARDCOLLECTION";
        public const string CANCEL_COLLECTION_ACTION = "CANCEL_COLLECTION";

        public const string PARKING_STATUS_CHANGE_REASONCODE = "PARKING_STATUS_CHANGE";
        public const string CLOSE_COLLECTION_REASONCODE = "CLOSE_COLLECTION";
        public const string DELAYDAYS_REASONTYPE = "DELAY_DAYS";
        public const string DELAY1_CHANGE_REASONCODE = "DELAY1";
        public const string DELAY2_CHANGE_REASONCODE = "DELAY2";
        public const string DELAY3_CHANGE_REASONCODE = "DELAY3";

        public const int SOFT_OVERDUE_DAYS = 9;
        // Стандартное значение дней просрочки для legalhard collection
        public const int LEGALHARD_OVERDUE_DAYS = 90;
        // Значение дней просрочки по недвижимости для legalhard collection
        public const int LEGALHARD_OVERDUE_DAYS_BY_REALESTATE = 40;

        #endregion
        
        #region Найстройки Auction
        public const string AUCTION_INTEGRATION_SETTING_CODE = "AUCTION";
        public const string AUCTION_HIERARCHY_TYPE_CODE = "EXPENSES_CARTAS"; // код типа(HierarchyType)
        public const string AUCTION_EXPENSE_BUSINESS_OPERATION_CODE = "EXPENSE_CREATION"; // код БО для создания расхода
        public const string AUCTION_CANCEL_EXPENSE_BUSINESS_OPERATION_CODE = "EXPENSE_CANCEL"; // код БО для отклонения расхода
        public const string AUCTION_SALE_BUSINESS_OPERATION_CODE = "EXPENSE_PAYMENT"; // код БО для создания продажи(Sale)
        public const string AUCTION_CARTAS_PAYMENT_BOS_CODE = "CARTAS_PAYMENT"; // Код бизнес операции для аванса по аукциону
        public const int AUCTION_KONTR_AGENT_USER_ID = 23; // Идентификатор контр агента
        public const decimal AUCTION_MIN_BYOUT_SUM = 1000; // Минимальая сумма ДКП

        // Коды BusinessOperationSettings для формирования заметок у КО для аукциона
        public const string AUCTION_BOS_SEILING_IN = "SELLING_IN";
        public const string AUCTION_BOS_SELLING_PROFIT_CASH = "SELLING_PROFIT_CASH";
        public const string AUCTION_BOS_EXPENSE_PAYMENT = "EXPENSE_PAYMENT";
        public const string AUCTION_BOS_SELLING_PROFIT = "SELLING_PROFIT";
        public const string AUCTION_BOS_EXPENSE_PAYMENT_CASH = "EXPENSE_PAYMENT_CASH";
        
        // // Коды BusinessOperationSettings для создания контр-агента у КО для аукциона
        public const string AUCTION_BOS_CASH_IN_SELLING = "CASH_IN_SELLING";
        
        // Тексты заметок для КО по аукциону
        public const string AUCTION_SEILING_IN_NOTE = "Фактическая реализация на сумму покупки";
        public const string AUCTION_SELLING_PROFIT_CASH_NOTE = "Прибыль: выручка CarTas от реализации";
        public const string AUCTION_SALE_BUSINESS_OPERATION_NOTE = "Убыток: выручка CarTas от реализации";
        public const string AUCTION_EXPENSE_PAYMENT_NOTE = "Погашение расхода (убыток), приход от клиента на сумму непогашенных расходов / Шығындарды өтеу Клиенттен ақша кіруі";
        public const string AUCTION_SELLING_PROFIT_NOTE = "Проводка для обнуления Бас кенсе с прибылью/Бас кеңсе калдығын қалпына келтіру (Cartas кіріс)";
        public const string AUCTION_EXPENSE_PAYMENT_CASH_NOTE = "Погашение расхода (убыток), приход от клиента на сумму непогашенных расходов / Шығындарды өтеу Клиенттен ақша кіруі";
        public const string AUCTION_CANCEL_WITHDRAW_EXPENSE_NOTE = "ОТМЕНА снятие денежных средств на расходы  CAR TAS(обнуление БК ) / ЖОЮ CAR TAS-қа аударылған шығын(БК қалдығын 0 теңеу)";
        public const string WITHDRAW_FUNDS_TO_CARTAS_NOTE = "Снятие денежных средств на CAR TAS(обнуление БК ) | CAR TAS-қа аударылған шығын(БК қалдығын 0 теңеу)";
        
        #endregion

        #region LegalCollection

        // общий ключ настроек legal collection
        public const string LEGAL_COLLECTION_INTEGRATION_SETTING_CODE = "LEGAL_COLLECTION";

        // название "ключей" для выборки кодов действий при обновлении
        public const string LEGAL_COLLECTION_DEAD_CLIENT_KEY = "DeadClientsAction"; // работа по умершим
        public const string LEGAL_COLLECTION_STOP_ACCRUALS_KEY = "StopAccrualActionsCode"; // остановка начислений
        public const string LEGAL_COLLECTION_RESUME_ACCRUALS_KEY = "ResumeAccrualActionsCode"; // возобновление начислений
        public const string LEGAL_COLLECTION_EXECUTE_INSCRIPTION_KEY = "ExecuteInscriptionActionsCode"; // исполнить исп. надпись
        public const string LEGAL_STAGE_SENT_PUBLIC_EXECUTOR = "SENT_PUBLIC_EXECUTOR"; // Стадия Передан ЧСИ
        public const string LEGAL_STAGE_EXECUTION_WRIT_RECEIVED = "EXECUTION_WRIT_RECEIVED"; // Стадия Исполнительный лист получен

        #endregion

        #region Соощения

        public const string NotEnoughRights = "Недостаточно прав для проведения данной операции";

        #endregion

        /// <summary>
        /// Максимальное значение APR/ГЭСВ
        /// </summary>
        public const int MAX_APR_OLD = 56;
        public static readonly DateTime APR_CHANGED_DATE = new DateTime(2024, 8, 20);
        public const int MAX_APR_V2 = 46;
        public static readonly DateTime NEW_MAX_APR_DATE = new DateTime(2024, 8, 20);
        public const int DAYS_FOR_PAYMENT_NOTIFICATION = 2;//Количество дней до даты оплаты
        public const int DAYS_FOR_DELAY_NOTIFICATION = 9;//Количество дней после просрочки
        public const int DAYS_FOR_SECOND_DELAY_NOTIFICATION_REALTY = 39; // Второе уведомление дней после просрочки
        public const int DAYS_FOR_SECOND_DELAY_NOTIFICATION_CAR = 89; // Второе уведомление дней после просрочки
        public const int DAYS_DELAY91 = 91;
        public static readonly DateTime CORE_MIGRATION_DATE = new DateTime(2021, 5, 17);
        public static readonly TimeSpan STOP_ONLINE_PAYMENTS = new TimeSpan(23, 31, 00);
        public static readonly TimeSpan START_ONLINE_PAYMENTS = new TimeSpan(10, 00, 00);

        public static string INSURANCE_TYPE_CODE = "accident";

        public const int DOCUMENT_PERIOD = 45;
        public const int INVALID_PASSWORD_ATTEMPTS = 3;
        public static readonly DateTime PENY_LIMIT_DATE = new DateTime(2021, 10, 1);
        public const decimal NBRK_PENALTY_RATE = 0.03M;
        public const int NBRK_PENALTY_DECREASE_PERIOD_FROM = 90;
        public static readonly DateTime INTEREST_ACCRUAL_ON_OVERDUE_DEBT_DATE = new DateTime(2021, 10, 17);

        public const int TECH_PASSPORT_DATE_TERM = 30;
        public static readonly DateTime TECH_PASSPORT_MIN_DATE = new DateTime(1991, 1, 1);
        public const int MIN_RELEASE_YEAR = 1950;

        public static int SECONDS_TO_CANCEL_POLICY_IN_SK = 180;

        public static int APPLICATION_OVER_ISSUE_AMOUNT = 25000;
        public static double MONTHS_IN_YEAR = 12.0;
        public const int MAX_NUMBER_OF_POSITIONS_WITH_CAR_COLLATERAL_TYPE_CONTRACT = 1;

        public const string KZ_LANGUAGE_CODE = "KZ";
        public const string RU_LANGUAGE_CODE = "RU";

        /// <summary>
        /// Даты начала и конца отработки сервиса по начислению на внебалансные начисления договоров с исп. надписью
        /// </summary>
        public static readonly DateTime INSCRIPTION_SERVICE_OFFBALANCE_ACCOUNT_ADDITION_START_DATE = new DateTime(2021, 10, 1);

        #region Коды адресов

        public static decimal LIVING_WAGE_4_UNDERAGE = 0.5m;

        #endregion

        #region Коды организации

        public const int OrganizationId = 1;

        #endregion

        #region Значение дохода по умолчанию для сервиса ПКБ

        public const decimal IncomeDefaultValue = 100;
        public const int FCB_SERVICE_REPORT_NOT_FOUND = 1108;
        public const int FCB_SERVICE_SUBJECT_NOT_FOUND = 1106;
        public const int FCB_SERVICE_SUBJECT_NOT_FOUND_2 = 1102;

        #endregion

        #region Типы срочности займа

        public const string PERIOD_TYPE_TERMS_ALL = "TERMS_ALL";
        public const string PERIOD_TYPE_TERMS_SHORT = "TERMS_SHORT";
        public const string PERIOD_TYPE_TERMS_LONG = "TERMS_LONG";

        #endregion
        public const int POSITION_ESTIMATE_EXPIRE_MONTH = 6;

        public const int CREDIT_LINE_MIN_PERIOD_ONLINE = 3;

        public const string BLACKLIST_REASON_CODE = "DIED";
        public const string BLACKLIST_REASON_CODE_SOLDIER = "SOLDIER";

        public const int CLIENT_MINIMAL_AGE = 21;

        public const int MAX_ADDITIONAL_SUM = 360000;

        #region Минимальный возраст заёмщика

        public const int MINIMAL_CLIENT_AGE = 18;

        #endregion

        #region Коды текстов локализации
        public const string LOCALIZATION_TRUSTABLE_ERROR_MESSAGE = "TRUSTABLE_ERROR_MESSAGE";
        #endregion

        #region Константы по страхованию через BPM процесс 
        public const string APPLICATION_ONLINE_FILE_BUSINESS_TYPE_LOAN_CONTRACT_SIGNED_EDS = "LOAN_CONTRACT_SIGNED_EDS";
        public const string APPLICATION_ONLINE_FILE_BUSINESS_TYPE_LOAN_CONTRACT = "LOAN_CONTRACT";
        public const string APPLICATION_ONLINE_FILE_BUSINESS_TYPE_OTHER = "OTHER";
        public const string CREATE_INSURANCE_BPM_MICROSERVICE = "CREATE_INSURANCE_BPM_MICROSERVICE";
        public const string ANNULMENT_INSURANCE_BPM_MICROSERVICE = "ANNULMENT_INSURANCE_BPM_MICROSERVICE";
        public const int FREEDOM_LIFE_INSURANCE_CLIENT_ID = 80108;
        public const int NOMAD_LIFE_INSURANCE_CLIENT_ID = 230272;
        public const string KZTCountryName = "КАЗАХСТАН";
        public const string MaleName = "Мужской";
        public const string FemaleName = "Женский";
        public const string OneYearInsurance = "1";
        #endregion

        public const int PAYMENT_RANGE_MIN_DAYS = 15;
        public const int PAYMENT_RANGE_MAX_DAYS = 45;

        public const string APPLICATION_ONLINE_REJECT_REASON_AUTOREJECTION = "AutoRejection";
        public const string APPLICATION_ONLINE_REJECT_REASON_NEGATIVECREDIT_HISTORY_FCB = "NegativecreditHistoryFCB";

        public const string FUNCTION_SETTING__DEPO_MASTERING = "DEPO_MASTERING";
        public const string FUNCTION_SETTING__ONLINE_FIRST_TRANCHE_MIN_AMOUNT = "ONLINE_FIRST_TRANCHE_MIN_AMOUNT";

        public const int INCOME_COUNT_MONTHS = 6;

        public static readonly DateTime TAKEAWAY_DATE_MOVE_HOLIDAY = new DateTime(2024, 1, 9);

        public const string AUTO_DELETE_MESSAGE = "Автоудаление дохода по причине истечения срока!";
        public const string INCOME_LINKED_CONTRACT_SIGNED = "Контракт привязанный к доходу был подписан!";

        #region Реструктуризация
        public const string DEFERMENT_TYPE_MILITARY_PERSONNEL = "MILITARY_PERSONNEL";
        public const string DEFERMENT_TYPE_EMERGENCY_MODE = "EMERGENCY_MODE";
        public const string DEFERMENT_TYPES_TYPE_DOMAIN = "DEFERMENT_TYPES";
        public const string BO_RESTRUCTURING_CRED = "RESTRUCTURING.CRED";
        public const string BO_RESTRUCTURING_TRANCHES = "RESTRUCTURING.TRANCHES";
        public const string BO_RESTRUCTURING_TRANSFER_TO_TRANSIT_CRED = "RESTRUCTURING.TRANSIT.CRED";
        public const string BO_RESTRUCTURING_TRANSFER_TO_TRANSIT_TRANCHES = "RESTRUCTURING.TRANSIT.TRANCHES";
        public const string BO_RESTRUCTURING_TRANSFER_TO_ACCOUNT_CRED = "RESTRUCTURING.ACCOUNT.CRED";
        public const string BO_RESTRUCTURING_TRANSFER_TO_ACCOUNT_TRANCHES = "RESTRUCTURING.ACCOUNT.TRANCHES";
        public const int SHORT_TERM_PERIOD_TYPE_ID = 2;
        public const int LONG_TERM_PERIOD_TYPE_ID = 3;
        public const string DEFERMENT_PROFIT_ACCOUNT = "DEFERMENT_PROFIT";
        public const string AMORTIZED_PROFIT_ACCOUNT = "AMORTIZED_PROFIT";
        public const string AMORTIZED_PENY_ACCOUNT_ACCOUNT = "AMORTIZED_PENY_ACCOUNT";
        public const string AMORTIZED_PENY_PROFIT_ACCOUNT = "AMORTIZED_PENY_PROFIT";
        public const string BO_BUYOUT_RESTRUCTURING_TRANCHES = "BUYOUT_RESTRUCTURING_TRANCHES";
        public const string BO_BUYOUT_RESTRUCTURING_CRED = "BUYOUT_RESTRUCTURING_CRED";
        public const string BO_SETTING_RESTRUCTURING_AMORTIZED_PROFIT = "RESTRUCTURING.AMORTIZED_PROFIT";
        public const string BO_SETTING_RESTRUCTURING_AMORTIZED_OVERDUE_PROFIT = "RESTRUCTURING.AMORTIZED_OVERDUE_PROFIT";
        public const string BO_SETTING_RESTRUCTURING_AMORTIZED_PENY_ACCOUNT = "RESTRUCTURING.AMORTIZED_PENY_ACCOUNT";
        public const string BO_SETTING_RESTRUCTURING_AMORTIZED_PENY_PROFIT = "RESTRUCTURING.AMORTIZED_PENY_PROFIT";
        public const string BO_SETTING_RESTRUCTURING_AMORTIZED_ACCOUNT = "RESTRUCTURING.AMORTIZED_ACCOUNT";

        public const string RECRUIT_ENDPOINT_KEY__FCB_GET_RECRUIT_LIST = "fcb_get_recruit_list";
        public const string RECRUIT_ENDPOINT_KEY__FCB_GET_RECRUIT_DELTA = "fcb_get_recruit_delta";
        public const string RECRUIT_ENDPOINT_KEY__MKB_GET_RECRUIT_LIST = "mkb_get_recruit_list";
        public const string RECRUIT_ENDPOINT_KEY__FCB_GET_RECRUIT_BY_IIN = "fcb_get_recruit_by_iin";
        public const string RECRUIT_ENDPOINT_KEY__MKB_GET_RECRUIT_BY_IIN = "mkb_get_recruit_by_iin";

        public const string RECRUIT_API__FCB_GET_RECRUIT_LIST = "/tgw-request/api/v1/fcb/get/recruit/list";
        public const string RECRUIT_API__FCB_GET_RECRUIT_DELTA = "/tgw-request/api/v1/fcb/get/recruit/delta?index=";
        public const string RECRUIT_API__MKB_GET_RECRUIT_LIST = "/tgw-request/api/v1/mkb/get/recruit/list";
        public const string RECRUIT_API__FCB_GET_RECRUIT_BY_IIN = "/tgw-request/api/v1/fcb/get/recruit?iin=";
        public const string RECRUIT_API__MKB_GET_RECRUIT_BY_IIN = "/tgw-request/api/v1/mkb/get/recruit?iin=";
        #endregion

        #region Коды смс уведомлений
        public const string BIRTHDAY = "BIRTHDAY";
        public const string PAYMENT_NOTIFICATION = "PAYMENT_NOTIFICATION";
        public const string APPLICATION_ONLINE_SIGN = "APPLICATION_ONLINE_SIGN";
        public const string MANAGER_CODE_APPROVE = "MANAGER_CODE_APPROVE";
        public const string DELAY_EARLY = "DELAY_EARLY";
        public const string DELAY_EARLY_RUS = "DELAY_EARLY_RUS";
        public const string DELAY_LATE = "DELAY_LATE";
        public const string INSURANCE_FAIL = "INSURANCE_FAIL";
        public const string MONTHLY_PAYMENT = "MONTHLY_PAYMENT";
        public const string LAST_PAYMENT_NOTIFICATION = "LAST_PAYMENT_NOTIFICATION";
        #endregion

        public const int COBORROWER_ACCOUNT_BALANCE_LIMIT_MRP = 20000;
        public const string DEFAULT_ATTRACTION_CHANNEL_CODE = "YOUTUBE";
        public const string BUSINESS_PURPOSES_REFERENCE_DATE = "2024-12-09T00:00:00.000";

        #region Коды файлов
        public const string APPLICATION_FILE_CLIENT_DOC_CODE = "ClientDocument";
        public const string ESTIMATE_FILE_CLIENT_CODE = "client";
        public const string ESTIMATE_FILE_PLEDGE_CODE = "pledge";
        #endregion
    }
}