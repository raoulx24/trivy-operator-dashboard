using System.Threading.Channels;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;

public interface ITrivyReportAggregator<TKubernetesObject, TReport, TKey>
    where TKubernetesObject : CustomResource
    where TReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    Task<IReadOnlyDictionary<TKey, TReport>> AggregateAsync(
        ChannelReader<TKubernetesObject> reader,
        CancellationToken cancellationToken = default);
}
