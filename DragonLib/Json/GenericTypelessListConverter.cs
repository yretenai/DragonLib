using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonLib.Json;

public class GenericTypelessListConverter<T> : JsonConverter<List<T>> {
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, List<T> list, JsonSerializerOptions options) {
        writer.WriteStartArray();
        foreach (var value in list) {
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(T), options);
        }

        writer.WriteEndArray();
    }
}
