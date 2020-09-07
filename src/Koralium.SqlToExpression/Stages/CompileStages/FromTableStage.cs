﻿using Koralium.SqlToExpression.Models;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Koralium.SqlToExpression.Stages.CompileStages
{
    internal class FromTableStage : IQueryStage
    {
        public string TableName { get; }

        public SqlTypeInfo TypeInfo { get; }

        public ParameterExpression ParameterExpression { get; }

        public Type CurrentType { get; }

        public FromAliases FromAliases { get; }

        public FromTableStage(
            string tableName, 
            SqlTypeInfo sqlTypeInfo, 
            ParameterExpression parameterExpression, 
            Type currentType,
            FromAliases fromAliases)
        {
            Debug.Assert(tableName != null, $"{nameof(tableName)} was null");
            Debug.Assert(sqlTypeInfo != null, $"{nameof(sqlTypeInfo)} was null");
            Debug.Assert(parameterExpression != null, $"{nameof(parameterExpression)} was null");
            Debug.Assert(currentType != null, $"{nameof(currentType)} was null");
            Debug.Assert(fromAliases != null, $"{nameof(fromAliases)} was null");

            TableName = tableName;
            TypeInfo = sqlTypeInfo;
            ParameterExpression = parameterExpression;
            CurrentType = currentType;
            FromAliases = fromAliases;
        }

        public void Accept(IQueryStageVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
