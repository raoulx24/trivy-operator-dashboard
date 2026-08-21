namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
