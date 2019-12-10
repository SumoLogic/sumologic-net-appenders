﻿/**
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
    using System.Globalization;
    using System.Threading.Tasks;
    using SumoLogic.Logging.Common.Log;
    using SumoLogic.Logging.Common.Queue;

    /// <summary>
    /// Task to perform a single flushing check.
    /// </summary>
    /// <typeparam name="TBufferItem">Type for input.</typeparam>
    /// <typeparam name="TMessage">Type for output.</typeparam>
    public abstract class BufferFlushingTask<TBufferItem, TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferFlushingTask{TBufferItem, TMessage}" /> class.
        /// </summary>
        /// <param name="messageQueue">The queue message for the instance <see cref="BufferWithEviction{TIn}" /> </param>
        /// <param name="log">The log service.</param>
        protected BufferFlushingTask(BufferWithEviction<TBufferItem> messageQueue, ILog log)
        {
            this.MessageQueue = messageQueue;
            this.LastFlushedOn = DateTime.UtcNow;
            this.Log = log ?? new DummyLog();
            this.IsFlushing = false;
        }

        /// <summary>
        /// Gets or sets max flush interval in milliseconds.
        /// </summary>
        protected TimeSpan MaxFlushInterval { get; set; }

        /// <summary>
        /// Gets or sets max messages per request.
        /// </summary>
        protected long MessagesPerRequest { get; set; }

        /// <summary>
        /// Gets or sets the name used when sending messages.
        /// </summary>
        protected string MessagesName { get; set; }

        /// <summary>
        /// Gets or sets the category used when sending messages.
        /// </summary>
        protected string MessagesCategory { get; set; }

        /// <summary>
        /// Gets or sets the host used when sending messages.
        /// </summary>
        protected string MessagesHost { get; set; }

        /// <summary>
        /// Gets or sets the Log service.
        /// </summary>
        protected ILog Log { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is flushing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in the process of flushing; otherwise, <c>false</c>.
        /// </value>
        protected bool IsFlushing { get; set; }

        /// <summary>
        /// Gets or sets the time of last flush.
        /// </summary>
        private DateTime LastFlushedOn { get; set; }

        /// <summary>
        /// Gets or sets the messages queue.
        /// </summary>
        private BufferWithEviction<TBufferItem> MessageQueue { get; set; }

        /// <summary>
        /// This flush and sends the messages.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception unknown at compile time.")]
        public async Task Run()
        {
            try
            {
                if (this.NeedsFlushing())
                {
                    this.IsFlushing = true;
                    await this.FlushAndSend().ConfigureAwait(false);
                }

                this.IsFlushing = false;
            }
            catch (Exception ex)
            {
                Log.Warn($"HTTP Sender flush failed: {ex.GetType()}: {ex.Message}");
            }
        }

        /// <summary>
        /// Flush and send all messages from the queue of messages.
        /// </summary>
        public async Task FlushAndSend()
        {
            var messages = new List<TBufferItem>();
            this.MessageQueue.DrainTo(messages);

            if (messages.Count > 0)
            {
                if (this.Log.IsDebugEnabled)
                {
                    this.Log.Debug(string.Format(CultureInfo.InvariantCulture, "{0} - Flushing and sending out {1} messages ({2} messages left)", DateTime.Now, messages.Count, this.MessageQueue.Count));
                }

                TMessage body = this.Aggregate(messages);
                await this.SendOut(body, this.MessagesName, this.MessagesCategory, this.MessagesHost).ConfigureAwait(false);
            }

            this.LastFlushedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Given the list of messages, aggregate them into a single Out object
        /// </summary>
        /// <param name="messages">List of messages to aggregate.</param>
        /// <returns>To do.</returns>
        protected abstract TMessage Aggregate(IList<TBufferItem> messages);

        /// <summary>
        /// Sends aggregated message out. Block until we've successfully sent it.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="name">The name.</param>
        [Obsolete("use SendOut(TMessage body, string name, string category, string host)")]
        protected abstract Task SendOut(TMessage body, string name);

        /// <summary>
        /// Sends aggregated message out. Block until we've successfully sent it.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="host">The host.</param>
        protected abstract Task SendOut(TMessage body, string name, string category, string host);

        /// <summary>
        /// Returns if the queue needs to flushing messages.
        /// </summary>
        /// <returns>True if flush is needed.</returns>
        private bool NeedsFlushing()
        {
            return this.MessageQueue.Count >= this.MessagesPerRequest || DateTime.UtcNow.Ticks - this.MaxFlushInterval.Ticks >= this.LastFlushedOn.Ticks || this.IsFlushing;
        }        
    }
}
