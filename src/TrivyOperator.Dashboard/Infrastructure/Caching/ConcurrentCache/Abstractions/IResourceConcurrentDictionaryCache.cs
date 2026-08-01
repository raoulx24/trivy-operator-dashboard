using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

public interface IResourceConcurrentDictionaryCache<TKey, TValue> : IConcurrentCache<ContextName, ConcurrentDictionary<TKey, TValue>>
where TKey : notnull;
