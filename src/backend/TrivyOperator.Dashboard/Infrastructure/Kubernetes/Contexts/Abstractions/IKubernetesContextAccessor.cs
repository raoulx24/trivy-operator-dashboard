using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

public interface IKubernetesContextAccessor : IKubernetesContextResolver
{
    IDisposable PushContext(ContextName context);
}
