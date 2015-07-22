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
namespace SumoLogic.Logging.NLog.Tests
{
    using System;
    using System.Threading;
    using global::NLog;
    using global::NLog.Config;
    using SumoLogic.Logging.Common.Sender;   
    using Xunit;

    /// <summary>
    /// Buffered Sumo Logic target test implementation.
    /// </summary>
    public class BufferedSumoLogicTargetTest : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private MockHttpMessageHandler messagesHandler;

        /// <summary>
        /// The log4net logger.
        /// </summary>
        private Logger logger;        

        /// <summary>
        /// The buffered SumoLogic target.
        /// </summary>
        private BufferedSumoLogicTarget bufferedSumoLogicTarget;
                
        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestSingleMessage()
        {
            this.SetUpLogger(1, 10000, 10);

            this.logger.Info("This is a message");

            Assert.Equal(0, this.messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            Assert.Equal(1, this.messagesHandler.ReceivedRequests.Count);
            Assert.Equal("INFO: This is a message\r\n", this.messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void TestMultipleMessages()
        {
            this.SetUpLogger(1, 10000, 10);

            int numMessages = 20;
            for (int i = 0; i < numMessages; i++)
            {
                this.logger.Info(i);
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(numMessages, this.messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test batching of multiple messages based on messages per request setting.
        /// </summary>
        [Fact]
        public void TestBatchingBySize()
        {
            // Huge time window, ensure all messages get batched into one
            this.SetUpLogger(100, 10000, 10);

            int numMessages = 100;
            for (int i = 0; i < numMessages; i++)
            {
                this.logger.Info(i);
            }

            Assert.Equal(0, this.messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(2000));
            Assert.Equal(1, this.messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test batching of multiple messages based on max flush interval setting.
        /// </summary>
        [Fact]
        public void TestBatchingByWindow()
        {
            // Small time window, ensure all messages get batched by time
            this.SetUpLogger(10000, 500, 10);

            for (int i = 1; i <= 5; ++i)
            {
                this.logger.Info(i);
            }

            Assert.Equal(0, this.messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(520));
            Assert.Equal(1, this.messagesHandler.ReceivedRequests.Count);

            for (int i = 6; i <= 10; ++i)
            {
                this.logger.Info(i);
            }

            Assert.Equal(1, this.messagesHandler.ReceivedRequests.Count);
            Thread.Sleep(TimeSpan.FromMilliseconds(520));
            Assert.Equal(2, this.messagesHandler.ReceivedRequests.Count);
        }
                
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
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
                this.bufferedSumoLogicTarget.Dispose();      
                this.messagesHandler.Dispose();
            }
        }

        /// <summary>
        /// Setups the logger with the <see cref="BufferedSumoLogicTarget"/> based on the given settings.
        /// </summary>
        /// <param name="messagesPerRequest">The maximum messages per request.</param>
        /// <param name="maxFlushInterval">The maximum flush interval, in milliseconds.</param>
        /// <param name="flushingAccuracy">The flushing accuracy, in milliseconds.</param>
        /// <param name="retryInterval">The retry interval, in milliseconds.</param>
        private void SetUpLogger(long messagesPerRequest, long maxFlushInterval, long flushingAccuracy, long retryInterval = 10000)
        {
            this.messagesHandler = new MockHttpMessageHandler();
            this.bufferedSumoLogicTarget = new BufferedSumoLogicTarget(null, this.messagesHandler);
            this.bufferedSumoLogicTarget.Url = "http://www.fakeadress.com";
            this.bufferedSumoLogicTarget.Layout = @"${level:upperCase=true}: ${message}";
            this.bufferedSumoLogicTarget.SourceName = "BufferedSumoLogicTargetTest";
            this.bufferedSumoLogicTarget.SourceName = "BufferedSumoLogicTargetTest";
            this.bufferedSumoLogicTarget.MessagesPerRequest = messagesPerRequest;
            this.bufferedSumoLogicTarget.MaxFlushInterval = maxFlushInterval;
            this.bufferedSumoLogicTarget.FlushingAccuracy = flushingAccuracy;
            this.bufferedSumoLogicTarget.RetryInterval = retryInterval;
            if (LogManager.Configuration == null)
            {
                LogManager.Configuration = new LoggingConfiguration();
            }

            LogManager.Configuration.AddTarget(this.bufferedSumoLogicTarget.SourceName, this.bufferedSumoLogicTarget);
            LoggingRule rule = new LoggingRule("BufferedSumoLogicTargetTest", LogLevel.Info, this.bufferedSumoLogicTarget);
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.Configuration.Reload();
            LogManager.ReconfigExistingLoggers();
            this.logger = LogManager.GetLogger("BufferedSumoLogicTargetTest");  
        }
    }
}
