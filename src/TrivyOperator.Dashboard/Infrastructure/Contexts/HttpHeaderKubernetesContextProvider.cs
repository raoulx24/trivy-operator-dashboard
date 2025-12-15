using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Contexts;

public class HttpHeaderKubernetesContesxtProvider(IHttpContextAccessor httpContextAccessor) : IKubernetesContextProvider
{
    public string GetCurrentContext()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers["X-Kubernetes-Context"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header))
            throw new InvalidOperationException("No Kubernetes context header provided.");
        return header;
    }
}
