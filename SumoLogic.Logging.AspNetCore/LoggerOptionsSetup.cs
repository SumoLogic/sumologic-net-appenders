using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace SumoLogic.Logging.AspNetCore
{
    internal class LoggerOptionsSetup : ConfigureFromConfigurationOptions<LoggerOptions>
    {
        public LoggerOptionsSetup(ILoggerProviderConfiguration<LoggerProvider> providerConfiguration) : base(providerConfiguration.Configuration)
        {
        }
    }
}
