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
using Koralium.SqlParser.Expressions;
using Koralium.SqlParser.Visitor;

namespace Koralium.Core.RowLevelSecurity
{
    class AddAliasVisitor : KoraliumSqlVisitor
    {
        private readonly string _tableAlias;

        public AddAliasVisitor(string tableAlias)
        {
            _tableAlias = tableAlias;
        }

        public override void VisitColumnReference(ColumnReference columnReference)
        {
            //Add the alias to the front
            columnReference.Identifiers.Insert(0, _tableAlias);
        }
    }
}
