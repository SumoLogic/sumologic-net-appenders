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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Cost Bounded Concurrent Queue.
    /// </summary>
    /// <typeparam name="T">Type concurrent queue.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Common collection name pattern")]
    public class CostBoundedConcurrentQueue<T>
    {
        /// <summary>
        /// Concurrent queue.
        /// </summary>
        private ConcurrentQueue<T> queue;

        /// <summary>
        /// The cost Assigner.
        /// </summary>
        private ICostAssigner<T> costAssigner;

        /// <summary>
        /// The queue capacity.
        /// </summary>
        private long capacity;

        /// <summary>
        /// The queue cost.
        /// </summary>
        private long cost;

        /// <summary>
        /// Initializes a new instance of the  <see cref="CostBoundedConcurrentQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">Capacity the queue.</param>
        /// <param name="costAssigner">The cost Assigner <see cref="ICostAssigner{T}"/>. </param>
        public CostBoundedConcurrentQueue(long capacity, ICostAssigner<T> costAssigner)
        {
            this.queue = new ConcurrentQueue<T>();
            this.costAssigner = costAssigner;
            this.capacity = capacity;
        }

        /// <summary>
        /// Gets the sum of the costs of all the elements contained in the queue.
        /// </summary>
        public long Cost
        {
            get { return this.cost; }
        }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        /// <returns>The number of elements in the queue.</returns>
        public int Count
        {
            get { return this.queue.Count; }
        }

        /// <summary>
        /// Removes all available elements from this queue and adds them to the given collection.
        /// </summary>
        /// <param name="collection">Destination collection.</param>
        /// <returns>The number of elements transferred.</returns>
        public int DrainTo(ICollection<T> collection)
        {
            int elementsDrained = 0;

            if (collection == null)
            {
                throw new InvalidOperationException();
            }
            else
            {
                while (this.queue.IsEmpty != true)
                {
                    T e;
                    if (this.queue.TryDequeue(out e) && collection != null)
                    {
                        collection.Add(e);
                        Interlocked.Add(ref this.cost, -this.costAssigner.Cost(e));
                        elementsDrained++;
                    }
                }
            }

            return elementsDrained;
        }

        /// <summary>
        /// Inserts the specified element into this queue if it is possible to do so immediately without
        /// violating capacity restrictions, returning true upon success and false if no space is
        /// currently available.
        /// </summary>
        /// <param name="element">True if element was successfully inserted;
        /// False is no space is currently available.</param>
        /// <returns>True or false.</returns>
        public bool Enqueue(T element)
        {
            long auxCost = this.costAssigner.Cost(element);

            // Atomically check capacity and optimistically increase usage
            lock (this)
            {
                if (auxCost + this.Cost > this.capacity)
                {
                    return false;
                }
                else
                {
                    Interlocked.Add(ref this.cost, auxCost);
                }
            }

            // Underlying queue is unbounded, so this is guaranteed to succeed.
            this.queue.Enqueue(element);
            return true;
        }

        /// <summary>
        /// Retrieves and removes the head of this queue, or returns null if this queue is empty.
        /// </summary>
        /// <returns>The head of this queue</returns>
        public T Dequeue()
        {
            T element;

            if (this.queue.TryDequeue(out element))
            {
                Interlocked.Add(ref this.cost, -this.costAssigner.Cost(element));
            }

            return element;
        }
    }
}
