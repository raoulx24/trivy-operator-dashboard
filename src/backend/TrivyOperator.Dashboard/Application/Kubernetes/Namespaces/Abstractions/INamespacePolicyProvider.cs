using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Namespaces.Abstractions;

public interface INamespacePolicyProvider
{
    // one policy per context; today the same configuration applies to every context
    // the first call fixes the policy for the life of the process
    NamespacePolicy Get(ContextName contextName);

    // namespaces used by internal features (e.g. scan on demand), excluded from the main pipeline;
    // allowed only before the policy is fixed (before the watchers start)
    void Reserve(NamespaceName namespaceName);
}
