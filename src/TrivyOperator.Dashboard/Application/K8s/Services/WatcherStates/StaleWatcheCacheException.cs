namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates;

public class StaleWatcheCacheException : Exception
{
    public StaleWatcheCacheException(string message, string watcherKey, Type kubernetesObjectType)
        : base(message)
    {
        WatcherKey = watcherKey;
        KubernetesObjectType = kubernetesObjectType;
    }

    public StaleWatcheCacheException(
        string message,
        string watcherKey,
        Type kubernetesObjectType,
        Exception innerException
    )
        : base(message, innerException)
    {
        WatcherKey = watcherKey;
        KubernetesObjectType = kubernetesObjectType;
    }

    public string WatcherKey { get; }
    public Type KubernetesObjectType { get; }
}
