namespace TrivyOperator.Dashboard.Application.Shared.Jobs.Models;

// what a job tells the registry about itself when it starts
// - Name: unique, e.g. "History retention", "KEV catalog"
// - Group: the jobs of one group are shown as one row (e.g. the details housekeeping of every report family); use the name
//   itself for a job that has its own row
// - Interval: how often it runs; zero when it does not run on a clock
public sealed record JobDefinition(string Name, string Group, TimeSpan Interval);
