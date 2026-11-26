namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

public enum ScanCancelResult
{
    // recorded; the runner cancels it on its next tick
    Accepted,
    NotFound,
    AlreadyFinished,
}
