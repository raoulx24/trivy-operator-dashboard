using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Models;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Mappers;

public static class TrivyReportDependenciesMappings
{
    public static DigestNode? ToDigestNode(
        this List<IImageReport> reports,
        Digest digest,
        IReadOnlyList<ConfigAuditReport> configAuditReports,
        IEnumerable<SnapshotIndexEntry> snapshots)
    {
        IImageReport? latest =
            reports
                .OrderByDescending(x => x.LastSeenAt.Value)
                .FirstOrDefault();

        ReportImageOccurrence? occurrence =
            latest?.Occurrences.FirstOrDefault();

        if (occurrence is null)
            return null;

        return new DigestNode
        {
            Id = NewFrontendId(),
            Description = $"{occurrence.ImageMeta.Repo.Value}:{occurrence.ImageMeta.Tag.Value}",

            ImageDigest = digest.Value,
            ImageName = occurrence.ImageMeta.Repo.Value,
            ImageTag = occurrence.ImageMeta.Tag.Value,
            ImageRepository = occurrence.ImageMeta.Registry.Value,

            TrivyReports = reports.ToTrivyReportNodes(),

            Workloads = reports.ToWorkloadsNode(configAuditReports),

            VrHistory = snapshots.ToVrHistoryNode(),
        };
    }

    public static TrivyReportNode[] ToTrivyReportNodes(
        this IEnumerable<IImageReport> reports)
    {
        return
        [
            .. reports.Select(report => report switch
                {
                    VulnerabilityReport v =>
                        new TrivyReportNode
                        {
                            Id = GetReportId(v),
                            Type = "Vulnerability",
                            Description = "Latest vulnerability report",
                            CriticalCount = v.SeverityCounters.CriticalCount,
                            HighCount = v.SeverityCounters.HighCount,
                            MediumCount = v.SeverityCounters.MediumCount,
                            LowCount = v.SeverityCounters.LowCount,
                            UnknownCount = v.SeverityCounters.UnknownCount,
                        },

                    ExposedSecretReport s =>
                        new TrivyReportNode
                        {
                            Id = GetReportId(s),
                            Type = "ExposedSecret",
                            Description = "Latest exposed secret report",
                            CriticalCount = s.SeverityCounters.CriticalCount,
                            HighCount = s.SeverityCounters.HighCount,
                            MediumCount = s.SeverityCounters.MediumCount,
                            LowCount = s.SeverityCounters.LowCount,
                            UnknownCount = 0,
                        },

                    SbomReport sb =>
                        new TrivyReportNode
                        {
                            Id = GetReportId(sb),
                            Type = "Sbom",
                            Description = "Latest SBOM report",
                            CriticalCount = 0,
                            HighCount = 0,
                            MediumCount = 0,
                            LowCount = 0,
                            UnknownCount = 0,
                        },

                    _ => null,
                })
            .Where(x => x is not null)
            .Select(x => x!)
        ];
    }

    public static WorkloadsNode ToWorkloadsNode(
        this IEnumerable<IImageReport> reports,
        IReadOnlyList<ConfigAuditReport> configAuditReports)
    {
        IEnumerable<ReportImageOccurrence> occurrences =
            reports.SelectMany(x => x.Occurrences);

        IEnumerable<IGrouping<
            (string Namespace, string Kind, string Name, string Container),
            ReportImageOccurrence>> groups =
            occurrences.GroupBy(x =>
            {
                OwnerReference? owner = FindWorkloadOwner(x);

                return (
                    x.Metadata.NamespaceName.Value,
                    owner?.Kind.Value ?? "N/A",
                    owner?.Name.Value ?? "N/A",
                    x.Container.Value
                );
            });

        WorkloadNode[] workloads =
        [
            .. groups.Select(group =>
            {
                (string ns, string kind, string name, _) = group.Key;

                ReportImageOccurrence occurrence = group.First();
                OwnerReference? owner = FindWorkloadOwner(occurrence);

                ConfigAuditReport? configAudit =
                    configAuditReports.FirstOrDefault(x =>
                        owner is not null &&
                        x.Metadata.OwnerReferences.Any(reference =>
                            reference.Uid == owner.Value.Uid));

                return new WorkloadNode
                {
                    Id = NewFrontendId(),
                    Type = kind,
                    Description = $"{kind}/{name}",

                    NamespaceName = ns,
                    ResourceKind = kind,
                    ResourceName = name,

                    ConfigAudits = configAudit is null
                        ? [CreateEmptyConfigAuditNode()]
                        : [configAudit.ToConfigAuditNode()],
                };
            }),
        ];

        return new WorkloadsNode
        {
            Id = NewFrontendId(),
            Description = "Workloads using this image",
            Workloads = workloads,
        };
    }

    public static ConfigAuditNode ToConfigAuditNode(
        this ConfigAuditReport report)
    {
        return new ConfigAuditNode
        {
            Id = report.Metadata.Uid.Value,
            Description = "Config audit report",

            CriticalCount = report.SeverityCounters.CriticalCount,
            HighCount = report.SeverityCounters.HighCount,
            MediumCount = report.SeverityCounters.MediumCount,
            LowCount = report.SeverityCounters.LowCount,
        };
    }

    public static VrHistoryNode ToVrHistoryNode(
        this IEnumerable<SnapshotIndexEntry> snapshots)
    {
        VrHistoryEntryNode[] entries =
        [
            .. snapshots.Select(snapshot =>
            {
                HistoryMetadata metadata = snapshot.HistoryMetadata;

                string name = string.IsNullOrWhiteSpace(
                    metadata.ImageMeta.Tag.Value)
                    ? metadata.ImageMeta.Repo.Value
                    : $"{metadata.ImageMeta.Repo.Value}:{metadata.ImageMeta.Tag.Value}";

                return new VrHistoryEntryNode
                {
                    Id = NewFrontendId(),
                    Description =
                        $"Snapshot at {snapshot.FirstSeenAt.Value:O}",

                    Name = name,
                    FirstSeenAt = snapshot.FirstSeenAt.Value,
                    LastSeenAt = snapshot.LastSeenAt.Value,

                    CriticalCount = metadata.Current.CriticalCount,
                    HighCount = metadata.Current.HighCount,
                    MediumCount = metadata.Current.MediumCount,
                    LowCount = metadata.Current.LowCount,
                    UnknownCount = metadata.Current.UnknownCount,
                };
            }),
        ];

        return new VrHistoryNode
        {
            Id = NewFrontendId(),
            Description = "History of vulnerability reports",
            Entries = entries,
        };
    }

    private static OwnerReference? FindWorkloadOwner(
        ReportImageOccurrence occurrence)
    {
        // TODO: find a better way than this
        return occurrence.Metadata.OwnerReferences.FirstOrDefault();
    }

    private static ConfigAuditNode CreateEmptyConfigAuditNode()
    {
        return new ConfigAuditNode
        {
            Id = NewFrontendId(),
            Description = "No config audit reports",

            CriticalCount = 0,
            HighCount = 0,
            MediumCount = 0,
            LowCount = 0,
        };
    }

    private static string GetReportId(IImageReport report) =>
        report.Occurrences
            .FirstOrDefault()
            ?.Metadata
            .Uid
            .Value
        ?? NewFrontendId();

    private static string NewFrontendId() =>
        Guid.NewGuid().ToString().ToLowerInvariant();
}
