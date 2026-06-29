using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record SbomMetadata
{
    public string BomFormat { get; }
    public string SpecVersion { get; }
    public SbomSerialNumber SerialNumber { get; }
    public long Version { get; }
    public Timestamp GeneratedAt { get; }

    public SbomMetadata(
        string bomFormat,
        string specVersion,
        SbomSerialNumber serialNumber,
        long version,
        Timestamp generatedAt)
    {
        BomFormat = string.IsNullOrWhiteSpace(bomFormat)
            ? throw new ArgumentException(null, nameof(bomFormat))
            : string.Intern(bomFormat.Trim());

        SpecVersion = string.IsNullOrWhiteSpace(specVersion)
            ? throw new ArgumentException(null, nameof(specVersion))
            : string.Intern(specVersion.Trim());

        SerialNumber = serialNumber;
        
        Version = version;

        GeneratedAt = generatedAt;
    }
}

public readonly record struct SbomSerialNumber(string Value)
{
    public override string ToString() => Value;
}
