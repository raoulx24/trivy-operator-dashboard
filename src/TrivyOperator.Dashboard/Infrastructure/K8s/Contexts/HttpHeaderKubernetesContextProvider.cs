using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

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
