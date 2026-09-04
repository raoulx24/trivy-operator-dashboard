using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public class HttpHeaderKubernetesContextResolver(IHttpContextAccessor httpContextAccessor) : IKubernetesContextResolver
{
    public bool TryResolveCurrentContext(out ContextName context)
    {
        string? httpContext = httpContextAccessor.HttpContext?.Request.Headers["X-Kubernetes-Context"].FirstOrDefault();

        context = new ContextName(httpContext);

        return !context.IsUnset;
    }
}
