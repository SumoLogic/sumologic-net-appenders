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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SumoLogic.Logging.Common.Log;
using SumoLogic.Logging.Common.Queue;
using SumoLogic.Logging.Common.Sender;
using System;
using System.Threading;

namespace SumoLogic.Logging.AspNetCore
{
    /// <summary>
    /// Sumo Logic Logger Provider implementation
    /// </summary>
    [ProviderAlias("SumoLogic")]
    public class LoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        public LoggerOptions LoggerOptions { get; private set; }

        private ILog DebuggingLogger { get; set; }

        private SumoLogicMessageSender sumoLogicMessageSender = null;

        private Timer flushBufferTimer = null;

        private SumoLogicMessageSenderBufferFlushingTask flushBufferTask = null;

        private volatile BufferWithEviction<string> messagesQueue = null;

        private readonly ExternalScopeProviderEnforcer externalScopeProviderEnforcer = new ExternalScopeProviderEnforcer();

        public LoggerProvider(IOptionsMonitor<LoggerOptions> options)
        {
            ReConfig(options.CurrentValue);
        }

        public LoggerProvider(LoggerOptions options)
        {
            ReConfig(options);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this, categoryName);
        }

        public void Dispose()
        {
            try
            {
                flushBufferTimer?.Dispose();
                flushBufferTask?.FlushAndSend().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                DebuggingLogger?.Warn($"SumoLogicProvider disposed with error. {ex.GetType()}: {ex.Message}");
            }
            finally
            {
                flushBufferTimer = null;
                flushBufferTask = null;
                sumoLogicMessageSender?.Dispose();
            }
        }

        /// <summary>
        /// Write a single message line to Sumo Logic
        /// </summary>
        /// <param name="message">the message line to be sent</param>
        /// <param name="ex">Exception if any</param>
        /// <param name="categoryName">Logger Category Name</param>
        /// <param name="logLevel">Log message level.</param>
        public void WriteLine(string message, Exception ex, string categoryName, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(message) && ex == null)
            {
                return;
            }

            if (sumoLogicMessageSender == null || !sumoLogicMessageSender.CanTrySend)
            {
                DebuggingLogger?.Warn("Sender is not initialized. Dropping log entry");
                return;
            }

            message = message ?? string.Empty;

            var fullMessage = LoggerOptions.MessageFormatterFunc(message.TrimEnd(Environment.NewLine.ToCharArray()), ex, categoryName,
                logLevel, externalScopeProviderEnforcer.GetScopeProperties());
            var line = string.Concat(fullMessage, Environment.NewLine);

            if (LoggerOptions.IsBuffered)
            {
                messagesQueue.Add(line);
            }
            else
            {
                WriteLineToSumo(line);
            }
        }

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            externalScopeProviderEnforcer.SetExternalScopeProvider(scopeProvider);
        }

        public IDisposable BeginScope(object state)
        {
            if (LoggerOptions.EnableScopes)
            {
                return externalScopeProviderEnforcer.BeginScope(state);
            }

            return null;
        }

        private void ReConfig(LoggerOptions options)
        {
            Dispose();
            if (string.IsNullOrWhiteSpace(options.Uri))
            {
                throw new ArgumentOutOfRangeException(nameof(options.Uri), $"{nameof(options.Uri)} cannot be empty.");
            }
            DebuggingLogger = options.DebuggingLogger != null ? new LoggerLog(options.DebuggingLogger) : null;
            InitSender(options);
            if (options.IsBuffered)
            {
                InitBuffer(options);
            }
            LoggerOptions = options;
        }

        private void InitSender(LoggerOptions options)
        {
            DebuggingLogger?.Debug("InitSender");
            sumoLogicMessageSender = new SumoLogicMessageSender(options.HttpMessageHandler, DebuggingLogger, "asp.net-core-logger")
            {
                Url = new Uri(options.Uri),
                ConnectionTimeout = options.ConnectionTimeout,
                RetryInterval = options.RetryInterval
            };
            DebuggingLogger?.Debug("InitSender::Completed");
        }

        private void InitBuffer(LoggerOptions options)
        {
            DebuggingLogger?.Debug("InitBuffer");

            messagesQueue = new BufferWithFifoEviction<string>(
                options.MaxQueueSizeBytes,
                new StringLengthCostAssigner(),
                DebuggingLogger);

            flushBufferTask = new SumoLogicMessageSenderBufferFlushingTask(
                messagesQueue,
                sumoLogicMessageSender,
                options.MaxFlushInterval,
                options.MessagesPerRequest,
                options.SourceName,
                options.SourceCategory,
                options.SourceHost,
                DebuggingLogger);

            flushBufferTimer = new Timer(
                callback: _ => flushBufferTask.Run(), // No task await to avoid unhandled exception
                state: null,
                dueTime: TimeSpan.FromMilliseconds(0),
                period: options.FlushingAccuracy);

            DebuggingLogger?.Debug("InitBuffer::Completed");
        }

        private void WriteLineToSumo(String body)
        {
            sumoLogicMessageSender.TrySend(
                body, 
                LoggerOptions.SourceName,
                LoggerOptions.SourceCategory,
                LoggerOptions.SourceHost)
                .GetAwaiter()
                .GetResult();
        }
    }
}
