namespace TrivyOperator.Dashboard.Application.Trivy.Services.Options;

public class FileExportOptions
{
    public string TempFolder { get; init; } = Path.GetTempPath();
}
