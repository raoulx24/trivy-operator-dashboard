using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;

public interface IContextProvider
{
    IEnumerable<ContextName> GetContexts();
    ContextName GetDefaultContext();
}
