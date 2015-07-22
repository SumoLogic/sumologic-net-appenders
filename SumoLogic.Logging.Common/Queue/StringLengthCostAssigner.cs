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
    /// <summary>
    /// String length cost assigner.
    /// </summary>
    public class StringLengthCostAssigner : ICostAssigner<string>
    {
        /// <summary>
        /// Return the cost of the element.
        /// </summary>
        /// <param name="element">The Element.</param>
        /// <returns>The cost.</returns>
        public long Cost(string element)
        {
            // Note: This is only an estimate for total byte usage, since in UTF-8 encoding,
            // the size of one character may be > 1 byte.
            return element == null ? 0 : element.Length;
        }
    }
}
