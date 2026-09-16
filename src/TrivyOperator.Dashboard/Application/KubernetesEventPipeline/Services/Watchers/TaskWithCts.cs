namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Watchers;

public sealed class TaskWithCts(Task task, CancellationTokenSource cts, CancellationTokenSource linkedCts) : IDisposable
{
    public Task Task { get; } = task ?? throw new ArgumentNullException(nameof(task));
    public CancellationTokenSource Cts { get; } = cts ?? throw new ArgumentNullException(nameof(cts));
    public CancellationTokenSource LinkedCts { get; } = linkedCts ?? throw new ArgumentNullException(nameof(linkedCts));

    public void Dispose()
    {
        LinkedCts?.Dispose();
        Cts?.Dispose();
    }
}
