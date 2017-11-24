using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SumoLogic.Logging.Common.Log;

namespace SumoLogic.Logging.AspNetCoreLogging
{
    /// <summary>
    /// An aspnet core options type for logging
    /// </summary>
    public class SumoLogicCoreLoggingOptions : IOptions<SumoLogicCoreLoggingOptions>
    {
        /// <summary>
        /// Gets or sets the SumoLogic server URL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property needs to be exposed as string for allowing configuration")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the name used for messages sent to SumoLogic server (sent as X-Sumo-Name header).
        /// </summary>
        public string SourceName { get; set; } = "AspNetCore-SumoObject";

        /// <summary>
        /// Gets or sets the source category used for messages sent to SumoLogic server (sent as X-Sumo-Category header).
        /// </summary>
        public string SourceCategory { get; set; }

        /// <summary>
        /// Gets or sets the source host used for messages sent to SumoLogic server (sent as X-Sumo-Host header).
        /// </summary>
        public string SourceHost { get; set; }
      
        /// <summary>
        /// Gets or sets the connection timeout, in milliseconds.
        /// </summary>
        public long ConnectionTimeout { get; set; } = 60000;

        /// <summary>
        /// Gets or sets a value indicating whether the console log should be used.
        /// </summary>
        public bool UseConsoleLog { get { return this.LogLog is ConsoleLog;} set { this.LogLog = value ? new ConsoleLog() as ILog : new DummyLog();}} 

        /// <summary>
        /// Gets or sets a value indicating whether the exception.ToString() should be automatically appended to the message being sent
        /// </summary>
        public bool AppendException { get; set; } = true;

        
        internal SumoLogic.Logging.Common.Log.ILog LogLog { get; set; } = new DummyLog();

        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public SumoLogicCoreLoggingOptions Value { get { return this;} }
    }
}