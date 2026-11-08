namespace TrivyOperator.Dashboard.Infrastructure.History.Models;

public sealed record CvesDeltasPersistenceModel(
    CveDeltaPersistenceModel[] Added,
    CveDeltaPersistenceModel[] Dropped
);

public sealed record CveDeltaPersistenceModel(
    string Id,
    string Severity,
    int Count
);
