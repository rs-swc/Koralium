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
using Koralium.SqlParser.Literals;
using Koralium.SqlParser.Statements;
using NUnit.Framework;
using System.Collections.Generic;

namespace Koralium.SqlParser.SmokeTests
{
    public partial class SmokeTestsBase
    {
        [Test]
        public void TestFunctionCall()
        {
            var actual = Parser.Parse("SELECT sum(c1) FROM test").Statements;
            var expected = new List<Statement>()
            {
                new SelectStatement()
                {
                    FromClause = new Clauses.FromClause()
                    {
                        TableReference = new FromTableReference()
                        {
                            TableName = "test"
                        }
                    },
                    SelectElements = new List<SelectExpression>()
                    {
                        new SelectScalarExpression()
                        {
                            Expression = new FunctionCall()
                            {
                                FunctionName = "sum",
                                Parameters = new List<SqlExpression>()
                                {
                                    new ColumnReference()
                                    {
                                        Identifiers = new List<string>(){ "c1" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            actual.Should().BeEquivalentTo(expected, x => x.RespectingRuntimeTypes());
        }

        [Test]
        public void TestFunctionCallUppercase()
        {
            var actual = Parser.Parse("SELECT SUM(c1) FROM test").Statements;
            var expected = new List<Statement>()
            {
                new SelectStatement()
                {
                    FromClause = new Clauses.FromClause()
                    {
                        TableReference = new FromTableReference()
                        {
                            TableName = "test"
                        }
                    },
                    SelectElements = new List<SelectExpression>()
                    {
                        new SelectScalarExpression()
                        {
                            Expression = new FunctionCall()
                            {
                                FunctionName = "sum",
                                Parameters = new List<SqlExpression>()
                                {
                                    new ColumnReference()
                                    {
                                        Identifiers = new List<string>(){ "c1" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            actual.Should().BeEquivalentTo(expected, x => x.RespectingRuntimeTypes());
        }

        [Test]
        public void TestFunctionCallWildcard()
        {
            var actual = Parser.Parse("SELECT count(*) FROM test").Statements;
            var expected = new List<Statement>()
            {
                new SelectStatement()
                {
                    FromClause = new Clauses.FromClause()
                    {
                        TableReference = new FromTableReference()
                        {
                            TableName = "test"
                        }
                    },
                    SelectElements = new List<SelectExpression>()
                    {
                        new SelectScalarExpression()
                        {
                            Expression = new FunctionCall()
                            {
                                FunctionName = "count",
                                Wildcard = true
                            }
                        }
                    }
                }
            };

            actual.Should().BeEquivalentTo(expected, x => x.RespectingRuntimeTypes());
        }

        [Test]
        public void TestFunctionCallWithLambda()
        {
            var actual = Parser.Parse("SELECT any_match(arr, x -> x = 'test') FROM test").Statements;
            var expected = new List<Statement>()
            {
                new SelectStatement()
                {
                    FromClause = new Clauses.FromClause()
                    {
                        TableReference = new FromTableReference()
                        {
                            TableName = "test"
                        }
                    },
                    SelectElements = new List<SelectExpression>()
                    {
                        new SelectScalarExpression()
                        {
                            Expression = new FunctionCall()
                            {
                                FunctionName = "any_match",
                                Wildcard = false,
                                Parameters = new List<SqlExpression>()
                                {
                                    new ColumnReference()
                                    {
                                        Identifiers = new List<string>() { "arr" }
                                    },
                                    new LambdaExpression()
                                    {
                                        Parameters = new List<string>() { "x" },
                                        Expression = new BooleanComparisonExpression()
                                        {
                                            Type = BooleanComparisonType.Equals,
                                            Left = new ColumnReference()
                                            {
                                                Identifiers = new List<string>() { "x" }
                                            },
                                            Right = new StringLiteral()
                                            {
                                                Value = "test"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            actual.Should().BeEquivalentTo(expected, x => x.RespectingRuntimeTypes());
        }

        [Test]
        public void TestFunctionCallInWhere()
        {
            var actual = Parser.Parse("SELECT * FROM test WHERE any_match(arr, x -> x = 'test')").Statements;
            var expected = new List<Statement>()
            {
                new SelectStatement()
                {
                    FromClause = new Clauses.FromClause()
                    {
                        TableReference = new FromTableReference()
                        {
                            TableName = "test"
                        }
                    },
                    SelectElements = new List<SelectExpression>()
                    {
                        new SelectStarExpression()
                    },
                    WhereClause = new Clauses.WhereClause()
                    {
                        Expression = new BooleanScalarExpression()
                        {
                            ScalarExpression = new FunctionCall()
                            {
                                FunctionName = "any_match",
                                Wildcard = false,
                                Parameters = new List<SqlExpression>()
                                {
                                    new ColumnReference()
                                    {
                                        Identifiers = new List<string>() { "arr" }
                                    },
                                    new LambdaExpression()
                                    {
                                        Parameters = new List<string>() { "x" },
                                        Expression = new BooleanComparisonExpression()
                                        {
                                            Type = BooleanComparisonType.Equals,
                                            Left = new ColumnReference()
                                            {
                                                Identifiers = new List<string>() { "x" }
                                            },
                                            Right = new StringLiteral()
                                            {
                                                Value = "test"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            actual.Should().BeEquivalentTo(expected, x => x.RespectingRuntimeTypes());
        }
    }
}
