﻿using Koralium.SqlParser.Expressions;
using Koralium.SqlParser.Visitor;
using System.Text;

namespace Koralium.SqlParser.OrderBy
{
    public class OrderExpression : OrderElement
    {
        public ScalarExpression Expression { get; set; }

        public override void Accept(KoraliumSqlVisitor visitor)
        {
            visitor.VisitOrderExpression(this);
        }
    }
}
