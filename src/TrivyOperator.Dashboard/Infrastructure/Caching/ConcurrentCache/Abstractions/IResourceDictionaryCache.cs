using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

public interface IResourceDictionaryCache<TKey, TValue> : ICache<ContextName, ConcurrentDictionary<TKey, TValue>>
where TKey : notnull;
