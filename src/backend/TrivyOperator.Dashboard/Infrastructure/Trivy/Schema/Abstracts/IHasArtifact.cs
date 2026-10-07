using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;

public interface IHasArtifact : IKubernetesObject<V1ObjectMeta>
{
    ArtifactCr Artifact { get; }
}