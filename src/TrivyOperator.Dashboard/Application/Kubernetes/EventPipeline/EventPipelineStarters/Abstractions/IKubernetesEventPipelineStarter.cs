namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters.Abstractions;

public interface IKubernetesEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
