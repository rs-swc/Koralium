﻿using Koralium.Transport.RowLevelSecurity.FormatConverters.Elasticsearch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Koralium.Transport.RowLevelSecurity.FormatConverters.Elasticsearch.Serializers
{
    class BoolSerializer : JsonConverter<Bool>
    {
        private static readonly JsonEncodedText _boolText = JsonEncodedText.Encode("bool");
        private static readonly JsonEncodedText _mustText = JsonEncodedText.Encode("must");
        private static readonly JsonEncodedText _shouldText = JsonEncodedText.Encode("should");
        private static readonly JsonEncodedText _mustNotText = JsonEncodedText.Encode("must_not");
        private static readonly JsonEncodedText _minimumShouldMatchText = JsonEncodedText.Encode("minimum_should_match");
        private static readonly BoolOperationSerializer boolOperationSerializer = new BoolOperationSerializer();
        public override Bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Bool value, JsonSerializerOptions options)
        {
            if (value.Must == null && value.Should == null && value.MustNot == null)
                return;

            writer.WriteStartObject();
            writer.WriteStartObject(_boolText);

            if (value.Must != null && value.Must.Count > 0)
            {
                writer.WriteStartArray(_mustText);
                foreach(var op in value.Must)
                {
                    boolOperationSerializer.Write(writer, op, options);
                }
                writer.WriteEndArray();
            }
            if (value.Should != null && value.Should.Count > 0)
            {
                writer.WriteStartArray(_shouldText);
                foreach (var op in value.Should)
                {
                    boolOperationSerializer.Write(writer, op, options);
                }
                writer.WriteEndArray();
                writer.WriteNumber(_minimumShouldMatchText, 1);
            }
            if (value.MustNot != null && value.MustNot.Count > 0)
            {
                writer.WriteStartArray(_mustNotText);
                foreach (var op in value.MustNot)
                {
                    boolOperationSerializer.Write(writer, op, options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
