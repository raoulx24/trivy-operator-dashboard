using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToPersistence;
using ControlResultPersistenceModel = TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels.ControlResultPersistenceModel;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.CacheEntryBuilders;

public class ClusterComplianceReportCacheEntryBuilder(ICacheEntityCodec codec)
    : ICacheEntryBuilder<ClusterComplianceReport, Uid>
{
    public CacheEntry<ClusterComplianceReport, Uid> ToCacheEntry(ClusterComplianceReport entry)
    {
        IReadOnlyList<ControlResult> controlChecks = entry.ControlChecks;
        int count = controlChecks.Count;
        ControlResultPersistenceModel[] persistenceModels = new ControlResultPersistenceModel[count];

        for (int i = 0; i < count; i++)
        {
            persistenceModels[i] = controlChecks[i].ToPersistenceModel();
        }

        return new CacheEntry<ClusterComplianceReport, Uid>
        {
            Entry = entry with { ControlChecks = [], },
            EncodedDetails = codec.Encode(persistenceModels),
        };
    }

    public ClusterComplianceReport ToEntity(CacheEntry<ClusterComplianceReport, Uid> cacheEntry)
    {
        ControlResultPersistenceModel[] persistenceModels = codec.Decode<ControlResultPersistenceModel[]>(cacheEntry.EncodedDetails);

        int count = persistenceModels.Length;
        List<ControlResult> controlResults = new(count);

        for (int i = 0; i < count; i++)
        {
            controlResults.Add(persistenceModels[i].ToDomain());
        }

        return cacheEntry.Entry with
        {
            ControlChecks = controlResults,
        };
    }
}
