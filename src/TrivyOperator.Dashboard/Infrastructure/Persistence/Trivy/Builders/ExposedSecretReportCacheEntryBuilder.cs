using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Mappers;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders;

public class ExposedSecretReportCacheEntryBuilder(ICacheEntityCodec codec)
    : ICacheEntryBuilder<ExposedSecretReport, Digest>
{
    public CacheEntry<ExposedSecretReport, Digest> ToCacheEntry(ExposedSecretReport entry)
    {
        IReadOnlyList<Secret> secrets = entry.Secrets;
        int count = secrets.Count;
        SecretPersistenceModel[] persistenceModels = new SecretPersistenceModel[count];

        for (int i = 0; i < count; i++)
        {
            persistenceModels[i] = secrets[i].ToPersistenceModel();
        }

        return new CacheEntry<ExposedSecretReport, Digest>
        {
            Entry = entry with { Secrets = [], },
            EncodedDetails = codec.Encode(persistenceModels),
        };
    }

    public ExposedSecretReport ToEntity(CacheEntry<ExposedSecretReport, Digest> cacheEntry)
    {
        SecretPersistenceModel[] persistenceModels = codec.Decode<SecretPersistenceModel[]>(cacheEntry.EncodedDetails);

        int count = persistenceModels.Length;
        List<Secret> secrets = new(count);

        for (int i = 0; i < count; i++)
        {
            secrets.Add(persistenceModels[i].ToDomain());
        }

        return cacheEntry.Entry with
        {
            Secrets = secrets,
        };
    }
}
