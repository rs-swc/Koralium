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

public class DisabledQueryCache
        implements QueryCache
{
    @Override
    public boolean containsQuery(String query)
    {
        return false;
    }

    @Override
    public QueryCacheEntry newEntry(String query)
    {
        return new DisabledQueryCacheEntry();
    }

    @Override
    public QueryCacheEntry get(String query)
    {
        return null;
    }

    @Override
    public void add(QueryCacheEntry entry)
    {
        //Do nothing
    }
}
