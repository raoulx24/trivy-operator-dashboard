namespace TrivyOperator.Dashboard.Application.Home.Queries.Models;

// the filters of the home page queries
// Since: start of the "since last visit" window (UTC); used only by the queries about changes
// SeverityIds: the severities taken into account; null or empty = critical and high
// Namespaces: only what runs in these namespaces (cluster scoped images and reports are then out); null or empty = all
// Limit: maximum number of items of a list
public sealed record HomeFilter(
    DateTime Since,
    IReadOnlyCollection<int>? SeverityIds,
    IReadOnlyCollection<string>? Namespaces,
    int Limit);
