namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Models;

// the report fields besides the big lists (vulnerabilities, secrets, components), which go through the codec
// occurrences are not stored: they would describe the throwaway scan pod

public sealed record ScannerPersistenceModel(string Name, string Vendor, string Version);

public sealed record ImageLayerPersistenceModel(string DiffId, string Digest);

public sealed record VulnerabilityReportHeaderPersistenceModel(
    string ImageDigest,
    DateTime LastSeenAt,
    ScannerPersistenceModel Scanner,
    string OsFamily,
    string OsName,
    bool? OsIsEndOfLife,
    int[] SeverityCounters);

public sealed record ExposedSecretReportHeaderPersistenceModel(
    string ImageDigest,
    DateTime LastSeenAt,
    ScannerPersistenceModel Scanner,
    int[] SeverityCounters);

public sealed record SbomReportHeaderPersistenceModel(
    string ImageDigest,
    DateTime LastSeenAt,
    ScannerPersistenceModel Scanner,
    int ComponentsCount,
    int DependenciesCount,
    string BomFormat,
    string SpecVersion,
    string SerialNumber,
    int Version,
    DateTime GeneratedAt,
    // position of the root in the stored components (see ComponentPersistenceMapper); null when not among them
    int? RootComponentIndex,
    // used only when RootComponentIndex is null
    string RootNodeBomRef,
    ImageLayerPersistenceModel[] Layers);
