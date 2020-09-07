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
package io.prestosql.plugin.grpc;

import io.prestosql.spi.connector.ConnectorTableMetadata;
import io.prestosql.spi.connector.SchemaTableName;
import io.prestosql.spi.predicate.TupleDomain;

import java.util.List;
import java.util.Optional;
import java.util.OptionalLong;

public class GrpcTable
{
    private final SchemaTableName schemaTableName;
    private final int tableId;
    private final ConnectorTableMetadata connectorTableMetadata;
    private final List<GrpcColumnHandle> columnHandles;
    private final List<GrpcTableIndex> indices;

    public GrpcTable(SchemaTableName schemaTableName, int tableId, ConnectorTableMetadata connectorTableMetadata, List<GrpcColumnHandle> columnHandles, List<GrpcTableIndex> indices)
    {
        this.schemaTableName = schemaTableName;
        this.tableId = tableId;
        this.connectorTableMetadata = connectorTableMetadata;
        this.columnHandles = columnHandles;
        this.indices = indices;
    }

    public SchemaTableName getSchemaTableName()
    {
        return schemaTableName;
    }

    public GrpcTableHandle getTableHandle()
    {
        return new GrpcTableHandle(schemaTableName.getSchemaName(), schemaTableName.getTableName(), TupleDomain.all(), Optional.empty(), tableId, OptionalLong.empty(), Optional.empty());
    }

    public ConnectorTableMetadata getConnectorTableMetadata()
    {
        return connectorTableMetadata;
    }

    public String getSchemaName()
    {
        return schemaTableName.getSchemaName();
    }

    public List<GrpcColumnHandle> getColumnHandles()
    {
        return columnHandles;
    }

    public List<GrpcTableIndex> getIndices()
    {
        return indices;
    }
}
