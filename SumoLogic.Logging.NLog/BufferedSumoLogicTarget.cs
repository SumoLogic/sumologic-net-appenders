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
namespace SumoLogic.Logging.NLog
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Timers;
    using global::NLog;
    using global::NLog.Targets;
    using SumoLogic.Logging.Common.Log;
    using SumoLogic.Logging.Common.Queue;
    using SumoLogic.Logging.Common.Sender;

    /// <summary>
    /// Buffered Sumo Logic target implementation.
    /// </summary>
    [Target("BufferedSumoLogicTarget")]
    public class BufferedSumoLogicTarget : TargetWithLayout
    {
        /// <summary>
        /// The Sumo HTTP sender executor.
        /// </summary>
        private Timer flushBufferTimer;

        /// <summary>
        /// The messages queue.
        /// </summary>
        private volatile BufferWithEviction<string> messagesQueue;

        /// <summary>
        /// The task that flushes the buffer.
        /// </summary>
        private SumoLogicMessageSenderBufferFlushingTask flushBufferTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedSumoLogicTarget"/> class.
        /// </summary>
        public BufferedSumoLogicTarget() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedSumoLogicTarget"/> class.
        /// </summary>
        /// <param name="log">The log service.</param>
        /// <param name="httpMessageHandler">The HTTP message handler.</param>
        public BufferedSumoLogicTarget(ILog log, HttpMessageHandler httpMessageHandler)
        {
            this.SourceName = "Nlog-SumoObject-Buffered";
            this.ConnectionTimeout = 60000;
            this.RetryInterval = 10000;
            this.MessagesPerRequest = 100;
            this.MaxFlushInterval = 10000;
            this.FlushingAccuracy = 250;
            this.MaxQueueSizeBytes = 1000000;
            this.LogLog = log ?? new DummyLog();
            this.HttpMessageHandler = httpMessageHandler;
            this.AppendException = true;
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
        /// Gets or sets the send message retry interval, in milliseconds.
        /// </summary>
        public long RetryInterval
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
        /// Gets or sets how often the messages queue is checked for messages to send, in milliseconds.
        /// </summary>
        public long FlushingAccuracy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum interval between flushes, in milliseconds.
        /// </summary>
        public long MaxFlushInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how many messages need to be in the queue before flushing.
        /// </summary>
        public long MessagesPerRequest
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the messages queue capacity, in bytes.
        /// </summary>
        /// <remarks>Messages are dropped When the queue capacity is exceeded.</remarks>
        public long MaxQueueSizeBytes
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
        /// Gets or sets a value indicating whether the exception.ToString() should be automatically appended to the message being sent
        /// </summary>
        public bool AppendException
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log service.
        /// </summary>
        private ILog LogLog
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
        /// Activate Console Log.
        /// </summary>
        public void ActivateConsoleLog()
        {
            this.LogLog = new ConsoleLog();
        }

        /// <summary>
        /// Initialize the target based on the options set
        /// </summary>
        /// <remarks>
        /// This is part of the nLog is called when the Configurations of the LogManager are set.       
        /// If any of the configuration properties are modified then you must set anew the Configuration of the LogManager. 
        /// </remarks>
        protected override void InitializeTarget()
        {
            if (this.UseConsoleLog)
            {
                this.ActivateConsoleLog();
            }

            if (this.LogLog.IsDebugEnabled)
            {
                this.LogLog.Debug("Activating options");
            }

            // Initialize the messages queue
            if (this.messagesQueue == null)
            {
                this.messagesQueue = new BufferWithFifoEviction<string>(this.MaxQueueSizeBytes, new StringLengthCostAssigner(), this.LogLog);
            }
            else
            {
                this.messagesQueue.Capacity = this.MaxQueueSizeBytes;
            }

            // Initialize the sender
            if (this.SumoLogicMessageSender == null)
            {
                this.SumoLogicMessageSender = new SumoLogicMessageSender(this.HttpMessageHandler, this.LogLog);
            }

            this.SumoLogicMessageSender.RetryInterval = TimeSpan.FromMilliseconds(this.RetryInterval);
            this.SumoLogicMessageSender.ConnectionTimeout = TimeSpan.FromMilliseconds(this.ConnectionTimeout);
            this.SumoLogicMessageSender.Url = string.IsNullOrEmpty(this.Url) ? null : new Uri(this.Url);

            // Initialize flusher
            if (this.flushBufferTimer != null)
            {
                this.flushBufferTimer.Stop();
                this.flushBufferTimer.Dispose();
            }

            // Ensure any existing buffer is flushed
            if (this.flushBufferTask != null)
            {
                this.flushBufferTask.FlushAndSend();
            }

            this.flushBufferTask = new SumoLogicMessageSenderBufferFlushingTask(
                this.messagesQueue,
                this.SumoLogicMessageSender,
                TimeSpan.FromMilliseconds(this.MaxFlushInterval),
                this.MessagesPerRequest,
                this.SourceName,
                this.LogLog);

            this.flushBufferTimer = new Timer(TimeSpan.FromMilliseconds(this.FlushingAccuracy).TotalMilliseconds);
            this.flushBufferTimer.Elapsed += (s, e) => this.flushBufferTask.Run();

            this.flushBufferTimer.Start();
        }

        /// <summary>
        /// Performs the actual logging of events.
        /// </summary>
        /// <param name="logEvent">The event to append.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException("logEvent");
            }

            if (this.SumoLogicMessageSender == null || !this.SumoLogicMessageSender.CanSend)
            {
                if (this.LogLog.IsWarnEnabled)
                {
                    this.LogLog.Warn("Buffered target not initialized. Dropping log entry");
                }

                return;
            }

            var bodyBuilder = new StringBuilder();
            using (var textWriter = new StringWriter(bodyBuilder, CultureInfo.InvariantCulture))
            {
                textWriter.Write(Layout.Render(logEvent));
                if (logEvent.Exception != null && this.AppendException)
                {
                    textWriter.Write(logEvent.Exception.ToString());
                }

                textWriter.WriteLine();
            }

            this.messagesQueue.Add(bodyBuilder.ToString());
        }

        /// <summary>
        /// Is called when the target is closed.
        /// </summary>
        /// /// <remarks>
        /// Releases any resources allocated within the target such as file handles, network connections, etc.        
        /// </remarks>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            if (this.SumoLogicMessageSender != null)
            {
                this.SumoLogicMessageSender.Dispose();
                this.SumoLogicMessageSender = null;
            }

            if (this.flushBufferTimer != null)
            {
                this.flushBufferTimer.Stop();
                this.flushBufferTimer.Dispose();
                this.flushBufferTimer = null;
            }
        }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(global::NLog.Common.AsyncContinuation asyncContinuation)
        {
            try
            {
                var flushBufferTask = this.flushBufferTask;
                if (flushBufferTask != null)
                {
                    flushBufferTask.FlushAndSend();
                }

                asyncContinuation(null);
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }
    }
}
