﻿/*
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
using Apache.Arrow;
using NUnit.Framework;
using System.Linq;

namespace Koralium.Transport.ArrowFlight.Tests
{
    public class FieldComparer
    {
        public static void Compare(Field expected, Field actual)
        {
            if (ReferenceEquals(expected, actual))
            {
                return;
            }

            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.IsNullable, actual.IsNullable);

            Assert.AreEqual(expected.HasMetadata, actual.HasMetadata);
            if (expected.HasMetadata)
            {
                Assert.AreEqual(expected.Metadata.Keys.Count(), actual.Metadata.Keys.Count());
                Assert.True(expected.Metadata.Keys.All(k => actual.Metadata.ContainsKey(k) && expected.Metadata[k] == actual.Metadata[k]));
                Assert.True(actual.Metadata.Keys.All(k => expected.Metadata.ContainsKey(k) && actual.Metadata[k] == expected.Metadata[k]));
            }

            actual.DataType.Accept(new ArrayTypeComparer(expected.DataType));
        }
    }
}
