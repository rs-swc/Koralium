﻿using Koralium.SqlParser.Visitor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koralium.SqlParser.From
{
    public class FromTableReference : TableReference
    {
        public string TableName { get; set; }

        public override void Accept(KoraliumSqlVisitor visitor)
        {
            visitor.VisitFromTableReference(this);
        }
    }
}
