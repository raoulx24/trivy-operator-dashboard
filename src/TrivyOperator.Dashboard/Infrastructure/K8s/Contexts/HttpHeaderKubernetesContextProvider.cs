using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public class HttpHeaderKubernetesContextProvider(IHttpContextAccessor httpContextAccessor) : IKubernetesContextProvider
{
    public bool TryGetCurrentContext(out ContextName context)
    {
        string? httpContext = httpContextAccessor.HttpContext?.Request.Headers["X-Kubernetes-Context"].FirstOrDefault();

        context = new ContextName(httpContext);

        return !context.IsDefault;
    }
}
