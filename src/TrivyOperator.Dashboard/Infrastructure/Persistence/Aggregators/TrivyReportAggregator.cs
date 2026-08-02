using System.Threading.Channels;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators;

public abstract class TrivyReportAggregator<TKubernetesObject, TReport, TKey>(
    ITrivyReportMapper<TKubernetesObject, TReport> mapper,
    ITrivyReportKeyProvider<TKubernetesObject, TKey> keyProvider)
    : ITrivyReportAggregator<TKubernetesObject, TReport, TKey>
    where TKubernetesObject : CustomResource
    where TReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    public async Task<IReadOnlyDictionary<TKey, TReport>> AggregateAsync(
        ChannelReader<TKubernetesObject> reader,
        CancellationToken cancellationToken = default)
    {
        Dictionary<TKey, TReport> reports = new();

        await foreach (TKubernetesObject cr in reader.ReadAllAsync(cancellationToken))
        {
            TKey key = keyProvider.GetKey(cr);

            TReport? existing = ResolveExisting(key, reports);

            TReport report = mapper.MapToDomain(cr, existing);

            reports[key] = report;
        }

        return reports;
    }

    protected virtual TReport? ResolveExisting(TKey key, Dictionary<TKey, TReport> reports) => null;
}