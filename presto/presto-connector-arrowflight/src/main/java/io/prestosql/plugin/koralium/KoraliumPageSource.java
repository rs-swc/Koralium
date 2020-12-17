package io.prestosql.plugin.koralium;

import com.google.common.collect.ImmutableMap;
import io.prestosql.plugin.koralium.client.KoraliumClient;
import io.prestosql.plugin.koralium.decoders.Int64Decoder;
import io.prestosql.plugin.koralium.decoders.KoraliumDecoder;
import io.prestosql.plugin.koralium.utils.SchemaToDecoders;
import io.prestosql.spi.Page;
import io.prestosql.spi.block.Block;
import io.prestosql.spi.block.BlockBuilder;
import io.prestosql.spi.connector.ConnectorPageSource;
import io.prestosql.spi.connector.ConnectorSession;
import org.apache.arrow.flight.FlightStream;
import org.apache.arrow.vector.BigIntVector;
import org.apache.arrow.vector.FieldVector;
import org.apache.arrow.vector.VectorSchemaRoot;
import org.apache.arrow.vector.VectorUnloader;
import org.apache.arrow.vector.types.pojo.Schema;

import java.io.IOException;
import java.net.URI;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicLong;

public class KoraliumPageSource
        implements ConnectorPageSource
{
    private final KoraliumSplit split;
    private final KoraliumClient client;
    private final List<KoraliumPrestoColumn> columns;
    private final KoraliumDecoder[] decoders;
    private final BlockBuilder[] columnBuilders;

    private final AtomicBoolean finished;
    private final ConcurrentLinkedQueue<Page> completedBatches = new ConcurrentLinkedQueue<>();
    private long completedBytes;
    private final AtomicLong readTimeNanos = new AtomicLong(0);
    private final long readStart;
    private Throwable error;

    public KoraliumPageSource(KoraliumSplit split,
                              List<KoraliumPrestoColumn> columns,
                              ConnectorSession session)
    {
        this.split = split;
        this.columns = columns;
        finished = new AtomicBoolean();
        readStart = System.nanoTime();

        columnBuilders = columns.stream()
                .map(KoraliumPrestoColumn::getType)
                .map(type -> type.createBlockBuilder(null, 1))
                .toArray(BlockBuilder[]::new);

        URI uri = split.getUriAddresses().get(0);
        this.client = new KoraliumClient(uri);

        String authToken = session.getIdentity().getExtraCredentials().get("auth_token");
        Map<String, String> headers = null;

        if (authToken != null) {
            headers = ImmutableMap.of("Authorization", "Bearer " + authToken);
        }

        FlightStream stream = client.GetStream(split.getTicket(), headers);
        Schema schema = stream.getSchema();


        //Create decoders using the schema
        decoders = SchemaToDecoders.createDecoders(session, schema, columns);

        if (split.isCount()) {
            readCount(stream);
        }
        else {
            readQuery(stream);
        }
    }

    private void readCount(FlightStream stream)
    {
        VectorSchemaRoot root = stream.getRoot();

        if(stream.next()) {
            BigIntVector vector = (BigIntVector) root.getVector(0);
            long val = vector.get(0);
            completedBatches.add(new Page((int)val));
        }
        else {
            completedBatches.add(new Page(0));
        }
        finished.set(true);
        setFinalReadTime();
    }

    private void readQuery(FlightStream stream)
    {
        CompletableFuture.runAsync(() -> {
            try{
                VectorSchemaRoot root = stream.getRoot();
                while (stream.next()) {
                    if (!stream.hasRoot()) {
                        continue;
                    }

                    int rowCount = root.getRowCount();

                    for (int i = 0; i < columnBuilders.length; i++) {
                        decoders[i].decode(root.getVector(i), columnBuilders[i], 0, rowCount);
                    }

                    completedBytes += Arrays.stream(columnBuilders)
                            .mapToLong(BlockBuilder::getSizeInBytes)
                            .sum();

                    Block[] blocks = new Block[columnBuilders.length];
                    for (int i = 0; i < columnBuilders.length; i++) {
                        blocks[i] = columnBuilders[i].build();
                        columnBuilders[i] = columnBuilders[i].newBlockBuilderLike(null);
                    }

                    completedBatches.add(new Page(blocks));
                }

                try {
                    stream.close(); //Close the stream
                } catch (Exception e) {
                    e.printStackTrace();
                }

                finished.set(true);
                setFinalReadTime();
            }
            catch (Exception e) {
                e.printStackTrace();
                this.error = e;
                finished.set(true);
                setFinalReadTime();
            }

        });
    }

    protected void setFinalReadTime()
    {
        readTimeNanos.set(System.nanoTime() - readStart);
    }

    @Override
    public long getCompletedBytes() {
        return completedBytes;
    }

    @Override
    public long getReadTimeNanos() {
        return readTimeNanos.get();
    }

    @Override
    public boolean isFinished() {
        return finished.get() && completedBatches.isEmpty();
    }

    @Override
    public Page getNextPage()
    {
        if (this.error != null) {
            throw new RuntimeException(error);
        }
        if(completedBatches.isEmpty())
        {
            return null;
        }
        return completedBatches.poll();
    }

    @Override
    public long getSystemMemoryUsage() {
        return 0;
    }

    @Override
    public void close() throws IOException {

    }
}
