﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonLib.Json;

public class GenericDictionaryConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {
        if (!typeToConvert.IsGenericType) {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter) (Activator.CreateInstance(
            typeof(GenericDictionaryConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()
                .Take(2)
                .ToArray())) ?? throw new JsonException());
}
