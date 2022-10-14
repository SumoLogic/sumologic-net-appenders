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
namespace SumoLogic.Logging.Common.Tests.Http
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using SumoLogic.Logging.Common.Sender;
    using Xunit;   

    /// <summary>
    /// <see cref="SumoLogicMessageSender"/> class related tests.
    /// </summary>
    public class SumoLogicMessageSenderTest : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private MockHttpMessageHandler messagesHandler;

        /// <summary>
        /// The SumoLogic HTTP messages sender.
        /// </summary>
        private SumoLogicMessageSender sumoLogicMessageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicMessageSenderTest"/> class.
        /// </summary>
        public SumoLogicMessageSenderTest()
        {
            messagesHandler = new MockHttpMessageHandler();
            sumoLogicMessageSender = new SumoLogicMessageSender(messagesHandler, null, "sumo-test");
            sumoLogicMessageSender.Url = new Uri("http://www.fakeadress.com");
            sumoLogicMessageSender.RetryInterval = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// This test if the http client is called.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void HttpClientCallWith200ResponseTest()
        {
            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);
            sumoLogicMessageSender.Send("body", "name", "category", "host").Wait();
            Assert.Equal(1, messagesHandler.ReceivedRequests.Count);
        }      

        /// <summary>
        /// This test if the request header is correct.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void RequestHeaderTest()
        {
            sumoLogicMessageSender.Send("body", "name", "category", "host").Wait();
            Assert.Equal("name", messagesHandler.LastReceivedRequest.Content.Headers.GetValues("X-Sumo-Name").First<string>());
            Assert.Equal("category", messagesHandler.LastReceivedRequest.Content.Headers.GetValues("X-Sumo-Category").First<string>());
            Assert.Equal("host", messagesHandler.LastReceivedRequest.Content.Headers.GetValues("X-Sumo-Host").First<string>());
            Assert.Equal("sumo-test", messagesHandler.LastReceivedRequest.Content.Headers.GetValues("X-Sumo-Client").First<string>());
        }

        /// <summary>
        /// This test if the request content is correct.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void RequestContentTest()
        {
            string body = "ContentBody";
            sumoLogicMessageSender.Send(body, "name", "category", "host").Wait();
            var contentInString = messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result;
            Assert.Equal(body, contentInString);
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
        /// This test if the http client is called and this status code is 'OK'.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void HttpResponseCodeTest()
        {
            Assert.True(sumoLogicMessageSender.CanSend);
            Assert.True(sumoLogicMessageSender.CanTrySend);
            sumoLogicMessageSender.Send("body", "name", "category", "host").Wait();
            Assert.Equal(HttpStatusCode.OK, messagesHandler.CurrentResponse.StatusCode);
        }

        /// <summary>
        /// This test if the http client is called when the url is null.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void HttpResponseCodeWhenUrlNullTest()
        {
            sumoLogicMessageSender.Url = null;
            Assert.False(sumoLogicMessageSender.CanSend);
            Assert.False(sumoLogicMessageSender.CanTrySend);
            sumoLogicMessageSender.Send("body", "name", "category", "host").Wait();
            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);          
        }

        /// <summary>
        /// This test if the CurrentResponse is Unavailable for few seconds.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void CurrentResponseIsUnavailableForFewSecondsTest()
        {
            int requestBeforeSuccess = 0;

            messagesHandler.CurrentResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            sumoLogicMessageSender.RetryInterval = TimeSpan.FromSeconds(1);
            Task changeResponseTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(sumoLogicMessageSender.RetryInterval + TimeSpan.FromSeconds(2));
                requestBeforeSuccess = messagesHandler.ReceivedRequests.Count;
                messagesHandler.CurrentResponse = new HttpResponseMessage(HttpStatusCode.OK);
            });
            sumoLogicMessageSender.Send("body", "name", "category", "host").Wait();
            changeResponseTask.GetAwaiter().GetResult();
            Assert.True(requestBeforeSuccess < messagesHandler.ReceivedRequests.Count);
            Assert.Equal(HttpStatusCode.OK, messagesHandler.CurrentResponse.StatusCode);
        }     

        /// <summary>
        /// This test if the http client is called after dispose.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void HttpResponseCodeWhenDisposedTest()
        {
            Dispose();
            Assert.False(sumoLogicMessageSender.CanSend);
            Assert.False(sumoLogicMessageSender.CanTrySend);
            sumoLogicMessageSender.Send("body", "name", "category", "host").Wait();
            Assert.Equal(0, messagesHandler.ReceivedRequests.Count);
        }
                
        /// <summary>
        /// Releases the unmanaged resources used and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                messagesHandler.Dispose();
                sumoLogicMessageSender.Dispose();
            }
        }       
    }
}
