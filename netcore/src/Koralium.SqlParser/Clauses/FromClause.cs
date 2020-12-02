﻿using Koralium.SqlParser.From;
using Koralium.SqlParser.Visitor;

namespace Koralium.SqlParser.Clauses
{
    public class FromClause : SqlNode
    {
        public TableReference TableReference { get; set; }

        public override void Accept(KoraliumSqlVisitor visitor)
        {
            visitor.VisitFromClause(this);
        }
    }
}
