using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;
using Metadata = TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Metadata;

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

    public Task<bool> TrivyDependenciesExistAsync(
        string imageDigest,
        string namespaceName,
        CancellationToken ct = default
    )
    {
        VulnerabilityReportCr[] vrReports = GetReports(vrCache, namespaceName, imageDigest);
        if (vrReports.Length != 0)
        {
            return Task.FromResult(true);
        }
        ExposedSecretReportCr[] esrReports = GetReports(esrCache, namespaceName, imageDigest);
        if (esrReports.Length != 0)
        {
            return Task.FromResult(true);
        }
        SbomReportCr[] srReports = GetReports(srCache, namespaceName, imageDigest);
        return Task.FromResult(srReports.Length != 0);
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
                Id = Guid.NewGuid().ToString().ToLowerInvariant(),
                Code = "W-N",
                Type = "Workloads",
                Description = "Workloads using this image",
                Workloads = [],
            },
            VrHistory = new VrHistoryNode
            {
                Id = Guid.NewGuid().ToString().ToLowerInvariant(),
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
                Id = vr.Metadata.Uid,
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
                Id = esr.Metadata.Uid,
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
                Id = sbom.Metadata.Uid,
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

        IEnumerable<IGrouping<(string, string), ITrivyReportWithImage>> groups = all.GroupBy(r =>
        {
            IDictionary<string, string> labels = r.Metadata.Labels ?? new Dictionary<string, string>();
            labels.TryGetValue("trivy-operator.resource.kind", out string? kind);
            labels.TryGetValue("trivy-operator.resource.name", out string? name);
            return (kind ?? "N/A", name ?? "N/A");
        });

        List<WorkloadNode> workloadNodes = [];

        foreach (IGrouping<(string, string), ITrivyReportWithImage> g in groups)
        {
            (string kind, string name) = g.Key;

            ConfigAuditReportCr[] matchingCar = [.. car.Where(c =>
            {
                IDictionary<string, string> labels = c.Metadata.Labels ?? new Dictionary<string, string>();
                return labels.TryGetValue("trivy-operator.resource.kind", out string? k) &&
                       k == kind &&
                       labels.TryGetValue("trivy-operator.resource.name", out string? n) &&
                       n == name;
            }),];

            workloadNodes.Add(new WorkloadNode
            {
                Id = Guid.NewGuid().ToString().ToLowerInvariant(),
                Code = "W",
                Type = kind,
                Description = $"{kind}/{name}",
                ResourceKind = kind,
                ResourceName = name,
                ConfigAudits = BuildConfigAuditNode(matchingCar),
            });
        }

        return new WorkloadsNode
        {
            Id = Guid.NewGuid().ToString().ToLowerInvariant(),
            Code = "W",
            Type = "Workloads",
            Description = "Workloads using this image",
            Workloads = workloadNodes.ToArray(),
        };
    }

    private static ConfigAuditNode[] BuildConfigAuditNode(ConfigAuditReportCr[] cars)
    {
        if (cars.Length == 0)
        {
            return [new ConfigAuditNode
            {
                Id = Guid.NewGuid().ToString().ToLowerInvariant(),
                Code = "CA",
                Type = "ConfigAudit",
                Description = "No config audit reports",
                CriticalCount = 0,
                HighCount = 0,
                MediumCount = 0,
                LowCount = 0,
            },];
        }
        
        List<ConfigAuditNode> result = [];

        foreach (ConfigAuditReportCr c in cars)
        {
            ConfigAuditNode can = new()
            {
                Id = c.Metadata.Uid,
                Code = "CA",
                Type = "ConfigAudit",
                Description = "Config audit report",
                CriticalCount = c.Report?.Summary?.CriticalCount ?? 0,
                HighCount = c.Report?.Summary?.HighCount ?? 0,
                MediumCount = c.Report?.Summary?.MediumCount ?? 0,
                LowCount = c.Report?.Summary?.LowCount ?? 0,
            };
            result.Add(can);
        }

        return [.. result,];
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
                        Id = Guid.NewGuid().ToString().ToLowerInvariant(),
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
            Id = Guid.NewGuid().ToString().ToLowerInvariant(),
            Code = "VRH-N",
            Type = "VulnerabilityReportHistory",
            Description = "History of vulnerability reports",
            Entries = entries,
        };
    }
}
