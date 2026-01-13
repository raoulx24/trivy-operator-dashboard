using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Domain.Utils;

public class JsonUtils
{
    public static void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Insert(0, new DateTimeJsonConverter());
        jsonSerializerOptions.Converters.Insert(0, new DateTimeNullableJsonConverter());
    }
}
