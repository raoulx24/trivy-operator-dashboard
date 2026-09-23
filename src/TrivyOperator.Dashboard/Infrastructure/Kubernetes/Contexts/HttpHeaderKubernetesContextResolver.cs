using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts;

public class HttpHeaderKubernetesContextResolver(IHttpContextAccessor httpContextAccessor) : IKubernetesContextResolver
{
    public bool TryGetCurrentContext(out ContextName context)
    {
        string? httpContext = httpContextAccessor.HttpContext?.Request.Headers["X-Kubernetes-Context"].FirstOrDefault();

        context = new ContextName(httpContext);

        return !context.IsUnset;
    }
}
