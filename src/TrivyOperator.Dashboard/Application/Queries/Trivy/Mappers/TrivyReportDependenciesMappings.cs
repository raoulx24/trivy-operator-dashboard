using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class TrivyReportDependenciesMappings
{
    public static DigestNode? ToDigestNode(
        this IEnumerable<IImageReport> reports,
        Digest digest)
    {
        IImageReport? latest =
            reports
                .OrderByDescending(x => x.LastSeenAt.Value)
                .FirstOrDefault();

        ReportImageOccurrence? occurrence = latest?.Occurrences.FirstOrDefault();

        if (occurrence is null)
            return null;

        return new DigestNode
        {
            Code = "D-N",
            Type = "Digest",
            Description = $"{occurrence.ImageMeta.Repo.Value}:{occurrence.ImageMeta.Tag.Value}",
            ImageDigest = digest.Value,
            ImageName = occurrence.ImageMeta.Repo.Value,
            ImageTag = occurrence.ImageMeta.Tag.Value,
            ImageRepository = occurrence.ImageMeta.Registry.Value,
            TrivyReports = [],
            Workloads = new WorkloadsNode
            {
                Id = NewFrontendId(),
                Code = "W-N",
                Type = "Workloads",
                Description = "Workloads using this image",
                Workloads = [],
            },
            VrHistory = new VrHistoryNode
            {
                Id = NewFrontendId(),
                Code = "VRH-N",
                Type = "VulnerabilityReportHistory",
                Description = "History of vulnerability reports",
                Entries = [],
            },
        };
    }

    public static TrivyReportNode[] ToTrivyReportNodes(this IEnumerable<IImageReport> reports)
    {
        List<TrivyReportNode> result = [];

        foreach (IImageReport report in reports)
        {
            if (report is VulnerabilityReport)
            {
                result.Add(new TrivyReportNode
                {
                    Id = report.Occurrences
                        .FirstOrDefault()?.Metadata.Uid.Value ?? NewFrontendId(),
                    Code = "TR",
                    Type = "Vulnerability",
                    Description = "Latest vulnerability report",
                    CriticalCount = report.SeverityCounters.CriticalCount,
                    HighCount = report.SeverityCounters.HighCount,
                    MediumCount = report.SeverityCounters.MediumCount,
                    LowCount = report.SeverityCounters.LowCount,
                    UnknownCount = report.SeverityCounters.UnknownCount,
                });

                continue;
            }
            
            if (report is ExposedSecretReport)
            {
                result.Add(new TrivyReportNode
                {
                    Id = report.Occurrences
                        .FirstOrDefault()?.Metadata.Uid.Value ?? NewFrontendId(),
                    Code = "TR",
                    Type = "ExposedSecret",
                    Description = "Latest exposed secret report",
                    CriticalCount = report.SeverityCounters.CriticalCount,
                    HighCount = report.SeverityCounters.HighCount,
                    MediumCount = report.SeverityCounters.MediumCount,
                    LowCount = report.SeverityCounters.LowCount,
                    UnknownCount = 0,
                });
                
                continue;
            }

            if (report is SbomReport)
            {
                result.Add(new TrivyReportNode
                {
                    Id = report.Occurrences
                        .FirstOrDefault()?.Metadata.Uid.Value ?? NewFrontendId(),
                    Code = "TR",
                    Type = "Sbom",
                    Description = "Latest SBOM report",
                    CriticalCount = 0,
                    HighCount = 0,
                    MediumCount = 0,
                    LowCount = 0,
                    UnknownCount = 0,
                });
            }
        }

        return [.. result];
    }

    public static WorkloadsNode ToWorkloadsNode(
        this IEnumerable<IImageReport> reports,
        IReadOnlyList<ConfigAuditReport> configAuditReports)
    {
        IEnumerable<ReportImageOccurrence> occurrences =
            reports.SelectMany(x => x.Occurrences);

        IEnumerable<IGrouping<(string Namespace, string Kind, string Name, string Container), ReportImageOccurrence>>
            groups = occurrences.GroupBy(x =>
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
                (string ns, string kind, string name, string container) = group.Key;

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
                    Code = "W",
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
            Code = "W-N",
            Type = "Workloads",
            Description = "Workloads using this image",
            Workloads = workloads,
        };
    }

    public static ConfigAuditNode ToConfigAuditNode(this ConfigAuditReport report)
    {
        return new ConfigAuditNode
        {
            Id = report.Metadata.Uid.Value,
            Code = "CA",
            Type = "ConfigAudit",
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

                string name = string.IsNullOrWhiteSpace(metadata.ImageMeta.Tag.Value)
                    ? metadata.ImageMeta.Repo.Value
                    : $"{metadata.ImageMeta.Repo.Value}:{metadata.ImageMeta.Tag.Value}";

                return new VrHistoryEntryNode
                {
                    Id = NewFrontendId(),
                    Code = "VRH",
                    Type = "VulnerabilityReportSnapshot",
                    Description = $"Snapshot at {snapshot.FirstSeenAt.Value:O}",
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
            Code = "VRH-N",
            Type = "VulnerabilityReportHistory",
            Description = "History of vulnerability reports",
            Entries = entries,
        };
    }

    private static OwnerReference? FindWorkloadOwner(
        ReportImageOccurrence occurrence)
    {
        // TODO: find a better way than this
        return occurrence.Metadata.OwnerReferences
            .FirstOrDefault();
    }

    private static ConfigAuditNode CreateEmptyConfigAuditNode()
    {
        return new ConfigAuditNode
        {
            Id = NewFrontendId(),
            Code = "CA",
            Type = "ConfigAudit",
            Description = "No config audit reports",
            CriticalCount = 0,
            HighCount = 0,
            MediumCount = 0,
            LowCount = 0,
        };
    }

    private static string NewFrontendId() =>
        Guid.NewGuid().ToString().ToLowerInvariant();
}
