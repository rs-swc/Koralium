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
using Koralium.Data.ArrowFlight;
using Koralium.WebTests.Entities;
using NUnit.Framework;

namespace Data.Koralium.Tests
{
    public class ClientTests
    {
        private TestWebFactory webFactory;
        [OneTimeSetUp]
        public void Setup()
        {
            webFactory = new TestWebFactory();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            webFactory.Stop();
        }

        [Test]
        public void TestSelectString()
        {
            KoraliumConnectionStringBuilder builder = new KoraliumConnectionStringBuilder();
            builder.DataSource = webFactory.GetUrl();

            KoraliumConnection connection = new KoraliumConnection();
            connection.ConnectionString = builder.ConnectionString;
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = "select * from project";

            var reader = cmd.ExecuteReader();

            var companyOrdinal = reader.GetOrdinal("company");
            while (reader.Read())
            {
                var company = reader.GetFieldValue<Company>(companyOrdinal);
            }
        }

        [Test]
        public void TestExecuteScalarCount()
        {
            KoraliumConnectionStringBuilder builder = new KoraliumConnectionStringBuilder();
            builder.DataSource = webFactory.GetUrl();

            KoraliumConnection connection = new KoraliumConnection();
            connection.ConnectionString = builder.ConnectionString;
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select count(*) from project";
            var value = cmd.ExecuteScalar();

            Assert.AreEqual(2, value);
        }
    }
}