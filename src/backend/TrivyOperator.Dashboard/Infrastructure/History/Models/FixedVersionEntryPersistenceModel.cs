namespace TrivyOperator.Dashboard.Infrastructure.History.Models;

// FixedVersion is null when there is no fix
public sealed record FixedVersionEntryPersistenceModel(
    string? FixedVersion,
    DateTime At
);
