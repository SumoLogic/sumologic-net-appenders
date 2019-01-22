using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace SumoLogic.Logging.AspNetCore
{
    public class LoggerOptions
    {
        private Uri uri;

        /// <summary>
        /// Gets or sets the SumoLogic server URL.
        /// </summary>
        public string Uri
        {
            get
            {
                return uri?.ToString();
            }
            set
            {
                uri = new Uri(value);
            }
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the name used for messages sent to SumoLogic server (sent as X-Sumo-Name header).
        /// </summary>
        public string SourceName { get; set; } = "asp.net-core-logger";

        /// <summary>
        /// Gets or sets the source category used for messages sent to SumoLogic server (sent as X-Sumo-Category header).
        /// </summary>
        public string SourceCategory { get; set; }

        /// <summary>
        /// Gets or sets the source host used for messages sent to SumoLogic server (sent as X-Sumo-Host header).
        /// </summary>
        public string SourceHost { get; set; } = System.Net.Dns.GetHostName();

        /// <summary>
        /// Gets or sets if the sender using a buffer
        /// </summary>
        public bool IsBufferred { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the send message retry interval.
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the maximum interval between flushes.
        /// </summary>
        public TimeSpan MaxFlushInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets how often the messages queue is checked for messages to send.
        /// </summary>
        public TimeSpan FlushingAccuracy { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Gets or sets how many messages need to be in the queue before flushing.
        /// </summary>
        public long MessagesPerRequest { get; set; } = 100;

        /// <summary>
        /// Gets or sets the messages queue capacity, in bytes.
        /// </summary>
        /// <remarks>Messages are dropped When the queue capacity is exceeded.</remarks>
        public long MaxQueueSizeBytes { get; set; } = 1_000_000;

        /// <summary>
        /// Gets or sets the HTTP message handler used by sender
        /// </summary>
        public HttpMessageHandler HttpMessageHandler { get; set; }

        /// <summary>
        /// Gets or sets the debugging logger for provider itself 
        /// </summary>
        public ILogger DebuggingLogger { get; set; }

    }
}
