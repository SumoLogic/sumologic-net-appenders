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

namespace SumoLogic.Logging.Log4Net.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using log4net;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using SumoLogic.Logging.Common.Sender;
    using SumoLogic.Logging.Common.Tests;
    using Xunit;

    /// <summary>
    /// <see cref="BufferedSumoLogicAppenderTest"/> class related tests.
    /// </summary>
    public class BufferedSumoLogicAppenderTest : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private MockHttpMessageHandler messagesHandler;

        /// <summary>
        /// The log4net log.
        /// </summary>
        private ILog log4netLog;

        /// <summary>
        /// The log4net logger.
        /// </summary>
        private Logger log4netLogger;

        /// <summary>
        /// The buffered SumoLogic appender.
        /// </summary>
        private BufferedSumoLogicAppender bufferedSumoLogicAppender;

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestSingleMessage()
        {
            SetUpLogger(1, 10000, 10);

            log4netLog.Info("This is a message");

            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
                Assert.Equal("This is a message" + Environment.NewLine + Environment.NewLine, messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
            });
        }

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestFlushMessage()
        {
            SetUpLogger(1, 10000, 10000);

            log4netLog.Info("This is a message");

            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);

            bool success = (log4netLogger.Repository as log4net.Appender.IFlushable)?.Flush(5000) ?? false;
            Assert.True(success);
            Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
            Assert.Equal("This is a message" + Environment.NewLine + Environment.NewLine, messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void TestMultipleMessages()
        {
            SetUpLogger(1, 10000, 10);

            int numMessages = 20;
            for (int i = 0; i < numMessages; i++)
            {
                log4netLog.Info("info " + i);
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            TestHelper.Eventually(() =>
            {
                Assert.Equal(numMessages, messagesHandler.ReceivedRequests.Count);
            });
        }

        /// <summary>
        /// Test batching of multiple messages based on messages per request setting.
        /// </summary>
        [Fact]
        public void TestBatchingBySize()
        {
            // Huge time window, ensure all messages get batched into one
            SetUpLogger(100, 10000, 10);

            int numMessages = 100;
            for (int i = 0; i < numMessages; i++)
            {
                log4netLog.Info("info " + i);
            }

            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
            });
        }

        /// <summary>
        /// Test batching of multiple messages based on max flush interval setting.
        /// </summary>
        [Fact]
        public void TestBatchingByWindow()
        {
            // Small time window, ensure all messages get batched by time
            SetUpLogger(10000, 500, 10);

            for (int i = 1; i <= 5; ++i)
            {
                log4netLog.Info("message" + i);
            }

            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
            });

            for (int i = 6; i <= 10; ++i)
            {
                log4netLog.Info("message" + i);
            }

            Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
            TestHelper.Eventually(() =>
            {
                Assert.Equal(2, messagesHandler.ReceivedRequests.Count);
            });
        }

        /// <summary>
        /// Test that setting <see cref="BufferedSumoLogicAppender.UseConsoleLog"/> to true 
        /// will cause the appender to log to the console.
        /// </summary>
        [Fact]
        public void TestConsoleLogging()
        {
            lock (ConsoleMutex.mutex)
            {
                var writer = new StringWriter();
                try
                {
                    Console.SetOut(writer);

                    SetUpLogger(10000, 500, 10);
                    log4netLog.Info("hello");
                    writer.Flush();
                    var consoleText = writer.GetStringBuilder().ToString();
                    Assert.True(!string.IsNullOrWhiteSpace(consoleText));
                }
                finally
                {
                    var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                    standardOutput.AutoFlush = true;
                    Console.SetOut(standardOutput);
                    writer.Close();
                }
            }
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
                log4netLogger.RemoveAllAppenders();
                messagesHandler.Dispose();
            }
        }

        
        [Fact]
        public void TestRequiresLayout()
        {
            SetUpLogger(1, 10000, 10);
            var oldLayout = bufferedSumoLogicAppender.Layout;
            var oldErrorHandler = bufferedSumoLogicAppender.ErrorHandler;
            try
            {
                bufferedSumoLogicAppender.ErrorHandler = new TestErrorHandler();
                bufferedSumoLogicAppender.Layout = null; // set to bogus null/missing value
                log4netLog.Info("oops"); // push through a message

                // nothing should be thrown, but an error should be generated
                var errHandler = (TestErrorHandler)bufferedSumoLogicAppender.ErrorHandler;
                Assert.Single(errHandler.Errors);
                Assert.Contains("No layout set", errHandler.Errors[0]);
            }
            finally
            {
                bufferedSumoLogicAppender.Layout = oldLayout;
                bufferedSumoLogicAppender.ErrorHandler = oldErrorHandler;
            }
        }

        /// <summary>
        /// Setups the logger with the <see cref="BufferedSumoLogicAppender"/> based on the given settings.
        /// </summary>
        /// <param name="messagesPerRequest">The maximum messages per request.</param>
        /// <param name="maxFlushInterval">The maximum flush interval, in milliseconds.</param>
        /// <param name="flushingAccuracy">The flushing accuracy, in milliseconds.</param>
        /// <param name="retryInterval">The retry interval, in milliseconds.</param>
        private void SetUpLogger(long messagesPerRequest, long maxFlushInterval, long flushingAccuracy, long retryInterval = 10000)
        {
            messagesHandler = new MockHttpMessageHandler();

            bufferedSumoLogicAppender = new BufferedSumoLogicAppender(null, messagesHandler);
            bufferedSumoLogicAppender.Url = "http://www.fakeadress.com";
            bufferedSumoLogicAppender.SourceName = "BufferedSumoLogicAppenderSourceName";
            bufferedSumoLogicAppender.SourceCategory = "BufferedSumoLogicAppenderSourceCategory";
            bufferedSumoLogicAppender.SourceHost = "BufferedSumoLogicAppenderSourceHost";
            bufferedSumoLogicAppender.MessagesPerRequest = messagesPerRequest;
            bufferedSumoLogicAppender.MaxFlushInterval = maxFlushInterval;
            bufferedSumoLogicAppender.FlushingAccuracy = flushingAccuracy;
            bufferedSumoLogicAppender.RetryInterval = retryInterval;
            bufferedSumoLogicAppender.Layout = new PatternLayout("%m%n");
            bufferedSumoLogicAppender.UseConsoleLog = true;
            bufferedSumoLogicAppender.ActivateOptions();

            log4netLog = LogManager.GetLogger(typeof(BufferedSumoLogicAppenderTest));
            log4netLogger = (Logger)log4netLog.Logger;
            log4netLogger.Additivity = false;
            log4netLogger.Level = Level.All;
            log4netLogger.RemoveAllAppenders();
            log4netLogger.AddAppender(bufferedSumoLogicAppender);
            log4netLogger.Repository.Configured = true;
        }
    }
}