namespace TrivyOperator.Dashboard.Application.K8s.Services.RawDomainQuery;

public class CacheNotRegisteredException : Exception
{
    public CacheNotRegisteredException(string? message) : base(message)
    {
    }
}
