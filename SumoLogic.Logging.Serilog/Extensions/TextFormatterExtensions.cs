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
namespace SumoLogic.Logging.Serilog.Extensions
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using global::Serilog.Events;
    using global::Serilog.Formatting;

    /// <summary>
    /// Extensions of <see cref="ITextFormatter"/>.
    /// </summary>
    internal static class TextFormatterExtensions
    {
        /// <summary>
        /// Format log event.
        /// </summary>
        /// <param name="formatter">The text formatter associated with sink.</param>
        /// <param name="logEvent">The log event to be written using formatter.</param>
        /// <returns>
        /// Formatted log message.
        /// </returns>
        public static string Format(this ITextFormatter formatter, LogEvent logEvent)
        {
            var bodyBuilder = new StringBuilder();
            using (var textWriter = new StringWriter(bodyBuilder, CultureInfo.InvariantCulture))
            {
                formatter.Format(logEvent, textWriter);
                textWriter.WriteLine();
            }

            return bodyBuilder.ToString();
        }
    }
}
