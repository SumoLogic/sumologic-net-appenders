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
using SumoLogic.Logging.Common.Log;

namespace SumoLogic.Logging.AspNetCore
{
    /// <summary>
    /// ILog wrapper of .NET Core ILogger, for internal logging
    /// </summary>
    public class LoggerLog: ILog
    {
        private ILogger logger;

        public LoggerLog(ILogger logger)
        {
            this.logger = logger;
        }

        bool ILog.IsTraceEnabled => true;

        bool ILog.IsDebugEnabled => true;

        bool ILog.IsInfoEnabled => true;

        bool ILog.IsWarnEnabled => true;

        bool ILog.IsErrorEnabled => true;

        void ILog.Debug(string msg)
        {
            logger.Log(LogLevel.Debug, msg);
        }

        void ILog.Error(string msg)
        {
            logger.Log(LogLevel.Error, msg);
        }

        void ILog.Info(string msg)
        {
            logger.Log(LogLevel.Information, msg);
        }

        void ILog.Trace(string msg)
        {
            logger.Log(LogLevel.Trace, msg);
        }

        void ILog.Warn(string msg)
        {
            logger.Log(LogLevel.Warning, msg);
        }
    }
}
