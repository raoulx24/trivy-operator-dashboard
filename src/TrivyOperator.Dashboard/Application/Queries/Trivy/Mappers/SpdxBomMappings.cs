using System.Text.Json.Serialization;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class SpdxBomMappings
{
    public static SpdxBom ToSpdx(this SbomReport report)
    {
        ReportImageOccurrence occurrence = report.Occurrences.Count == 0
            ? new ReportImageOccurrence()
            : report.Occurrences[0];

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
                    .SelectMany(component =>
                        component.DependsOnIds.Select(dependencyId =>
                            new SpdxRelationship
                            {
                                SpdxElementId = $"SPDXRef-{component.Id.Value}",
                                RelatedSpdxElement = $"SPDXRef-{dependencyId.Value}",
                                RelationshipType = "DEPENDS_ON",
                            }
                        )
                    ),
            ],
        };
    }
}
