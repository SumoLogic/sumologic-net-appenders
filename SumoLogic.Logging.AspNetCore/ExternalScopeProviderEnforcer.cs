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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SumoLogic.Logging.AspNetCore
{
    internal class ExternalScopeProviderEnforcer
    {
        private IExternalScopeProvider externalScopeProvider;
        private bool initializedExternally;

        public void SetExternalScopeProvider(IExternalScopeProvider externalScopeProvider)
        {
            if (this.externalScopeProvider != null)
            {
                return;
            }

            this.externalScopeProvider = externalScopeProvider;
            initializedExternally = true;
        }

        public IDisposable BeginScope(object state)
        {
            if (initializedExternally)
            {
                // BeginScope should never be called if IExternalScopeProvider was created and managed by hosting framework.
                // Just in case, return null if it's called
                return null;
            }

            //if IExternalScopeProvider wasn't created by hosting framework, instantiate our oun to provide Scope support.
            if (externalScopeProvider == null)
            {
                externalScopeProvider = new LoggerExternalScopeProvider();
            }

            return externalScopeProvider.Push(state);
        }

        public IDictionary<string, object> GetScopeProperties()
        {
            var result = new Dictionary<string, object>();

            externalScopeProvider?.ForEachScope((scope, dictionary) =>
            {
                // scope can be object of any type, but frameworks by convention use IDictionnary<string, object>
                // to provide any meaningful information for the scope.
                if (scope is IDictionary<string, object> dictionaryScope)
                {
                    foreach (var o in dictionaryScope)
                    {
                        // if the key already exists, override it's value
                        result[o.Key] = o.Value;
                    }
                }
            }, result);

            return result;
        }
    }
}
