/*
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
package io.trino.plugin.koralium.utils;

import io.trino.plugin.koralium.KoraliumType;
import io.trino.spi.type.BigintType;
import io.trino.spi.type.BooleanType;
import io.trino.spi.type.DecimalType;
import io.trino.spi.type.DoubleType;
import io.trino.spi.type.IntegerType;
import io.trino.spi.type.RealType;
import io.trino.spi.type.SmallintType;
import io.trino.spi.type.TimestampType;
import io.trino.spi.type.VarbinaryType;
import org.apache.arrow.vector.types.pojo.ArrowType;

import static io.trino.spi.type.VarcharType.createUnboundedVarcharType;

public class PrestoArrowTypeVisitor
        implements ArrowType.ArrowTypeVisitor<TypeConvertResult>
{
    @Override
    public TypeConvertResult visit(ArrowType.Null aNull)
    {
        throw new IllegalArgumentException("Null type is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Struct struct)
    {
        return null;
    }

    @Override
    public TypeConvertResult visit(ArrowType.List list)
    {
        return null;
    }

    @Override
    public TypeConvertResult visit(ArrowType.LargeList largeList)
    {
        throw new IllegalArgumentException("Large list is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.FixedSizeList fixedSizeList)
    {
        throw new IllegalArgumentException("Fixed size list is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Union union)
    {
        throw new IllegalArgumentException("Union is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Map map)
    {
        throw new IllegalArgumentException("Map is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Int anInt)
    {
        if (anInt.getIsSigned()) {
            switch (anInt.getBitWidth()) {
                case 8:
                    return new TypeConvertResult(SmallintType.SMALLINT, KoraliumType.INT32);
                case 16:
                    return new TypeConvertResult(SmallintType.SMALLINT, KoraliumType.INT16);
                case 32:
                    return new TypeConvertResult(IntegerType.INTEGER, KoraliumType.INT32);
                case 64:
                    return new TypeConvertResult(BigintType.BIGINT, KoraliumType.INT64);
                default:
                    throw new IllegalArgumentException("only 8, 16, 32, 64 supported: " + anInt);
            }
        }
        else {
            switch (anInt.getBitWidth()) {
                case 8:
                    return new TypeConvertResult(SmallintType.SMALLINT, KoraliumType.UINT8);
                case 32:
                    return new TypeConvertResult(BigintType.BIGINT, KoraliumType.UINT32);
                case 64:
                    return new TypeConvertResult(BigintType.BIGINT, KoraliumType.UINT64);
                default:
                    throw new IllegalArgumentException("only 8, 16, 32, 64 supported: " + anInt);
            }
        }
    }

    @Override
    public TypeConvertResult visit(ArrowType.FloatingPoint floatingPoint)
    {
        switch (floatingPoint.getPrecision()) {
            case HALF:
                throw new UnsupportedOperationException("Half is not supported");
            case SINGLE:
                return new TypeConvertResult(RealType.REAL, KoraliumType.FLOAT);
            case DOUBLE:
                return new TypeConvertResult(DoubleType.DOUBLE, KoraliumType.DOUBLE);
            default:
                throw new IllegalArgumentException("unknown precision: " + floatingPoint);
        }
    }

    @Override
    public TypeConvertResult visit(ArrowType.Utf8 utf8)
    {
        return new TypeConvertResult(createUnboundedVarcharType(), KoraliumType.STRING);
    }

    @Override
    public TypeConvertResult visit(ArrowType.LargeUtf8 largeUtf8)
    {
        throw new IllegalArgumentException("Large utf8 is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Binary binary)
    {
        return new TypeConvertResult(VarbinaryType.VARBINARY, KoraliumType.BINARY);
    }

    @Override
    public TypeConvertResult visit(ArrowType.LargeBinary largeBinary)
    {
        throw new IllegalArgumentException("Large binary is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.FixedSizeBinary fixedSizeBinary)
    {
        throw new IllegalArgumentException("Fixed size binary is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Bool bool)
    {
        return new TypeConvertResult(BooleanType.BOOLEAN, KoraliumType.BOOL);
    }

    @Override
    public TypeConvertResult visit(ArrowType.Decimal decimal)
    {
        return new TypeConvertResult(DecimalType.createDecimalType(decimal.getPrecision(), decimal.getScale()), KoraliumType.DECIMAL);
    }

    @Override
    public TypeConvertResult visit(ArrowType.Date date)
    {
        throw new IllegalArgumentException("Date is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Time time)
    {
        throw new IllegalArgumentException("Time is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Timestamp timestamp)
    {
        return new TypeConvertResult(TimestampType.TIMESTAMP_MILLIS, KoraliumType.TIMESTAMP);
    }

    @Override
    public TypeConvertResult visit(ArrowType.Interval interval)
    {
        throw new IllegalArgumentException("Interval is not supported");
    }

    @Override
    public TypeConvertResult visit(ArrowType.Duration duration)
    {
        throw new IllegalArgumentException("Duration is not supported");
    }
}
