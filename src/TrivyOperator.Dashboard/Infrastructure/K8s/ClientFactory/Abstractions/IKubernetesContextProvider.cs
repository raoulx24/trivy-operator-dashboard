namespace TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

public interface IKubernetesContextProvider
{
    bool TryGetCurrentContext(out string? context);
}
