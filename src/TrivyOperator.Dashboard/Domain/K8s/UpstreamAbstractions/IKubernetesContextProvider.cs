namespace TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions
{
    public interface IKubernetesContextProvider
    {
        bool TryGetCurrentContext(out string? context);
    }
}