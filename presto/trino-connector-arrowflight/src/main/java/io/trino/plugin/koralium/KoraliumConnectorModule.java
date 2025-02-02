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
package io.trino.plugin.koralium;

import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.deser.std.FromStringDeserializer;
import com.google.inject.Binder;
import com.google.inject.Module;
import com.google.inject.Scopes;
import io.trino.plugin.koralium.cache.QueryCacheFactory;
import io.trino.plugin.koralium.client.KoraliumClient;
import io.trino.plugin.koralium.client.PrestoKoraliumMetadataClient;
import io.trino.spi.NodeManager;
import io.trino.spi.type.Type;
import io.trino.spi.type.TypeId;
import io.trino.spi.type.TypeManager;

import javax.inject.Inject;

import static io.airlift.configuration.ConfigBinder.configBinder;
import static io.airlift.json.JsonBinder.jsonBinder;
import static io.airlift.json.JsonCodec.listJsonCodec;
import static io.airlift.json.JsonCodecBinder.jsonCodecBinder;
import static java.util.Objects.requireNonNull;

public class KoraliumConnectorModule
        implements Module
{
    private final TypeManager typeManager;
    private final NodeManager nodeManager;

    public KoraliumConnectorModule(TypeManager typeManager, NodeManager nodeManager)
    {
        this.typeManager = typeManager;
        this.nodeManager = nodeManager;
    }

    @Override
    public void configure(Binder binder)
    {
        binder.bind(TypeManager.class).toInstance(typeManager);
        binder.bind(NodeManager.class).toInstance(nodeManager);

        binder.bind(KoraliumConnector.class).in(Scopes.SINGLETON);
        binder.bind(KoraliumMetadata.class).in(Scopes.SINGLETON);
        binder.bind(PrestoKoraliumMetadataClient.class).in(Scopes.SINGLETON);
        binder.bind(KoraliumClient.class).in(Scopes.SINGLETON);
        binder.bind(KoraliumSplitManager.class).in(Scopes.SINGLETON);
        binder.bind(KoraliumPageSourceProvider.class).in(Scopes.SINGLETON);
        binder.bind(QueryCacheFactory.class).in(Scopes.SINGLETON);
        configBinder(binder).bindConfig(KoraliumConfig.class);

        jsonBinder(binder).addDeserializerBinding(Type.class).to(TypeDeserializer.class);
        jsonCodecBinder(binder).bindMapJsonCodec(String.class, listJsonCodec(KoraliumPrestoTable.class));
    }

    private static final class TypeDeserializer
            extends FromStringDeserializer<Type>
    {
        private static final long serialVersionUID = 1L;

        private final TypeManager typeManager;

        @Inject
        public TypeDeserializer(TypeManager typeManager)
        {
            super(Type.class);
            this.typeManager = requireNonNull(typeManager, "typeManager is null");
        }

        @Override
        protected Type _deserialize(String value, DeserializationContext context)
        {
            return typeManager.getType(TypeId.of(value));
        }
    }
}
