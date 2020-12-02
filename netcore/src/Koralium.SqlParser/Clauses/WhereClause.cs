﻿using Koralium.SqlParser.Expressions;
using Koralium.SqlParser.Visitor;

namespace Koralium.SqlParser.Clauses
{
    public class WhereClause : SqlNode
    {
        public BooleanExpression Expression { get; set; }

        public override void Accept(KoraliumSqlVisitor visitor)
        {
            visitor.VisitWhereClause(this);
        }
    }
}
