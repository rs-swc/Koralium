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
using FluentAssertions;
using Koralium.SqlParser.Expressions;
using Koralium.SqlParser.From;
using Koralium.SqlParser.Statements;
using NUnit.Framework;
using System.Collections.Generic;

namespace Koralium.SqlParser.SmokeTests
{
    public partial class SmokeTestsBase
    {
        [Test]
        public void TestSelectFromSubquery()
        {
            var actual = Parser.Parse("SELECT * FROM (SELECT * FROM TEST) t").Statements;

            var expected = new List<Statement>()
            {
                new SelectStatement()
                {
                    FromClause = new Clauses.FromClause()
                    {
                        TableReference = new Subquery()
                        {
                            Alias = "t",
                            SelectStatement = new SelectStatement()
                            {
                                SelectElements = new List<SelectExpression>()
                                {
                                    new SelectStarExpression()
                                },
                                FromClause = new Clauses.FromClause()
                                {
                                    TableReference = new FromTableReference()
                                    {
                                        TableName = "TEST"
                                    }
                                }
                            }
                        }
                    },
                    SelectElements = new List<SelectExpression>()
                    {
                       new SelectStarExpression()
                    }
                }
            };

            actual.Should().BeEquivalentTo(expected, x => x.RespectingRuntimeTypes());
        }
    }
}
