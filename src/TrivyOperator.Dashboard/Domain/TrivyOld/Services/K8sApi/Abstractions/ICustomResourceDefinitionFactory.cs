using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.Services.K8sApi.Abstractions;

public interface ICustomResourceDefinitionFactory
{
    CustomResourceDefinition Get<TKubernetesObject>();
}
