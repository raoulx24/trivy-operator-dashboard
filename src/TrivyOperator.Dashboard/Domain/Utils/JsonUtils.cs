using k8s;
using k8s.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Domain.Utils;

public static class JsonUtils
{
    public static JsonSerializerOptions GetKubernetesJsonSerializerOptions()
    {
        JsonSerializerOptions jsonOptions = new();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonOptions.Converters.Add(new KubernetesJson.Iso8601TimeSpanConverter());
        jsonOptions.Converters.Add(new KubernetesJson.KubernetesDateTimeConverter());
        jsonOptions.Converters.Add(new KubernetesJson.KubernetesDateTimeOffsetConverter());
        jsonOptions.Converters.Add(new V1Status.V1StatusObjectViewConverter());

        return jsonOptions;
    }

    public static void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Insert(0, new DateTimeJsonConverter());
        jsonSerializerOptions.Converters.Insert(0, new DateTimeNullableJsonConverter());
    }
}
