namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;

public interface IResourceKeyProvider<in TKubernetesResource, out TKey>
{
    TKey GetKey(TKubernetesResource kubernetesResource);
}
