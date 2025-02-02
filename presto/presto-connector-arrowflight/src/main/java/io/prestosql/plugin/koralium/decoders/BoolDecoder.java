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
package io.prestosql.plugin.koralium.decoders;

import io.prestosql.spi.block.BlockBuilder;
import io.prestosql.spi.connector.ConnectorSession;
import io.prestosql.spi.type.Type;
import org.apache.arrow.vector.BitVector;
import org.apache.arrow.vector.types.pojo.Field;

import static io.prestosql.spi.type.BooleanType.BOOLEAN;

public class BoolDecoder
        extends PrimitiveDecoder<BitVector>
{
    @Override
    protected PrimitiveDecoder<BitVector> createDecoder(Field field, ConnectorSession connectorSession, Type prestoType)
    {
        return new BoolDecoder();
    }

    @Override
    void writeValue(BitVector vector, BlockBuilder builder, int index)
    {
        BOOLEAN.writeBoolean(builder, vector.get(index) == 1);
    }
}
