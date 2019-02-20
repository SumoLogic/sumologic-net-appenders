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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SumoLogic.Logging.AspNetCore
{
    /// <summary>
    /// Extensions for ASP.NET DI
    /// </summary>
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="options">Configure an instance of the <see cref="LoggerOptions" /> to set logging options</param>
        public static ILoggerFactory AddSumoLogic(this ILoggerFactory factory, LoggerOptions options)
        {
            factory.AddProvider(new LoggerProvider(options));
            return factory;
        }

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        public static ILoggerFactory AddSumoLogic(this ILoggerFactory factory)
            => factory.AddSumoLogic(new LoggerOptions());

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="uri">Sets the uri of Sumo Logic ingesting endpoint/param>
        public static ILoggerFactory AddSumoLogic(this ILoggerFactory factory, string uri)
            => factory.AddSumoLogic(new LoggerOptions() { Uri = uri });

#if !NETCOREAPP1_1
        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="options">Configure an instance of the <see cref="LoggerOptions" /> to set logging options</param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder, LoggerOptions options)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new LoggerProvider(options));
            return builder;
        }

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder)
            => builder.AddSumoLogic(new LoggerOptions());

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="uri">Sets the uri of Sumo Logic ingesting endpoint/param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder, string uri)
            => builder.AddSumoLogic(new LoggerOptions() { Uri = uri });
#endif
    }
}
