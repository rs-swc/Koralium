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

namespace Koralium.SqlParser.Statements
{
    public class SetVariableStatement : Statement
    {
        public VariableReference VariableReference { get; set; }

        public ScalarExpression ScalarExpression { get; set; }

        public override void Accept(KoraliumSqlVisitor visitor)
        {
            visitor.VisitSetVariableStatement(this);
        }

        public override SqlNode Clone()
        {
            return new SetVariableStatement()
            {
                ScalarExpression = ScalarExpression.Clone() as ScalarExpression,
                VariableReference = VariableReference.Clone() as VariableReference
            };
        }
    }
}
