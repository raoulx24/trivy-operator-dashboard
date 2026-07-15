using System.Collections.Concurrent;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

public interface IConcurrentDictionaryCache<TValue> : IConcurrentCache<string, ConcurrentDictionary<string, TValue>>;
