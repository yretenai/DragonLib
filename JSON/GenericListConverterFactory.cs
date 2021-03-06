﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.JSON
{
    [PublicAPI]
    public class GenericListConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType) return false;

            return typeToConvert.GetGenericTypeDefinition() == typeof(List<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter) (Activator.CreateInstance(typeof(GenericListConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0])) ?? throw new JsonException());
    }
}
