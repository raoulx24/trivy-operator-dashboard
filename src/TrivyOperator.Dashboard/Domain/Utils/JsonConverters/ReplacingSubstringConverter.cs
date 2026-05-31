using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Utils.JsonConverters;

public class ReplacingSubstringConverter(string toSearchFor, string toReplaceWith, bool onlyRead = true) : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? s = reader.GetString();

        return s?.Replace(toSearchFor, toReplaceWith) ??  string.Empty;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(!onlyRead ? value.Replace(toSearchFor, toReplaceWith) : value);
    }
}
