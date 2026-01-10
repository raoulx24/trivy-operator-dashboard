namespace TrivyOperator.Dashboard.Application.Utils;

public class RetryDurationCalculator(double maxBackoffSeconds)
{
    public TimeSpan GetNextRetryDuration(int retryAttempt) =>
        TimeSpan.FromSeconds(maxBackoffSeconds * Math.Log(retryAttempt + 1));
}
