using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Models;

public class WatcherEvent<TResource, TKey>
    where TResource : IEntity<TKey>
{
    public ResourceLocation Key { get; init; }
    public WatcherEventType WatcherEventType { get; init; }
    public TResource? Resource { get; init; }
    public Uid? ResourceId { get; init; }
    public Exception? Exception { get; init; }
}
