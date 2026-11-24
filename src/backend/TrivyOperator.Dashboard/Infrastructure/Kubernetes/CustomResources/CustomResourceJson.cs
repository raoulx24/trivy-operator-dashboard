using k8s;
using k8s.Models;
using System.Text.Json;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;

// deserializes custom resources one item at a time, so one malformed object is skipped instead of failing
// the whole list page or the whole watch
public static class CustomResourceJson
{
    // a raw list response (JsonElement) to a page of valid items and malformed ones
    public static ResourcePage<TKubernetesObject> ToResourcePage<TKubernetesObject>(JsonElement list)
        where TKubernetesObject : CustomResource
    {
        List<TKubernetesObject> items = [];
        List<MalformedObject> malformed = [];

        if (list.ValueKind == JsonValueKind.Object
            && list.TryGetProperty("items", out JsonElement itemsElement)
            && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in itemsElement.EnumerateArray())
            {
                if (TryDeserialize(item, out TKubernetesObject? kubernetesObject, out MalformedObject? malformedObject))
                {
                    items.Add(kubernetesObject);
                }
                else
                {
                    malformed.Add(malformedObject);
                }
            }
        }

        V1ListMeta? listMeta = list.ValueKind == JsonValueKind.Object
                               && list.TryGetProperty("metadata", out JsonElement metadataElement)
            ? KubernetesJson.Deserialize<V1ListMeta>(metadataElement)
            : null;

        return new ResourcePage<TKubernetesObject>(items, malformed, listMeta?.ContinueProperty, listMeta?.ResourceVersion);
    }

    // a raw watch event object (JsonElement) to a watch event carrying either the object or the malformed info
    public static WatchEvent<TKubernetesObject> ToWatchEvent<TKubernetesObject>(WatchEventType type, object item)
        where TKubernetesObject : CustomResource
    {
        JsonElement element = (JsonElement)item;

        return TryDeserialize(element, out TKubernetesObject? kubernetesObject, out MalformedObject? malformedObject)
            ? new WatchEvent<TKubernetesObject> { Type = type, Object = kubernetesObject, }
            : new WatchEvent<TKubernetesObject> { Type = type, Malformed = malformedObject, };
    }

    private static bool TryDeserialize<TKubernetesObject>(
        JsonElement element,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TKubernetesObject? kubernetesObject,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out MalformedObject? malformedObject)
        where TKubernetesObject : CustomResource
    {
        try
        {
            // straight from the element (the client already parsed the line); no string copy of the object,
            // which for large reports (SBOMs of MBs) would land on the large object heap; same options as the
            // string overload (KubernetesJson's shared options, with ours added by AddJsonOptions)
            kubernetesObject = KubernetesJson.Deserialize<TKubernetesObject>(element);

            if (kubernetesObject is null)
            {
                throw new JsonException("Deserialization returned null.");
            }

            malformedObject = null;
            return true;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException
                                       or FormatException or ArgumentException)
        {
            kubernetesObject = null;
            malformedObject = ToMalformed(element, ex);
            return false;
        }
    }

    // best effort: the identity of the object, read straight from the raw metadata
    private static MalformedObject ToMalformed(JsonElement element, Exception exception)
    {
        JsonElement metadata = element.ValueKind == JsonValueKind.Object
                               && element.TryGetProperty("metadata", out JsonElement metadataElement)
                               && metadataElement.ValueKind == JsonValueKind.Object
            ? metadataElement
            : default;

        return new MalformedObject(
            GetString(metadata, "uid"),
            GetString(metadata, "name"),
            GetString(metadata, "namespace"),
            GetString(metadata, "resourceVersion"),
            exception
        );
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out JsonElement property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
}
