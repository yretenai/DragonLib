using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.JSON
{
    [PublicAPI]
    public class GenericDictionaryConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType) return false;

            return typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) && typeToConvert.GetGenericArguments()[0].IsEquivalentTo(typeof(string));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter) (Activator.CreateInstance(typeof(GenericDictionaryConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[1])) ?? throw new JsonException());
    }
}
