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
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;

namespace Koralium.EntityFrameworkCore.ArrowFlight.Storage.Internal
{
    class EnumTypeMapping : RelationalTypeMapping
    {

        public EnumTypeMapping(string storeType, Type clrType, DbType? dbType = null, bool unicode = false, int? size = null)
            : base(storeType, clrType, dbType, unicode, size)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        {
            throw new NotImplementedException();
        }

        public override string GenerateSqlLiteral(object value)
        {
            return $"'{value.ToString()}'";
        }
    }
}
