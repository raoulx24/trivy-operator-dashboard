using System.Text.Json.Serialization;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public class SpdxBom
{
    [JsonPropertyName("spdxVersion")]
    [XmlElement("spdxVersion")]
    public string SpdxVersion { get; set; } = "SPDX-2.3";

    [JsonPropertyName("dataLicense")]
    [XmlElement("dataLicense")]
    public string DataLicense { get; set; } = "CC0-1.0";

    [JsonPropertyName("SPDXID")]
    [XmlElement("SPDXID")]
    public string SPDXID { get; set; } = "SPDXRef-DOCUMENT";

    [JsonPropertyName("name")]
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("documentNamespace")]
    [XmlElement("documentNamespace")]
    public string DocumentNamespace { get; set; } = string.Empty;

    [JsonPropertyName("creationInfo")]
    [XmlElement("creationInfo")]
    public SpdxCreationInfo CreationInfo { get; set; } = new();

    [JsonPropertyName("packages")]
    [XmlArray("packages")]
    [XmlArrayItem("package")]
    public List<SpdxPackage> Packages { get; set; } = [];

    [JsonPropertyName("relationships")]
    [XmlArray("relationships")]
    [XmlArrayItem("relationship")]
    public List<SpdxRelationship> Relationships { get; set; } = [];
}

public class SpdxCreationInfo
{
    [JsonPropertyName("created")]
    [XmlElement("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("creators")]
    [XmlArray("creators")]
    [XmlArrayItem("creator")]
    public List<string> Creators { get; set; } = ["Tool: Custom SBOM Converter",];
}

public class SpdxPackage
{
    [JsonPropertyName("SPDXID")]
    [XmlAttribute("SPDXID")]
    public string SPDXID { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("versionInfo")]
    [XmlElement("versionInfo")]
    public string VersionInfo { get; set; } = string.Empty;

    [JsonPropertyName("downloadLocation")]
    [XmlElement("downloadLocation")]
    public string DownloadLocation { get; set; } = "NOASSERTION";

    [JsonPropertyName("licenseConcluded")]
    [XmlElement("licenseConcluded")]
    public string LicenseConcluded { get; set; } = "NOASSERTION";

    [JsonPropertyName("licenseDeclared")]
    [XmlElement("licenseDeclared")]
    public string LicenseDeclared { get; set; } = "NOASSERTION";

    [JsonPropertyName("filesAnalyzed")]
    [XmlElement("filesAnalyzed")]
    public bool FilesAnalyzed { get; set; }
}

public class SpdxRelationship
{
    [JsonPropertyName("spdxElementId")]
    [XmlAttribute("spdxElementId")]
    public string SpdxElementId { get; set; } = string.Empty;

    [JsonPropertyName("relationshipType")]
    [XmlElement("relationshipType")]
    public string RelationshipType { get; set; } = "DESCRIBES";

    [JsonPropertyName("relatedSpdxElement")]
    [XmlElement("relatedSpdxElement")]
    public string RelatedSpdxElement { get; set; } = string.Empty;
}

public static partial class SbomReportMappings
{
    public static SpdxBom ToSpdx(
        this SbomReport report,
        ReportImageOccurrence occurrence)
    {
        return new SpdxBom
        {
            Name = occurrence.ImageMeta.Repo.Value,

            DocumentNamespace = $"http://spdx.org/spdxdocs/{Guid.NewGuid()}",

            CreationInfo = new SpdxCreationInfo
            {
                Created = report.SbomMetadata.GeneratedAt.Value,
                Creators =
                [
                    $"Tool: {report.Scanner.Name.Value} {report.Scanner.Version.Value}",
                    $"Organization: {report.Scanner.Vendor.Value}",
                ],
            },

            Packages =
            [
                .. report.Components.Select(component => new SpdxPackage
                {
                    SPDXID = $"SPDXRef-{component.Id.Value}",
                    Name = component.Name.Value,
                    VersionInfo = component.Version.Value,
                    LicenseDeclared =
                        component.Licenses.FirstOrDefault()?.Id
                        ?? "NOASSERTION",
                    LicenseConcluded = "NOASSERTION",
                }),
            ],

            Relationships =
            [
                .. report.Components
                    .Where(component => !component.DependsOnIds.IsDefaultOrEmpty)
                    .Select(component => new SpdxRelationship
                    {
                        SpdxElementId = $"SPDXRef-{component.Id.Value}",
                        RelatedSpdxElement = string.Join(
                            ", ",
                            component.DependsOnIds.Select(
                                dependencyId => $"SPDXRef-{dependencyId.Value}"
                            )
                        ),
                        RelationshipType = "DEPENDS_ON",
                    }),
            ],
        };
    }

}
