using Microsoft.Extensions.Logging;
using System;

namespace SumoLogic.Logging.AspNetCore
{
    public class Logger : ILogger
    {

        private readonly LoggerProvider provider;

        private readonly string categoryName;

        public Logger(LoggerProvider provider, string categoryName)
        {
            this.provider = provider;
            this.categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var line = $"{formatter(state, exception)}";
            provider.WriteLine(line, categoryName);
        }
    }
}
