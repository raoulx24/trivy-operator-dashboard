using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

public interface ICrdFactory
{
    CustomResourceDefinition Get<TKubernetesObject>();
}
