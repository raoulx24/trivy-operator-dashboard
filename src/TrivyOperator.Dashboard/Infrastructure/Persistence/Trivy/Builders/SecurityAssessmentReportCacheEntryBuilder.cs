using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders;

public class SecurityAssessmentReportCacheEntryBuilder<TReport, TId>(ICacheEntityCodec codec)
    : ICacheEntryBuilder<TReport, TId>
    where TReport : ISecurityAssessmentReport<TReport, TId>
{
    public CacheEntry<TReport, TId> ToCacheEntry(TReport entry)
    {
        IReadOnlyList<Check> chacks = entry.Checks;
        int count = chacks.Count;
        CheckPersistenceModel[] persistenceModels = new CheckPersistenceModel[count];

        for (int i = 0; i < count; i++)
        {
            persistenceModels[i] = chacks[i].ToPersistenceModel();
        }

        return new CacheEntry<TReport, TId>
        {
            Entry = entry.WithChecks([]),
            EncodedDetails = codec.Encode(persistenceModels),
        };
    }

    public TReport ToEntity(CacheEntry<TReport, TId> cacheEntry)
    {
        CheckPersistenceModel[] persistenceModels = codec.Decode<CheckPersistenceModel[]>(cacheEntry.EncodedDetails);

        int count = persistenceModels.Length;
        List<Check> checks = new(count);

        for (int i = 0; i < count; i++)
        {
            checks.Add(persistenceModels[i].ToDomain());
        }

        return cacheEntry.Entry.WithChecks(checks);
    }
}
