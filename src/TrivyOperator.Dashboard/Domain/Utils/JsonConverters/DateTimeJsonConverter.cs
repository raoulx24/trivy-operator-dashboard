using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Utils.JsonConverters;

public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ssZ";

    public override bool HandleNull => true;

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(Format));

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => throw new ArgumentException("Error converting value {null} to type"),
            JsonTokenType.String => reader.TryGetDateTime(out DateTime v) ? v : DateTime.MinValue,
            _ => throw new NotSupportedException("Not supported: " + reader.TokenType),
        };
}
