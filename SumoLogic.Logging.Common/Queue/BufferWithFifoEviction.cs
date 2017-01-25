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
namespace SumoLogic.Logging.Common.Queue
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using SumoLogic.Logging.Common.Log;

    /// <summary>
    /// Buffer with FIFO eviction.
    /// </summary>
    /// <typeparam name="T">Type element in buffer.</typeparam>
    public class BufferWithFifoEviction<T> : BufferWithEviction<T>
    {
        /// <summary>
        /// Lock object used when adding 
        /// an element to the queue
        /// </summary>
        private readonly object queueAddLock = new object();

        /// <summary>
        /// Cost bounded concurrent queue.
        /// </summary>
        private CostBoundedConcurrentQueue<T> queue;

        /// <summary>
        /// Cost Assigner.
        /// </summary>
        private ICostAssigner<T> costAssigner;

        /// <summary>
        /// The logger.
        /// </summary>
        private ILog log;

        /// <summary>
        /// Initializes a new instance of the  <see cref="BufferWithFifoEviction{T}"/> class.
        /// </summary>
        /// <param name="capacity">The buffer capacity.</param>
        /// <param name="costAssigner">The buffer cost assigner, <see cref="ICostAssigner{T}"/>.</param>
        /// <param name="log">The log service, <see cref="ILog"/>.</param>
        public BufferWithFifoEviction(long capacity, ICostAssigner<T> costAssigner, ILog log)
            : base(capacity)
        {
            if (costAssigner == null)
            {
                throw new ArgumentException("CostAssigner cannot be null");
            }

            this.log = log ?? new DummyLog();
            this.queue = new CostBoundedConcurrentQueue<T>(capacity, costAssigner);
            this.costAssigner = costAssigner;
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="BufferWithFifoEviction{T}"/> class.
        /// </summary>
        /// <param name="capacity">The buffer Capacity.</param>
        /// <param name="cost">The buffer cost Assigner <see cref="ICostAssigner{T}"/>.</param>
        public BufferWithFifoEviction(long capacity, ICostAssigner<T> cost)
            : this(capacity, cost, null)
        {
        }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        /// <returns>The count.</returns>
        public override int Count
        {
            get { return this.queue.Count; }
        }

        /// <summary>
        /// Removes all available elements from this queue and adds them to the given collection.
        /// </summary>
        /// <param name="collection">The Collection</param>
        /// <returns>The number of elements transferred.</returns>
        public override int DrainTo(ICollection<T> collection)
        {
            return this.queue.DrainTo(collection);
        }

        /// <summary>
        /// Add element in the queue.
        /// </summary>
        /// <param name="element">Element to add in queue.</param>
        /// <returns>If it add was successful.</returns>
        public override bool Add(T element)
        {
            lock (this.queueAddLock)
            {
                bool success = this.queue.Enqueue(element);
                if (success == false)
                {
                    this.Evict(this.costAssigner.Cost(element));
                    return this.queue.Enqueue(element);
                }

                return true;
            }
        }

        /// <summary>
        /// Evict buffer.
        /// </summary>
        /// <returns>Retrieves the element with type Q.</returns>
        protected override T Evict()
        {
            return this.queue.Dequeue();
        }

        /// <summary>
        /// Make room for inserting an element with cost.
        /// </summary>
        /// <param name="cost">The space cost</param>
        /// <returns>True or false.</returns>
        protected override bool Evict(long cost)
        {
            int numEvicted = 0;

            if (cost > this.Capacity)
            {
                return false;
            }

            long targetCost = this.Capacity - cost;

            while (this.queue.Cost > targetCost)
            {
                numEvicted++;
                this.Evict();
            }

            if (numEvicted > 0 && this.log.IsWarnEnabled)
            {              
                this.log.Warn("Evicted " + numEvicted + " messages from buffer");
            }

            return true;
        }
    }
}
