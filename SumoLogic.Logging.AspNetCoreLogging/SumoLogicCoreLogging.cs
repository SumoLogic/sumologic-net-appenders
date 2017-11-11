using System;
using Microsoft.Extensions.Logging;

namespace SumoLogic.Logging.AspNetCoreLogging
{
    public class SumoLogicCoreLogging : ILogger
    {
        public SumoLogicCoreLogging()
        {
        }

        public IDisposable BeginScope<TState>(TState state) => new NoopDisposable();

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }
    }
}