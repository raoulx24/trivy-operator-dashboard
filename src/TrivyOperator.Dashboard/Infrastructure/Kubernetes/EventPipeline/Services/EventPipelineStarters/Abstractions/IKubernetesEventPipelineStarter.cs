namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters.Abstractions;

public interface IKubernetesEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
