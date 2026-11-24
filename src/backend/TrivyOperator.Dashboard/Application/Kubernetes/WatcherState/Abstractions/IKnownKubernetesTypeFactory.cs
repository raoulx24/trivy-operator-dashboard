namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Abstractions;

public interface IKnownKubernetesTypeFactory
{
    Type Get(string name);

    // the domain type name of a watched kubernetes object type (e.g. SbomReportCr -> SbomReport); null if unknown
    string? GetName(Type watchedKubernetesObjectType);
}

