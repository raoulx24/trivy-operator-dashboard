namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Models;

// the API server refused the scan pod (missing service account, quota, admission, invalid spec, ...); retrying does not help
public sealed class ScanPodRejectedException(string message, Exception? innerException = null)
    : Exception($"The scan pod was rejected: {message}", innerException);
