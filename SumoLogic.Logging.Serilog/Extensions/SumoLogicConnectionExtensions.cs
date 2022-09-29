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
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using SumoLogic.Logging.Serilog.Config;

    /// <summary>
    /// Extensions of <see cref="SumoLogicConnection"/>.
    /// </summary>
    internal static class SumoLogicConnectionExtensions
    {
        /// <summary>
        /// Sets time span if not empty to provided configuration.
        /// </summary>
        /// <param name="target">Target configuration.</param>
        /// <param name="member">Time span property expression.</param>
        /// <param name="value">Value to be set.</param>
        /// <returns>
        /// Target instance.
        /// </returns>
        public static SumoLogicConnection SetTimeSpanIfNotEmpty(
            this SumoLogicConnection target,
            Expression<Func<SumoLogicConnection, TimeSpan>> member,
            long? value)
        {
            if (value.HasValue
                && member.Body is MemberExpression memberExpression
                && memberExpression.Member is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(target, TimeSpan.FromMilliseconds(value.Value), null);
            }

            return target;
        }
    }
}
