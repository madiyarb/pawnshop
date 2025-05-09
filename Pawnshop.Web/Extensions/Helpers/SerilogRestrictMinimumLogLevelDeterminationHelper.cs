using Serilog.Events;
using System;

namespace Pawnshop.Web.Extensions.Helpers
{
    public static class SerilogRestrictMinimumLogLevelDeterminationHelper
    {
        public static LogEventLevel Determinate(string logLevel)
        {
            LogEventLevel logEventLevel;
            if (Enum.TryParse(logLevel, out logEventLevel))
            {
                return logEventLevel;
            }
            return LogEventLevel.Warning;
        }
    }
}
