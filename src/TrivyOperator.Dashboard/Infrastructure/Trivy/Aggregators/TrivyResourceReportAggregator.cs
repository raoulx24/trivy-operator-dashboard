using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Aggregators;

public class TrivyResourceReportAggregator<TKubernetesObject, TReport>(
    ITrivyReportMapper<TKubernetesObject, TReport> mapper,
    ITrivyReportKeyProvider<TKubernetesObject, Uid> keyProvider
) : TrivyReportAggregator<TKubernetesObject, TReport, Uid>(mapper, keyProvider)
    where TKubernetesObject : CustomResource
    where TReport : class, IResourceReport;
