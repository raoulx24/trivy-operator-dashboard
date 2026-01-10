using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

public class FilteredStringConverter(string filterOut, string filterWith) : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() ?? string.Empty;

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        string filtered = value.Replace(filterOut, filterWith);
        writer.WriteStringValue(filtered);
    }
}
