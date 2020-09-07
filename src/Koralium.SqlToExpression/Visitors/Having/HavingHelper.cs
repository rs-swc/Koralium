﻿using Koralium.SqlToExpression.Exceptions;
using Koralium.SqlToExpression.Stages.CompileStages;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Koralium.SqlToExpression.Visitors.Having
{
    internal static class HavingHelper
    {
        public static HavingStage GetHavingStage(IQueryStage queryStage, HavingClause havingClause, VisitorMetadata visitorMetadata)
        {
            if(queryStage is GroupedStage groupedStage)
            {
                HavingVisitor havingVisitor = new HavingVisitor(groupedStage, visitorMetadata);
                havingClause.Accept(havingVisitor);

                return new HavingStage(
                        groupedStage.CurrentType,
                        groupedStage.ValueType,
                        groupedStage.TypeInfo,
                        groupedStage.KeyTypeInfo,
                        groupedStage.ParameterExpression,
                        groupedStage.KeyParameterExpression,
                        groupedStage.ValueParameterExpression,
                        groupedStage.FromAliases,
                        havingVisitor.Expression
                    );
            }
            else
            {
                throw new SqlErrorException("Having can only be used in a grouped query, or a query with only aggregations");
            }
        }
    }
}
