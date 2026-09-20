using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Hashes;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.History.Models;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Infrastructure.History.Mappers;

public static class HistoryMetadataMappers
{
    public static HistoryMetadataPersistenceModel ToPersistenceModel(this HistoryMetadata source)
    {
        return new HistoryMetadataPersistenceModel(
            [.. source.NamespaceNames.Select(x => x.Value),],
            source.ImageMeta.Registry.Value,
            source.ImageMeta.Repo.Value,
            source.ImageMeta.Tag.Value,
            [.. source.Current.Values,],
            source.VulnerabilitiesHash.Value,
            [.. source.AddedCvesDeltas.Values,],
            [.. source.DroppedCvesDeltas.Values,]
        );
    }

    public static HistoryMetadata ToDomain(this HistoryMetadataPersistenceModel source)
    {
        return new HistoryMetadata(
            source.NamespaceNames.Select(x => new NamespaceName(x)),
            new ImageMeta(
                new ImageRegistry(source.Registry),
                new ImageRepository(source.Repository),
                new ImageTag(source.Tag)
            ),
            new SeverityCounters(source.Current),
            new VulnerabilitiesHash(source.VulnerabilitiesHash),
            new SeverityCounters(source.AddedCvesDeltas),
            new SeverityCounters(source.DroppedCvesDeltas)
        );
    }

    public static Snapshot ToHistorySnapshot(this VulnerabilityReportCr vrCr)
    {
        ArgumentNullException.ThrowIfNull(vrCr);

        if (string.IsNullOrWhiteSpace(vrCr.Artifact.Digest))
            throw new ArgumentException(
                "Digest in Vulnerability Report Custom Reports is null or empty",
                nameof(vrCr));

        SnapshotKey key = vrCr.ToSnapshotKey();
        
        Vulnerability[] vulnerabilities = [..vrCr.Report.Vulnerabilities.Select(x => x.ToVulnerability()),];

        HistoryMetadata historyMetadata = vrCr.ToVrMetadata();

        Timestamp modifyMoment = new(vrCr.Report.UpdateTimestamp);

        return new Snapshot(key, vulnerabilities, historyMetadata, modifyMoment);
    }

    private static HistoryMetadata ToVrMetadata(this VulnerabilityReportCr vrCr, IReadOnlyList<Vulnerability>? vulnerabilities = null)
    {
        VulnerabilitiesHash hash =
            (vulnerabilities ?? [.. vrCr.Report.Vulnerabilities.Select(x => x.ToVulnerability()),])
            .ToVulnerabilitiesHash();
        return new HistoryMetadata(
            [new NamespaceName(vrCr.Metadata.NamespaceProperty),],
            new ImageMeta(
                new ImageRegistry(vrCr.Report.Registry?.Server),
                new ImageRepository(vrCr.Artifact.Repository),
                new ImageTag(vrCr.Artifact.Tag)),
            TrivySharedMappingExtensions.ToSeverityCounters(vrCr.Report.Summary),
            hash
        );
    }
    
    private static SnapshotKey ToSnapshotKey(this VulnerabilityReportCr vrCr)
    {
        return new SnapshotKey(
            new Digest(vrCr.Artifact.Digest ?? ""),
            vrCr.Report.Vulnerabilities.Select(x => x.ToVulnerability()).ToArray().ToCveHash());
    }
    
    // private static string ComputeCvesSha256(VulnerabilityReportCr vrCr)
    // {
    //     VulnerabilityCr[] vulns = vrCr.Report.Vulnerabilities;
    //
    //     StringBuilder sb = new(vulns.Length * 20);
    //
    //     foreach (VulnerabilityCr v in vulns
    //                  .OrderBy(v => v.VulnerabilityId, StringComparer.Ordinal))
    //     {
    //         sb.Append(v.VulnerabilityId)
    //             .Append('|')
    //             .Append(v.SeverityCr)
    //             .Append(',');
    //     }
    //
    //     byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
    //     byte[] hash = SHA256.HashData(bytes);
    //     return Convert.ToHexString(hash);
    // }
}
