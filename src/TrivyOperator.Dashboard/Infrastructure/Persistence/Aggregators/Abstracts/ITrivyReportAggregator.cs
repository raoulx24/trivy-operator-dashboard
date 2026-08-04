using System.Threading.Channels;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;

public interface ITrivyReportAggregator<TKubernetesObject, TReport, TKey>
    where TKubernetesObject : CustomResource
    where TReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    IReadOnlyDictionary<TKey, TReport> AggregateAsync(
        IEnumerable<TKubernetesObject> resources,
        CancellationToken cancellationToken = default
    );
    
    Task<IReadOnlyDictionary<TKey, TReport>> AggregateFromChannelAsync(
        ChannelReader<TKubernetesObject> reader,
        CancellationToken cancellationToken = default);
}
