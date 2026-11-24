using System.Text.Json;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;

// reads one page of custom resources for the watch sessions, with the same memory profile as a plain typed list
// - normal case: one typed list call (exactly as before)
// - only when the page fails to deserialize: the same page is read again one item at a time (limit 1, typed),
//   and only the item that fails is read raw, to report it as malformed; so at most one item is held twice
// the continue token of a paginated list may be resumed with a different limit, which this relies on
public static class CustomResourcePageReader
{
    public static async Task<ResourcePage<TKubernetesObject>> Read<TKubernetesObject>(
        Func<int?, string?, CancellationToken, Task<ResourcePage<TKubernetesObject>>> readTypedPage,
        Func<string?, CancellationToken, Task<JsonElement>> readSingleRawItem,
        int? pageLimit,
        string? continueToken,
        CancellationToken cancellationToken)
        where TKubernetesObject : CustomResource
    {
        try
        {
            return await readTypedPage(pageLimit, continueToken, cancellationToken);
        }
        catch (Exception ex) when (IsDeserializationFailure(ex))
        {
            return await ReadItemByItem(readTypedPage, readSingleRawItem, pageLimit, continueToken, cancellationToken);
        }
    }

    private static async Task<ResourcePage<TKubernetesObject>> ReadItemByItem<TKubernetesObject>(
        Func<int?, string?, CancellationToken, Task<ResourcePage<TKubernetesObject>>> readTypedPage,
        Func<string?, CancellationToken, Task<JsonElement>> readSingleRawItem,
        int? pageLimit,
        string? continueToken,
        CancellationToken cancellationToken)
        where TKubernetesObject : CustomResource
    {
        List<TKubernetesObject> items = [];
        List<MalformedObject> malformed = [];
        string? currentToken = continueToken;
        string? resourceVersion = null;
        int limit = pageLimit is > 0 ? pageLimit.Value : int.MaxValue;

        for (int read = 0; read < limit; read++)
        {
            ResourcePage<TKubernetesObject> single;

            try
            {
                single = await readTypedPage(1, currentToken, cancellationToken);
            }
            catch (Exception ex) when (IsDeserializationFailure(ex))
            {
                JsonElement raw = await readSingleRawItem(currentToken, cancellationToken);
                single = CustomResourceJson.ToResourcePage<TKubernetesObject>(raw);
            }

            items.AddRange(single.Items);
            malformed.AddRange(single.Malformed);
            resourceVersion = single.ResourceVersion ?? resourceVersion;
            currentToken = single.ContinueToken;

            if (string.IsNullOrEmpty(currentToken))
            {
                break;
            }
        }

        return new ResourcePage<TKubernetesObject>(items, malformed, currentToken, resourceVersion);
    }

    // the client may throw the JsonException as is or wrapped
    private static bool IsDeserializationFailure(Exception ex)
    {
        for (Exception? current = ex; current is not null; current = current.InnerException)
        {
            if (current is JsonException)
            {
                return true;
            }
        }

        return false;
    }
}
