namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Abstractions;

public interface IKnownKubernetesTypeFactory
{
    Type Get(string name);
}

