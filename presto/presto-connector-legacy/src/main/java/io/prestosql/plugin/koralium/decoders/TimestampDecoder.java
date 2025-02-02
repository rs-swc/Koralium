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

import com.google.protobuf.Timestamp;
import io.prestosql.plugin.koralium.KoraliumExecutionColumn;
import io.prestosql.plugin.koralium.Presto;
import io.prestosql.spi.block.BlockBuilder;
import io.prestosql.spi.connector.ConnectorSession;
import io.prestosql.spi.type.Timestamps;
import io.prestosql.spi.type.Type;

import java.time.Instant;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.List;

import static io.prestosql.spi.type.TimestampType.TIMESTAMP_MILLIS;

public class TimestampDecoder
        implements KoraliumDecoder
{
    private static final ZoneId ZULU = ZoneId.of("Z");
    private final ZoneId zoneId;
    private final Type type;

    private int count;
    private int globalCount;
    private int nullCounter;

    public TimestampDecoder()
    {
        zoneId = ZoneId.of("Z");
        this.type = TIMESTAMP_MILLIS;
    }

    public TimestampDecoder(ConnectorSession session, Type type)
    {
        this.zoneId = ZoneId.of(session.getTimeZoneKey().getId());
        this.type = type;
    }

    @Override
    public KoraliumDecoder create(int columnId, KoraliumExecutionColumn column, ConnectorSession session)
    {
        return new TimestampDecoder(session, column.getPrestoType());
    }

    @Override
    public int decode(Presto.Block block, BlockBuilder builder, Presto.Page page)
    {
        List<Integer> nulls = block.getNullsList();
        List<Timestamp> timestamps = block.getTimestamps().getValuesList();

        int count = 0;
        int globalCount = 0;
        int nullCounter = 0;
        while (nulls.size() > nullCounter || timestamps.size() > count) {
            //Check null list if there are elements left
            if (nulls.size() > nullCounter) {
                if (nulls.get(nullCounter) == globalCount) {
                    builder.appendNull();
                    nullCounter++;
                    globalCount++;
                    continue;
                }
            }
            if (timestamps.size() > count) {
                Timestamp grpcTimestamp = timestamps.get(count);

                Instant instant = Instant.ofEpochSecond(grpcTimestamp.getSeconds(), grpcTimestamp.getNanos());
                LocalDateTime timestamp = LocalDateTime.ofInstant(instant, ZULU);

                long epochMillis = timestamp.atZone(zoneId)
                        .toInstant()
                        .toEpochMilli() * Timestamps.MICROSECONDS_PER_MILLISECOND;

                type.writeLong(builder, epochMillis);
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
        List<Timestamp> timestamps = block.getTimestamps().getValuesList();

        int localCount = 0;
        while ((nulls.size() > nullCounter || timestamps.size() > count) && numberOfElements > localCount) {
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
            if (timestamps.size() > count) {
                Timestamp grpcTimestamp = timestamps.get(count);

                Instant instant = Instant.ofEpochSecond(grpcTimestamp.getSeconds(), grpcTimestamp.getNanos());

                type.writeLong(builder, instant.toEpochMilli() * Timestamps.MICROSECONDS_PER_MILLISECOND);
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
