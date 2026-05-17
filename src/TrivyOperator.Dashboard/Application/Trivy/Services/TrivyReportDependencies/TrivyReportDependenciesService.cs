using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.Report.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies;

public sealed class TrivyReportDependenciesService(
    IConcurrentDictionaryCache<ConfigAuditReportCr> carCache,
    IConcurrentDictionaryCache<ExposedSecretReportCr> esrCache,
    IConcurrentDictionaryCache<SbomReportCr> srCache,
    IConcurrentDictionaryCache<VulnerabilityReportCr> vrCache,
    IVulnerabilityReportsHistoryStore vrHistoryStore
) : ITrivyReportDependenciesService
{
    public async Task<TrivyDependencyTreeDto?> GetTrivyDependencyTreeAsync(
        string imageDigest,
        string namespaceName,
        CancellationToken ct = default)
    {
        //
        // 1. Load reports from caches
        //
        ExposedSecretReportCr[] esrReports = GetReports(esrCache, namespaceName, imageDigest);
        SbomReportCr[] srReports  = GetReports(srCache, namespaceName, imageDigest);
        VulnerabilityReportCr[] vrReports  = GetReports(vrCache, namespaceName, imageDigest);

        ConfigAuditReportCr[] carReports =
            carCache.TryGetValue(namespaceName, out ConcurrentDictionary<string, ConfigAuditReportCr>? carDict)
                ? carDict.Values.ToArray()
                : [];

        //
        // 2. Load VR history snapshots
        //
        IReadOnlyList<SnapshotIndexEntry> vrHistorySnapshots = await vrHistoryStore.GetSnapshotIndexesAsync(
            new NamespaceName(namespaceName),
            new Digest(imageDigest),
            ct);

        //
        // 3. Build digest node (root)
        //
        DigestNode? digestNode = BuildDigestNode(namespaceName, imageDigest, vrReports, esrReports, srReports);
        if (digestNode is null)
            return null;

        //
        // 4. Build TR nodes (latest VR, ESR, SBOM)
        //
        digestNode.TrivyReports = BuildTrivyReportNodes(vrReports, esrReports, srReports);

        //
        // 5. Build workloads subtree (W-N → W → CA)
        //
        digestNode.Workloads = BuildWorkloadsNode(vrReports, esrReports, srReports, carReports);

        //
        // 6. Build VR history subtree (VRH-N → VRH)
        //
        digestNode.VrHistory = BuildVrHistoryNode(vrHistorySnapshots);

        //
        // 7. Return final tree
        //
        return new TrivyDependencyTreeDto
        {
            Digest = digestNode
        };
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private static T[] GetReports<T>(
        IConcurrentDictionaryCache<T> cache,
        string ns,
        string digest) where T : ITrivyReportWithImage
    {
        if (!cache.TryGetValue(ns, out ConcurrentDictionary<string, T>? dict))
            return [];

        return [.. dict.Values.Where(r => r.ImageArtifact?.Digest == digest),];
    }

    private static T? PickLatest<T>(IEnumerable<T> reports)
        where T : ITrivyReportWithImage
    {
        return reports
            .OrderByDescending(r => r.UpdateTimestamp)
            .FirstOrDefault();
    }

    private static DigestNode? BuildDigestNode(
        string ns,
        string digest,
        VulnerabilityReportCr[] vr,
        ExposedSecretReportCr[] esr,
        SbomReportCr[] sbom
    )
    {
        ITrivyReportWithImage? latest =
            (ITrivyReportWithImage?)PickLatest(vr) ??
            (ITrivyReportWithImage?)PickLatest(esr) ??
            (ITrivyReportWithImage?)PickLatest(sbom);


        if (latest is null)
            return null;

        return new DigestNode
        {
            Code = "D-N",
            Type = "Digest",
            Description = $"{latest.ImageArtifact?.Repository}:{latest.ImageArtifact?.Tag}",
            NamespaceName = ns,
            ImageDigest = digest,
            ImageName = latest.ImageArtifact?.Repository ?? "",
            ImageTag = latest.ImageArtifact?.Tag ?? "",
            ImageRepository = latest.ImageRegistry?.Server ?? "",
            TrivyReports = [],
            Workloads = new WorkloadsNode
            {
                Code = "W-N",
                Type = "Workloads",
                Description = "Workloads using this image",
                Workloads = [],
            },
            VrHistory = new VrHistoryNode
            {
                Code = "VRH-N",
                Type = "VulnerabilityReportHistory",
                Description = "History of vulnerability reports",
                Entries = [],
            },
        };
    }

    private static TrivyReportNode[] BuildTrivyReportNodes(
        VulnerabilityReportCr[] vrReports,
        ExposedSecretReportCr[] esrReports,
        SbomReportCr[] srReports)
    {
        List<TrivyReportNode> list = [];

        // VR
        if (PickLatest(vrReports) is { } vr)
        {
            list.Add(new TrivyReportNode
            {
                Code = "TR",
                Type = "Vulnerability",
                Description = "Latest vulnerability report",
                CriticalCount = vr.Report?.Summary?.CriticalCount ?? 0,
                HighCount = vr.Report?.Summary?.HighCount ?? 0,
                MediumCount = vr.Report?.Summary?.MediumCount ?? 0,
                LowCount = vr.Report?.Summary?.LowCount ?? 0,
                UnknownCount = vr.Report?.Summary?.UnknownCount ?? 0,
            });
        }

        // ESR
        if (PickLatest(esrReports) is { } esr)
        {
            list.Add(new TrivyReportNode
            {
                Code = "TR",
                Type = "ExposedSecret",
                Description = "Latest exposed secret report",
                CriticalCount = esr.Report?.Summary?.CriticalCount ?? 0,
                HighCount = esr.Report?.Summary?.HighCount ?? 0,
                MediumCount = esr.Report?.Summary?.MediumCount ?? 0,
                LowCount = esr.Report?.Summary?.LowCount ?? 0,
                UnknownCount = 0,
            });
        }

        // SBOM
        if (PickLatest(srReports) is { } sbom)
        {
            list.Add(new TrivyReportNode
            {
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

        return list.ToArray();
    }

    private static WorkloadsNode BuildWorkloadsNode(
        VulnerabilityReportCr[] vr,
        ExposedSecretReportCr[] esr,
        SbomReportCr[] sbom,
        ConfigAuditReportCr[] car
    )
    {
        ITrivyReportWithImage[] all = [.. vr.Cast<ITrivyReportWithImage>().Concat(esr).Concat(sbom),];

        IEnumerable<IGrouping<(string, string, string), ITrivyReportWithImage>> groups = all.GroupBy(r =>
        {
            IDictionary<string, string> labels = r.Metadata.Labels ?? new Dictionary<string, string>();
            labels.TryGetValue("trivy-operator.resource.kind", out string? kind);
            labels.TryGetValue("trivy-operator.resource.name", out string? name);
            labels.TryGetValue("trivy-operator.container.name", out string? container);
            return (kind ?? "N/A", name ?? "N/A", container ?? "N/A");
        });

        List<WorkloadNode> workloadNodes = [];

        foreach (IGrouping<(string, string, string), ITrivyReportWithImage> g in groups)
        {
            (string kind, string name, string container) = g.Key;

            ConfigAuditReportCr[] matchingCar = car.Where(c =>
            {
                var labels = c.Metadata.Labels ?? new Dictionary<string, string>();
                return labels.TryGetValue("trivy-operator.resource.kind", out string? k) && k == kind &&
                       labels.TryGetValue("trivy-operator.resource.name", out string? n) && n == name &&
                       labels.TryGetValue("trivy-operator.container.name", out string? cn) && cn == container;
            }).ToArray();

            workloadNodes.Add(new WorkloadNode
            {
                Code = "W",
                Type = kind,
                Description = $"{kind}/{name}",
                ResourceKind = kind,
                ResourceName = name,
                ContainerName = container,
                ConfigAudit = BuildConfigAuditNode(matchingCar),
            });
        }

        return new WorkloadsNode
        {
            Code = "W",
            Type = "Workloads",
            Description = "Workloads using this image",
            Workloads = workloadNodes.ToArray(),
        };
    }

    private static ConfigAuditNode BuildConfigAuditNode(ConfigAuditReportCr[] cars)
    {
        if (cars.Length == 0)
        {
            return new ConfigAuditNode
            {
                Code = "CA",
                Type = "ConfigAudit",
                Description = "No config audit reports",
                CriticalCount = 0,
                HighCount = 0,
                MediumCount = 0,
                LowCount = 0,
            };
        }

        long crit = 0, high = 0, med = 0, low = 0;

        foreach (var c in cars)
        {
            crit += c.Report?.Summary?.CriticalCount ?? 0;
            high += c.Report?.Summary?.HighCount ?? 0;
            med += c.Report?.Summary?.MediumCount ?? 0;
            low += c.Report?.Summary?.LowCount ?? 0;
        }

        return new ConfigAuditNode
        {
            Code = "CA",
            Type = "ConfigAudit",
            Description = "Aggregated config audit for workload",
            CriticalCount = crit,
            HighCount = high,
            MediumCount = med,
            LowCount = low
        };
    }

    private static VrHistoryNode BuildVrHistoryNode(IReadOnlyList<SnapshotIndexEntry> snapshots)
    {
        VrHistoryEntryNode[] entries =
        [
            .. snapshots.Select(s =>
                {
                    Metadata m = s.Metadata;
                    string name = string.IsNullOrWhiteSpace(m.ImageTag) ? m.ImageName : $"{m.ImageName}:{m.ImageTag}";

                    return new VrHistoryEntryNode
                    {
                        Code = "VRH",
                        Type = "VulnerabilityReportSnapshot",
                        Description = $"Snapshot at {s.FirstSeenAt.Value:O}",
                        Name = name,
                        FirstSeenAt = s.FirstSeenAt.Value,
                        LastSeenAt = s.LastSeenAt.Value,
                        CriticalCount = m.CriticalCount,
                        HighCount = m.HighCount,
                        MediumCount = m.MediumCount,
                        LowCount = m.LowCount,
                        UnknownCount = m.UnknownCount,
                    };
                }
            ),
        ];

        return new VrHistoryNode
        {
            Code = "VRH-N",
            Type = "VulnerabilityReportHistory",
            Description = "History of vulnerability reports",
            Entries = entries,
        };
    }
}
