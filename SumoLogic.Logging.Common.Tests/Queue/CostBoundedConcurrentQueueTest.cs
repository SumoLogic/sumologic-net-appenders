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
    using System.Threading;
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
        /// Drain queue while simultaneously writing into it
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void DrainToWithWritersTest()
        {
            var queue = new CostBoundedConcurrentQueue<string>(10000, new StringLengthCostAssigner());
            for (int i = 0; i < 5000; i++)
            {
                queue.Enqueue(BuildStringOfSize(2));
            }

            Assert.Equal(10000, queue.Cost);

            // start background writer that will attempt to enqueue
            // new messages while we drain
            var token = new CancellationTokenSource();
            var writerThread = StartBackgroundWriter(queue, 1, 7000, token);
            try
            {
                Thread.Sleep(100);
                var drainCollection = new List<string>();
                queue.DrainTo(drainCollection);

                // only the original 5000 messages should be drained, none
                // of the newly-written messages
                TestHelper.Eventually(() =>
                {
                    Assert.Equal(5000, drainCollection.Count);
                });

                // background writer should finish writing
                Thread.Sleep(1000);
                drainCollection.Clear();
                queue.DrainTo(drainCollection);
                TestHelper.Eventually(() =>
                {
                    Assert.Equal(7000, drainCollection.Count);
                });
            }
            finally
            {
                if (writerThread.IsAlive)
                {
                    token.Cancel();
                }
            }
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

        /// <summary>
        /// Starts a background thread that will aggressively write elements into the specified queue.
        /// </summary>
        /// <param name="queue">Queue into which elements should be written</param>
        /// <param name="elementCost">Cost of each element</param>
        /// <param name="howMany">How many total elements should be written</param>
        /// <returns>The resultant thread. The thread will already be started.</returns>
        private static Thread StartBackgroundWriter(CostBoundedConcurrentQueue<string> queue, int elementCost, int howMany, CancellationTokenSource token)
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                var str = BuildStringOfSize(elementCost);

                // spin as fast as possible, adding elements as soon as capacity becomes available
                for (int i = 0; i < howMany; i++)
                {
                    while (!queue.Enqueue(str))
                    {
                        if(token.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                }
            }));

            t.Start();
            return t;
        }
    }
}
