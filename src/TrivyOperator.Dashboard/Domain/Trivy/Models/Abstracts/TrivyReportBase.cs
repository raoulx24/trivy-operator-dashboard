using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.Models.Abstracts;

public abstract record TrivyReportBase(
    ReportMetadata Metadata)
{
    protected abstract Kind ExpectedKind { get; }
    protected abstract bool IsClusterScoped { get; }

    public virtual void Validate()
    {
        ValidateKind();
        ValidateNamespace();
    }

    protected void ValidateKind()
    {
        if (Metadata.Kind != ExpectedKind)
            throw new InvalidOperationException(
                $"Expected '{ExpectedKind}', got '{Metadata.Kind}'");
    }

    protected void ValidateNamespace()
    {
        if (IsClusterScoped != Metadata.NamespaceName.IsClusterScoped)
        {
            throw new InvalidOperationException(
                $"Scope mismatch. Expected clusterScoped={IsClusterScoped}, " +
                $"actual clusterScoped={Metadata.NamespaceName.IsClusterScoped}, " +
                $"type={GetType().Name}");
        }
    }
}