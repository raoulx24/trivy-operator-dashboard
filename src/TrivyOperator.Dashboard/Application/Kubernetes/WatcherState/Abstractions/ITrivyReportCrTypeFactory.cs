namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Abstractions;

public interface ITrivyReportCrTypeFactory
{
    Type Get(string name);
}

