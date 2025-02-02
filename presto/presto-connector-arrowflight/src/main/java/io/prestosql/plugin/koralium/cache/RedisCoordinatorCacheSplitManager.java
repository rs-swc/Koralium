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
package io.prestosql.plugin.koralium.cache;

import io.prestosql.spi.HostAddress;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;

import static java.util.Objects.requireNonNull;

public class RedisCoordinatorCacheSplitManager
        implements QueryCacheSplitManager
{
    private final JedisPool jedisPool;

    public RedisCoordinatorCacheSplitManager(HostAddress redisUrl)
    {
        requireNonNull(redisUrl, "redisUrl was null");

        JedisPoolConfig poolConfig = new JedisPoolConfig();
        poolConfig.setMaxTotal(128);
        jedisPool = new JedisPool(poolConfig, requireNonNull(redisUrl.getHostText(), "redisUrl was null"), redisUrl.getPort());
    }

    @Override
    public HostAddress getCacheNode(String query)
    {
        Jedis jedis = null;
        try {
            jedis = jedisPool.getResource();

            String hostAddress = jedis.get(query);

            if (hostAddress == null) {
                return null;
            }

            return HostAddress.fromString(hostAddress);
        }
        catch (Exception e) {
            e.printStackTrace();
        }
        finally {
            if (jedis != null) {
                jedis.close();
            }
        }
        return null;
    }
}
