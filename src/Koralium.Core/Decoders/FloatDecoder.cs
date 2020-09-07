﻿using Koralium.Core.Interfaces;
using Koralium.Core.Metadata;
using Koralium.Grpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koralium.Core.Decoders
{
    public class FloatDecoder : BaseDecoder
    {
        public override IDecoder Create(int columnId, TableColumn column)
        {
            return new FloatDecoder();
        }

        public override void Decode(Block block, Action<int, object> setter, int? numberOfRows = int.MaxValue)
        {
            var nulls = block.Nulls;
            var values = block.Floats.Values;

            Decode(nulls, values, setter, numberOfRows);
        }
    }
}
