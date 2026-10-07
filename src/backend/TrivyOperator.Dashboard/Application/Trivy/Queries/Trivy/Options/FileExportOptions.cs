namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Options;

public class FileExportOptions
{
    public string TempFolder { get; init; } = Path.GetTempPath();
}
