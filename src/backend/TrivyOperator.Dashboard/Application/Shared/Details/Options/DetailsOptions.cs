namespace TrivyOperator.Dashboard.Application.Shared.Details.Options;

// the details (e.g. the check data of the security assessments) shared by the reports of a family
// the housekeeping removes the details no stored report references and that were not touched in the last GraceInHours
// a detail is touched by every report event, but at most once per TouchRefreshInSeconds; keep it well below GraceInHours
// the fixed version history has its own, longer grace (FixedVersionHistoryGraceInHours): a removed pair loses its
// history, and when its report comes back it is baselined again, as new; the grace must outlast the report gaps
// (report TTL replacement, rescan queue), not just the touch refresh
public class DetailsOptions
{
    public int HousekeepingIntervalInSeconds { get; init; } = 600;
    public int GraceInHours { get; init; } = 1;
    public int FixedVersionHistoryGraceInHours { get; init; } = 72;
    public int TouchRefreshInSeconds { get; init; } = 60;
}
