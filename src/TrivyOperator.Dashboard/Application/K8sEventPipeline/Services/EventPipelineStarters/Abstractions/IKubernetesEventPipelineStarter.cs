namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters.Abstractions;

public interface IKubernetesEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
