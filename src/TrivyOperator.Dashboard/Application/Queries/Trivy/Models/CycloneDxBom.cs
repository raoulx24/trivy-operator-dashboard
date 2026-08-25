using System.Text.Json.Serialization;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

[XmlRoot("bom", Namespace = "http://cyclonedx.org/schema/bom/1.6")]
public class CycloneDxBom
{
    [JsonPropertyName("bomFormat")]
    [XmlIgnore]
    public string BomFormat { get; set; } = "CycloneDX";

    [JsonPropertyName("specVersion")]
    [XmlIgnore]
    public string SpecVersion { get; set; } = "1.6";

    [JsonPropertyName("serialNumber")]
    [XmlAttribute("serialNumber")]
    public string SerialNumber { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    [XmlAttribute("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("metadata")]
    [XmlElement("metadata")]
    public CycloneDxMetadata? Metadata { get; set; }

    [JsonPropertyName("components")]
    [XmlArray("components")]
    [XmlArrayItem("component")]
    public List<CycloneDxComponent> Components { get; set; } = [];

    [JsonPropertyName("dependencies")]
    [XmlArray("dependencies")]
    [XmlArrayItem("dependency")]
    public List<CycloneDxDependency> Dependencies { get; set; } = [];
}

public class CycloneDxMetadata
{
    [JsonPropertyName("timestamp")]
    [XmlElement("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("tools")]
    [XmlArray("tools")]
    [XmlArrayItem("tool")]
    public CycloneDxTool[]? Tools { get; set; }

    [JsonPropertyName("component")]
    [XmlElement("component")]
    public CycloneDxComponent? Component { get; set; }
}

public class CycloneDxTool
{
    [JsonPropertyName("vendor")]
    [XmlElement("vendor")]
    public string? Vendor { get; set; }

    [JsonPropertyName("name")]
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;
}

public class CycloneDxComponent
{
    [JsonPropertyName("type")]
    [XmlAttribute("type")]
    public string Type { get; set; } = "library";

    [JsonPropertyName("bom-ref")]
    [XmlAttribute("bom-ref")]
    public string BomRef { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("supplier")]
    [XmlElement("supplier")]
    public CycloneDxSupplier? Supplier { get; set; }

    [JsonPropertyName("name")]
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("licenses")]
    [XmlIgnore]
    public List<CycloneDxLicenseContainer>? LicensesJson => LicensesXml?.Select(xmlLicense =>
            new CycloneDxLicenseContainer
            {
                License = xmlLicense,
            }
        )
        .ToList();

    [XmlArray("licenses")]
    [XmlArrayItem("license")]
    [JsonIgnore]
    public List<CycloneDxLicense>? LicensesXml { get; set; }

    [JsonPropertyName("purl")]
    [XmlElement("purl")]
    public string Purl { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    [XmlArray("properties")]
    [XmlArrayItem("property")]
    public List<CycloneDxProperty> Properties { get; set; } = [];
}

public class CycloneDxSupplier
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("email")]
    [XmlElement("email")]
    public string? Email { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    [XmlElement("name")]
    public string? Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("phone")]
    [XmlElement("phone")]
    public string? Phone { get; set; }
}

public class CycloneDxLicenseContainer
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("license")]
    public CycloneDxLicense License { get; set; } = new();
}

public class CycloneDxLicense
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    [XmlElement("id")]
    public string? Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    [XmlElement("name")]
    public string? Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("url")]
    [XmlElement("url")]
    public string? Url { get; set; }
}

public class CycloneDxProperty
{
    [JsonPropertyName("name")]
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    [XmlText]
    public string Value { get; set; } = string.Empty;
}

public class CycloneDxDependency
{
    [JsonPropertyName("ref")]
    [XmlAttribute("ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("dependsOn")]
    [XmlIgnore]
    public List<string> DependsOnJson => DependsOnXml?.ConvertAll(dep => dep.Ref) ?? [];

    [JsonIgnore]
    [XmlElement("dependency")]
    public List<CycloneDxDependency>? DependsOnXml { get; set; }
}
