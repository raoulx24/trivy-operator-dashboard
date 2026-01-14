namespace TrivyOperator.Dashboard.Application.K8s.Services.RawDomain.Abstracts;

public interface IRawDomainQueryService
{
    Task<IReadOnlyList<object>> GetAllAsync(Type valueType, string key, CancellationToken ct = default);
}
