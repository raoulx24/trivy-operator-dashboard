using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToPersistence;
using ComponentPersistenceModel = TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels.ComponentPersistenceModel;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.CacheEntryBuilders;

public class SbomReportCacheEntryBuilder<TReport, TId>(ICacheEntityCodec codec)
    : ICacheEntryBuilder<TReport, TId>
    where TReport : ISbomReport<TReport, TId>
{
    public CacheEntry<TReport, TId> ToCacheEntry(TReport entry)
    {
        IReadOnlyList<Component> components = entry.Components;
        int count = components.Count;
        ComponentPersistenceModel[] persistenceModels = new ComponentPersistenceModel[count];
        Dictionary<ComponentId, int> indexById = ComponentPersistenceMapper.ToIndexById(components);

        for (int i = 0; i < count; i++)
        {
            persistenceModels[i] = components[i].ToPersistenceModel(indexById);
        }

        // the root must point to the id its component gets on decode
        ComponentId rootNodeBomRef = indexById.TryGetValue(entry.RootNodeBomRef, out int rootIndex)
            ? ComponentPersistenceMapper.CreateComponentIds(GetReportKey(entry), count)[rootIndex]
            : entry.RootNodeBomRef;

        return new CacheEntry<TReport, TId>
        {
            Entry = entry.WithComponents([]).WithRootNodeBomRef(rootNodeBomRef),
            EncodedDetails = codec.Encode(persistenceModels),
        };
    }

    public TReport ToEntity(CacheEntry<TReport, TId> cacheEntry)
    {
        ComponentPersistenceModel[] persistenceModels = codec.Decode<ComponentPersistenceModel[]>(cacheEntry.EncodedDetails);

        int count = persistenceModels.Length;
        List<Component> components = new(count);
        ComponentId[] ids = ComponentPersistenceMapper.CreateComponentIds(GetReportKey(cacheEntry.Entry), count);

        for (int i = 0; i < count; i++)
        {
            components.Add(persistenceModels[i].ToDomain(ids[i], ids));
        }

        return cacheEntry.Entry.WithComponents(components);
    }

    private static string GetReportKey(TReport report) => report.Id?.ToString() ?? string.Empty;
}
