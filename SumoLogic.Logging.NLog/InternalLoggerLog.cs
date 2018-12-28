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

namespace SumoLogic.Logging.NLog
{
    using global::NLog.Common;

    internal class InternalLoggerLog : Common.Log.ILog
    {
        private readonly Common.Log.ILog _customLog;
        private readonly string _prefix;

        public bool IsTraceEnabled => InternalLogger.IsTraceEnabled || _customLog?.IsTraceEnabled == true;

        public bool IsDebugEnabled => InternalLogger.IsDebugEnabled || _customLog?.IsDebugEnabled == true;

        public bool IsInfoEnabled => InternalLogger.IsInfoEnabled || _customLog?.IsInfoEnabled == true;

        public bool IsWarnEnabled => InternalLogger.IsWarnEnabled || _customLog?.IsWarnEnabled == true;

        public bool IsErrorEnabled => InternalLogger.IsErrorEnabled || InternalLogger.IsFatalEnabled || _customLog?.IsErrorEnabled == true;

        public bool IsConsoleEnabled => InternalLogger.LogToConsole || InternalLogger.LogToConsoleError;

        public InternalLoggerLog(string prefix, Common.Log.ILog customLog)
        {
            _customLog = customLog;
            _prefix = prefix;
        }

        public void Debug(string msg)
        {
            if (InternalLogger.IsDebugEnabled)
            {
                InternalLogger.Debug(string.Concat(_prefix, msg));
            }
            if (_customLog?.IsDebugEnabled == true)
            {
                _customLog.Debug(msg);
            }
        }

        public void Error(string msg)
        {
            if (InternalLogger.IsErrorEnabled)
            {
                InternalLogger.Error(string.Concat(_prefix, msg));
            }
            if (_customLog?.IsErrorEnabled == true)
            {
                _customLog.Error(msg);
            }
        }

        public void Info(string msg)
        {
            if (InternalLogger.IsInfoEnabled)
            {
                InternalLogger.Info(string.Concat(_prefix, msg));
            }
            if (_customLog?.IsInfoEnabled == true)
            {
                _customLog.Info(msg);
            }
        }

        public void Trace(string msg)
        {
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace(string.Concat(_prefix, msg));
            }
            if (_customLog?.IsTraceEnabled == true)
            {
                _customLog.Trace(msg);
            }
        }

        public void Warn(string msg)
        {
            if (InternalLogger.IsWarnEnabled)
            {
                InternalLogger.Warn(string.Concat(_prefix, msg));
            }
            if (_customLog?.IsWarnEnabled == true)
            {
                _customLog.Warn(msg);
            }
        }
    }
}
