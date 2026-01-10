namespace TrivyOperator.Dashboard.Application.K8s.Services.Options;

public record BackgroundQueueOptions
{
    public int Capacity { get; init; } = 500;
}
