using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed class TrivyDependencyTreeDto
{
    public required DigestNode Digest { get; init; }
}

public sealed class DigestNode
{
    public required string Code { get; init; } = "D-N";
    public required string Type { get; init; } = "Digest";
    public required string Description { get; init; }

    public string Id => GuidUtils.GetDeterministicGuid(
        ImageDigest,
        ImageName,
        ImageTag,
        ImageRepository).ToString().ToLowerInvariant();

    public required string ImageDigest { get; init; }
    public required string ImageName { get; init; }
    public required string ImageTag { get; init; }
    public required string ImageRepository { get; init; }

    public required TrivyReportNode[] TrivyReports { get; set; }
    public required WorkloadsNode Workloads { get; set; }
    public required VrHistoryNode VrHistory { get; set; }
}

public sealed class TrivyReportNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "TR";
    public required string Type { get; init; }
    public required string Description { get; init; }

    public required int CriticalCount { get; init; }
    public required int HighCount { get; init; }
    public required int MediumCount { get; init; }
    public required int LowCount { get; init; }
    public required int UnknownCount { get; init; }
}

public sealed class WorkloadsNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "W-N";
    public required string Type { get; init; } = "Workloads";
    public required string Description { get; init; }

    public required WorkloadNode[] Workloads { get; init; }
}

public sealed class WorkloadNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "W";
    public required string Type { get; init; }
    public required string Description { get; init; }

    public required string NamespaceName { get; init; }
    public required string ResourceKind { get; init; }
    public required string ResourceName { get; init; }

    public required ConfigAuditNode[] ConfigAudits { get; init; }
}

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

public sealed class VrHistoryNode
{
    public required string Id { get; init; }
    public required string Code { get; init; } = "VRH-N";
    public required string Type { get; init; } = "VulnerabilityReportHistory";
    public required string Description { get; init; }

    public required VrHistoryEntryNode[] Entries { get; init; }
}

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
