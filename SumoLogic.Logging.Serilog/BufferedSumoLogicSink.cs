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
namespace SumoLogic.Logging.Serilog
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using global::Serilog.Core;
    using global::Serilog.Events;
    using global::Serilog.Formatting;
    using SumoLogic.Logging.Common.Log;
    using SumoLogic.Logging.Common.Queue;
    using SumoLogic.Logging.Common.Sender;
    using SumoLogic.Logging.Serilog.Config;
    using SumoLogic.Logging.Serilog.Extensions;

    /// <summary>
    /// Buffered SumoLogic Serilog sink.
    /// </summary>
    public sealed class BufferedSumoLogicSink : ILogEventSink, IDisposable
    {
        /// <summary>
        /// Log service.
        /// </summary>
        private readonly ILog logService;

        /// <summary>
        /// Event log text formatter.
        /// </summary>
        private readonly ITextFormatter formatter;

        /// <summary>
        /// SumoLogic message sender.
        /// </summary>
        private readonly SumoLogicMessageSender messageSender;

        /// <summary>
        /// Message queue.
        /// </summary>
        private readonly BufferWithEviction<string> messageQueue;

        /// <summary>
        /// The Sumo HTTP sender executor.
        /// </summary>
        private readonly Timer flushBufferTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedSumoLogicSink"/> class.
        /// </summary>
        /// <param name="log">The log service.</param>
        /// <param name="httpMessageHandler">HTTP message handler.</param>
        /// <param name="connection">Connection configuration.</param>
        /// <param name="source">Event source describer.</param>
        /// <param name="formatter">Text formatter.</param>
        public BufferedSumoLogicSink(
            ILog log,
            HttpMessageHandler httpMessageHandler,
            SumoLogicConnection connection,
            SumoLogicSource source,
            ITextFormatter formatter)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            this.logService = log ?? new DummyLog();

            this.messageSender = new SumoLogicMessageSender(httpMessageHandler, this.logService)
            {
                Url = connection.Uri,
                ClientName = connection.ClientName,
                ConnectionTimeout = connection.ConnectionTimeout,
                RetryInterval = connection.RetryInterval,
            };

            this.messageQueue = new BufferWithFifoEviction<string>(
                connection.MaxQueueSizeBytes,
                new StringLengthCostAssigner(),
                this.logService);

            SumoLogicMessageSenderBufferFlushingTask flushBufferTask = new SumoLogicMessageSenderBufferFlushingTask(
                this.messageQueue,
                this.messageSender,
                connection.MaxFlushInterval,
                connection.MessagesPerRequest,
                source.SourceName,
                source.SourceCategory,
                source.SourceHost,
                this.logService);

            this.flushBufferTimer = new Timer(
                async _ => await flushBufferTask.Run(),
                null,
                TimeSpan.FromMilliseconds(0),
                connection.FlushingAccuracy);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logEvent"/> is <c>null</c>.</exception>
        public void Emit(LogEvent logEvent)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (!this.messageSender.CanSend)
            {
                this.logService.Warn("Sink not initialized. Dropping log entry");

                return;
            }

            this.messageQueue.Add(this.formatter.Format(logEvent));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.messageSender?.Dispose();
            this.flushBufferTimer?.Dispose();
        }
    }
}
