using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.K8s.Abstractions;

public interface ICustomResourceDefinitionFactory
{
    CustomResourceDefinition Get<TKubernetesObject>();
}
