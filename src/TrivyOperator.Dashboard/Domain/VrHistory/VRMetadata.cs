namespace TrivyOperator.Dashboard.Domain.VrHistory;

public class VrMetadata
{
    public string NamespaceName { get; }
    public string Digest { get; }
    public Dictionary<string, VrSummary> CvesHashes { get; }

    public VrMetadata(string namespaceName, string digest)
    {
        if (string.IsNullOrEmpty(namespaceName)) throw new ArgumentException("Namespace cannot be null or empty.");
        if (string.IsNullOrEmpty(digest)) throw new ArgumentException("Digest cannot be null or empty.");

        NamespaceName = namespaceName;
        Digest = digest;
        CvesHashes = new Dictionary<string, VrSummary>();
    }
}