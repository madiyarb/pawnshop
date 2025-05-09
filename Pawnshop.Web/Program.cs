using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using Pawnshop.Web.Extensions.Helpers;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Pawnshop.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.Sources.Clear();
                    config.SetBasePath(hostContext.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("appsettings.json", true, true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddNLog(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.Console(restrictedToMinimumLevel:
                        SerilogRestrictMinimumLogLevelDeterminationHelper
                            .Determinate(hostingContext.Configuration.GetValue<string>("Serilog:LogLevel")), //Это все родилось из за того что new ExpressionTemplate не поддерживается из appsettings
                        formatter: new ExpressionTemplate(
                            "{ {timestamp: @t, message: @m, severity: @l, exception: @x, trace_id: TraceId, span_id : SpanId, ..@p } }\r\n",
                theme: TemplateTheme.Code)));
    }
}
