using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using SumoLogic.Logging.Common.Sender;

namespace SumoLogic.Logging.AspNetCoreLogging
{
    public class SumoLogicCoreLogging : ILogger
    {
        public SumoLogicCoreLoggingOptions Options { get; }

        /// <summary>
        /// Gets or sets the HTTP message handler.
        /// </summary>
        private HttpMessageHandler HttpMessageHandler { get; set; }

        /// <summary>
        /// Gets or sets the SumoLogic message sender.
        /// </summary>
        private SumoLogicMessageSender SumoLogicMessageSender { get; set; }

        public SumoLogicCoreLogging(SumoLogicCoreLoggingOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            this.Options = options;
            this.SumoLogicMessageSender = new SumoLogicMessageSender(this.HttpMessageHandler, options.LogLog);
            this.SumoLogicMessageSender.ConnectionTimeout = TimeSpan.FromMilliseconds(this.Options.ConnectionTimeout);
            this.SumoLogicMessageSender.Url = new Uri(this.Options.Url);
        }

        // for now lets not support scopes (i'll add it later)
        public IDisposable BeginScope<TState>(TState state) => new NoopDisposable(); 

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.Options.LogLevel >= logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
    
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
    
            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            var bodyBuilder = new StringBuilder();
            using (var textWriter = new StringWriter(bodyBuilder, CultureInfo.InvariantCulture))
            {
                textWriter.Write(message);
                if (exception != null && this.Options.AppendException)
                {
                    textWriter.Write(exception.ToString());
                }

                textWriter.WriteLine();            
            }
            this.SumoLogicMessageSender.TrySend(bodyBuilder.ToString(), this.Options.SourceName, this.Options.SourceCategory, this.Options.SourceHost);
        }
    }
}