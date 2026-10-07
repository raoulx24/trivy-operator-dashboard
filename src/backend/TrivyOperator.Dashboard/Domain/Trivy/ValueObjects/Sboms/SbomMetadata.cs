using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record SbomMetadata
{
    private const string Sentinel = "n/a";
    public string BomFormat { get; }
    public string SpecVersion { get; }
    public SbomSerialNumber SerialNumber { get; }
    public int Version { get; }
    public Timestamp GeneratedAt { get; }

    public SbomMetadata(
        string bomFormat,
        string specVersion,
        SbomSerialNumber serialNumber,
        int version,
        Timestamp generatedAt)
    {
        BomFormat = string.IsNullOrWhiteSpace(bomFormat)
            ? Sentinel
            : string.Intern(bomFormat.Trim());

        SpecVersion = string.IsNullOrWhiteSpace(specVersion)
            ? Sentinel
            : string.Intern(specVersion.Trim());

        SerialNumber = serialNumber;
        
        Version = version;

        GeneratedAt = generatedAt;
    }
}

public readonly record struct SbomSerialNumber
{
    private const string Sentinel = "N/A";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;
    
    public SbomSerialNumber(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }
    
    public SbomSerialNumber() : this(Sentinel) { }
    
    public override string ToString() => Value;
}
