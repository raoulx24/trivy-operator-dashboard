using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators;

public class DigestKeyedReportAggregator<TKubernetesObject, TReport>(
    IResourceMapper<TKubernetesObject, TReport> mapper,
    IResourceKeyProvider<TKubernetesObject, Digest> keyProvider)
    : ResourceAggregator<TKubernetesObject, TReport, Digest>(
        mapper,
        keyProvider)
    where TKubernetesObject : CustomResource
    where TReport : class, IImageReport<TReport>
{
    protected override TReport? ResolveExisting(
        Digest key,
        Dictionary<Digest, TReport> reports)
    {
        reports.TryGetValue(key, out TReport? report);

        return report;
    }
}
