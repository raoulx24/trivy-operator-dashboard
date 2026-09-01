namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Options;

public class FileExportOptions
{
    public string TempFolder { get; init; } = Path.GetTempPath();
}
