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
    using global::Serilog.Core;
    using global::Serilog.Events;
    using global::Serilog.Formatting;
    using SumoLogic.Logging.Common.Log;
    using SumoLogic.Logging.Common.Sender;
    using SumoLogic.Logging.Serilog.Config;
    using SumoLogic.Logging.Serilog.Extensions;

    /// <summary>
    /// SumoLogic Serilog sink (without buffering).
    /// </summary>
    public sealed class SumoLogicSink : ILogEventSink, IDisposable
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
        /// SumoLogic event source describer.
        /// </summary>
        private readonly SumoLogicSource source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicSink"/> class.
        /// </summary>
        /// <param name="log">The log service.</param>
        /// <param name="httpMessageHandler">HTTP message handler.</param>
        /// <param name="connection">Connection configuration.</param>
        /// <param name="source">Event source describer.</param>
        /// <param name="formatter">Text formatter.</param>
        public SumoLogicSink(
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

            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            this.logService = log ?? new DummyLog();

            this.messageSender = new SumoLogicMessageSender(httpMessageHandler, this.logService)
            {
                Url = connection.Uri,
                ClientName = connection.ClientName,
                ConnectionTimeout = connection.ConnectionTimeout,
                RetryInterval = connection.RetryInterval,
            };
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logEvent"/> is <c>null</c>.</exception>
        public void Emit(LogEvent logEvent)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (!this.messageSender.CanTrySend)
            {
                this.logService.Warn("Sink not initialized. Dropping log entry");

                return;
            }

            this.messageSender.TrySend(
                    this.formatter.Format(logEvent),
                    this.source.SourceName,
                    this.source.SourceCategory,
                    this.source.SourceHost)
                .Wait();
        }

        /// <inheritdoc/>
        public void Dispose() => this.messageSender?.Dispose();
    }
}
