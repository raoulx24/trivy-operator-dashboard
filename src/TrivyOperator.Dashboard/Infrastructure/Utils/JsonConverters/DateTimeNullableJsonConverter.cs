using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

public sealed class DateTimeNullableJsonConverter : JsonConverter<DateTime?>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ssZ";

    public override bool HandleNull => true;

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(Format));
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.TryGetDateTime(out DateTime v) ? v : null,
            _ => throw new NotSupportedException("Not supported: " + reader.TokenType),
        };
}
