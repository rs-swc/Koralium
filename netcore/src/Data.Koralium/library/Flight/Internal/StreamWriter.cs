﻿// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to You under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License.  You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace Apache.Arrow.Flight.Internal
{
    internal class StreamWriter<TIn, TOut> : IAsyncStreamWriter<TIn>
    {
        private readonly IAsyncStreamWriter<TOut> _inputStream;
        private readonly Func<TIn, TOut> _convertFunction;
        internal StreamWriter(IAsyncStreamWriter<TOut> inputStream, Func<TIn, TOut> convertFunction)
        {
            _inputStream = inputStream;
            _convertFunction = convertFunction;
        }

        public WriteOptions WriteOptions
        {
            get
            {
                return _inputStream.WriteOptions;
            }
            set
            {
                _inputStream.WriteOptions = value;
            }
        }

        public Task WriteAsync(TIn message)
        {
            return _inputStream.WriteAsync(_convertFunction(message));
        }
    }
}
