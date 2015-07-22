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

    /// <summary>
    /// Buffer with eviction.
    /// </summary>
    /// <typeparam name="T">Type buffer.</typeparam>
    public abstract class BufferWithEviction<T>
    {
        /// <summary>
        /// The buffer capacity.
        /// </summary>
        private long capacity;

        /// <summary>
        /// Initializes a new instance of the BufferWithEviction class.
        /// </summary>
        /// <param name="capacity">Capacity to set</param>
        protected BufferWithEviction(long capacity)
        {
            this.Capacity = capacity;
        }

        /// <summary>
        /// Gets or sets the buffer capacity.
        /// </summary>
        public long Capacity
        {
            get
            {
                return this.capacity;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Capacity must be at least 1");
                }

                this.capacity = value;
            }
        }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        /// <returns>The count.</returns>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Removes all available elements from this queue and adds them to the given collection.
        /// </summary>
        /// <param name="collection">The Collection</param>
        /// <returns>The number of elements transferred.</returns>
        public abstract int DrainTo(ICollection<T> collection);

        /// <summary>
        /// Add element in the queue.
        /// </summary>
        /// <param name="element">Element to add in queue.</param>
        /// <returns>If it add was successful.</returns>
        public abstract bool Add(T element);

        /// <summary>
        /// Evict buffer.
        /// </summary>
        /// <returns>Retrieves the element with type Q.</returns>
        protected abstract T Evict();

        /// <summary>
        /// Make room for inserting an element with cost.
        /// </summary>
        /// <param name="cost">The space cost</param>
        /// <returns>True or false.</returns>
        protected abstract bool Evict(long cost);
    }
}
