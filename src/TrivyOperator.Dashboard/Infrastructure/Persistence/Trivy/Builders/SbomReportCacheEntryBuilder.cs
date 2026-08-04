using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders;

public class SbomReportCacheEntryBuilder<TReport, TId>(ICacheEntityCodec codec)
    : ICacheEntryBuilder<TReport, TId>
    where TReport : ISbomReport<TReport, TId>
{
    public CacheEntry<TReport, TId> ToCacheEntry(TReport entry)
    {
        IReadOnlyList<Component> components = entry.Components;
        int count = components.Count;
        ComponentPersistenceModel[] persistenceModels = new ComponentPersistenceModel[count];

        for (int i = 0; i < count; i++)
        {
            persistenceModels[i] = components[i].ToPersistenceModel();
        }

        return new CacheEntry<TReport, TId>
        {
            Entry = entry.WithComponents([]),
            EncodedDetails = codec.Encode(persistenceModels),
        };
    }

    public TReport ToEntity(CacheEntry<TReport, TId> cacheEntry)
    {
        ComponentPersistenceModel[] persistenceModels = codec.Decode<ComponentPersistenceModel[]>(cacheEntry.EncodedDetails);

        int count = persistenceModels.Length;
        List<Component> components = new(count);

        for (int i = 0; i < count; i++)
        {
            components.Add(persistenceModels[i].ToDomain());
        }

        return cacheEntry.Entry.WithComponents(components);
    }
}
