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
namespace SumoLogic.Logging.Common.Sender
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Fake http message handler for use in tests.
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpMessageHandler" /> class.
        /// </summary>
        public MockHttpMessageHandler()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpMessageHandler" /> class.
        /// </summary>
        /// <param name="currentResponse">The fake response object.</param>
        public MockHttpMessageHandler(HttpResponseMessage currentResponse)
        {
            ReceivedRequests = new List<HttpRequestMessage>();
            CurrentResponse = currentResponse ?? new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets or sets the current (mocked) server response.
        /// </summary>
        public HttpResponseMessage CurrentResponse
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the received requests.
        /// </summary>
        public IList<HttpRequestMessage> ReceivedRequests
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last received request.
        /// </summary>
        public HttpRequestMessage LastReceivedRequest
        {
            get { return ReceivedRequests.LastOrDefault(); }
        }

        /// <summary>
        /// Releases the unmanaged resources used and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (var request in ReceivedRequests)
                {
                    request.Dispose();
                }

                ReceivedRequests.Clear();
            }
        }

        /// <summary>
        /// Simulates a Post async task.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A fake response.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var responseTask = new TaskCompletionSource<HttpResponseMessage>();
            responseTask.SetResult(CurrentResponse);
            ReceivedRequests.Add(CloneRequest(request));
            return responseTask.Task;
        }

        /// <summary>
        /// Clones the given HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>The cloned HTTP request message.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object references retained by callers")]
        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var requestClone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

            foreach (var requestHeader in request.Headers)
            {
                requestClone.Headers.TryAddWithoutValidation(requestHeader.Key, requestHeader.Value);
            }

            foreach (var requestProperty in request.Properties)
            {
                requestClone.Properties.Add(requestProperty.Key, requestProperty.Value);
            }

            if (request.Content != null)
            {
                var requestContentStream = new MemoryStream();
                request.Content.CopyToAsync(requestContentStream).GetAwaiter().GetResult();
                requestContentStream.Position = 0;

                requestClone.Content = new StreamContent(requestContentStream);
                foreach (var requestContentHeader in request.Content.Headers)
                {
                    requestClone.Content.Headers.TryAddWithoutValidation(requestContentHeader.Key, requestContentHeader.Value);
                }
            }

            return requestClone;
        }
    }
}
