using System.Collections.Concurrent;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

public interface IConcurrentDictionaryCache<TValue> : IConcurrentCache<string, ConcurrentDictionary<string, TValue>>;
