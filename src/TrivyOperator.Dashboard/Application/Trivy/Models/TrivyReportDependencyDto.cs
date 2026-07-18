using System;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

// Root DTO
public sealed class TrivyDependencyTreeDto
{
    public required DigestNode Digest { get; init; }
}

// D-N: Digest node (root)
public sealed class DigestNode
{
    public required string Code { get; init; } = "D-N";
    public required string Type { get; init; } = "Digest";
    public required string Description { get; init; }

    public string Id => GuidUtils.GetDeterministicGuid(
        NamespaceName,
        ImageDigest,
        ImageName,
        ImageTag,
        ImageRepository).ToString().ToLowerInvariant();

    public required string NamespaceName { get; init; }
    public required string ImageDigest { get; init; }
    public required string ImageName { get; init; }
    public required string ImageTag { get; init; }
    public required string ImageRepository { get; init; }

    public required TrivyReportNode[] TrivyReports { get; set; }
    public required WorkloadsNode Workloads { get; set; }
    public required VrHistoryNode VrHistory { get; set; }
}

// TR: Trivy report (vr, es, sbom)
public sealed class TrivyReportNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "TR";
    public required string Type { get; init; }   // "Vulnerability", "ExposedSecret", "Sbom"
    public required string Description { get; init; }

    public required int CriticalCount { get; init; }
    public required int HighCount { get; init; }
    public required int MediumCount { get; init; }
    public required int LowCount { get; init; }
    public required int UnknownCount { get; init; }
}

// W-N: Workloads aggregator node
public sealed class WorkloadsNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "W-N";
    public required string Type { get; init; } = "Workloads";
    public required string Description { get; init; }

    public required WorkloadNode[] Workloads { get; init; }
}

// W: Workload node
public sealed class WorkloadNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "W";
    public required string Type { get; init; }   // resource kind
    public required string Description { get; init; }

    public required string ResourceKind { get; init; }
    public required string ResourceName { get; init; }
    // public required string ContainerName { get; init; }

    public required ConfigAuditNode[] ConfigAudits { get; init; }
}

// CA: Config audit node per workload
public sealed class ConfigAuditNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "CA";
    public required string Type { get; init; } = "ConfigAudit";
    public required string Description { get; init; }

    public required int CriticalCount { get; init; }
    public required int HighCount { get; init; }
    public required int MediumCount { get; init; }
    public required int LowCount { get; init; }
}

// VRH-N: VR history aggregator node
public sealed class VrHistoryNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "VRH-N";
    public required string Type { get; init; } = "VulnerabilityReportHistory";
    public required string Description { get; init; }

    public required VrHistoryEntryNode[] Entries { get; init; }
}

// VRH: Single VR history entry
public sealed class VrHistoryEntryNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "VRH";
    public required string Type { get; init; } = "VulnerabilityReportSnapshot";
    public required string Description { get; init; }

    public required string Name { get; init; }
    public required DateTime FirstSeenAt { get; init; }
    public required DateTime LastSeenAt { get; init; }

    public required int CriticalCount { get; init; }
    public required int HighCount { get; init; }
    public required int MediumCount { get; init; }
    public required int LowCount { get; init; }
    public required int UnknownCount { get; init; }
}
