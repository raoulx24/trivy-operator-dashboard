namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public readonly record struct ComponentProperty(string Name, string Value);

// Trivy property names (without the "aquasecurity:trivy:" prefix) that are moved into SbomReport.Layers.
public static class LayerPropertyNames
{
    public const string DiffId = "DiffID";
    public const string LayerDiffId = "LayerDiffID";
    public const string LayerDigest = "LayerDigest";
}
