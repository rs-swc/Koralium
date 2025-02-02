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
using Koralium.Shared;
using Koralium.SqlParser;
using Koralium.SqlParser.Expressions;
using Koralium.SqlParser.Literals;
using Koralium.SqlParser.Statements;
using Koralium.SqlParser.Visitor;
using Koralium.SqlToExpression.Stages.CompileStages;
using Koralium.SqlToExpression.Utils;
using Koralium.SqlToExpression.Visitors.Analyzers;
using Koralium.SqlToExpression.Visitors.Distinct;
using Koralium.SqlToExpression.Visitors.From;
using Koralium.SqlToExpression.Visitors.GroupBy;
using Koralium.SqlToExpression.Visitors.Having;
using Koralium.SqlToExpression.Visitors.Offset;
using Koralium.SqlToExpression.Visitors.OrderBy;
using Koralium.SqlToExpression.Visitors.Select;
using Koralium.SqlToExpression.Visitors.Where;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Koralium.SqlToExpression.Visitors
{
    internal class MainVisitor : KoraliumSqlVisitor
    {
        private readonly List<IQueryStage> _stages = new List<IQueryStage>();
        private readonly VisitorMetadata _visitorMetadata;
        private readonly HashSet<PropertyInfo> _usedProperties = new HashSet<PropertyInfo>();
        private FromTableStage _fromTable;

        public IReadOnlyList<IQueryStage> Stages => _stages;

        private IQueryStage LastStage => _stages.Last();

        public MainVisitor(VisitorMetadata visitorMetadata)
        {
            _visitorMetadata = visitorMetadata;
        }

        private void HandleFromClause(SelectStatement selectStatement)
        {
            if (selectStatement.FromClause != null)
            {
                var fromStages = FromHelper.GetFromTableStage(selectStatement.FromClause, _visitorMetadata);

                //Check if it is only a from table stage, this is used to add used properties into
                if (fromStages.Count == 1 && fromStages[0] is FromTableStage fromTableStage)
                {
                    _fromTable = fromTableStage;
                }

                _stages.AddRange(fromStages);
            }
            else
            {
                throw new SqlErrorException("Selects must always have FROM");
            }
        }

        private void HandleWhereClause(SelectStatement selectStatement)
        {
            if (selectStatement.WhereClause != null)
            {
                var whereStage = WhereHelper.GetWhereStage(LastStage, selectStatement.WhereClause, _visitorMetadata, _usedProperties);

                if (LastStage is FromTableStage fromTableStage)
                {
                    //Push the where condition into the table resolvers
                    fromTableStage.WhereExpression = whereStage.WhereExpression;
                    fromTableStage.ContainsFullTextSearch = whereStage.ContainsFullTextSearch;
                }
                else
                {
                    //If its not possible to push up, add it as a normal stage
                    _stages.Add(whereStage);
                }
            }
        }

        private void HandleGroupByClause(SelectStatement selectStatement, bool containsAggregates)
        {
            if (selectStatement.GroupByClause != null)
            {
                _stages.Add(GroupByHelper.GetGroupByStage(LastStage, selectStatement.GroupByClause, _usedProperties, _visitorMetadata));
            }
            else if (containsAggregates && !IsSimpleAggregate(selectStatement))
            {
                _stages.Add(GroupByUtils.CreateStaticGroupBy(LastStage));
            }
        }

        private static bool IsSimpleAggregate(SelectStatement selectStatement)
        {
            if (selectStatement.SelectElements.Count > 1 || selectStatement.HavingClause != null || selectStatement.GroupByClause != null)
            {
                return false;
            }
            if (selectStatement.SelectElements.Count == 1 &&
                selectStatement.SelectElements[0] is SelectScalarExpression selectScalarExpression &&
                selectScalarExpression.Expression is FunctionCall)
            {
                return true;
            }
            return false;
        }

        private void HandleHavingClause(SelectStatement selectStatement)
        {
            if (selectStatement.HavingClause != null)
            {
                _stages.Add(HavingHelper.GetHavingStage(LastStage, selectStatement.HavingClause, _visitorMetadata, _usedProperties));
            }
        }

        private void HandleOrderByClause(SelectStatement selectStatement)
        {
            if (selectStatement.OrderByClause != null)
            {
                _stages.Add(OrderByHelper.GetOrderByStage(LastStage, selectStatement.OrderByClause, _visitorMetadata, _usedProperties));
            }
        }

        private void HandleSelect(SelectStatement selectStatement, bool containsAggregates)
        {
            if (containsAggregates && IsSimpleAggregate(selectStatement))
            {
                _stages.Add(SelectHelper.GetSelectAggregateFunctionStage(LastStage, selectStatement.SelectElements, _visitorMetadata, _usedProperties));
            }
            else
            {
                _stages.Add(SelectHelper.GetSelectStage(LastStage, selectStatement.SelectElements, _visitorMetadata, _usedProperties));
            }
        }

        private void HandleDistinct(SelectStatement selectStatement)
        {
            if (selectStatement.Distinct)
            {
                _stages.Add(DistinctHelper.GetDistinctStage(LastStage));
            }
        }

        private void HandleOffsetClause(SelectStatement selectStatement)
        {
            if (selectStatement.OffsetLimitClause != null)
            {
                var offsetStage = OffsetHelper.GetOffsetStage(LastStage, selectStatement.OffsetLimitClause, _visitorMetadata);

                //Check if we can push the values into the table scan
                if (LastStage is FromTableStage fromTableStage)
                {
                    fromTableStage.Limit = offsetStage.Take;
                    fromTableStage.Offset = offsetStage.Skip;
                }
                else
                {
                    _stages.Add(OffsetHelper.GetOffsetStage(LastStage, selectStatement.OffsetLimitClause, _visitorMetadata));
                }
            }
        }

        public override void VisitSetVariableStatement(SetVariableStatement setVariableStatement)
        {
            //Parameters from other sources go before inline parameters
            if (!_visitorMetadata.Parameters.Contains(setVariableStatement.VariableReference.Name))
            {
                if (setVariableStatement.ScalarExpression is StringLiteral stringLiteral)
                {
                    _visitorMetadata.Parameters.Add(SqlParameter.Create(setVariableStatement.VariableReference.Name, stringLiteral.Value));
                }
                else if (setVariableStatement.ScalarExpression is IntegerLiteral integerLiteral)
                {
                    _visitorMetadata.Parameters.Add(SqlParameter.Create(setVariableStatement.VariableReference.Name, integerLiteral.Value));
                }
                else if (setVariableStatement.ScalarExpression is NumericLiteral numericLiteral)
                {
                    _visitorMetadata.Parameters.Add(SqlParameter.Create(setVariableStatement.VariableReference.Name, numericLiteral.Value));
                }
                else if (setVariableStatement.ScalarExpression is Base64Literal base64Literal)
                {
                    var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64Literal.Value));
                    _visitorMetadata.Parameters.Add(SqlParameter.Create(setVariableStatement.VariableReference.Name, decodedString));
                }
                else if (setVariableStatement.ScalarExpression is BooleanLiteral booleanLiteral)
                {
                    _visitorMetadata.Parameters.Add(SqlParameter.Create(setVariableStatement.VariableReference.Name, booleanLiteral.Value));
                }
                else
                {
                    throw new NotImplementedException($"The parameter type: {setVariableStatement.ScalarExpression.GetType().Name} is not implemented");
                }
            }
        }

        public override void VisitSelectStatement(SelectStatement selectStatement)
        {
            HandleFromClause(selectStatement);
            HandleWhereClause(selectStatement);

            var containsAggregates = ContainsAggregateHelper.ContainsAggregate(selectStatement.SelectElements);

            HandleGroupByClause(selectStatement, containsAggregates);
            HandleHavingClause(selectStatement);
            HandleOrderByClause(selectStatement);
            HandleSelect(selectStatement, containsAggregates);
            HandleDistinct(selectStatement);
            HandleOffsetClause(selectStatement);

            //Add a select stage that selects all the properties required
            //This should be added to be done directly after the 'from table' stage
            //This helps solve some issues with automapper related issues where extra complicated queries in
            //entity framework are created
            if (_fromTable != null && _usedProperties.Count > 0)
            {
                _fromTable.SelectExpression = SelectExpressionUtils.CreateSelectExpression(_fromTable, _usedProperties);
            }
        }
    }
}
