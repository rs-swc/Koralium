﻿/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Koralium.SqlToExpression
{
    public class SqlParameters
    {
        private readonly Dictionary<string, SqlParameter> _parameters = new Dictionary<string, SqlParameter>();

        public SqlParameters Add(SqlParameter sqlParameter)
        {
            if (!sqlParameter.Name.StartsWith("@"))
            {
                _parameters.Add($"@{sqlParameter.Name.ToLower()}", sqlParameter);
            }
            else
            {
                _parameters.Add(sqlParameter.Name.ToLower(), sqlParameter);
            }
            return this;
        }

        public bool TryGetParameter(string name, out SqlParameter sqlParameter)
        {
            return _parameters.TryGetValue(name.ToLower(), out sqlParameter);
        }
    }
}
