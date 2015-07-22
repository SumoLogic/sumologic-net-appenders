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
namespace SumoLogic.Logging.Common.Tests.Aggregation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using SumoLogic.Logging.Common.Queue;
    using SumoLogic.Logging.Common.Sender;

    /// <summary>
    /// This is a dummy buffer flushing task for tests.
    /// </summary>
    public class DummyBufferFlushingTask : BufferFlushingTask<string, IList<string>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyBufferFlushingTask" /> class.
        /// </summary>
        /// <param name="queue">The queue.</param>
        public DummyBufferFlushingTask(BufferWithEviction<string> queue)
            : base(queue, null)
        {
            this.SentOut = new List<List<string>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyBufferFlushingTask" /> class.
        /// </summary>
        /// <param name="queue">The queue message for the instance <see cref="BufferWithEviction{TIn}" /> </param>
        /// <param name="maxFlushInterval">The maximum interval for flushing.</param>
        /// <param name="messagesPerRequest">The maximum messages per request.</param>
        /// <param name="name">The name.</param>
        public DummyBufferFlushingTask(BufferWithEviction<string> queue, TimeSpan maxFlushInterval, long messagesPerRequest, string name)
            : this(queue)
        {
            this.MaxFlushInterval = maxFlushInterval;
            this.MessagesPerRequest = messagesPerRequest;
            this.MessagesName = name;
        }

        /// <summary>
        /// Gets the output tasks
        /// </summary>
        public IList<List<string>> SentOut
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a string that content all messages.
        /// </summary>
        /// <param name="messages">List of messages.</param>
        /// <returns>The concatenated messages.</returns>
        protected override IList<string> Aggregate(IList<string> messages)
        {
            return messages;
        }

        /// <summary>
        /// This sends out a message.
        /// </summary>
        /// <param name="body">Message body.</param>
        /// <param name="name">Message name.</param>
        protected override void SendOut(IList<string> body, string name)
        {
            this.SentOut.Add(new List<string>(body));
        }
    }
}
