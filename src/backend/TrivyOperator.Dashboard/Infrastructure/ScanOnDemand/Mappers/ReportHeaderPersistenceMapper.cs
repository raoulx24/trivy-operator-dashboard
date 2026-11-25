using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToPersistence;
using TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Mappers;

// a report is stored as a JSON header plus a codec-encoded body (the big list); occurrences are not stored
public static class ReportHeaderPersistenceMapper
{
    // vulnerability report

    public static (VulnerabilityReportHeaderPersistenceModel Header, VulnerabilityPersistenceModel[] Body)
        ToPersistenceModels(this VulnerabilityReport domain)
        => (new VulnerabilityReportHeaderPersistenceModel(
                domain.ImageDigest.Value,
                domain.LastSeenAt.Value,
                domain.Scanner.ToPersistenceModel(),
                domain.Os.Family.Value,
                domain.Os.Name.Value,
                domain.Os.IsEndOfLife,
                [.. domain.SeverityCounters.Values,]),
            [.. domain.Vulnerabilities.Select(static x => x.ToPersistenceModel()),]);

    public static VulnerabilityReport ToDomain(
        this VulnerabilityReportHeaderPersistenceModel header,
        VulnerabilityPersistenceModel[] body)
        => new(
            [],
            new Digest(header.ImageDigest),
            new Timestamp(header.LastSeenAt),
            header.Scanner.ToDomain(),
            new Os(new OsFamily(header.OsFamily), new OsName(header.OsName), header.OsIsEndOfLife),
            new SeverityCounters(header.SeverityCounters),
            [.. body.Select(static x => x.ToDomain()),]);

    // exposed secret report

    public static (ExposedSecretReportHeaderPersistenceModel Header, SecretPersistenceModel[] Body)
        ToPersistenceModels(this ExposedSecretReport domain)
        => (new ExposedSecretReportHeaderPersistenceModel(
                domain.ImageDigest.Value,
                domain.LastSeenAt.Value,
                domain.Scanner.ToPersistenceModel(),
                [.. domain.SeverityCounters.Values,]),
            [.. domain.Secrets.Select(static x => x.ToPersistenceModel()),]);

    public static ExposedSecretReport ToDomain(
        this ExposedSecretReportHeaderPersistenceModel header,
        SecretPersistenceModel[] body)
        => new(
            [],
            new Digest(header.ImageDigest),
            new Timestamp(header.LastSeenAt),
            header.Scanner.ToDomain(),
            new SeverityCounters(header.SeverityCounters),
            [.. body.Select(static x => x.ToDomain()),]);

    // sbom report; component ids are positions, as in SbomReportCacheEntryBuilder

    public static (SbomReportHeaderPersistenceModel Header, ComponentPersistenceModel[] Body)
        ToPersistenceModels(this SbomReport domain)
    {
        IReadOnlyList<Component> components = domain.Components;
        Dictionary<ComponentId, int> indexById = ComponentPersistenceMapper.ToIndexById(components);
        ComponentPersistenceModel[] body = new ComponentPersistenceModel[components.Count];

        for (int i = 0; i < components.Count; i++)
        {
            body[i] = components[i].ToPersistenceModel(indexById);
        }

        SbomReportHeaderPersistenceModel header = new(
            domain.ImageDigest.Value,
            domain.LastSeenAt.Value,
            domain.Scanner.ToPersistenceModel(),
            domain.Summary.ComponentsCount,
            domain.Summary.DependenciesCount,
            domain.SbomMetadata.BomFormat,
            domain.SbomMetadata.SpecVersion,
            domain.SbomMetadata.SerialNumber.Value,
            domain.SbomMetadata.Version,
            domain.SbomMetadata.GeneratedAt.Value,
            indexById.TryGetValue(domain.RootNodeBomRef, out int rootIndex) ? rootIndex : null,
            domain.RootNodeBomRef.Value,
            [.. domain.Layers.Select(static x => new ImageLayerPersistenceModel(x.DiffId.Value, x.Digest.Value)),]);

        return (header, body);
    }

    public static SbomReport ToDomain(this SbomReportHeaderPersistenceModel header, ComponentPersistenceModel[] body)
    {
        Digest digest = new(header.ImageDigest);
        ComponentId[] ids = ComponentPersistenceMapper.CreateComponentIds(digest.Value, body.Length);
        List<Component> components = new(body.Length);

        for (int i = 0; i < body.Length; i++)
        {
            components.Add(body[i].ToDomain(ids[i], ids));
        }

        ComponentId root = header.RootComponentIndex is { } index && index >= 0 && index < ids.Length
            ? ids[index]
            : new ComponentId(header.RootNodeBomRef);

        return new SbomReport(
            [],
            digest,
            new Timestamp(header.LastSeenAt),
            header.Scanner.ToDomain(),
            new SbomSummary(header.ComponentsCount, header.DependenciesCount),
            new SbomMetadata(
                header.BomFormat,
                header.SpecVersion,
                new SbomSerialNumber(header.SerialNumber),
                header.Version,
                new Timestamp(header.GeneratedAt)),
            root,
            [.. header.Layers.Select(static x => new ImageLayer(new Digest(x.DiffId), new Digest(x.Digest))),],
            components);
    }

    // shared

    private static ScannerPersistenceModel ToPersistenceModel(this Scanner domain)
        => new(domain.Name.Value, domain.Vendor.Value, domain.Version.Value);

    private static Scanner ToDomain(this ScannerPersistenceModel dto)
        => new(new ScannerName(dto.Name), new ScannerVendor(dto.Vendor), new ScannerVersion(dto.Version));
}
