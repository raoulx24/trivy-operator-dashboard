namespace TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;

public interface IResourceKeyProvider<in TKubernetesResource, out TKey>
{
    TKey GetKey(TKubernetesResource kubernetesResource);
}
