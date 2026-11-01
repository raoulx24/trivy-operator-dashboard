using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;

public static class CycloneDxBomMappings
{
    public static CycloneDxBom ToCycloneDx(this SbomReport report)
    {
        Component? rootComponent = report.Components
            .FirstOrDefault(component => component.Id == report.RootNodeBomRef);

        return new CycloneDxBom
        {
            BomFormat = report.SbomMetadata.BomFormat,
            SpecVersion = report.SbomMetadata.SpecVersion,
            SerialNumber = report.SbomMetadata.SerialNumber.Value,
            Version = report.SbomMetadata.Version,

            Metadata = new CycloneDxMetadata
            {
                Timestamp = report.SbomMetadata.GeneratedAt.Value,

                Tools =
                [
                    new CycloneDxTool
                    {
                        Vendor = report.Scanner.Vendor.Value,
                        Name = report.Scanner.Name.Value,
                        Version = report.Scanner.Version.Value,
                    },
                ],

                Component = rootComponent is null
                    ? null
                    : ToCycloneDxComponent(rootComponent, report.Layers, isRoot: true),
            },

            Components =
            [
                .. report.Components
                    .Where(component => component.Id != report.RootNodeBomRef)
                    .Select(component => ToCycloneDxComponent(component, report.Layers, isRoot: false)),
            ],

            Dependencies =
            [
                .. report.Components
                    .Where(component => !component.DependsOnIds.IsDefaultOrEmpty)
                    .Select(component => new CycloneDxDependency
                    {
                        Ref = component.Id.Value,

                        DependsOnXml =
                        [
                            .. component.DependsOnIds.Select(dependencyId =>
                                new CycloneDxDependency
                                {
                                    Ref = dependencyId.Value,
                                }),
                        ],
                    }),
            ],
        };
    }

    private static CycloneDxComponent ToCycloneDxComponent(
        Component component,
        IReadOnlyList<ImageLayer> layers,
        bool isRoot)
    {
        return new CycloneDxComponent
        {
            Name = component.Name.Value,
            Version = component.Version.Value,
            Type = component.Type.Value,
            BomRef = component.Id.Value,
            Purl = component.Purl?.Value ?? string.Empty,

            Supplier = component.Supplier is null
                ? null
                : new CycloneDxSupplier
                {
                    Name = component.Supplier.Name,
                    Email = component.Supplier.Email,
                    Phone = component.Supplier.Phone,
                },

            LicensesXml =
            [
                .. component.Licenses.Select(license => new CycloneDxLicense
                {
                    Id = license.Id,
                    Name = license.Name,
                    Url = license.Url?.ToString(),
                }),
            ],

            Properties =
            [
                .. component.GetAllProperties(layers, isRoot).Select(property => new CycloneDxProperty
                {
                    Name = property.Name,
                    Value = property.Value,
                }),
            ],
        };
    }
}
