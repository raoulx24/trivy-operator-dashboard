namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPipelineStarters.Abstractions;

public interface IKubernetesEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
