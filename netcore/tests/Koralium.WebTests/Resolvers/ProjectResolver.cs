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
using Koralium.WebTests.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koralium.WebTests
{
    public class ProjectResolver : TableResolver<Project>
    {
        protected override async Task<IQueryable<Project>> GetQueryableData(IQueryOptions<Project> queryOptions, ICustomMetadata customMetadata)
        {

            return new List<Project>()
                {
                     new Project()
                    {
                        Id = 1,
                        Name = "alex",
                        Company = new Company()
                        {
                            CompanyId = "1",
                            Name = "Test company"
                        }
                    },
                    new Project()
                    {
                        Id = 2,
                        Name = "alex2"
                    }
                }.AsQueryable();
        }
    }
}
