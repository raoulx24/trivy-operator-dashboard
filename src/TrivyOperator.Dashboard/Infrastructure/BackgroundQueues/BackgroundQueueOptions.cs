namespace TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

public record BackgroundQueueOptions
{
    public int Capacity { get; init; } = 500;
}
