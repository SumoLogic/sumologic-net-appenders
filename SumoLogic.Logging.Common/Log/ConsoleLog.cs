#if netfull
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
namespace SumoLogic.Logging.Common.Log
{
    using System;

    /// <summary>
    /// Console log implementation.
    /// </summary>
    public class ConsoleLog : ILog
    {
        /// <summary>
        /// Gets a value indicating whether logging is enabled for the Trace level.
        /// </summary>
        /// <returns> A value of true if logging is enabled for the Trace level, otherwise it returns false.</returns>     
        public bool IsTraceEnabled
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the Debug level.
        /// </summary>
        /// <returns> A value of true if logging is enabled for the Debug level, otherwise it returns false.</returns>     
        public bool IsDebugEnabled
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the Info level.
        /// </summary>
        /// <returns> A value of true if logging is enabled for the Info level, otherwise it returns false.</returns>     
        public bool IsInfoEnabled
        {
            get { return true; }   
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the Warn level.
        /// </summary>
        /// <returns> A value of true if logging is enabled for the Warn level, otherwise it returns false.</returns>     
        public bool IsWarnEnabled
        {
           get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the Error level.
        /// </summary>
        /// <returns> A value of true if logging is enabled for the Error level, otherwise it returns false.</returns>     
        public bool IsErrorEnabled
        {
           get { return true; }
        }

        /// <summary>
        /// Logs a message object with the Trace level.
        /// </summary>
        /// <param name="msg">The message object to log.</param>
        public void Trace(string msg)
        {
            WriteToConsole("DEBUG: ", msg);
        }
        
        /// <summary>
        /// Logs a message object with the Debug level.
        /// </summary>
        /// <param name="msg">The message object to log.</param>
        public void Debug(string msg)
        {
            WriteToConsole("DEBUG: ", msg);
        }

        /// <summary>
        /// Logs a message object with the Info level.
        /// </summary>
        /// <param name="msg">The message object to log.</param>
        public void Info(string msg)
        {
            WriteToConsole("INFO: ", msg);
        }

        /// <summary>
        /// Logs a message object with the Warn level.
        /// </summary>
        /// <param name="msg">The message object to log.</param>
        public void Warn(string msg)
        {
            WriteToConsole("WARN: ", msg);
        }

        /// <summary>
        /// Logs a message object with the Error level.
        /// </summary>
        /// <param name="msg">The message object to log.</param>
        public void Error(string msg)
        {
            WriteToConsole("ERROR: ", msg);
        }

        /// <summary>
        /// Writes the specified content to the Console.
        /// </summary>
        /// <param name="prefix">The message prefix.</param>
        /// <param name="msg">The message content.</param>
        private static void WriteToConsole(string prefix, string msg)
        {
            Console.Write(prefix);
            Console.WriteLine(msg);
        }
    }
}
#endif