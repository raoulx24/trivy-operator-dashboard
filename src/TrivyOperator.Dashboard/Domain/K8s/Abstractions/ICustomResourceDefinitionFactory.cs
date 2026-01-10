using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface ICustomResourceDefinitionFactory
{
    CustomResourceDefinition Get<TKubernetesObject>();
}
