using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Contexts;

public class HttpHeaderKubernetesContesxtProvider(IHttpContextAccessor httpContextAccessor) : IKubernetesContextProvider
{
    public bool TryGetCurrentContext(out string? context)
    {
        context = httpContextAccessor.HttpContext?
            .Request.Headers["X-Kubernetes-Context"]
            .FirstOrDefault();

        return !string.IsNullOrWhiteSpace(context);
    }
}
