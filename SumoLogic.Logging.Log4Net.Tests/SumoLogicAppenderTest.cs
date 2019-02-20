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
    using log4net;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using SumoLogic.Logging.Common.Sender;
    using SumoLogic.Logging.Common.Tests;
    using Xunit;

    /// <summary>
    /// <see cref="SumoLogicAppender"/> class related tests.
    /// </summary>
    public class SumoLogicAppenderTest : IDisposable
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
        /// The SumoLogic appender.
        /// </summary>
        private SumoLogicAppender sumoLogicAppender;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicAppenderTest"/> class.
        /// </summary>
        public SumoLogicAppenderTest()
        {
            messagesHandler = new MockHttpMessageHandler();

            sumoLogicAppender = new SumoLogicAppender(null, messagesHandler);
            sumoLogicAppender.Url = "http://www.fakeadress.com";
            sumoLogicAppender.SourceName = "SumoLogicAppenderSourceName";
            sumoLogicAppender.SourceCategory = "SumoLogicAppenderSourceCategory";
            sumoLogicAppender.SourceHost = "SumoLogicAppenderSourceHost";
            sumoLogicAppender.Layout = new PatternLayout("-- %m%n");
            sumoLogicAppender.UseConsoleLog = true;
            sumoLogicAppender.ActivateOptions();

            log4netLog = LogManager.GetLogger(typeof(SumoLogicAppenderTest));
            log4netLogger = (Logger)log4netLog.Logger;
            log4netLogger.Additivity = false;
            log4netLogger.Level = Level.All;
            log4netLogger.RemoveAllAppenders();
            log4netLogger.AddAppender(sumoLogicAppender);
            log4netLogger.Repository.Configured = true;
        }

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestSingleMessage()
        {
            log4netLog.Info("This is a message");

            Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
            Assert.Equal("-- This is a message" + Environment.NewLine + Environment.NewLine, messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void TestMultipleMessages()
        {
            int numMessages = 20;
            for (int i = 0; i < numMessages / 2; i++)
            {
                log4netLog.Info("info " + i);
                log4netLog.Error("error " + i);
            }
            TestHelper.Eventually(() =>
            {
                Assert.Equal(numMessages, messagesHandler.ReceivedRequests.Count);
            });
        }

        /// <summary>
        /// Test that setting <see cref="SumoLogicAppender.UseConsoleLog"/> to true 
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
        /// Tests that an error is generated automatically if no format is provided.
        /// </summary>
        [Fact]
        public void TestRequiresLayout()
        {
            var oldLayout = sumoLogicAppender.Layout;
            var oldErrorHandler = sumoLogicAppender.ErrorHandler;
            try
            {
                sumoLogicAppender.ErrorHandler = new TestErrorHandler();
                sumoLogicAppender.Layout = null; // set to bogus null/missing value
                log4netLog.Info("oops"); // push through a message

                // nothing should be thrown, but an error should be generated
                var errHandler = (TestErrorHandler)sumoLogicAppender.ErrorHandler;
                Assert.Single(errHandler.Errors);
                Assert.Contains("No layout set", errHandler.Errors[0]);
            }
            finally
            {
                sumoLogicAppender.Layout = oldLayout;
                sumoLogicAppender.ErrorHandler = oldErrorHandler;
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
    }
}