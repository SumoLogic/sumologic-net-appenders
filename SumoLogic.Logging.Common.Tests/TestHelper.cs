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
using System;
using System.Threading;
using Xunit.Sdk;

namespace SumoLogic.Logging.Common.Tests
{
    public static class TestHelper
    {
        public delegate void VerifierDelegate();

        public static TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(10);

        public static TimeSpan DEFAULT_INTERVAL = TimeSpan.FromMilliseconds(100);

        public static void Eventually(TimeSpan timeout, TimeSpan interval, VerifierDelegate verifier)
        {
            DateTime finalTime = DateTime.Now + timeout;
            Exception err = null;
            int tried = 0;
            while (DateTime.Now < finalTime)
            {
                Thread.Sleep(interval);
                try
                {
                    verifier();
                    err = null;
                    return;
                }
                catch(Exception ex)
                {
                    err = ex;
                    tried++;
                }
            }
            if (null != err)
            {
                string message = $"Still get \"{err.GetType().Name}\" after {timeout} and {tried} trys.";
                throw new EventuallyException(message, err);
            }
            
        }

        public static void Eventually(VerifierDelegate verifier)
        {
            Eventually(DEFAULT_TIMEOUT, DEFAULT_INTERVAL, verifier);
        }

        public class EventuallyException: XunitException {
            public EventuallyException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
}
