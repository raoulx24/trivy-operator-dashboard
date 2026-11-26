namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

public enum ScanDeleteResult
{
    Deleted,
    NotFound,
    // pending or running
    NotFinished,
}
