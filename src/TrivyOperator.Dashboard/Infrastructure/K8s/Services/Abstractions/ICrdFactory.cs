using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

public interface ICrdFactory
{
    CustomResourceDefinition Get<TKubernetesObject>();
}
