namespace TrivyOperator.Dashboard.Application.K8s.Services.RawDomain;

public class CacheNotRegisteredException : Exception
{
    public CacheNotRegisteredException(string? message) : base(message)
    {
    }
}
