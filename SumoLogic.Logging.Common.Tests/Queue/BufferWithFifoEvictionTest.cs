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
    using System.Globalization;
    using SumoLogic.Logging.Common.Queue;
    using Xunit;

    /// <summary>
    /// Test buffer with eviction.
    /// </summary>
    public class BufferWithFifoEvictionTest
    {
        /// <summary>
        /// Try to insert more element than the capacity is available.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void TryToInsertMoreElementTest()
        {
            var buffer = new BufferWithFifoEviction<string>(2, new StringLengthCostAssigner());
            Assert.True(buffer.Add(BuildStringOfSize(2)));
            Assert.True(buffer.Add(BuildStringOfSize(1)));
            Assert.True(buffer.Add(BuildStringOfSize(1)));
            Assert.False(buffer.Add(BuildStringOfSize(5)));
            Assert.Equal(2, buffer.Count);
        }

        /// <summary>
        /// Drain to test.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void DrainToTest()
        {           
            var buffer = new BufferWithFifoEviction<string>(10, new StringLengthCostAssigner());
            for (int i = 0; i < buffer.Capacity; i++)
            {
                string a = i.ToString(CultureInfo.CurrentCulture);
                Assert.True(buffer.Add(a));
            }

            Assert.Equal(10, buffer.Count);

            var drainedItems = new List<string>();
            buffer.DrainTo(drainedItems);
            for (int i = 0; i < drainedItems.Count; i++)
            {
                Assert.Equal(i.ToString(CultureInfo.CurrentCulture), drainedItems[i]);
            }
        }

        /// <summary>
        /// Insert more element and drain to test.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unit test")]
        [Fact]
        public void InsertMoreElementAndDrainToTest()
        {
            int offset = 5;
            BufferWithFifoEviction<string> buffer = new BufferWithFifoEviction<string>(5, new StringLengthCostAssigner());
            for (int i = 0; i < buffer.Capacity; i++)
            {
                string a = i.ToString(CultureInfo.CurrentCulture);
                Assert.True(buffer.Add(a));
            }

            long limit = buffer.Capacity + offset;
            for (int i = 5; i < limit; i++)
            {
                string a = i.ToString(CultureInfo.CurrentCulture);
                Assert.True(buffer.Add(a));
            }

            Assert.Equal(5, buffer.Count);

            var drainedItems = new List<string>();
            buffer.DrainTo(drainedItems);
            for (int i = 0; i < buffer.Capacity; i++)
            {
                long expected = i + offset;
                Assert.Equal(expected.ToString(CultureInfo.CurrentCulture), drainedItems[i]);
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
    }
}
