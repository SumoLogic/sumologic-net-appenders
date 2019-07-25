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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SumoLogic.Logging.Common.Sender;
using SumoLogic.Logging.Common.Tests;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace SumoLogic.Logging.AspNetCore.Tests
{
    /// <summary>
    /// Sumo Logic Logger Provider test implementation.
    /// </summary>
    [Collection("Logger provider tests")]
    public class LoggerProviderTests: IDisposable
    {
        private MockHttpMessageHandler _messagesHandler;

        private LoggerProvider _provider;

        private ILogger _logger;

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestSingleMessage()
        {
            SetupLogger(1, 10000, 10);

            _logger.LogInformation("This is a message");

            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
                Assert.Equal($"This is a message{Environment.NewLine}", _messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
            });
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void TestMultipleMessages()
        {
            SetupLogger(1, 10000, 10);

            var numMessages = 20;
            for (var i = 0; i < numMessages; i++)
            {
                _logger.LogInformation(i.ToString());
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
            TestHelper.Eventually(() =>
            {
                Assert.Equal(numMessages, _messagesHandler.ReceivedRequests.Count);
            });
        }

        /// <summary>
        /// Test batching of multiple messages based on messages per request setting.
        /// </summary>
        [Fact]
        public void TestBatchingBySize()
        {
            // Huge time window, ensure all messages get batched into one
            SetupLogger(100, 10000, 10);

            var numMessages = 100;
            for (var i = 0; i < numMessages; i++)
            {
                _logger.LogInformation(i.ToString());
            }

            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            });
        }

        /// <summary>
        /// Test batching of multiple messages based on max flush interval setting.
        /// </summary>
        [Fact]
        public void TestBatchingByWindow()
        {
            // Small time window, ensure all messages get batched by time
            SetupLogger(10000, 500, 10);

            for (var i = 1; i <= 5; ++i)
            {
                _logger.LogInformation(i.ToString());
            }

            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            });

            for (var i = 6; i <= 10; ++i)
            {
                _logger.LogInformation(i.ToString());
            }

            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(2, _messagesHandler.ReceivedRequests.Count);
            });
        }

        [Fact]
        public void TestConfigIsReadFromConfigProvider()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("SumoLogic:Uri", "http://www.fakeadress2.com"),
                new KeyValuePair<string, string>("SumoLogic:FlushingAccuracy", "00:00:00.123"),
            }).Build();

            var loggerProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConfiguration(configuration).AddSumoLogic())
                .BuildServiceProvider()
                .GetRequiredService<ILoggerProvider>();

            var sumoLogicLoggerProvider = Assert.IsType<LoggerProvider>(loggerProvider);
            Assert.Equal("http://www.fakeadress2.com/", sumoLogicLoggerProvider.LoggerOptions.Uri);
            Assert.Equal(123, sumoLogicLoggerProvider.LoggerOptions.FlushingAccuracy.Milliseconds);
        }

        [Fact]
        public void TestConfigUpdatesWithConfigProvider()
        {
            var monitor = new TestOptionsMonitor(new LoggerOptions() { Uri = "http://www.fakeadress2.com"});
            var loggerProvider = new LoggerProvider(monitor);

            Assert.Equal("http://www.fakeadress2.com/", loggerProvider.LoggerOptions.Uri);

            monitor.Set(new LoggerOptions() { Uri = "https://www.fakeadress2.com" });

            Assert.Equal("https://www.fakeadress2.com/", loggerProvider.LoggerOptions.Uri);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _provider?.Dispose();
                _messagesHandler?.Dispose();
            }
        }

        private void SetupUnbufferedLogger()
        {
            _messagesHandler = new MockHttpMessageHandler();

            _provider = new LoggerProvider(new LoggerOptions()
            {
                Uri = "http://www.fakeadress.com",
                SourceName = "LoggerProviderTestSourceName",
                SourceCategory = "LoggerProviderTestSourceCategory",
                SourceHost = "LoggerProviderTestSourceHost",
                IsBuffered = false,
                HttpMessageHandler = _messagesHandler,
            });

            _logger = _provider.CreateLogger("OverriddenCategory");
        }

        private void SetupLogger(long messagesPerRequest, long maxFlushInterval, long flushingAccuracy, long retryInterval = 10000)
        {
            _messagesHandler = new MockHttpMessageHandler();

            _provider = new LoggerProvider(new LoggerOptions()
            {
                Uri = "http://www.fakeadress.com",
                SourceName = "LoggerProviderTestSourceName",
                SourceCategory = "LoggerProviderTestSourceCategory",
                SourceHost = "LoggerProviderTestSourceHost",
                MessagesPerRequest = messagesPerRequest,
                MaxFlushInterval = TimeSpan.FromMilliseconds(maxFlushInterval),
                FlushingAccuracy = TimeSpan.FromMilliseconds(flushingAccuracy),
                RetryInterval = TimeSpan.FromMilliseconds(retryInterval),
                IsBuffered = true,
                HttpMessageHandler = _messagesHandler,
                DebuggingLogger = new Microsoft.Extensions.Logging.Console.ConsoleLogger("debugging", (a, b) => true, true)
        });

            _logger = _provider.CreateLogger("OverriddenCategory");
        }

        // Taken from https://github.com/aspnet/Extensions/blob/2.2.0/src/Logging/test/ConsoleLoggerTest.cs#L1081
        public class TestOptionsMonitor : IOptionsMonitor<LoggerOptions>
        {
            private LoggerOptions _options;
            private event Action<LoggerOptions, string> _onChange;

            public TestOptionsMonitor(LoggerOptions options)
            {
                _options = options;
            }

            public LoggerOptions Get(string name) => _options;

            public IDisposable OnChange(Action<LoggerOptions, string> listener)
            {
                _onChange += listener;
                return null;
            }

            public LoggerOptions CurrentValue => _options;

            public void Set(LoggerOptions options)
            {
                _options = options;
                _onChange?.Invoke(options, "");
            }
        }
    }
}
