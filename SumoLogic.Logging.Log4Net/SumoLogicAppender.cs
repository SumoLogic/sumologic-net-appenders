/**
 *    _____ _____ _____ _____    __    _____ _____ _____ _____
 *   |   __|  |  |     |     |  |  |  |     |   __|     |     |
 *   |__   |  |  | | | |  |  |  |  |__|  |  |  |  |-   -|   --|
 *   |_____|_____|_|_|_|_____|  |_____|_____|_____|_____|_____|
 *
 *                UNICORNS AT WARP SPEED SINCE 2010
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SumoLogic.Logging.Log4Net
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using log4net.Appender;
    using log4net.Core;
    using SumoLogic.Logging.Common.Log;
    using SumoLogic.Logging.Common.Sender;

    /// <summary>
    /// Buffered SumoLogic Appender implementation.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposing of disposabled donde on OnClose method, called by base class")]
    public class SumoLogicAppender : AppenderSkeleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicAppender"/> class.
        /// </summary>
        public SumoLogicAppender()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicAppender"/> class.
        /// </summary>
        /// <param name="log">The log service.</param>
        /// <param name="httpMessageHandler">The HTTP message handler.</param>
        public SumoLogicAppender(SumoLogic.Logging.Common.Log.ILog log, HttpMessageHandler httpMessageHandler)
        {
            this.SourceName = "Log4Net-SumoObject";
            this.ConnectionTimeout = 60000;
            this.LogLog = log ?? new DummyLog();
            this.HttpMessageHandler = httpMessageHandler;
        }

        /// <summary>
        /// Gets or sets the SumoLogic server URL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property needs to be exposed as string for allowing configuration")]
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name used for messages sent to SumoLogic server (sent as X-Sumo-Name header).
        /// </summary>
        public string SourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection timeout, in milliseconds.
        /// </summary>
        public long ConnectionTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the console log should be used.
        /// </summary>
        public bool UseConsoleLog
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log service.
        /// </summary>
        private SumoLogic.Logging.Common.Log.ILog LogLog
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the HTTP message handler.
        /// </summary>
        private HttpMessageHandler HttpMessageHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the SumoLogic message sender.
        /// </summary>
        private SumoLogicMessageSender SumoLogicMessageSender
        {
            get;
            set;
        }

        /// <summary>
        /// Initialize the appender based on the options set
        /// </summary>
        /// <remarks>
        /// This is part of the log4net.Core.IOptionHandler delayed object activation scheme. The ActivateOptions() method must be called
        /// on this object after the configuration properties have been set. Until ActivateOptions() is called this object is in an undefined
        /// state and must not be used. If any of the configuration properties are modified then ActivateOptions() must be called again.
        /// </remarks>
        public override void ActivateOptions()
        {
            if (this.UseConsoleLog)
            {
                this.ActivateConsoleLog();
            }

            if (this.LogLog.IsDebugEnabled)
            {
                this.LogLog.Debug("Activating options");
            }

            // Initialize the sender
            if (this.SumoLogicMessageSender == null)
            {
                this.SumoLogicMessageSender = new SumoLogicMessageSender(this.HttpMessageHandler, this.LogLog);
            }

            this.SumoLogicMessageSender.ConnectionTimeout = TimeSpan.FromMilliseconds(this.ConnectionTimeout);
            this.SumoLogicMessageSender.Url = new Uri(this.Url);
        }

        /// <summary>
        /// Performs the actual logging of events.
        /// </summary>
        /// <param name="loggingEvent">The event to append.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            if (this.SumoLogicMessageSender == null || !this.SumoLogicMessageSender.CanTrySend)
            {
                if (this.LogLog.IsWarnEnabled)
                {
                    this.LogLog.Warn("Appender not initialized. Dropping log entry");
                }
                
                return;
            }

            var bodyBuilder = new StringBuilder();
            using (var textWriter = new StringWriter(bodyBuilder, CultureInfo.InvariantCulture))
            {
                Layout.Format(textWriter, loggingEvent);

                if (Layout.IgnoresException)
                {
                    textWriter.Write(loggingEvent.GetExceptionString());
                    textWriter.WriteLine();
                }
            }

            this.SumoLogicMessageSender.TrySend(bodyBuilder.ToString(), this.SourceName);
        }

        /// <summary>
        /// Is called when the appender is closed. Derived classes should override this method if resources need to be released.
        /// </summary>
        /// <remarks>
        /// Releases any resources allocated within the appender such as file handles, network connections, etc.
        /// It is a programming error to append to a closed appender.
        /// </remarks>
        protected override void OnClose()
        {
            base.OnClose();

            if (this.SumoLogicMessageSender != null)
            {
                this.SumoLogicMessageSender.Dispose();
                this.SumoLogicMessageSender = null;
            }
        }

        /// <summary>
        /// Activate Console Log.
        /// </summary>
        private void ActivateConsoleLog()
        {
            this.LogLog = new ConsoleLog();
        }
    }
}