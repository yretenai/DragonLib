using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonLib.JSON {
    public class GenericTypelessDictionaryConverterFactory : JsonConverterFactory {
        public GenericTypelessDictionaryConverterFactory(bool overrideIsStringKey, params Type[] stringTypes) {
            OverrideIsStringKey = overrideIsStringKey;
            AdditionalStringTypes = stringTypes;
        }

        private bool OverrideIsStringKey { get; }

        public Type[] AdditionalStringTypes { get; }

        public override bool CanConvert(Type typeToConvert) {
            if (!typeToConvert.IsGenericType) return false;

            return typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
            return (JsonConverter)(Activator.CreateInstance(typeof(GenericTypelessDictionaryConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments().Take(2).ToArray()), OverrideIsStringKey, AdditionalStringTypes) ?? throw new JsonException());
        }
    }
}
