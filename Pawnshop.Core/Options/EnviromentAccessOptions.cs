namespace Pawnshop.Core.Options
{
    public class EnviromentAccessOptions
    {
        public string DatabaseConnectionString { get; set; }
        public string ReportDatabaseConnectionString { get; set; }
        public string StorageConnectionString { get; set; }
        public int ExpireDay { get; set; }
        public bool PaymentNotification { get; set; }
        public bool MintosUpload { get; set; }
        public bool OnlinePayment { get; set; }
        public bool AccountantUpload { get; set; }
        public bool DictionaryActualizationEGOV { get; set; }
        public bool CBUpload { get; set; }
        public bool CrmUpload { get; set; }
        public bool CrmPaymentUpload { get; set; }
        public bool DelayNotification { get; set; }
        public string NskEmailAddress { get; set; }
        public string NskEmailName { get; set; }
        public string NskEmailCopyAddress { get; set; }
        public string NskEmailCopyName { get; set; }
        public string InsuranseManagerAddress { get; set; }
        public string InsuranseManagerName { get; set; }
        public string ErrorNotifierAddress { get; set; }
        public string ErrorNotifierName { get; set; }
        public string MintosUrl { get; set; }
        public string[] JobQueues { get; set; }
        public bool SchedulePayments { get; set; }

        public string InfobipUser { get; set; }
        public string InfobipPassword { get; set; }
        public string InfobipFrom { get; set; }

        public bool SendStandardCode { get; set; }
        public bool SendSmsNotifications { get; set; }
        public bool SendEmailNotifications { get; set; }
        public string NoReplyEmailName { get; set; }
        public string NoReplyEmailPassword { get; set; }
        public string SmtpServerName { get; set; }
        public int SmtpServerPort { get; set; }

        public bool UpdateSmsDeliveryStatuses { get; set; }
        public bool GenerateBirthdayNotifications { get; set; }

        public string MobileAppUrl { get; set; }
        public string MobileAppUser { get; set; }
        public string MobileAppPassword { get; set; }

        public string InsuranceErrorNotifierAddress { get; set; }
        public string InsuranceErrorNotifierName { get; set; }

        public decimal KDN { get; set; }
        public decimal KDNLowPriority { get; set; }
        public decimal KDNK4 { get; set; }

        public int BitrixHttpTimeoutSeconds { get; set; } = 5;
        public decimal KDNBusiness { get; set; }

        public string ApplicationOnlineSingType { get; set; }
    }
}