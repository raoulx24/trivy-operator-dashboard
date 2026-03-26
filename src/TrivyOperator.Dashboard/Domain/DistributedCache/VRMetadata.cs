namespace TrivyOperator.Dashboard.Domain.DistributedCache;

public class VRMetadata
{
    public string NamespaceNameName { get; }
    public string Digest { get; }
    public Dictionary<string, DateTime> CvesHashes { get; }

    public VRMetadata(string namespaceName, string digest)
    {
        if (string.IsNullOrEmpty(namespaceName)) throw new ArgumentException("Namespace cannot be null or empty.");
        if (string.IsNullOrEmpty(digest)) throw new ArgumentException("Digest cannot be null or empty.");

        NamespaceNameName = namespaceName;
        Digest = digest;
        CvesHashes = new Dictionary<string, DateTime>(); // Maps cvesHash -> last seen timestamp
    }

    // The key for the metadata in Redis/Valkey
    public string GetMetadataKey() => $"vrmeta:{{{NamespaceNameName}}}:{Digest}";
}