namespace TrivyOperator.Dashboard.Domain.DistributedCache;

public class VRSnapshot
{
    public string NamespaceName { get; }
    public string Digest { get; }
    public string CvesHash { get; }
    public byte[] CompressedSnapshot { get; }

    public VRSnapshot(string namespaceName, string digest, string cvesHash, byte[] compressedSnapshot)
    {
        if (string.IsNullOrEmpty(namespaceName)) throw new ArgumentException("Namespace cannot be null or empty.");
        if (string.IsNullOrEmpty(digest)) throw new ArgumentException("Digest cannot be null or empty.");
        if (string.IsNullOrEmpty(cvesHash)) throw new ArgumentException("CvesHash cannot be null or empty.");

        NamespaceName = namespaceName;
        Digest = digest;
        CvesHash = cvesHash;
        CompressedSnapshot = compressedSnapshot;
    }

    // The key for storing the snapshot in Redis/Valkey
    public string GetSnapshotKey() => $"vr:{{{NamespaceName}}}:{Digest}:{CvesHash}";
}
