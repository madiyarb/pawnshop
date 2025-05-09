using System;
using System.Runtime.InteropServices;
using Hangfire;
using Hangfire.Storage;
using Pawnshop.Web.Engine.Jobs.AccountingJobs;
using Pawnshop.Web.Engine.Jobs.MigrationJobs;
using TimeZoneConverter;

namespace Pawnshop.Web.Engine.Jobs
{
    public static class JobRegistry
    {
        /*
            * * * * * выполняемая команда
            - - - - -
            | | | | |
            | | | | ----- день недели (0—7) (воскресенье = 0 или 7)
            | | | ------- месяц (1—12)
            | | --------- день (1—31)
            | ----------- час (0—23)
            ------------- минута (0—59)
         */

        private const string strTimeZone = "Asia/Almaty";

        private static string GetTimeZone()
        {

            string timeZone = "Central Asia Standard Time";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Central Asia Standard Time";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Asia/Almaty";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "Central Asia Standard Time";
            }

            return timeZone;
        }

        public static void Register()
        {

            //Queues:
            //"default"
            //Загрузка курсов нацбанка
            RecurringJob.AddOrUpdate<NationalBankCurrencyUpdateJob>("NationalBankCurrencyUpdate", x => x.Execute(), Cron.Daily(7), TZConvert.GetTimeZoneInfo(GetTimeZone()));
            //Формирование данных для аналитики
            //RecurringJob.AddOrUpdate<ReportDataJob>("ReportData", x => x.Execute(), Cron.Daily(7), TZConvert.GetTimeZoneInfo(strTimeZone));
            //Загрузка и актуализация справочников из EGOV
            RecurringJob.AddOrUpdate<EGOVDictionaryActualizationJob>("EGOVDictionaryActualization", x => x.Execute(), Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()));

            //"onlinepayments"
            //Проверка статусов Job'ов с уведомлением администрации на почту
            //RecurringJob.AddOrUpdate<OnlinePaymentCheckStatusJob>("MorningOnlinePaymentCheckStatus", x => x.Execute(), "45 8,20 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "onlinepayments");
            //Освоение денег, поступивших с онлайн оплат
            RecurringJob.AddOrUpdate<OnlinePaymentJob>("OnlinePayment", x => x.Execute(), "*/1 10-23 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "onlinepayments");

            //CardTopUpJob отключаем, так как для нового МПк он не нужен
            //RecurringJob.AddOrUpdate<CardTopUpJob>("CardTopUpJob", x => x.Execute(), "*/5 * * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "onlinepayments");
            //"payments"
            RecurringJob.AddOrUpdate<UsePrepaymentForMonthlyPaymentJob>("UsePrepaymentForMonthlyPayment", x => x.Execute(), "*/30 10-23 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "payments");
            RecurringJob.AddOrUpdate<UsePrepaymentForEndPeriodContractsJob>("UsePrepaymentForEndPeriodContractsJob", x => x.Execute(), "*/30 10-23 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "payments");
            RecurringJob.AddOrUpdate<UsePrepaymentForCreditLineForMonthlyPaymentJob>("UsePrepaymentForCreditLineForMonthlyPaymentJob", x => x.Execute(), "*/30 10-23 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "payments");

            //////"mintos"
            ////Выгрузка договоров в Mintos
            //RecurringJob.AddOrUpdate<MintosContractUploadJob>("MintosContractUpload", x => x.Execute(), Cron.Hourly(40), TZConvert.GetTimeZoneInfo(GetTimeZone()), "mintos");
            ////Выгрузка оплат в Mintos
            //RecurringJob.AddOrUpdate<MintosPaymentJob>("MintosPayment", x => x.Execute(), Cron.Daily(0, 0), TZConvert.GetTimeZoneInfo(GetTimeZone()), "mintos");
            ////Проверка графика и статуса оплаты по договору в Mintos
            //RecurringJob.AddOrUpdate<MintosContractStatusCheckJob>("MintosContractFullCheck", x => x.Execute(), "10 2,6,21 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "mintos");
            ////Массовый акт сверки с Mintos
            //RecurringJob.AddOrUpdate<MintosContractStatusCheckJob>("MintosContractFastCheck", x => x.CheckStatuses(), Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()), "mintos");

            //"crm"
            //Выгрузка договоров в CRM(Bitrix24)
            RecurringJob.AddOrUpdate<CrmUploadJob>("CrmUpload", x => x.Execute(), "*/10 7-22 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "crm");
            //Обновление контактов в CRM(Bitrix24)
            RecurringJob.AddOrUpdate<CrmUploadContactJob>("CrmUploadContact", x => x.Execute(), Cron.Hourly(45), TZConvert.GetTimeZoneInfo(GetTimeZone()), "crm");
            //Создание очереди платежей для CRM(Bitrix24)
            RecurringJob.AddOrUpdate<CrmGeneratePaymentQueue>("CrmGeneratePaymentQueue", x => x.Execute(), Cron.Daily(7), TZConvert.GetTimeZoneInfo(GetTimeZone()), "crm");
            //Выгрузка платежей в CRM(Bitrix24)
            RecurringJob.AddOrUpdate<CrmUploadPaymentJob>("CrmUploadPayment", x => x.Execute(), "*/10 7-22 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "crm");

            //"1c"
            // Выгрузка в 1С
            RecurringJob.AddOrUpdate<AccountantIntegrationJob>("AccountantIntegration", x => x.Execute(), "*/10 7-22 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "1c");

            //"senders"
            //Рассылка сообщений/ уведомлений
            RecurringJob.AddOrUpdate<MessageSenderJob>("MessageSender", x => x.Execute(), Cron.Minutely, TZConvert.GetTimeZoneInfo(GetTimeZone()), "senders");
            //Уведомление клиентов о предстоящей оплате
            RecurringJob.AddOrUpdate<PaymentNotificationJob>("PaymentNotification", x => x.Execute(), Cron.Daily(7, 50), TZConvert.GetTimeZoneInfo(GetTimeZone()), "senders");
            // Синхронизация статусов СМСок
            RecurringJob.AddOrUpdate<UpdateDeliveryStatusesOfSmsNotifications>("UpdateDeliveryStatusesOfSmsNotifications", x => x.Execute(), "*/35 * * * *", TZConvert.GetTimeZoneInfo(strTimeZone), "senders");
            //Уведомление о имеющейся задолженности
            RecurringJob.AddOrUpdate<DelayNotificationJob>("DelayNotification", x => x.Execute(), Cron.Daily(7, 50), TZConvert.GetTimeZoneInfo(GetTimeZone()), "senders");
            // Поздравление о дне рождения клиентов
            RecurringJob.AddOrUpdate<ClientsBirthdayNotificationsJob>("ClientsBirthdayNotificationsJob", x => x.Execute(), Cron.Daily(7, 50), TZConvert.GetTimeZoneInfo(GetTimeZone()), "senders");

            //"cb"
            // Создание пакетов для выгрузки в КБ 
            //ежедневный
            RecurringJob.AddOrUpdate<CBBatchCreationDailyJob>("cb01.CBBatchCreationDaily", x => x.Execute(), Cron.Daily(22, 45), TZConvert.GetTimeZoneInfo(strTimeZone), "cb");
            //ежеполумесячный - поменяли на еженедельный, в ночь со вторника на среду 
            RecurringJob.AddOrUpdate<CBBatchCreationMonthlyJob>("cb01.CBBatchCreationMonthly", x => x.Execute(), Cron.Weekly(DayOfWeek.Wednesday, 3, 45), TZConvert.GetTimeZoneInfo(strTimeZone), "cb");

            // Заполнение данными пакетов для выгрузки в КБ - запуск из консоли Hangifre для заполнения данных
            RecurringJob.AddOrUpdate<CBBatchDataFulfillJob>("cb02.CBBatchDataFulfill", x => x.Execute(), Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()), "cb");
            // Проверка состояния пакета в КБ - запуск из консоли Hangifre для заполнения данных
            RecurringJob.AddOrUpdate<CBXMLFileCreationJob>("cb03.CBXMLFileCreation", x => x.Execute(), Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()), "cb");
            // Проверка состояния пакета в КБ
            RecurringJob.AddOrUpdate<CBBatchStatusCheckJob>("cb05.CBBatchStatusCheck", x => x.Execute(), Cron.Hourly, TZConvert.GetTimeZoneInfo(GetTimeZone()), "cb");
            // Загрузка пакетов в КБ - запуск из консоли Hangifre для заполнения данных
            RecurringJob.AddOrUpdate<CBBatchUploadJob>("cb04.CBBatchUpload", x => x.Execute(), Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()), "cb");
            // Отправка ошибок по батчам в телеграм
            RecurringJob.AddOrUpdate<CBBatchContractsUploadJob>("cb01.CBBatchContractsUploadJob", x => x.Execute(), "*/59 8-23 * * *", TZConvert.GetTimeZoneInfo(strTimeZone), "cb");

            //accruals
            // НАЧИСЛЕНИЕ ПРОЦЕНТОВ
            // Начисление в любую дату
            RecurringJob.AddOrUpdate<InterestAccrualJob>("InterestAccrualOnAnyDate", x => x.EnqueueAllOnAnyDate(), "0 5 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "accruals");
            // Начисление на просрочку на старые короткие дискреты
            RecurringJob.AddOrUpdate<InterestAccrualOnOverdueDebtJob>("InterestAccrualOnOverdueDebtJob", x => x.Execute(), "30 4 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "accruals");

            // ВЫНОС НА ПРОСРОЧКУ
            //Вынос договоров на просрочку
            RecurringJob.AddOrUpdate<TakeAwayToDelayJob>("TakeAwayToDelay", x => x.Execute(), "15 1 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "accruals");

            // НАЧИСЛЕНИЕ ПЕНИ!!!!!
            RecurringJob.AddOrUpdate<PenaltyAccrualJob>("PenaltyAccrualJob", x => x.Execute(), "0 3 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "accruals");
            RecurringJob.AddOrUpdate<PenaltyLimitAccrualJob>("PenaltyLimitAccrual", x => x.Execute(), "0 2 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "accruals");
            RecurringJob.AddOrUpdate<PenaltyRateDecreaseJob>("PenaltyRateDecreaseJob", x => x.Execute(), "30 1 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "accruals");

            //insurance
            //Аннулирование страховых полисов
            RecurringJob.AddOrUpdate<InsurancePoliciesCancelJob>("InsurancePoliciesCancellation", x => x.Execute(), Cron.Daily(22, 30), TZConvert.GetTimeZoneInfo(GetTimeZone()), "insurance");

            //Аннулирование Заявок МП и удаление драфтов Договоров
            RecurringJob.AddOrUpdate<ApplicationsRejectAndDeleteRefContractsJob>("ApplicationsRejectAndDeleteRefContracts", x => x.Execute(), Cron.Daily(22, 40), TZConvert.GetTimeZoneInfo(GetTimeZone()), "applications");

            //RecurringJob.AddOrUpdate<TasOnlinePaymentsSendJob>("TasOnlinePaymentsSendJob", x => x.Execute(), Cron.Daily(0, 30), TZConvert.GetTimeZoneInfo(strTimeZone), "senders");

            //Отмена незавершенных незаапрувленных действий
            RecurringJob.AddOrUpdate<CancelAwaitingContracActionsJob>("CancelAwaitingContracActionsJob", x => x.Execute(), Cron.Daily(23, 00), TZConvert.GetTimeZoneInfo(GetTimeZone()), "senders");

            //Отправка уведомления CRM о закрытии займа
            RecurringJob.AddOrUpdate<SendCloseNotifCrmJob>("SendCloseNotifCrmJob", x => x.Execute(), Cron.Daily(6, 30), TZConvert.GetTimeZoneInfo(strTimeZone), "senders");

            //Повторная попытка отправки страхового полиса в сервис СК
            //RecurringJob.AddOrUpdate<RepeatedSendInsuranceTasOnlineJob>("RepeatedSendInsuranceTasOnlineJob", x => x.Execute(), "0 9-21/1 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "insurance");

            //Отправка уведомления CRM о выходе на просрочку и просроченных займах
            //RecurringJob.AddOrUpdate<SendOverdueNotifCrmJob>("SendOverdueNotifCrmJob", x => x.Execute(), Cron.Daily(06, 00), TZConvert.GetTimeZoneInfo(strTimeZone), "senders");

            //Экспорт данных в 1C
            RecurringJob.AddOrUpdate<Online1CJob>("Online1CJob", x => x.Execute(), Cron.Daily(6), TZConvert.GetTimeZoneInfo(GetTimeZone()), "online1c");

            //Экспорт данных в Коллекшн
            RecurringJob.AddOrUpdate<CollectionJob>("CollectionJob", x => x.Execute(), Cron.Daily(6, 45), TZConvert.GetTimeZoneInfo(GetTimeZone()), "senders");

            //КФМ загрузка субъектов по списку
            RecurringJob.AddOrUpdate<KFMPersonsUploadJob>("KFMPersonsUploadJob", x => x.Execute(), Cron.Daily(3, 50), TZConvert.GetTimeZoneInfo(GetTimeZone()), "kfm");

            RecurringJob.AddOrUpdate<OnlineTaskDelayApplicationJob>("OnlineTaskDelayApplicationJob", x => x.Execute(), "*/5 9-19 * * *", TZConvert.GetTimeZoneInfo(GetTimeZone()), "applications");

            // отмена/отклонение автокредитов которые без ПВ больше чем один день (один день пока что не решен, и статус тоже пока что не решен)
            RecurringJob.AddOrUpdate<AutocreditContractCancelJob>("AutocreditContractCancelJob", x => x.Execute(), Cron.Daily(23, 00), TZConvert.GetTimeZoneInfo(strTimeZone), "senders");

            // отмена/отклонение онлайн заявок по истеченюю срока
            RecurringJob.AddOrUpdate<CancelApplicationOnlineJob>("CancelApplicationOnlineJob", x => x.Execute(), Cron.Daily(23, 00), TZConvert.GetTimeZoneInfo(strTimeZone), "applications");

            // реструктуризация (военнослужащие)
            RecurringJob.AddOrUpdate<ClientDefermentsForRecruitJob>("ClientDefermentsForRecruitListMKBJob", x => x.ExecuteListMKB(), Cron.Daily(), TZConvert.GetTimeZoneInfo(strTimeZone));
            //RecurringJob.AddOrUpdate<ClientDefermentsForRecruitJob>("ClientDefermentsForRecruitListJob", x => x.ExecuteList(), Cron.Daily(), TZConvert.GetTimeZoneInfo(strTimeZone));


            //"migrations"
            // Открытие счетов
            //RecurringJob.AddOrUpdate<OpenAccountsForContractsJob>("OpenAccountsForContracts", x => x.Enqueue(),
            //Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()));
            // Пересчёт балансов по счетам
            //RecurringJob.AddOrUpdate<RecalculateAccountBalanceJob>("RecalculateAccountBalanceJob", x => x.EnqueueAll(),
            // Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()));
            ////Вынос на просрочку
            //RecurringJob.AddOrUpdate<MigrateToDelayJob>("MigrateToDelayJob", x => x.Execute(),
            //    Cron.Never, TZConvert.GetTimeZoneInfo(GetTimeZone()));
        }

        public static void ClearOldRecurringJobs()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var job in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(job.Id);
                }
            }
        }
    }
}