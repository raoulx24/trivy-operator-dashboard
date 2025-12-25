using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Contexts;

public class DefaultKubernetesContextProvider(IKubernetesClientFactory kubernetesClientFactory) : IKubernetesContextProvider
{
    private readonly string currentContext = kubernetesClientFactory.GetCurrentContext();

    public string GetCurrentContext() => currentContext;
}
