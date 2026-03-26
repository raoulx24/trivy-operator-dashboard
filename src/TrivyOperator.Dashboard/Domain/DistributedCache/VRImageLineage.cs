namespace TrivyOperator.Dashboard.Domain.DistributedCache;

public class VRImageLineage
{
    public string NamespaceName { get; }
    public Dictionary<string, string> DigestToImageTag { get; }

    public VRImageLineage(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName)) throw new ArgumentException("Namespace cannot be null or empty.");

        NamespaceName = namespaceName;
        DigestToImageTag = new Dictionary<string, string>(); // Maps digest -> image:tag
    }

    // The key for the image lineage in Redis/Valkey
    public string GetImageLineageKey() => $"vrimage:{{{NamespaceName}}}";
}
