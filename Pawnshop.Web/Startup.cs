using System;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pawnshop.Core.Options;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Jobs;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pawnshop.Services.Insurance.SignalR;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.CardCashOut;
using Pawnshop.Services.CardTopUp;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Services.LegalCollection;
using Pawnshop.Web.Extensions;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientsGeoPositions;
using Pawnshop.Web.Models.ClientsMobilePhoneContacts;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.DebtorRegisrty.CourtOfficer.HttpService;
using Pawnshop.Services.DebtorRegisrty.HttpService;
using Pawnshop.Services.Estimation;
using Pawnshop.Services.Estimation.Images;
using Pawnshop.Services.Estimation.v2;
using KafkaFlow;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Web.Hubs;
using Pawnshop.Web.Kafka;
using Microsoft.AspNetCore.Http.Connections;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.LegalCollection.HttpServices;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.SUSN;
using Pawnshop.Services.DebtorRegistry;
using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.Collection.http;
using Serilog;
using Pawnshop.Services.TasLabBankrupt;
using Pawnshop.Services.Bankruptcy;
using System.Net.Http;
using Pawnshop.Services.Auction.HttpServices;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Pawnshop.Services.Insurance.HttpHelper;
using Pawnshop.Web.Extensions.Helpers;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using Pawnshop.Services.Crm;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Pawnshop.Services.Auction.Mapping.HttpServices.Interfaces;
using Pawnshop.Services.Auction.Mapping.HttpServices;
using Pawnshop.Services.Auction.Mapping.Interfaces;
using Pawnshop.Services.Auction.Mapping;
using Pawnshop.Services.TasCore.Options;

namespace Pawnshop.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }
        public IContainer ApplicationContainer { get; set; }

        private bool swaggerEnabled = true;

        private const string PawnshopApiServer = "Pawnshop API server";
        private readonly SecurityKey _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("m3gaSecretKe3y!devman.kz"));

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            InitializeOptions(services);
            InitializeAuth(services);

            services.AddSignalR(hubOptions =>
            {
                hubOptions.MaximumReceiveMessageSize = 9223372036854775807;
                hubOptions.EnableDetailedErrors = true;
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(5);
            });
            services.AddMvc(x => x.AllowEmptyInputInBodyModelBinding = true)
            .AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssemblyContaining<ClientGeoPositionBinding>();
                        fv.RegisterValidatorsFromAssemblyContaining<PageBinding>();
                        fv.RegisterValidatorsFromAssemblyContaining<ClientsMobilePhoneContactBinding>();
                        fv.RegisterValidatorsFromAssemblyContaining<ClientGeoPositionBinding>();
                    })
                .AddNewtonsoftJson()
                .AddMvcOptions(options => {
                options.MaxValidationDepth = 999;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel:
                    SerilogRestrictMinimumLogLevelDeterminationHelper
                        .Determinate(Configuration.GetValue<string>("Serilog:LogLevel")),
                    formatter: new ExpressionTemplate(
                        "{ {timestamp: @t, message: @m, severity: @l, exception: @x, trace_id: TraceId, span_id : SpanId, ..@p } }\r\n",
                        theme: TemplateTheme.Code))
                .CreateLogger();

            services.AddLogging(r =>
            {
                r.AddSerilog(logger);
            });


            services.AddHttpClient<IUKassaHttpService, UKassaHttpService>();
            services.AddHttpClient<IFcb4Kdn, Fcb4Kdn>();
            services.AddHttpClient<IRequestMobileAppService, RequestMobileAppService>();
            services.AddHttpClient<CBBatchContractsUploadJob>();
            services.AddHttpClient<ICrmUploadService, CrmUploadService>();
            services.AddHttpClient<ICollectionHttpService<CollectionActions>, CollectionActionHttpService>();
            services.AddHttpClient<ICollectionHttpService<CollectionHistory>, CollectionContractStatusHistoryHttpService>();
            services.AddHttpClient<ICollectionHttpService<CollectionStatus>, CollectionStatusHttpService>();
            services.AddHttpClient<ICollectionHttpService<CollectionReason>, CollectionReasonHttpService>();
            services.AddHttpClient<ICollectionHttpService<CollectionStatusScenario>, CollectionStatusScenarioHttpService>();
            services.AddHttpClient<ICollectionHttpService<LegalCaseStageScenarioDto>, LegalCollectionStageScenariosHttpService>();
            
            // legal collection
            services.AddHttpClient<ILegalCaseHttpService, LegalCaseHttpService>();
            services.AddHttpClient<ILegalCollectionStatusesHttpService, LegalCollectionStatusesHttpService>();
            services.AddHttpClient<ILegalCollectionCoursesHttpService, LegalCollectionCoursesHttpService>();
            services.AddHttpClient<ILegalCollectionStagesHttpService, LegalCollectionStagesHttpService>();
            services.AddHttpClient<ILegalCollectionActionsHttpService, LegalCollectionActionsHttpService>();
            services.AddHttpClient<ILegalCollectionPrintTemplateHttpService, LegalCollectionPrintTemplateHttpService>();
            services.AddHttpClient<ILegalCollectionChangeCourseHttpService, LegalCollectionChangeCourseHttpService>();
            services.AddHttpClient<ILegalCollectionDocumentTypeHttpService, LegalCollectionDocumentTypeHttpService>();
            services.AddHttpClient<ILegalCollectionNotificationTemplateHttpService, LegalCollectionNotificationTemplateHttpService>();
            services.AddHttpClient<ILegalCollectionNotificationHttpService, LegalCollectionNotificationHttpService>();
            services.AddHttpClient<ILegalCollectionTaskStatusHttpService, LegalCollectionTaskStatusHttpService>();
            services.AddHttpClient<IDebtorRegisterHttpService, DebtorRegisterHttpService>();
            services.AddHttpClient<ICourtOfficerHttpService, CourtOfficerHttpService>();
            
            // Auction
            services.Configure<CarAuctionSettings>(Configuration.GetSection("AuctionSettings"));
            services.AddHttpClient<IAuctionOperationHttpService, AuctionOperationHttpService>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<CarAuctionSettings>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
            });

            services.AddHttpClient<ICarAuctionHttpService, CarAuctionHttpService>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<CarAuctionSettings>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
            });
            
            services.AddScoped<IAuctionMappingService, AuctionMappingService>(); // удалить после успешного маппинга
            services.AddHttpClient<IAuctionMappingHttpService, AuctionMappingHttpService>(); // удалить после успешного маппинга

            services.AddSingleton<ICardCashOutService, CardCashOutService>();
            services.AddSingleton<IApplicationOnlineEstimationImageService, ApplicationOnlineEstimationImageService>();
            services.AddSingleton<OldEstimationService>();
            services.AddSingleton<EstimationService>();
            services.AddSingleton<ICardTopUpService, CardTopUpService>();
            services.AddHttpClient<AccountantIntegrationJob>();
            services.AddHttpClient<BankruptcyService>().ConfigurePrimaryHttpMessageHandler( () => new HttpClientHandler { 
               ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true
            });

            services.AddHttpClient<IHttpRequestSenderService, HttpRequestSenderService>();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            services.AddHealthChecks()
                .AddSqlServer(Configuration.GetConnectionString("database"));

            // InitializeSwagger(services);
            swaggerEnabled = Configuration.GetValue<bool>("AppSettings:SwaggerEnabled");
            if (true)
            {
                services.AddSwagger();
            }

            bool kafkaEnabled = false;
            kafkaEnabled = Configuration.GetValue<bool>("AppSettings:KafkaEnabled");
            if (kafkaEnabled)
            {
                KafkaInitializerHelper.Initialize(services, Configuration);
            }
            InitializeHangFire(services);
            ApplicationContainer = new AppContainer().Build(services);
            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseExceptionHandling();
            app.UseSessionContext();
            app.UseBranchContext();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(policyBuilder =>
                policyBuilder.AllowAnyHeader().
                    AllowAnyMethod().
                    SetIsOriginAllowed(host => true)
                    .AllowCredentials());

            app.UseHangfireDashboard();

            var hangfireServerOptions = new BackgroundJobServerOptions
            {
                //WorkerCount = Environment.ProcessorCount * 5 > 50 ? 50 : Environment.ProcessorCount < 20 ? 20 : Environment.ProcessorCount * 5,
                WorkerCount = Configuration.GetValue<int>("AppSettings:HangFireWorkers"),
                Queues = Configuration.GetSection("AppSettings:jobQueues")?.Get<string[]>() ?? new[] { "default" }
            };

            if (env.IsProduction())
            {
                app.UseHangfireServer(hangfireServerOptions);
            }
            string swaggerBasePath = "api/app";
            if (true)
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/{swaggerBasePath}/swagger/v1/swagger.json", $"Pawnshop API");
                    c.RoutePrefix = $"{swaggerBasePath}/swagger";
                    c.ConfigObject.AdditionalItems.Add("syntaxHighlight", false); //Turns off syntax highlight which causing performance issues...
                    c.ConfigObject.AdditionalItems.Add("theme", "agate"); //Reverts Swagger UI 2.x  theme which is simpler not much performance benefit...
                });
            }


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<InsuranceHub>("/insurance");
                endpoints.MapHub<TasOnlineUsersHub>("/api/tasonlineusers", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard( new DashboardOptions
                {
                    IgnoreAntiforgeryToken = true                                 // <--This
                });
                endpoints.MapControllerRoute("api", "api/{controller}/{action}");
                if (swaggerEnabled)
                {
                    endpoints.MapSwagger(swaggerBasePath + "/swagger/{documentName}/swagger.json");
                }

                if (env.IsProduction())
                {
                    endpoints.MapControllerRoute("default", "{controller=home}/{action=index}", new
                    {
                        controller = "home",
                        action = "index"
                    }, new
                    {
                        httpMethod = new HttpMethodRouteConstraint("GET")
                    });
                }
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });

            app.UseSerilogRequestLogging();

            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationContainer?.Dispose();
                ApplicationContainer = null;
            });
        }

        //app services
        public void InitializeHangFire(IServiceCollection services)
        {

            var sqlStorage = new SqlServerStorage(Configuration.GetConnectionString("HangFireDatabase"));
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("HangFireDatabase"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));
            JobStorage.Current = sqlStorage;
            GlobalConfiguration.Configuration.UseAutofacActivator(new AppContainer().Build(services), false);
            JobRegistry.Register();
        }
        public void InitializeOptions(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = PawnshopApiServer;
                options.Audience = PawnshopApiServer;
                options.SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
            });
            services.Configure<EnviromentAccessOptions>(options =>
            {
                options.DatabaseConnectionString = Configuration.GetConnectionString("database");
                options.ReportDatabaseConnectionString = Configuration.GetConnectionString("ReportDatabase");
                options.StorageConnectionString = Configuration.GetConnectionString("storage");
                options.ExpireDay = 45;
                options.PaymentNotification = bool.Parse(Configuration.GetSection("AppSettings:paymentNotification").Value);
                options.MintosUpload = bool.Parse(Configuration.GetSection("AppSettings:mintosUpload").Value);
                options.OnlinePayment = bool.Parse(Configuration.GetSection("AppSettings:onlinePayment").Value);
                options.AccountantUpload = bool.Parse(Configuration.GetSection("AppSettings:accountantUpload").Value);
                options.DictionaryActualizationEGOV = bool.Parse(Configuration.GetSection("AppSettings:dictionaryActualizationEGOV").Value);
                options.CBUpload = bool.Parse(Configuration.GetSection("AppSettings:cbUpload").Value);
                options.CrmUpload = bool.Parse(Configuration.GetSection("AppSettings:crmUpload").Value);
                options.JobQueues = Configuration.GetSection("AppSettings:jobQueues").Get<string[]>();
                options.CrmPaymentUpload = bool.Parse(Configuration.GetSection("AppSettings:crmPaymentUpload").Value);
                options.SchedulePayments = Configuration.GetValue<bool>("AppSettings:schedulePayments");
                options.DelayNotification = bool.Parse(Configuration.GetSection("AppSettings:delayNotification").Value);

                // infobip configuration
                options.InfobipUser = Configuration.GetValue<string>("AppSettings:infobipUser");
                options.InfobipPassword = Configuration.GetValue<string>("AppSettings:infobipPassword");
                options.InfobipFrom = Configuration.GetValue<string>("AppSettings:infobipFrom");

                // включить/выключить отправление писем и смс
                options.SendEmailNotifications = Configuration.GetValue<bool>("AppSettings:sendEmailNotifications");
                options.SendSmsNotifications = Configuration.GetValue<bool>("AppSettings:sendSmsNotifications");
                options.UpdateSmsDeliveryStatuses = Configuration.GetValue<bool>("AppSettings:updateSmsDeliveryStatuses");
                options.GenerateBirthdayNotifications = Configuration.GetValue<bool>("AppSettings:generateBirthdayNotifications");
                options.NoReplyEmailName = Configuration.GetValue<string>("AppSettings:noReplyEmailName");
                options.NoReplyEmailPassword = Configuration.GetValue<string>("AppSettings:noReplyEmailPassword");
                options.SmtpServerName = Configuration.GetValue<string>("AppSettings:smtpServerName");
                options.SmtpServerPort = Configuration.GetValue<int>("AppSettings:smtpServerPort");

                options.NskEmailAddress = "aseln@nsk.kz";
                options.NskEmailName = "Асель Найзабекова";
                options.NskEmailCopyAddress = "arkhat@tascredit.kz";
                options.NskEmailCopyName = "Бекен Архат";
                options.InsuranseManagerAddress = "m.zhaniya@tascredit.kz";
                options.InsuranseManagerName = "Жания Малгаджарова";
                options.ErrorNotifierAddress = "errors@tascredit.kz";
                options.ErrorNotifierName = "WEB NOTIFIER";
                options.MintosUrl = "https://www.mintos.com/lender-api/v2/";

                // mobile app configuration
                options.MobileAppUser = Configuration.GetValue<string>("AppSettings:mobileAppUser");
                options.MobileAppPassword = Configuration.GetValue<string>("AppSettings:mobileAppPassword");
                options.MobileAppUrl = Configuration.GetValue<string>("AppSettings:mobileAppUrl");

                options.InsuranceErrorNotifierAddress = "insurance-errors@tascredit.kz";
                options.InsuranceErrorNotifierName = "INSURANCE NOTIFIER";
                options.KDN = Configuration.GetValue<decimal>("AppSettings:KDN");
                options.KDNLowPriority = Configuration.GetValue<decimal>("AppSettings:KDNLowPriority");
                options.KDNK4 = Configuration.GetValue<decimal>("AppSettings:KDNK4");
                options.BitrixHttpTimeoutSeconds = Configuration.GetValue<int>("AppSettings:BitrixHttpTimeoutSeconds");
                options.KDNBusiness = Configuration.GetValue<decimal>("AppSettings:KDNBusiness");
                options.ApplicationOnlineSingType = Configuration.GetValue<string>("AppSettings:ApplicationOnlineSingType");
            });

            services.Configure<CardMerchantOptions>(Configuration.GetSection("CardMerchantOptions"));
            services.Configure<CardTopUpOptions>(Configuration.GetSection("CardTopUpOptions"));
            services.Configure<OldEstimationServiceOptions>(Configuration.GetSection("EstimationServiceOptions"));
            services.Configure<EstimationServiceOptions>(Configuration.GetSection("NewEstimationServiceOptions"));
            services.Configure<FileStorageOptions>(Configuration.GetSection("FileStorageOptions"));
            services.Configure<TasLabSUSNServiceOptions>(Configuration.GetSection("TasLabSUSNServiceOptions"));
            services.Configure<DebtorRegistryServiceOptions>(Configuration.GetSection("DebtorRegistryServiceOptions"));
            services.Configure<TasLabBankruptInfoServiceOptions>(
                Configuration.GetSection("TasLabBankruptInfoServiceOptions"));
            services.Configure<TasCoreOptions>(Configuration.GetSection("TasCoreOptions"));
        }

        public void InitializeSwagger(IServiceCollection services)
        {
           services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Pawnshop Service",
                    Description = "Pawnshop API Swagger surface",
                    Contact = new OpenApiContact
                    {
                        Name = "Nikolay Nikolskiy",
                        Email = "n.nikolay@tascredit.kz",
                        Url = new Uri("https://www.tascredit.kz")
                    }
                });

                s.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                //First we define the security scheme s.AddSecurityDefinition("Bearer", 
                //Name the security scheme new OpenApiSecurityScheme { 
                //Description = "JWT Authorization header using the Bearer scheme.", Type = SecuritySchemeType.Http, 
                //We set the scheme type to http since we're using bearer authentication Scheme = "bearer" 
                //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer". 

                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                s.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    Description = "Basic Authorization header using the Basic scheme (Example: 'Basic login:password') encoded in Base64",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Basic"
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Basic"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                s.CustomSchemaIds(type => type.ToString());
            });
        }
        
        public void InitializeAuth(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = PawnshopApiServer,

                        ValidateAudience = true,
                        ValidAudience = PawnshopApiServer,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = _key,

                        RequireExpirationTime = true,
                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.Zero
                    };
                })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationDefaults.AuthenticationScheme, null);
        }
    }
}
