namespace TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;

public interface IArtifact
{
    string Digest { get; init; }
    string Repository { get; init; }
    string Tag { get; init; }
}
