using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

public interface IResourceConcurrentDictionaryCache<TKey, TValue> : IConcurrentCache<NamespaceName, ConcurrentDictionary<TKey, TValue>>
where TKey : notnull;
