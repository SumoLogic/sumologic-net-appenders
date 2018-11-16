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
namespace SumoLogic.Logging.Serilog.Tests
{
    using System;
    using System.Globalization;
    using System.Threading;
    using global::Serilog;
    using global::Serilog.Core;
    using global::Serilog.Formatting.Display;
    using SumoLogic.Logging.Common.Sender;
    using SumoLogic.Logging.Serilog.Config;
    using Xunit;

    /// <summary>
    /// Buffered Sumo Logic target test implementation.
    /// </summary>
    [Collection("Serilog tests")]
    public class BufferedSumoLogicSinkTests : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private MockHttpMessageHandler _messagesHandler;

        /// <summary>
        /// The buffered SumoLogic sink.
        /// </summary>
        private BufferedSumoLogicSink sink;

        /// <summary>
        /// The Serilog logger.
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestSingleMessage()
        {
            SetUpLogger(1, 10000, 10);

            logger.Information("This is a message");

            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            Assert.Equal($"INFORMATION: This is a message{Environment.NewLine}", _messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void TestMultipleMessages()
        {
            SetUpLogger(1, 10000, 10);

            var numMessages = 20;
            for (var i = 0; i < numMessages; i++)
            {
                logger.Information(i.ToString());
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(numMessages, _messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test batching of multiple messages based on messages per request setting.
        /// </summary>
        [Fact]
        public void TestBatchingBySize()
        {
            // Huge time window, ensure all messages get batched into one
            SetUpLogger(100, 10000, 10);

            var numMessages = 100;
            for (var i = 0; i < numMessages; i++)
            {
                logger.Information(i.ToString());
            }

            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(2000));
            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test batching of multiple messages based on max flush interval setting.
        /// </summary>
        [Fact]
        public void TestBatchingByWindow()
        {
            // Small time window, ensure all messages get batched by time
            SetUpLogger(10000, 500, 10);

            for (var i = 1; i <= 5; ++i)
            {
                logger.Information(i.ToString());
            }

            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(520));
            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);

            for (var i = 6; i <= 10; ++i)
            {
                logger.Information(i.ToString());
            }

            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(520));
            Assert.Equal(2, _messagesHandler.ReceivedRequests.Count);
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
                sink.Dispose();      
                _messagesHandler.Dispose();
            }
        }

        /// <summary>
        /// Setups the logger with the <see cref="SumoLogicSink"/> based on the given settings.
        /// </summary>
        /// <param name="messagesPerRequest">The maximum messages per request.</param>
        /// <param name="maxFlushInterval">The maximum flush interval, in milliseconds.</param>
        /// <param name="flushingAccuracy">The flushing accuracy, in milliseconds.</param>
        /// <param name="retryInterval">The retry interval, in milliseconds.</param>
        private void SetUpLogger(long messagesPerRequest, long maxFlushInterval, long flushingAccuracy, long retryInterval = 10000)
        {
            _messagesHandler = new MockHttpMessageHandler();

            sink = new BufferedSumoLogicSink(
                null,
                _messagesHandler,
                new SumoLogicConnection
                {
                    Uri = new Uri("http://www.fakeadress.com"),
                    ClientName = "BufferedSumoLogicSinkTest",
                    MessagesPerRequest = messagesPerRequest,
                    MaxFlushInterval = TimeSpan.FromMilliseconds(maxFlushInterval),
                    FlushingAccuracy = TimeSpan.FromMilliseconds(flushingAccuracy),
                    RetryInterval = TimeSpan.FromMilliseconds(retryInterval),
                },
                new SumoLogicSource
                {
                    SourceName = "BufferedSumoLogicSinkTest",
                    SourceCategory = "BufferedSumoLogicSinkSourceCategory",
                    SourceHost = "BufferedSumoLogicSinkSourceHost",
                },
                new MessageTemplateTextFormatter("{Level:u}: {Message}", CultureInfo.InvariantCulture));

            logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Sink(sink)
                .CreateLogger();
        }
    }
}
