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
    using System.Text;
    using SumoLogic.Logging.Common.Log;
    using SumoLogic.Logging.Common.Queue;

    /// <summary>
    /// Task class for sending buffered messages to SumoLogic server.
    /// </summary>
    public class SumoLogicMessageSenderBufferFlushingTask : BufferFlushingTask<string, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicMessageSenderBufferFlushingTask" /> class.
        /// </summary>
        /// <param name="messagesQueue">The queue message for the instance <see cref="BufferWithEviction{TIn}" /> </param>
        /// <param name="messageSender">The http sender.</param>
        /// <param name="maxFlushInterval">The maximum interval for flushing.</param>
        /// <param name="messagesPerRequest">The maximum messages per request.</param>
        /// <param name="messagesName">The messages name.</param>
        /// <param name="log">The log service.</param>
        public SumoLogicMessageSenderBufferFlushingTask(
            BufferWithEviction<string> messagesQueue,
            SumoLogicMessageSender messageSender,
            TimeSpan maxFlushInterval,
            long messagesPerRequest,
            string messagesName,
            ILog log)
            : base(messagesQueue, log)
        {
            this.MaxFlushInterval = maxFlushInterval;
            this.MessagesPerRequest = messagesPerRequest;
            this.MessagesName = messagesName;
            this.MessageSender = messageSender;
        }

        /// <summary>
        /// Gets or sets the SumoLogic messages sender.
        /// </summary>
        private SumoLogicMessageSender MessageSender
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string that content all messages.
        /// </summary>
        /// <param name="messages">List of messages.</param>
        /// <returns>The concatenated messages.</returns>
        protected override string Aggregate(IList<string> messages)
        {
            if (messages == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(messages.Count * 10);
            foreach (string message in messages)
            {
                builder.Append(message);
            }

            return builder.ToString();
        }

        /// <summary>
        /// This sends out a message.
        /// </summary>
        /// <param name="body">Message body.</param>
        protected override void SendOut(string body, String name)
        {
            if (!this.MessageSender.CanSend)
            {
                if (this.Log.IsErrorEnabled)
                {
                    Log.Error("HTTP Sender is not initialized");
                }
                
                return;
            }
            this.MessageSender.SourceName = name;
            this.MessageSender.Send(body);
        }
    }
}
