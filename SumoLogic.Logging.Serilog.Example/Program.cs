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
namespace SumoLogic.Logging.Serilog.Example
{
    using System;
    using Microsoft.Extensions.Configuration;
    using global::Serilog;
    using global::Serilog.Core;
    using SumoLogic.Logging.Serilog.Extensions;

    public class Program
    {
        /// <summary>
        /// An example application that logs.
        /// </summary>
        static void Main(string[] args)
        {
            // init logger programatically
            // (NB! using unbuffered SumoLogic sink)
            var url = new Uri("http://localhost");
            var sourceName = "w4k-serilog-sumologic";
            Logger logFromFluent = new LoggerConfiguration()
                .WriteTo.SumoLogic(url, sourceName: sourceName)
                .CreateLogger();

            logFromFluent.Information("Event: Initialized logger");

            // init logger from configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Logger logFromConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            logFromConfig.Information("Event: Configured logger");

            // we are done!
            Console.ReadLine();
        }
    }
}
