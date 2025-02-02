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

import io.prestosql.plugin.koralium.KoraliumExecutionColumn;
import io.prestosql.plugin.koralium.Presto;
import io.prestosql.spi.block.BlockBuilder;
import io.prestosql.spi.connector.ConnectorSession;

import java.util.List;

import static io.prestosql.spi.type.IntegerType.INTEGER;

public class IntDecoder
        implements KoraliumDecoder
{
    private int count;
    private int globalCount;
    private int nullCounter;

    @Override
    public KoraliumDecoder create(int columnId, KoraliumExecutionColumn column, ConnectorSession session)
    {
        return new IntDecoder();
    }

    @Override
    public int decode(Presto.Block block, BlockBuilder builder, Presto.Page page)
    {
        List<Integer> nulls = block.getNullsList();
        List<Integer> integers = block.getInts().getValuesList();

        int count = 0;
        int globalCount = 0;
        int nullCounter = 0;

        while (nulls.size() > nullCounter || integers.size() > count) {
            //Check null list if there are elements left
            if (nulls.size() > nullCounter) {
                if (nulls.get(nullCounter) == globalCount) {
                    builder.appendNull();
                    nullCounter++;
                    globalCount++;
                    continue;
                }
            }
            if (integers.size() > count) {
                INTEGER.writeLong(builder, integers.get(count));
                count++;
                globalCount++;
            }
        }
        return globalCount;
    }

    @Override
    public int decode(Presto.Block block, BlockBuilder builder, Presto.Page page, int numberOfElements)
    {
        List<Integer> nulls = block.getNullsList();
        List<Integer> integers = block.getInts().getValuesList();

        int localCount = 0;
        while ((nulls.size() > nullCounter || integers.size() > count) && numberOfElements > localCount) {
            //Check null list if there are elements left
            if (nulls.size() > nullCounter) {
                if (nulls.get(nullCounter) == globalCount) {
                    builder.appendNull();
                    nullCounter++;
                    globalCount++;
                    localCount++;
                    continue;
                }
            }
            if (integers.size() > count) {
                INTEGER.writeLong(builder, integers.get(count));
                count++;
                globalCount++;
                localCount++;
            }
        }
        return 0;
    }

    @Override
    public void newPage(Presto.Page page)
    {
        this.count = 0;
        this.globalCount = 0;
        this.nullCounter = 0;
    }
}
