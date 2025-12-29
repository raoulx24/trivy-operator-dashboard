namespace TrivyOperator.Dashboard.Infrastructure.Abstractions;

public interface IKubernetesContextProvider
{
    bool TryGetCurrentContext(out string? context);
}