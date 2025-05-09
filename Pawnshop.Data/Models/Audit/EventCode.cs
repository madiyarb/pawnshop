using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Pawnshop.Data.Models.Audit
{
    /// <summary>
    /// Виды событий
    /// При добавлении нового значения, обязательно нужно зарегистрировать его
    /// в Pawnshop.Data.Models.Audit.EventLogItem
    /// </summary>
    /// <remarks>Добавлять только в конец каждой группы</remarks>
    public enum EventCode
    {
        // common
        [Display(Name = "Авторизация пользователя")]
        UserAuthentication = 1,
        [Display(Name = "Изменение пароля пользователя")]
        UserPasswordSaved,
        [Display(Name = "Изменение профиля пользователя")]
        UserProfileSaved,
        [Display(Name = "Изменение конфигурации организации")]
        OrganizationConfigSaved,
        [Display(Name = "Изменение конфигурации филиала")]
        BranchConfigSaved,
        [Display(Name = "Загрузка файла завершена")]
        FileUploaded,
        // dicts
        [Display(Name = "Запись в справочнике \"Счета\" сохранена")]
        DictAccountSaved = 100,
        [Display(Name = "Запись в справочнике \"Счета\" удалена")]
        DictAccountDeleted,
        [Display(Name = "Запись в справочнике \"Счета\" восстановлена")]
        DictAccountRecovery,
        [Display(Name = "Запись в справочнике \"Автомобили\" сохранена")]
        DictCarSaved,
        [Display(Name = "Запись в справочнике \"Автомобили\" удалена")]
        DictCarDeleted,
        [Display(Name = "Запись в справочнике \"Автомобили\" восстановлена")]
        DictCarRecovery,
        [Display(Name = "Запись в справочнике \"Недвижимости\" сохранена")]
        DictRealtySaved,
        [Display(Name = "Запись в справочнике \"Недвижимости\" удалена")]
        DictRealtyDeleted,
        [Display(Name = "Запись в справочнике \"Категории\" сохранена")]
        DictCategorySaved,
        [Display(Name = "Запись в справочнике \"Категории\" удалена")]
        DictCategoryDeleted,
        [Display(Name = "Запись в справочнике \"Категории\" восстановлена")]
        DictCategoryRecovery,
        [Display(Name = "Запись в справочнике \"Золото\" сохранена")]
        DictGoldSaved,
        [Display(Name = "Запись в справочнике \"Золото\" удалена")]
        DictGoldDeleted,
        [Display(Name = "Запись в справочнике \"Золото\" восстановлена")]
        DictGoldRecovery,
        [Display(Name = "Запись в справочнике \"Товары\" сохранена")]
        DictGoodSaved,
        [Display(Name = "Запись в справочнике \"Товары\" удалена")]
        DictGoodDeleted,
        [Display(Name = "Запись в справочнике \"Товары\" восстановлена")]
        DictGoodRecovery,
        [Display(Name = "Запись в справочнике \"Пробы\" сохранена")]
        DictPuritySaved,
        [Display(Name = "Запись в справочнике \"Пробы\" удалена")]
        DictPurityDeleted,
        [Display(Name = "Запись в справочнике \"Пробы\" восстановлена")]
        DictPurityRecovery,
        [Display(Name = "Запись в справочнике \"Проценты\" сохранена")]
        DictLoanPercentSaved,
        [Display(Name = "Запись в справочнике \"Проценты\" удалена")]
        DictLoanPercentDeleted,
        [Display(Name = "Запись в справочнике \"Проценты\" восстановлена")]
        DictLoanPercentRecovery,
        [Display(Name = "Запись в справочнике \"Спецтехника\" сохранена")]
        DictMachinerySaved,
        [Display(Name = "Запись в справочнике \"Спецтехника\" удалена")]
        DictMachineryDeleted,
        [Display(Name = "Запись в справочнике \"Причины добавления пользователя в черный список\" сохранена")]
        DictBlackListReasonSaved,
        [Display(Name = "Запись в справочнике \"Причины добавления пользователя в черный список\" удалена")]
        DictBlackListReasonDeleted,
        [Display(Name = "Запись в справочнике \"Аннуитеты\" сохранена")]
        AnnuitySettingSaved,
        [Display(Name = "Запись в справочнике \"Аннуитеты\" удалена")]
        AnnuitySettingDeleted,
        [Display(Name = "Запись в справочнике \"Пробы\" сохранена")]
        DictAttractionChannelSaved,
        [Display(Name = "Запись в справочнике \"Пробы\" удалена")]
        DictAttractionChannelDeleted,
        [Display(Name = "Запись в справочнике \"Пробы\" восстановлена")]
        DictAttractionChannelRecovery,
        [Display(Name = "Запись в справочнике \"Действия постановки на стоянку\" сохранена")]
        DictParkingActionSaved,
        [Display(Name = "Запись в справочнике \"Действия постановки на стоянку\" удалена")]
        DictParkingActionDeleted,
        [Display(Name = "Запись в справочнике \"Действия постановки на стоянку\" восстановлена")]
        DictParkingActionRecovery,
        [Display(Name = "Запись в справочнике \"Статусы постановки на стоянку\" сохранена")]
        DictParkingStatusSaved,
        [Display(Name = "Запись в справочнике \"Статусы постановки на стоянку\" удалена")]
        DictParkingStatusDeleted,
        [Display(Name = "Запись в справочнике \"Статусы постановки на стоянку\" восстановлена")]
        DictParkingStatusRecovery,
        [Display(Name = "Запись в справочнике \"Социально уязвимые слои населения\" сохранена")]
        DictSociallyVulnerableGroupSaved,
        [Display(Name = "Запись в справочнике \"Социально уязвимые слои населения\" удалена")]
        DictSociallyVulnerableGroupDeleted,
        [Display(Name = "Запись в справочнике \"Социально уязвимые слои населения\" восстановлена")]
        DictSociallyVulnerableGroupRecovery,
        [Display(Name = "Запись в справочнике \"Настройки начисления процентов\" сохранена")]
        DictBlackoutSaved,
        [Display(Name = "Запись в справочнике \"Виды отсрочек\" сохранена")]
        DictPostponementSaved,
        [Display(Name = "Запись в справочнике \"Виды отсрочек\" сохранена")]
        DictPostponementDeleted,
        [Display(Name = "Запись в справочнике \"Виды реквизитов\" сохранена")]
        DictRequisiteTypeSaved,
        [Display(Name = "Запись в справочнике \"Виды реквизитов\" сохранена")]
        DictRequisiteTypeDeleted,
        [Display(Name = "Запись в справочнике \"Виды оплаты\" сохранена")]
        DictPayTypeSaved,
        [Display(Name = "Запись в справочнике \"Виды оплаты\" сохранена")]
        DictPayTypeDeleted,
        [Display(Name = "Запись в справочнике \"Подтверждения по действиям договора\" сохранена")]
        DictContractActionCheckSaved,
        [Display(Name = "Запись в справочнике \"Подтверждения по действиям договора\" удалена")]
        DictContractActionCheckDeleted,
        [Display(Name = "Запись в справочнике \"Подтверждения по договорам\" сохранена")]
        DictContractCheckSaved,
        [Display(Name = "Запись в справочнике \"Подтверждения по договорам\" удалена")]
        DictContractCheckDeleted,
        [Display(Name = "Запись в справочнике \"Правовые формы клиента\" сохранена")]
        DictClientLegalFormSaved,
        [Display(Name = "Запись в справочнике \"Правовые формы клиента\" удалена")]
        DictClientLegalFormDeleted,
        [Display(Name = "Запись в справочнике \"Правовые формы клиента\" восстановлена")]
        DictClientLegalFormRecovery,
        [Display(Name = "Запись в справочнике \"Виды документов клиента\" сохранена")]
        DictClientDocumentTypeSaved,
        [Display(Name = "Запись в справочнике \"Виды документов клиента\" удалена")]
        DictClientDocumentTypeDeleted,
        [Display(Name = "Запись в справочнике \"Виды документов клиента\" восстановлена")]
        DictClientDocumentTypeRecovery,
        [Display(Name = "Запись в справочнике \"Органы выдачи документов клиента\" сохранена")]
        DictClientDocumentProviderSaved,
        [Display(Name = "Запись в справочнике \"Органы выдачи документов клиента\" удалена")]
        DictClientDocumentProviderDeleted,
        [Display(Name = "Запись в справочнике \"Органы выдачи документов клиента\" восстановлена")]
        DictClientDocumentProviderRecovery,
        [Display(Name = "Запись в справочнике \"Расходы клиента по прочим платежам (расчет ручной)\" сохранена")]
        DictManualCalculationClientExpenseSaved,
        [Display(Name = "Запись в справочнике \"Расходы клиента по прочим платежам (расчет ручной)\" удалена")]
        DictManualCalculationClientExpenseDeleted,
        [Display(Name = "Запись в справочнике \"Расходы клиента по прочим платежам (расчет ручной)\" восстановлена")]
        DictManualCalculationClientExpenseRecovery,
        [Display(Name = "Запись в справочнике \"Страны\" сохранена")]
        DictCountrySaved,
        [Display(Name = "Запись в справочнике \"Страны\" удалена")]
        DictCountryDeleted,
        [Display(Name = "Запись в справочнике \"Страны\" восстановлена")]
        DictCountryRecovery,
        [Display(Name = "Запись в справочнике \"Выходные\" сохранена")]
        DictHolidaySaved,
        [Display(Name = "Запись в справочнике \"Выходные\" удалена")]
        DictHolidayDeleted,
        [Display(Name = "Запись в справочнике \"Субъекты\" сохранена")]
        DictLoanSubjectSaved,
        [Display(Name = "Запись в справочнике \"Субъекты\" удалена")]
        DictLoanSubjectDeleted,
        [Display(Name = "Запись в справочнике \"Виды продуктов\" сохранена")]
        DictLoanProductTypeSaved,
        [Display(Name = "Запись в справочнике \"Виды продуктов\" удалена")]
        DictLoanProductTypeDeleted,
        [Display(Name = "Запись в справочнике \"Шаблоны печатных форм\" сохранена")]
        DictPrintTemplateSaved,
        [Display(Name = "Запись в справочнике \"Шаблоны печатных форм\" удалена")]
        DictPrintTemplateDeleted,
        [Display(Name = "Запись в справочнике \"Марки автотранспорта\" сохранена")]
        DictVehicleMarksSaved,
        [Display(Name = "Запись в справочнике \"Марки автотранспорта\" удалена")]
        DictVehicleMarksDeleted,
        [Display(Name = "Запись в справочнике \"Марки автотранспорта\" восстановлена")]
        DictVehicleMarksRecovery,
        [Display(Name = "Запись в справочнике \"Модели автотранспорта\" сохранена")]
        DictVehicleModelsSaved,
        [Display(Name = "Запись в справочнике \"Модели автотранспорта\" удалена")]
        DictVehicleModelsDeleted,
        [Display(Name = "Запись в справочнике \"Модели автотранспорта\" восстановлена")]
        DictVehicleModelsRecovery,
        [Display(Name = "Запись в справочнике \"Ликвидность автотранспорта\" сохранена")]
        DictVehicleLiquiditySaved,
        [Display(Name = "Запись в справочнике \"Ликвидность автотранспорта\" удалена")]
        DictVehicleLiquidityDeleted,
        [Display(Name = "Запись в справочнике \"Ликвидность автотранспорта\" восстановлена")]
        DictVehicleLiquidityRecovery,
        [Display(Name = "Запись в справочнике \"Ликвидность автотранспорта: Максимальные сроки кредита\" сохранена")]
        DictContractPeriodVehicleLiquiditySaved,
        [Display(Name = "Запись в справочнике \"Ликвидность автотранспорта: Максимальные сроки кредита\" удалена")]
        DictContractPeriodVehicleLiquidityDeleted,
        [Display(Name = "Запись в справочнике \"Ликвидность автотранспорта: Максимальные сроки кредита\" восстановлена")]
        DictContractPeriodVehicleLiquidityRecovery,
        [Display(Name = "Запись в справочнике \"Индексы изготовителей автотранспорта\" сохранена")]
        DictVehicleWMIsSaved,
        [Display(Name = "Запись в справочнике \"Индексы изготовителей автотранспорта\" удалена")]
        DictVehicleWMIsDeleted,
        [Display(Name = "Запись в справочнике \"Индексы изготовителей автотранспорта\" восстановлена")]
        DictVehicleWMIsRecovery,
        [Display(Name = "Запись в справочнике \"Изготовители автотранспорта\" сохранена")]
        DictVehicleManufacturersSaved,
        [Display(Name = "Запись в справочнике \"Изготовители автотранспорта\" удалена")]
        DictVehicleManufacturersDeleted,
        [Display(Name = "Запись в справочнике \"Изготовители автотранспорта\" восстановлена")]
        DictVehicleManufacturersRecovery,
        [Display(Name = "Запись в справочнике \"Коды стран-изготовителей автотранспорта\" сохранена")]
        DictVehicleCountryCodesSaved,
        [Display(Name = "Запись в справочнике \"Коды стран-изготовителей автотранспорта\" удалена")]
        DictVehicleCountryCodesDeleted,
        [Display(Name = "Запись в справочнике \"Коды стран-изготовителей автотранспорта\" восстановлена")]
        DictVehicleCountryCodesRecovery,
        [Display(Name = "Запись в справочнике \"Черный список VIN-кодов автотранспортов\" сохранена")]
        DictVehiclesBlackListItemSaved,
        [Display(Name = "Запись в справочнике \"Черный список VIN-кодов автотранспортов\" удалена")]
        DictVehiclesBlackListItemDeleted,
        [Display(Name = "Запись в справочнике \"Черный список VIN-кодов автотранспортов\" восстановлена")]
        DictVehiclesBlackListItemRecovery,
        [Display(Name = "Запись в справочнике \"Черный список клиентов\" сохранен")]
        DictClientsBlackListSaved = 188,
        [Display(Name = "Запись в справочнике \"Черный список клиентов\" удалена")]
        DictClientsBlackListDeleted = 189,
        [Display(Name = "Запись в справочнике \"Черный список клиентов\" восстановлен")]
        DictClientsBlackListRecovery = 190,
        // membership
        [Display(Name = "Запись в таблице \"Группы\" сохранена")]
        GroupSaved = 200,
        [Display(Name = "Запись в таблице \"Группы\" удалена")]
        GroupDeleted,
        [Display(Name = "Запись в таблице \"Группы\" восстановлена")]
        GroupRecovery,
        [Display(Name = "Запись в таблице \"Пользователи\" сохранена")]
        UserSaved,
        [Display(Name = "Запись в таблице \"Пользователи\" удалена")]
        UserDeleted,
        [Display(Name = "Запись в таблице \"Пользователи\" восстановлена")]
        UserRecovery,
        [Display(Name = "Запись в таблице \"Роли\" сохранена")]
        RoleSaved,
        [Display(Name = "Запись в таблице \"Роли\" удалена")]
        RoleDeleted,
        [Display(Name = "Запись в таблице \"Роли\" восстановлена")]
        RoleRecovery,
        //administration
        [Display(Name = "Запись в таблице \"Настройки персональных скидок\" сохранена")]
        PersonalDiscountSaved = 220,
        // business
        [Display(Name = "Запись в таблице \"История постановки автотранспорта на стоянку\" сохранена")]
        ParkingHistorySaved = 280,
        [Display(Name = "Проверка задолженности по договору")]
        ContractDebtCheck,
        [Display(Name = "Корректировка скидок по договору")]
        ContractDiscountSaved,
        [Display(Name = "Удаление скидки по договору")]
        ContractDiscountDelete,
        [Display(Name = "Договор рефинансирован")]
        ContractRefinance,
        [Display(Name = "Распечатка ордеров для действия по договору")]
        ContractActionPrint,
        [Display(Name = "Платежная операция сохранена")]
        PayOperationSaved,
        [Display(Name = "Платежная операция удалена")]
        PayOperationDeleted,
        [Display(Name = "Открытие доступа к внесению первоначального взноса")]
        InitialFeeOpen,
        [Display(Name = "Перерегистрация или овормление документов по залогу")]
        PositionRegistration,
        [Display(Name = "Распечатка документа по договору")]
        ContractDocumentPrint,
        [Display(Name = "Построение графика по договору")]
        ContractBuildSchedule,
        [Display(Name = "Сохранение графика по договору")]
        ContractSaveSchedule,
        [Display(Name = "Подтверждение действия по договору")]
        ContractActionApprove,
        [Display(Name = "Возврат предоплаты по договору")]
        PrepaymentReturn,
        // clients
        [Display(Name = "Запись в таблице \"Клиенты\" сохранена")]
        ClientSaved = 300,
        [Display(Name = "Запись в таблице \"Клиенты\" удалена")]
        ClientDeleted,
        [Display(Name = "Запись в таблице \"Клиенты\" восстановлена")]
        ClientRecovery,
        // contracts
        [Display(Name = "Договор сохранен")]
        ContractSaved = 320,
        [Display(Name = "Договор удален")]
        ContractDeleted,
        [Display(Name = "Договор восстановлен")]
        ContractRecovery,
        [Display(Name = "Действие отменено")]
        ContractActionCancel,
        [Display(Name = "Действие восстановлено")]
        ContractActionRecovery,
        [Display(Name = "Договор отправлен на реализацию")]
        ContractSelling,
        [Display(Name = "Договор подписан")]
        ContractSign,
        [Display(Name = "Договор продлен")]
        ContractProlong,
        [Display(Name = "Изменена контрольная дата погашения")]
        ContractControlDateChanged,
        [Display(Name = "Отменено изменение контрольной даты погашения")]
        CancelContractControlDateChanged,
        [Display(Name = "Договор выкуплен")]
        ContractBuyout,
        [Display(Name = "Договор частично выкуплен")]
        ContractPartialBuyout,
        [Display(Name = "Договор частично погашен")]
        ContractPartialPayment,
        [Display(Name = "Примечание к договору сохранено")]
        ContractNoteSaved,
        [Display(Name = "Примечание к договору удалено")]
        ContractNoteDeleted,
        [Display(Name = "Договор передан")]
        ContractTransferred,
        [Display(Name = "Ежемесячное погашение договора")]
        ContractMonthlyPayment,
        [Display(Name = "Добор")]
        ContractAddition,
        [Display(Name = "Авансовый платеж")]
        Prepayment,
        [Display(Name = "Изменение баланса авансового счёта")]
        PrepaymentCostChange,
        [Display(Name = "Ошибка в автоматическом освоение денег")]
        OnlinePaymentError,
        [Display(Name = "Автоматическое освоение денег")]
        OnlinePayment,
        // cashorders
        [Display(Name = "Кассовый ордер сохранен")]
        CashOrderSaved = 340,
        [Display(Name = "Кассовый ордер удален")]
        CashOrderDeleted,
        [Display(Name = "Кассовый ордер восстановлен")]
        CashOrderRecovery,
        [Display(Name = "Кассовый ордер одобрен")]
        CashOrderApproved,
        [Display(Name = "Кассовый ордер отклонен")]
        CashOrderProhibited,
        [Display(Name = "Кассовый ордер отменен(обратная проводка)")]
        CashOrderСanceled,
        [Display(Name = "Кассовый ордер согласован")]
        CashOrderConfirmed,
        [Display(Name = "Кассовый ордер изменен")]
        CashOrderUpdated,
        // sellings
        [Display(Name = "Позиция на реализации сохранена")]
        SellingSaved = 360,
        [Display(Name = "Позиция на реализации удалена")]
        SellingDeleted,
        [Display(Name = "Позиция на реализации восстановлена")]
        SellingRecovery,
        [Display(Name = "Позиция на реализации продана")]
        SellingSell,
        [Display(Name = "Продажа позиции на реализации отменена")]
        SellingSellCancel,
        // contracts additional
        [Display(Name = "Добавлена даты выдачи к договору")]
        ContractAddSignDate = 380,
        [Display(Name = "Договор согласован")]
        ContractConfirmed,
        //notifications
        [Display(Name = "Уведомление сохранено")]
        NotificationSaved = 400,
        [Display(Name = "Уведомление удалено")]
        NotificationDeleted,
        [Display(Name = "Уведомление установлено для отправки")]
        NotificationSetForSend,
        [Display(Name = "Формирование уведомлений об оплате отключено")]
        PaymentNotificationOff,
        [Display(Name = "Начало процесса отправки СМС")]
        StartSMSSending,
        [Display(Name = "Успешная отправка сообщения")]
        SMSSentSuccess,
        [Display(Name = "Не успешная отправка сообщения")]
        SMSSentFailed,
        [Display(Name = "Начало получения отчета по СМС")]
        StartSMSReport,
        [Display(Name = "Успешное получение отчета по СМС")]
        SMSReportSuccess,
        [Display(Name = "Не успешное получение отчета по СМС")]
        SMSReportFail,
        [Display(Name = "Персональное уведомление сохранено")]
        InnerNotificationSaved = 420,
        [Display(Name = "Персональное уведомление удалено")]
        InnerNotificationDeleted,
        [Display(Name = "Персональное уведомление прочитано")]
        InnerNotificationRead,
        [Display(Name = "Персональное уведомление выполнено")]
        InnerNotificationDone,
        // bitrix
        [Display(Name = "Ошибка подключения к Битрикс24")]
        BitrixConnectionFailed = 430,
        [Display(Name = "Выгрузка договора в Битрикс24")]
        BitrixUpload,
        [Display(Name = "Ошибка ввода данных из Битрикс24")]
        BitrixDataTypeError,
        [Display(Name = "Данные в Битрикс введены")]
        BitrixDataSuccess,
        //1с
        [Display(Name = "Выгрузка в 1С")]
        AccountantUpload = 440,
        [Display(Name = "Проверка статуса в 1С")]
        AccountantStatusCheck,
        // online payments
        [Display(Name = "Запрос стороннего сервиса об онлайн оплате")]
        OnlinePaymentTry = 450,
        // remittances
        [Display(Name = "Перевод сохранен")]
        RemittanceSaved = 480,
        [Display(Name = "Перевод удален")]
        RemittanceDeleted,
        [Display(Name = "Перевод получен")]
        RemittanceReceived,
        [Display(Name = "Полученный перевод отменен")]
        RemittanceReceiveCanceled,
        // reports
        [Display(Name = "Запрос на скачивание отчёта")]
        ReportDownload = 500,
        [Display(Name = "Запрос на скачивание дополнительного отчёта")]
        ReportWordDownload = 501,
        //client expenses
        [Display(Name = "Расход")]
        ContractExpenseSave = 530,
        [Display(Name = "Отмена расхода")]
        ContractExpenseCancel,
        //client postponements
        [Display(Name = "Сохранение отсрочки")]
        ContractPostponementSave = 535,
        [Display(Name = "Удаление отсрочки")]
        ContractPostponementDelete,
        //contract inscriptions
        [Display(Name = "Сохранение/создание исполнительной надписи")]
        InscriptionSaved = 540,
        [Display(Name = "Удаление исполнительной надписи")]
        InscriptionDeleted,
        [Display(Name = "Утверждение исполнительной надписи")]
        InscriptionApproved,
        [Display(Name = "Отзыв исполнительной надписи")]
        InscriptionDenied,
        [Display(Name = "Приведение в действие исполнительной надписи")]
        InscriptionExecuted,
        [Display(Name = "Отмена действия в исполнительной надписи")]
        InscriptionActionCanceled,
        //reportData
        [Display(Name = "Успешная выгрузка")]
        ReportDataSuccess = 600,
        [Display(Name = "Ошибка выгрузки")]
        ReportDataError,
        //nationalBankCurrencyRates
        [Display(Name = "Oбновление курса нацбанка")]
        NationalBankRatesUpdate = 610,
        //Mintos
        [Display(Name = "Загрузка договора в Mintos")]
        MintosContractUpload = 620,
        [Display(Name = "Проверка статуса договора в Mintos")]
        MintosContractStatusCheck,
        [Display(Name = "Обновление договора Mintos")]
        MintosContractUpdate,
        [Display(Name = "Удаление договора Mintos")]
        MintosContractDelete,
        [Display(Name = "Выгрузка оплаты по договору в Mintos")]
        MintosContractPayment,
        [Display(Name = "Продление графика по договору в Mintos")]
        MintosContractExtendSchedule,
        [Display(Name = "Rebuy договора в Mintos")]
        MintosContractRebuy,
        [Display(Name = "Валидация договоров в Mintos")]
        MintosValidation,
        //EGOVDictionaries
        [Display(Name = "Обновление справочника EGOV")]
        EGOVDictionaryUpdate = 640,

        //Website
        [Display(Name = "Получение данных по договору (Старый)")]
        WebsiteContractFind = 700,
        [Display(Name = "Получение данных по договору (Новый)")]
        WebsiteContractCheck,

        //Credit Bureau Integration
        [Display(Name = "Создание пакета для выгрузки в Кредитное бюро")]
        CBBatchCreation = 800,
        [Display(Name = "Заполнение пакета договорами для выгрузки в Кредитное бюро")]
        CBBatchContractsAdded,
        [Display(Name = "Создание и архивирование пакета XML для КБ")]
        CBXMLCreation,
        [Display(Name = "Выгрузка пакета в КБ")]
        CBBatchUpload,
        [Display(Name = "Проверка статуса пакета в КБ")]
        CBStatusCheck,
        [Display(Name = "Подготовка договора к выгрузке в КБ")]
        CBBatchDataFulfill,
        [Display(Name = "Соединение с КБ для выгрузки батчей")]
        CBBatchConnection,
        [Display(Name = "Выгружено в телеграм")]
        CBBatchContractsUploaded,
        [Display(Name = "Ошибка выгрузки в телеграм")]
        CBBatchContractsUploadError,

        [Display(Name = "Запрос верификации")]
        GetVerification = 1000,
        [Display(Name = "Верификация")]
        VerifyRequest = 1001,
        [Display(Name = "Значение домена было создано")]
        DomainValueCreated = 1002,
        [Display(Name = "Значение домена было обновлено")]
        DomainValueUpdated = 1003,
        [Display(Name = "Значение домена было удалено")]
        DomainValueDeleted = 1004,
        [Display(Name = "Контакт клиента был создан")]
        ClientContactCreated = 1005,
        [Display(Name = "Контакт клиента был обновлен")]
        ClientContactUpdated = 1006,
        [Display(Name = "Контакты клиент был удален")]
        ClientContactDeleted = 1007,

        //Учетное ядро(AccountingCore)
        [Display(Name = "Сохранение в справочнике \"Типы и их иерархия\"")]
        DictTypeSaved = 1050,
        [Display(Name = "Удаление записи в справочнике \"Типы и их иерархия\"")]
        DictTypeDeleted,
        [Display(Name = "Сохранение в справочнике \"Бизнес-операции\"")]
        DictBusinessOperationSaved,
        [Display(Name = "Удаление записи в справочнике \"Бизнес-операции\"")]
        DictBusinessOperationDeleted,
        [Display(Name = "Сохранение в справочнике \"Настройки бизнес-операций\"")]
        DictBusinessOperationSettingSaved,
        [Display(Name = "Удаление записи в справочнике \"Настройки бизнес-операций\"")]
        DictBusinessOperationSettingDeleted,
        [Display(Name = "Сохранение в справочнике \"Планы счетов\"")]
        DictAccountPlanSaved,
        [Display(Name = "Удаление записи в справочнике \"Планы счетов\"")]
        DictAccountPlanDeleted,
        [Display(Name = "Сохранение в справочнике \"Настройки плана счетов\"")]
        DictAccountPlanSettingSaved,
        [Display(Name = "Удаление записи в справочнике \"Настройки плана счетов\"")]
        DictAccountPlanSettingDeleted,
        [Display(Name = "Сохранение в справочнике \"Настройки очередности погашения\"")]
        DictPaymentOrderSaved,
        [Display(Name = "Сохранение в справочнике \"Базы начисления\"")]
        DictAccrualBaseSaved,
        [Display(Name = "Сохранение в справочнике \"Статьи расхода\"")]
        DictExpenseArticleTypeSaved,
        [Display(Name = "Удаление записи в справочнике \"Статьи расхода\"")]
        DictExpenseArticleTypeDeleted,

        [Display(Name = "Открытие счетов для договора")]
        ContractOpenAccounts,

        [Display(Name = "Начисление процентов по договору")]
        ContractInterestAccrual = 1100,
        [Display(Name = "Погашение задолженности по договору")]
        Payment = 1101,
        [Display(Name = "Пересчёт баланса по договору")]
        AccountBalanceRecalculate,
        [Display(Name = "Начисление штрафов по договору")]
        ContractPenaltyAccrual,
        [Display(Name = "Скидка при начислении штрафов по договору")]
        ContractPenaltyAccrualDiscount,
        [Display(Name = "Вынос на просрочку")]
        TakeAwayToDelay,
        [Display(Name = "Начисление процентов на просроченный ОД по договору")]
        ContractInterestAccrualOnOverdueDebt,

        [Display(Name = "Запись в справочнике \"Виды экономической деятельности клиента\" сохранена")]
        DictClientEconomicActivityTypeSaved,
        [Display(Name = "Запись в справочнике \"Виды экономической деятельности клиента\" удалена")]
        DictClientEconomicActivityTypeDeleted,
        [Display(Name = "Запись в справочнике \"ОКЭД клиента\" сохранена")]
        DictClientEconomicActivitySaved,
        [Display(Name = "Запись в справочнике \"ОКЭД клиента\" удалена")]
        DictClientEconomicActivityDeleted,
        [Display(Name = "Запись в справочнике \"Доступные документы для подписантов юр лиц\" сохранена")]
        DictClientSignersAllowedDocumentTypeSaved,
        [Display(Name = "Запись в справочнике \"Доступные документы документов для подписантов юр лиц\" удалена")]
        DictClientSignersAllowedDocumentTypeDeleted,

        [Display(Name = "Отправка запроса в ТасОнлайн")]
        TasOnlineRequestSending,
        [Display(Name = "Обработка данных прищедших из ТасОнлайн")]
        TasOnlineRequestDeserialize,
        [Display(Name = "Отправка платежей на почту ТасОнлайн")]
        TasOnlineReportSend,

        [Display(Name = "Перенос аванса с родительного договора")]
        MovePrepaymentSourceContract,
        [Display(Name = "Прием переноса аванса на дочернем договоре")]
        MovePrepaymentRecipientContract,
        [Display(Name = "Запись в справочнике \"Тарифы страховых компаний\" сохранена")]
        DictInsuranceRateSaved,
        [Display(Name = "Запись в справочнике \"Тарифы страховых компаний\" удалена")]
        DictInsuranceRateDeleted,
        [Display(Name = "Сохранение онлайн запроса с сайта ТасКредит")]
        InsuranceOnlineRequestSave,
        [Display(Name = "Сохранение онлайн запроса при доборе")]
        InsuranceOnlineRequestOnAdditionSave,
        [Display(Name = "Подсчет суммы для запроса для страховой компании")]
        InsurancePolicyRequestCalculation,
        [Display(Name = "Запрос в Страховую компанию на регистрацию страхового полиса")]
        RegisterInsurancePolice = 1200,
        [Display(Name = "Запрос в Страховую компанию на аннулирование страхового полиса")]
        CancelInsurancePolice = 1201,
        [Display(Name = "Запрос в Страховую компанию для авторизации и получения SessionID")]
        AuthenticationInsuranceRequest = 1202,
        [Display(Name = "Начисление лимита пени по договору")]
        ContractPenaltyLimitAccrual,
        [Display(Name = "Уменьшение ставки пени по договору")]
        ContractPenaltyRateDecrease,
        [Display(Name = "Увеличение ставки пени по договору")]
        ContractPenaltyRateIncrease,
        [Display(Name = "Успешное оформление страхового полиса")]
        AcceptPolicy,
        [Display(Name = "Страховой полис не создан")]
        KillPolicy,
        [Display(Name = "Успешное аннулирован страхового полиса")]
        AcceptCancelPolicy,
        [Display(Name = "Аннулирование страхового полиса не произошло")]
        KillCancelPolicy,
        [Display(Name = "Высчитывание суммы для страхового премия")]
        InsurancePremiumCalculation,
        // Для MobileAppController
        [Display(Name = "Регистрация заявки от МП")]
        ApplicationSaved = 1300,
        [Display(Name = "Сохранение пользователя в базе мобильного приложения")]
        MobileAppSaveUser,
        [Display(Name = "Обновление заявки от МП по AppId")]
        ApplicaitonUpdatedByApId,
        [Display(Name = "Событие запуска контроллера SaveApprovedContract")]
        ApplicationController,
        [Display(Name = "Событие запуска сервиса CallRequestMobileAppService")]
        CallRequestMobileAppService,
        [Display(Name = "Отправка договора Хард Коллекшн в MobApp")]
        MobileAppHardCollectionSendPortfel,
        [Display(Name = "Проверка клиента на банкротство через шлюз TasLab")]
        CheckClientBankruptcy,
        
        // Для ApplicationMerchant
        [Display(Name = "Сохранение торговца в БД")]
        ApplicationMerchantSaved = 1400,
        [Display(Name = "Сохранение торговца в БД")]
        ApplicationMerchantUpdated,

        //Логирование расчета КДН
        [Display(Name = "Расчет КДН")]
        KdnCaclSucces = 1500,

        [Display(Name = "Добавление ликвидности авто")]
        AddModelLiquidity = 1600,

        [Display(Name = "Изменение ликвидности авто")]
        ChangeModelLiquidity = 1601,
        //Логирование запросов оплаты ТМФ
        [Display(Name = "Отправка запроса в TMF")]
        TmfRequestSending = 1700,
        [Display(Name = "Обработка данных прищедших из TMF")]
        TMFRequestDeserialize = 1701,

        [Display(Name = "Отправка уведомления в CRM о закрытии займа")]
        SendCloseNotifCrm = 1800,

        [Display(Name = "Отправка уведомления в CRM о выходе на просрочку")]
        SendOverdueNotifCrm = 1801,

        [Display(Name = "Повторная попытка отправки страхового полиса в сервис СК")]
        RepeatedSendInsuranceTasOnline = 1810,

        //Логирование запросов в 1C
        [Display(Name = "Отправка запроса в 1С")]
        Online1CReportSending = 1900,

        //Логирование для начисления пени и процентов на внебалансе через сервис
        [Display(Name = "Начисление для внебалансовых договоров для филиала")]
        InscriptionOffBalanceForBranch = 2000,
        [Display(Name = "Начисление для одного договора на внебалансе")]
        InscriptionOffBalanceForContract = 2001,
        [Display(Name = "Частичное погашение с изменением категория аналитики на - с правом вождения")]
        CategoryChangeWithDrive,
        [Display(Name = "Частичное погашение с изменением категория аналитики на - без права вождения")]
        CategoryChangeWithoutDrive,
        [Display(Name = "Перенос КД на следующий день в связи с выходными")]
        MoveControlDateToNextDay,
        
        [Display(Name = "Отклонение/отмена заявки по истечению срока")]
        CancelApplicationOnline = 2100,

        // реструктуризация
        [Display(Name = "Запрос списка военнослужащих через щлюз")]
        GetRecruitList = 2200,
        [Display(Name = "Запрос изменения списка военнослужащих через щлюз")]
        GetRecruitDelta,
        [Display(Name = "Запрос военнослужащих через щлюз по ИИН")]
        GetRecruitByIIN,
        [Display(Name = "Регистрация военнослужащего")]
        RegistrRecruit,
        [Display(Name = "Список действий отменено")]
        ContractActionListCancel = 2300,
    }
}