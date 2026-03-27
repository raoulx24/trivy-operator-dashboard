using TrivyOperator.Dashboard.Domain.VrHistory;

namespace TrivyOperator.Dashboard.Domain.DistributedCache;

public class VrImageLineage
{
    public string NamespaceName { get; }
    public Dictionary<string, VrImageInfo> DigestToImageTag { get; }

    public VrImageLineage(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName)) throw new ArgumentException("Namespace cannot be null or empty.");

        NamespaceName = namespaceName;
        DigestToImageTag = new Dictionary<string, VrImageInfo>();
    }
}
