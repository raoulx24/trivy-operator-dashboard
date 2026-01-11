namespace TrivyOperator.Dashboard.Application.Common.BackgroundQueues;

public record BackgroundQueueOptions
{
    public int Capacity { get; init; } = 500;
}
