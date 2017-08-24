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
    using global::NLog;
    using global::NLog.Config;
    using global::NLog.Targets;
    using SumoLogic.Logging.Common.Sender;   
    using Xunit;

    /// <summary>
    /// Sumo logic target test implementation.   
    /// </summary>
    [Collection("NLog tests")]
    public class SumoLogicTargetTest : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private MockHttpMessageHandler messagesHandler;
               
        /// <summary>
        /// The SumoLogic appender.
        /// </summary>
        private SumoLogicTarget sumoLogicTarget;
                
        /// <summary>
        /// The Nlog logger.
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicTargetTest"/> class.
        /// </summary>
        public SumoLogicTargetTest()
        {
            this.messagesHandler = new MockHttpMessageHandler();
            this.sumoLogicTarget = new SumoLogicTarget(null, this.messagesHandler);           
            this.sumoLogicTarget.Url = "http://www.fakeadress.com";
            this.sumoLogicTarget.Layout = @"${level:upperCase=true}: ${message}";
            this.sumoLogicTarget.SourceName = "SumoLogicTargetTest";
            this.sumoLogicTarget.SourceCategory = "SumoLogicTargetSourceCategory";
            this.sumoLogicTarget.SourceHost = "SumoLogicTargetSourceHost";
            this.sumoLogicTarget.SourceHost = "FakeTargetSourceHost";
            this.sumoLogicTarget.Name = "SumoLogicTargetTest";           
            if (LogManager.Configuration == null)
            {
                LogManager.Configuration = new LoggingConfiguration();
            }
           
            LogManager.Configuration.AddTarget(this.sumoLogicTarget.SourceName, this.sumoLogicTarget);
            LoggingRule rule = new LoggingRule("SumoLogicTargetTest", LogLevel.Debug, this.sumoLogicTarget);           
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.Configuration.Reload();
            LogManager.ReconfigExistingLoggers();
            this.logger = LogManager.GetLogger("SumoLogicTargetTest");         
         }
       
        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void SingleMessageTest()
        {
            this.logger.Info("This is a message");           
            Assert.Equal(1, this.messagesHandler.ReceivedRequests.Count);
            Assert.Equal("INFO: This is a message\r\n", this.messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void MultipleMessagesTest()
        {
            int numMessages = 50;
            for (int i = 0; i < numMessages / 5; i++)
            {               
                this.logger.Debug(i);
                this.logger.Info(i);
                this.logger.Warn(i);
                this.logger.Error(i);
                this.logger.Fatal(i);
            }

            Assert.Equal(numMessages, this.messagesHandler.ReceivedRequests.Count);
        }        

        /// <summary>
        /// Test do not logging on the trace level.
        /// </summary>
        [Fact]
        public void NoLogOnTheLevelTraceTest()
        {
            this.logger.Trace("This is message");
            Assert.Equal(0, this.messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test logging multiple message and checking the request content.
        /// </summary>
        [Fact]
        public void CheckedRequestContentTest()
        {
            this.logger.Debug("This is first message");
            this.logger.Info("This is second message");
            this.logger.Warn("This is third message");
            this.logger.Error("This is fourth message");
            this.logger.Fatal("This is fifh message");
            Assert.Equal("FATAL: This is fifh message\r\n", this.messagesHandler.ReceivedRequests[4].Content.ReadAsStringAsync().Result);
            Assert.Equal("ERROR: This is fourth message\r\n", this.messagesHandler.ReceivedRequests[3].Content.ReadAsStringAsync().Result);
            Assert.Equal("WARN: This is third message\r\n", this.messagesHandler.ReceivedRequests[2].Content.ReadAsStringAsync().Result);
            Assert.Equal("INFO: This is second message\r\n", this.messagesHandler.ReceivedRequests[1].Content.ReadAsStringAsync().Result);
            Assert.Equal("DEBUG: This is first message\r\n", this.messagesHandler.ReceivedRequests[0].Content.ReadAsStringAsync().Result);
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
                this.sumoLogicTarget.Dispose();
                this.messagesHandler.Dispose();
            }
        }
    }
}
