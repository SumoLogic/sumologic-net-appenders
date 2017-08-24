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
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using SumoLogic.Logging.Common.Log;

    /// <summary>
    /// A class for sending messages to SumoLogic server.
    /// </summary>
    public class SumoLogicMessageSender : IDisposable
    {
        /// <summary>
        /// text/plain media type
        /// </summary>
        private const string TextPlainMediaType = "text/plain";
        private const string SUMO_SOURCE_NAME_HEADER = "X-Sumo-Name";
        private const string SUMO_SOURCE_CATEGORY_HEADER = "X-Sumo-Cateory";
        private const string SUMO_SOURCE_HOST_HEADER = "X-Sumo-Host";

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicMessageSender" /> class.
        /// </summary>
        /// <param name="httpMessageHandler">The HTTP message handler.</param>
        /// <param name="log">The log service.</param>
        public SumoLogicMessageSender(HttpMessageHandler httpMessageHandler, ILog log)
        {
            this.Log = log ?? new DummyLog();
            this.HttpClient = httpMessageHandler == null ? new HttpClient() : new HttpClient(httpMessageHandler);
        }

        /// <summary>
        /// Gets or sets the destination URL.
        /// </summary>
        public Uri Url
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source category
        /// </summary>
        public string SourceCategory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source host
        /// </summary>
        public string SourceHost
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the retry interval.
        /// </summary>
        public TimeSpan RetryInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        public TimeSpan ConnectionTimeout
        {
            get { return this.HttpClient.Timeout; }
            set { this.HttpClient.Timeout = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the HTTP client has been initialized with proper values or not.
        /// </summary>
        /// <returns>True if the http client has been initialized, false otherwise.</returns>
        public bool CanTrySend
        {
            get { return this.Url != null && this.ConnectionTimeout != TimeSpan.Zero; }
        }

        /// <summary>
        /// Gets a value indicating whether the HTTP client has been initialized with proper values or not.
        /// </summary>
        /// <returns>True if the http client has been initialized, false otherwise.</returns>
        public bool CanSend
        {
            get { return this.Url != null && this.RetryInterval != TimeSpan.Zero && this.ConnectionTimeout != TimeSpan.Zero; }
        }

        /// <summary>
        /// Gets or sets the log service.
        /// </summary>
        private ILog Log
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the HTTP client.
        /// </summary>
        private HttpClient HttpClient
        {
            get;
            set;
        }

        /// <summary>
        /// Blocks while sending a message to the SumoLogic server, retrying as many time as needed.
        /// </summary>
        /// <param name="body">The message body.</param>
        /// <param name="name">The message name.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Send(string body, string name)
        {
            bool success = false;
            do
            {
                try
                {
                    this.TrySend(body, name);
                    success = true;
                }
                catch (Exception ex)
                {
                    if (!(ex is IOException || ex is HttpRequestException || ex is WebException))
                    {
                        throw;
                    }

                    if (this.Log.IsErrorEnabled)
                    {
                        this.Log.Error("Error trying send messages. Exception: " + ex.Message);
                    }

                    try
                    {
                        Task.Delay(this.RetryInterval).GetAwaiter().GetResult();
                    }
                    catch (Exception ex2)
                    {
                        if (this.Log.IsErrorEnabled)
                        {
                            this.Log.Error(ex2.Message);
                        }

                        break;
                    }
                }
            }
            while (!success);
        }

        /// <summary>
        /// Blocks while sending a message to the SumoLogic server, no retries are performed.
        /// </summary>
        /// <param name="body">The message body.</param>
        /// <param name="name">The message name.</param>
        public void TrySend(string body, string name)
        {
            if (this.Url == null)
            {
                if (this.Log.IsWarnEnabled)
                {
                    this.Log.Warn("Could not send log to Sumo Logic (null Url)");
                }

                return;
            }

            using (var httpContent = new StringContent(body, Encoding.UTF8, TextPlainMediaType))
            {
                httpContent.Headers.Add(SUMO_SOURCE_NAME_HEADER, name);
                // set headers for the source category and the source host
                // when they are not null nor empty strings
                if(SourceCategory != null || !SourceCategory.Trim().Equals(""))
                {
                    httpContent.Headers.Add(SUMO_SOURCE_CATEGORY_HEADER, SourceCategory);
                }
                if (SourceHost != null || !SourceHost.Trim().Equals(""))
                {
                    httpContent.Headers.Add(SUMO_SOURCE_HOST_HEADER, SourceHost);
                }
                try
                {
                    var response = this.HttpClient.PostAsync(this.Url, httpContent).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        if (this.Log.IsWarnEnabled)
                        {
                            this.Log.Warn("Received HTTP error from Sumo Service: " + response.StatusCode);
                        }

                        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            throw new IOException("The service is unavailable");
                        }
                    }

                    if (this.Log.IsDebugEnabled)
                    {
                        this.Log.Debug("Successfully sent log request to Sumo Logic");
                    }
                }
                catch (AggregateException ex)
                {
                    if (this.Log.IsWarnEnabled)
                    {
                        this.Log.Warn("Could not send log to Sumo Logic");
                    }

                    if (this.Log.IsDebugEnabled)
                    {
                        this.Log.Debug("Reason: " + ex.InnerException.Message);
                    }

                    throw ex.InnerException;
                }
                catch (IOException ex)
                {
                    if (this.Log.IsWarnEnabled)
                    {
                        this.Log.Warn("Could not send log to Sumo Logic");
                    }

                    if (this.Log.IsDebugEnabled)
                    {
                        this.Log.Debug("Reason: " + ex.Message);
                    }

                    throw;
                }
            }
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
            if (this.HttpClient != null)
            {
                this.HttpClient.Dispose();
                this.HttpClient = null;
            }
        }
    }
}
