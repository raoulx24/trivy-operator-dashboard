using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Utils.JsonConverters;

public class StringInternalsJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return string.IsNullOrWhiteSpace(value) ? string.Empty : string.Intern(value);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
