using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

public interface ICrdFactory
{
    CustomResourceDefinition Get<TKubernetesObject>();
}
