namespace TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Models;

public sealed record TrivyDependencyTreeDto
{
    public required DigestNode Digest { get; init; }
}

public sealed record DigestNode
{
    public string Code => "D-N";
    public string Type => "Digest";

    public required string Id { get; init; }
    public required string Description { get; init; }

    public required string ImageDigest { get; init; }
    public required string ImageName { get; init; }
    public required string ImageTag { get; init; }
    public required string ImageRepository { get; init; }

    public required TrivyReportNode[] TrivyReports { get; init; }
    public required WorkloadsNode Workloads { get; init; }
    public required VrHistoryNode VrHistory { get; init; }
}

public sealed record TrivyReportNode
{
    public string Code => "TR";

    public required string Id { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }

    public required int CriticalCount { get; init; }
    public required int HighCount { get; init; }
    public required int MediumCount { get; init; }
    public required int LowCount { get; init; }
    public required int UnknownCount { get; init; }
}

public sealed record WorkloadsNode
{
    public string Code => "W-N";
    public string Type => "Workloads";

    public required string Id { get; init; }
    public required string Description { get; init; }

    public required WorkloadNode[] Workloads { get; init; }
}

public sealed record WorkloadNode
{
    public string Code => "W";

    public required string Id { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }

    public required string NamespaceName { get; init; }
    public required string ResourceKind { get; init; }
    public required string ResourceName { get; init; }

    public required ConfigAuditNode[] ConfigAudits { get; init; }
}

public sealed record ConfigAuditNode
{
    public string Code => "CA";
    public string Type => "ConfigAudit";

    public required string Id { get; init; }
    public required string Description { get; init; }

    public required int CriticalCount { get; init; }
    public required int HighCount { get; init; }
    public required int MediumCount { get; init; }
    public required int LowCount { get; init; }
}

public sealed record VrHistoryNode
{
    public string Code => "VRH-N";
    public string Type => "VulnerabilityReportHistory";

    public required string Id { get; init; }
    public required string Description { get; init; }

    public required VrHistoryEntryNode[] Entries { get; init; }
}

public sealed record VrHistoryEntryNode
{
    public string Code => "VRH";
    public string Type => "VulnerabilityReportSnapshot";

    public required string Id { get; init; }
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
