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
using System;
using System.Collections.Generic;

namespace Koralium.Transport.ArrowFlight.Encoders
{
    class StringEncoder : IArrowEncoder
    {
        private StringArray.Builder _builder;
        private readonly Func<object, object> _getFunc;
        private readonly bool _nullable;

        private long _size;
        public StringEncoder(Column column)
        {
            _getFunc = column.GetFunction;
            _nullable = column.IsNullable;
        }

        public IArrowArray BuildArray()
        {
            return _builder.Build();
        }

        public void NewBatch()
        {
            _size = 0;
            _builder = new StringArray.Builder();
        }

        public void Encode(object row)
        {
            var val = _getFunc(row);

            if (_nullable && val == null)
            {
                _builder.AppendNull();
            }
            else
            {
                var stringVal = (string)val;
                _size += stringVal.Length;
                _builder.Append(stringVal);
            }
        }

        public void Encode(IReadOnlyList<object> rows)
        {
            if (_nullable)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var val = _getFunc(rows[i]);
                    if (val == null)
                    {
                        _builder.AppendNull();
                    }
                    else
                    {
                        _builder.Append((string)val);
                    }
                }
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var val = _getFunc(rows[i]);
                    _builder.Append((string)val);
                }
            }
        }

        public long Size()
        {
            return _size;
        }

        public void Pad(int length)
        {
            if (_nullable)
            {
                for (int i = 0; i < length; i++)
                {
                    _builder.AppendNull();
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    _builder.Append(string.Empty);
                }
            }
        }
    }
}
