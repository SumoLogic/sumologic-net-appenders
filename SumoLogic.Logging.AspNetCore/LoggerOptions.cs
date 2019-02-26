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
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace SumoLogic.Logging.AspNetCore
{
    /// <summary>
    /// Holding the settings of Sumo Logic sender
    /// </summary>
    public class LoggerOptions
    {
        private Uri uri;

        /// <summary>
        /// Gets or sets the SumoLogic server URL.
        /// </summary>
        public string Uri
        {
            get
            {
                return uri?.ToString();
            }
            set
            {
                uri = new Uri(value);
            }
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the name used for messages sent to SumoLogic server (sent as X-Sumo-Name header).
        /// </summary>
        public string SourceName { get; set; } = "asp.net-core-logger";

        /// <summary>
        /// Gets or sets the source category used for messages sent to SumoLogic server (sent as X-Sumo-Category header).
        /// </summary>
        public string SourceCategory { get; set; }

        /// <summary>
        /// Gets or sets the source host used for messages sent to SumoLogic server (sent as X-Sumo-Host header).
        /// </summary>
        public string SourceHost { get; set; } = System.Net.Dns.GetHostName();

        /// <summary>
        /// Gets or sets if the sender using a buffer
        /// </summary>
        public bool IsBuffered { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the send message retry interval.
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the maximum interval between flushes.
        /// </summary>
        public TimeSpan MaxFlushInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets how often the messages queue is checked for messages to send.
        /// </summary>
        public TimeSpan FlushingAccuracy { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Gets or sets how many messages need to be in the queue before flushing.
        /// </summary>
        public long MessagesPerRequest { get; set; } = 100;

        /// <summary>
        /// Gets or sets the messages queue capacity, in bytes.
        /// </summary>
        /// <remarks>Messages are dropped When the queue capacity is exceeded.</remarks>
        public long MaxQueueSizeBytes { get; set; } = 1_000_000;

        /// <summary>
        /// Gets or sets the HTTP message handler used by sender
        /// </summary>
        public HttpMessageHandler HttpMessageHandler { get; set; }

        /// <summary>
        /// Gets or sets the debugging logger for provider itself 
        /// </summary>
        public ILogger DebuggingLogger { get; set; }

    }
}
