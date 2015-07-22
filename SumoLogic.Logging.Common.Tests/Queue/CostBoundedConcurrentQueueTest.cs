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
namespace SumoLogic.Logging.Common.Tests.Queue
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using SumoLogic.Logging.Common.Queue;
    using Xunit;

    /// <summary>
    /// This is a NUnit tests of cost bounded concurrent queue.
    /// </summary>
    public class CostBoundedConcurrentQueueTest
    {
        /// <summary>
        /// Insert One element in queue with capacity.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void InsertElementInQueueTest()
        {
            var queue = new CostBoundedConcurrentQueue<string>(100, new StringLengthCostAssigner());
            queue.Enqueue("hello");
            queue.Enqueue("bye");
            Assert.Equal(2, queue.Count);
            Assert.Equal(8, queue.Cost);
            Assert.Equal("hello", queue.Dequeue());
            Assert.Equal(1, queue.Count);
            Assert.Equal("bye", queue.Dequeue());
            Assert.Equal(0, queue.Count);
        }

        /// <summary>
        /// Insert Beyond Capacity.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void InsertBeyondCapacityTest()
        {
            var queue = new CostBoundedConcurrentQueue<string>(20, new StringLengthCostAssigner());
            Assert.True(queue.Enqueue(BuildStringOfSize(9)));
            Assert.True(queue.Enqueue(BuildStringOfSize(11)));
            Assert.Equal(2, queue.Count);
            Assert.False(queue.Enqueue(BuildStringOfSize(1)));
            Assert.Equal(20, queue.Cost);
        }

        /// <summary>
        /// Add elements and test the size and capacity of the queue.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void SizeAndCapacityTest()
        {
            var queue = new CostBoundedConcurrentQueue<string>(20, new StringLengthCostAssigner());
            queue.Enqueue(BuildStringOfSize(9));
            queue.Enqueue(BuildStringOfSize(11));
            queue.Enqueue(BuildStringOfSize(11));
            Assert.Equal(20, queue.Cost);
            Assert.Equal(2, queue.Count);
            queue.Dequeue();
            Assert.Equal(11, queue.Cost);
            Assert.Equal(1, queue.Count);
            queue.Enqueue(BuildStringOfSize(3));
            Assert.Equal(2, queue.Count);
            queue.Enqueue(BuildStringOfSize(3));
            Assert.Equal(3, queue.Count);
            Assert.Equal(17, queue.Cost);
        }

        /// <summary>
        /// Drain to queue.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void DrainToTest()
        {
            var queue = new CostBoundedConcurrentQueue<string>(1000, new StringLengthCostAssigner());
            queue.Enqueue(BuildStringOfSize(600));
            queue.Enqueue(BuildStringOfSize(200));
            Assert.Equal(800, queue.Cost);
            queue.DrainTo(new List<string>());
            Assert.Equal(0, queue.Cost);
            Assert.Equal(0, queue.Count);
        }

        /// <summary>
        /// Builds a string with a length of 'n' characters.
        /// </summary>
        /// <param name="n">The string length.</param>
        /// <returns>The built string.</returns>
        private static string BuildStringOfSize(int n)
        {
            string str = null;
            for (int i = 0; i < n; i++)
            {
                str += "*";
            }

            return str;
        }
    }
}
