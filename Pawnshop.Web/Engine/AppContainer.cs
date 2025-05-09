using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Pawnshop.Data;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Controllers.Api;
using Pawnshop.Services;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Calculation;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.Export.Reports;
using Pawnshop.Web.Engine.Jobs;
using Pawnshop.Web.Engine.Jobs.AccountingJobs;
using Pawnshop.Web.Engine.Jobs.MigrationJobs;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Services.Storage;
using Pawnshop.Services.Clients;
using Pawnshop.Services.ReportDatas;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.PensionAges;
using Pawnshop.Services.Collection;
using System.Reflection;
using Pawnshop.Services.Sms;
using Pawnshop.Services.Subjects;
using Pawnshop.Services.Notifications;
using Pawnshop.Web.Extensions.Helpers;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Pawnshop.Data.Models.Notifications.Interfaces;

namespace Pawnshop.Web.Engine
{
    public class AppContainer
    {

        public IContainer Build(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            RegisterMvc(services);
            RegisterPermissions(services);
            builder.Populate(services);

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel:
                    SerilogRestrictMinimumLogLevelDeterminationHelper
                        .Determinate("Warning"), //Это все родилось из за того что new ExpressionTemplate не поддерживается из appsettings
                    formatter: new ExpressionTemplate(
                        "{ {timestamp: @t, message: @m, severity: @l, exception: @x, trace_id: TraceId, span_id : SpanId, ..@p } }\r\n",
                        theme: TemplateTheme.Code))
                .CreateLogger();

            builder.RegisterInstance<ILogger>(logger);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            builder.RegisterModule<CoreModule>();
            builder.RegisterModule<DataModule>();
            builder.RegisterModule<AccountingCoreModule>();
            builder.RegisterModule<ServicesModule>();

            builder.RegisterType<TokenProvider>().AsSelf();
            builder.RegisterType<SaltedHash>().AsSelf();

            builder.RegisterType<AzureStorage>().As<IStorage>();
            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<EnviromentAccessOptions>>().Value;
                var account = CloudStorageAccount.Parse(options.StorageConnectionString);

                return account.CreateCloudBlobClient();
            }).As<CloudBlobClient>().InstancePerLifetimeScope();

            builder.RegisterType<BranchContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<EventLog>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<JobLog>().As<IJobLog>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<ReportContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<DbContext>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<CashOrdersExcelBuilder>().AsSelf();
            builder.RegisterType<ContractsExcelBuilder>().AsSelf();
            builder.RegisterType<TransferContractsExcelBuilder>().AsSelf();
            builder.RegisterType<SellingsExcelBuilder>().AsSelf();
            builder.RegisterType<EventLogExcelBuilder>().AsSelf();
            builder.RegisterType<ContractMonitoringExcelBuilder>().AsSelf();
            builder.RegisterType<AccountAnalysisExcelBuilder>().AsSelf();
            builder.RegisterType<OnlinePaymentsManageExcelBuilder>().AsSelf();
            builder.RegisterType<PayOperationExcelBuilder>().AsSelf();
            builder.RegisterType<TasOnlinePaymentExcelBuilder>().AsSelf();
            builder.RegisterType<InsuranceReviseRowsExcelBuilder>().AsSelf();

            builder.RegisterType<ContractWordBuilder>().AsSelf();
            builder.RegisterType<AnnuityContractWordBuilder>().AsSelf();

            builder.RegisterType<EmailSender>().AsSelf();
            builder.RegisterType<SmsSender>().AsSelf();
            builder.RegisterType<MintosApi.MintosApi>().AsSelf();
            builder.RegisterType<MobileAppApi.MobileAppApi>().AsSelf();

            builder.RegisterType<CrmUploadJob>().InstancePerDependency();
            builder.RegisterType<MessageSenderJob>().InstancePerDependency();
            builder.RegisterType<PaymentNotificationJob>().InstancePerDependency();
            builder.RegisterType<OnlinePaymentJob>().InstancePerDependency();
            builder.RegisterType<OnlinePaymentCheckStatusJob>().InstancePerDependency();
            builder.RegisterType<UsePrepaymentForMonthlyPaymentJob>().InstancePerDependency();
            builder.RegisterType<UsePrepaymentForCreditLineForMonthlyPaymentJob>().InstancePerDependency();
            builder.RegisterType<UsePrepaymentForEndPeriodContractsJob>().InstancePerDependency();
            builder.RegisterType<ReportDataJob>().InstancePerDependency();
            builder.RegisterType<NationalBankCurrencyUpdateJob>().InstancePerDependency();
            builder.RegisterType<MintosContractUploadJob>().InstancePerDependency();
            builder.RegisterType<MintosContractStatusCheckJob>().InstancePerDependency();
            builder.RegisterType<MintosPaymentJob>().InstancePerDependency();
            builder.RegisterType<EGOVDictionaryActualizationJob>().InstancePerDependency();
            builder.RegisterType<CrmGeneratePaymentQueue>().InstancePerDependency();
            builder.RegisterType<CrmUploadContactJob>().InstancePerDependency();
            builder.RegisterType<CrmUploadPaymentJob>().InstancePerDependency();
            builder.RegisterType<UpdateDeliveryStatusesOfSmsNotifications>().InstancePerDependency();
            builder.RegisterType<DelayNotificationJob>().InstancePerDependency();
            builder.RegisterType<ClientsBirthdayNotificationsJob>().InstancePerDependency();
            builder.RegisterType<InterestAccrualJob>().InstancePerDependency();
            builder.RegisterType<PenaltyAccrualJob>().InstancePerDependency();
            builder.RegisterType<InterestAccrualOnOverdueDebtJob>().InstancePerDependency();
            builder.RegisterType<SendCloseNotifCrmJob>().InstancePerDependency();
            builder.RegisterType<SendOverdueNotifCrmJob>().InstancePerDependency();
            builder.RegisterType<RepeatedSendInsuranceTasOnlineJob>().InstancePerDependency();
            builder.RegisterType<KFMPersonsUploadJob>().InstancePerDependency();
            builder.RegisterType<OnlineTaskDelayApplicationJob>().InstancePerDependency();
            builder.RegisterType<CancelApplicationOnlineJob>().InstancePerDependency();

            builder.RegisterType<InsuranceEmailSender>().AsSelf();
            builder.RegisterType<ContractRefinance>().AsSelf();

            //Интеграция с ПКБ и ГКБ
            builder.RegisterType<CBBatchCreationDailyJob>().InstancePerDependency();
            builder.RegisterType<CBBatchCreationMonthlyJob>().InstancePerDependency();
            builder.RegisterType<CBXMLFileCreationJob>().InstancePerDependency();
            builder.RegisterType<CBBatchStatusCheckJob>().InstancePerDependency();
            builder.RegisterType<CBBatchUploadJob>().InstancePerDependency();
            builder.RegisterType<CBBatchDataFulfillJob>().InstancePerDependency();

            // Добавление контроллеров в DI контейнер
            builder.RegisterType<ContractActionController>().InstancePerDependency();

            // регистрация сервисов
            builder.RegisterType<NotificationTemplateService>().As<INotificationTemplateService>().AsSelf().InstancePerDependency();
            builder.RegisterType<InfobipSmsService>().As<IInfobipSmsService>().InstancePerDependency();
            builder.RegisterType<ClientContactService>().As<IClientContactService>().InstancePerDependency();
            builder.RegisterType<VerificationService>().As<IVerificationService>().InstancePerDependency();
            builder.RegisterType<DomainService>().As<IDomainService>().InstancePerDependency();
            builder.RegisterType<ClientProfileService>().As<IClientProfileService>().InstancePerDependency();
            builder.RegisterType<ClientExpenseService>().As<IClientExpenseService>().InstancePerDependency();
            builder.RegisterType<ClientEmploymentService>().As<IClientEmploymentService>().InstancePerDependency();
            builder.RegisterType<ClientAdditionalContactService>().As<IClientAdditionalContactService>().InstancePerDependency();
            builder.RegisterType<ClientAdditionalIncomeService>().As<IClientAdditionalIncomeService>().InstancePerDependency();
            builder.RegisterType<ClientAssetService>().As<IClientAssetService>().InstancePerDependency();
            builder.RegisterType<ClientService>().As<IClientService>().InstancePerDependency();
            builder.RegisterType<ClientQuestionnaireService>().As<IClientQuestionnaireService>().InstancePerDependency();
            builder.RegisterType<ContractActionPrepaymentService>().As<IContractActionPrepaymentService>().InstancePerDependency();
            builder.RegisterType<ContractActionProlongService>().As<IContractActionProlongService>().InstancePerDependency();
            builder.RegisterType<ContractActionSignService>().As<IContractActionSignService>().InstancePerDependency();
            builder.RegisterType<ContractActionAdditionService>().As<IContractActionAdditionService>().InstancePerDependency();
            builder.RegisterType<ContractCloneService>().As<IContractCloneService>().InstancePerDependency();
            builder.RegisterType<ContractActionBuyoutService>().As<IContractActionBuyoutService>().InstancePerDependency();
            builder.RegisterType<InnerNotificationService>().As<IInnerNotificationService>().InstancePerDependency();
            builder.RegisterType<ClientSignerService>().As<IClientSignerService>().As<IBaseService<ClientSigner>>().InstancePerDependency();
            builder.RegisterType<KazInfoTechSmsService>().As<IKazInfoTechSmsService>().InstancePerDependency();
            builder.RegisterType<PensionAgesService>().As<IPensionAgesService>().InstancePerDependency();
            builder.RegisterType<ContractSigningService>().As<IContractSigningService>().InstancePerDependency();
            builder.RegisterType<LoanSubjectService>().As<ILoanSubjectService>().InstancePerDependency();
            builder.RegisterType<SmsNotificationService>().As<ISmsNotificationService>().InstancePerDependency();


            //миграции
            builder.RegisterType<MigrateActionsJob>().InstancePerDependency();
            builder.RegisterType<RecalculateAccountBalanceJob>().InstancePerDependency();

            //финансовые джобы
            builder.RegisterType<TakeAwayToDelayJob>().InstancePerDependency();

            builder.RegisterType<ReportDataService>().As<IReportDataService>().InstancePerDependency();

            //тасОнлайн
            builder.RegisterType<TasOnlinePaymentsSendJob>().InstancePerDependency();


            //джоб для аннулирования страховых полисов
            builder.RegisterType<InsurancePoliciesCancelJob>().InstancePerDependency();
            //джоб по начислению лимита пени
            builder.RegisterType<PenaltyLimitAccrualJob>().InstancePerDependency();
            //джоб по уменьшению ставки пени
            builder.RegisterType<PenaltyRateDecreaseJob>().InstancePerDependency();
            //джоб для аннулирования Заявок МП и удаления драфтов Договоров
            builder.RegisterType<ApplicationsRejectAndDeleteRefContractsJob>().InstancePerDependency();

            //schedule
            builder.RegisterType<ContractScheduleBuilder>().As<IContractScheduleBuilder>().InstancePerDependency();

            //Интеграция с 1C
            builder.RegisterType<Online1CJob>().InstancePerDependency();

            //Интеграция с Коллекшн
            builder.RegisterType<CollectionJob>().InstancePerDependency();
            builder.RegisterType<CollectionService>().As<ICollectionService>().InstancePerDependency();

            // отмена/отклонение автокредитов которые без ПВ больше чем один день (один день пока что не решен, и статус тоже пока что не решен)
            builder.RegisterType<AutocreditContractCancelJob>().InstancePerDependency();

            // реструктуризация (военнослужащие)
            builder.RegisterType<ClientDefermentsForRecruitJob>().InstancePerDependency();

            //SignalRNotificationService 
            builder.RegisterType<SignalRNotificationService>().As<ISignalRNotificationService>()
                .InstancePerDependency();

            return builder.Build();
        }

        private void RegisterPermissions(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                var permissions = Permissions.All;
                foreach (var permission in permissions)
                {
                    var permissionName = permission.Name;
                    options.AddPolicy(permissionName, p => p.RequireClaim(TokenProvider.PermissionClaim, permissionName));
                }
            });
        }

        private void RegisterMvc(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddRazorPages();
        }
    }
}