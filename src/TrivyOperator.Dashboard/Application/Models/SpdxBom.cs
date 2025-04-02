﻿namespace TrivyOperator.Dashboard.Application.Models;

using System.Text.Json.Serialization;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

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
    public List<string> Creators { get; set; } = [ "Tool: Custom SBOM Converter" ];
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
    public bool FilesAnalyzed { get; set; } = false;
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

public static partial class SbomReportCrExtensions
{
    public static SpdxBom ToSpdx(this SbomReportCr sbomReport)
    {
        var spdxDocument = new SpdxBom
        {
            Name = sbomReport.Report?.Artifact.Repository ?? "Unknown SBOM",
            DocumentNamespace = $"http://spdx.org/spdxdocs/{Guid.NewGuid()}",
            CreationInfo = new SpdxCreationInfo
            {
                Created = DateTime.UtcNow,
                Creators =
                [
                    $"Tool: {sbomReport.Report?.Scanner.Name} {sbomReport.Report?.Scanner.Version}",
                    $"Organization: {sbomReport.Report?.Registry.Server}"
                ]
            },
            Packages = [.. sbomReport.Report?.Components.ComponentsComponents.Select(comp => new SpdxPackage
            {
                SPDXID = $"SPDXRef-{comp.BomRef}",
                Name = comp.Name,
                VersionInfo = comp.Version,
                LicenseDeclared = comp.Licenses?.FirstOrDefault()?.License?.Id ?? "NOASSERTION",
                LicenseConcluded = "NOASSERTION"
            }) ?? []],
            Relationships = [.. sbomReport.Report?.Components.Dependencies.Select(dep => new SpdxRelationship
            {
                SpdxElementId = $"SPDXRef-{dep.Ref}",
                RelatedSpdxElement = string.Join(", ", dep.DependsOn.Select(d => $"SPDXRef-{d}")),
                RelationshipType = "DEPENDS_ON"
            })?? []]
        };

        return spdxDocument;
    }
}
