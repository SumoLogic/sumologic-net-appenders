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
    using global::Serilog;
    using global::Serilog.Core;
    using global::Serilog.Formatting.Display;
    using SumoLogic.Logging.Common.Sender;
    using SumoLogic.Logging.Serilog.Config;
    using Xunit;

    /// <summary>
    /// Sumo logic target test implementation.   
    /// </summary>
    [Collection("Serilog tests")]
    public class SumoLogicSinkTests : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private readonly MockHttpMessageHandler _messagesHandler;
               
        /// <summary>
        /// The SumoLogic sink.
        /// </summary>
        private readonly SumoLogicSink _sink;

        /// <summary>
        /// The Serilog logger.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicUnbufferedSink"/> class.
        /// </summary>
        public SumoLogicSinkTests()
        {
            _messagesHandler = new MockHttpMessageHandler();

            _sink = new SumoLogicSink(
                null,
                _messagesHandler,
                new SumoLogicConnection
                {
                    Uri = new Uri("http://www.fakeadress.com"),
                    ClientName = "SumoLogicSinkTest",
                },
                new SumoLogicSource
                {
                    SourceName = "SumoLogicSinkTest",
                    SourceCategory = "SumoLogicSinkSourceCategory",
                    SourceHost = "SumoLogicSinkSourceHost",
                },
                new MessageTemplateTextFormatter("{Level:u}: {Message}", CultureInfo.InvariantCulture));

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(_sink)
                .CreateLogger();
        }
       
        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void SingleMessageTest()
        {
            _logger.Information("This is a message");           
            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            Assert.Equal($"INFORMATION: This is a message{Environment.NewLine}", _messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void MultipleMessagesTest()
        {
            var numMessages = 50;
            for (var i = 0; i < numMessages / 5; i++)
            {               
                _logger.Debug(i.ToString());
                _logger.Information(i.ToString());
                _logger.Warning(i.ToString());
                _logger.Error(i.ToString());
                _logger.Fatal(i.ToString());
            }

            Assert.Equal(numMessages, _messagesHandler.ReceivedRequests.Count);
        }        

        /// <summary>
        /// Test do not logging on the trace level.
        /// </summary>
        [Fact]
        public void NoLogOnTheLevelTraceTest()
        {
            _logger.Verbose("This is message");
            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test logging multiple message and checking the request content.
        /// </summary>
        [Fact]
        public void CheckedRequestContentTest()
        {
            _logger.Debug("This is first message");
            _logger.Information("This is second message");
            _logger.Warning("This is third message");
            _logger.Error("This is fourth message");
            _logger.Fatal("This is fifh message");
            Assert.Equal($"FATAL: This is fifh message{Environment.NewLine}", _messagesHandler.ReceivedRequests[4].Content.ReadAsStringAsync().Result);
            Assert.Equal($"ERROR: This is fourth message{Environment.NewLine}", _messagesHandler.ReceivedRequests[3].Content.ReadAsStringAsync().Result);
            Assert.Equal($"WARNING: This is third message{Environment.NewLine}", _messagesHandler.ReceivedRequests[2].Content.ReadAsStringAsync().Result);
            Assert.Equal($"INFORMATION: This is second message{Environment.NewLine}", _messagesHandler.ReceivedRequests[1].Content.ReadAsStringAsync().Result);
            Assert.Equal($"DEBUG: This is first message{Environment.NewLine}", _messagesHandler.ReceivedRequests[0].Content.ReadAsStringAsync().Result);
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
                _sink.Dispose();
                _messagesHandler.Dispose();
            }
        }
    }
}
