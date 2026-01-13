using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Domain.Utils;

public class JsonUtils
{
    public static void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Insert(0, new DateTimeJsonConverter());
        jsonSerializerOptions.Converters.Insert(0, new DateTimeNullableJsonConverter());

        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
