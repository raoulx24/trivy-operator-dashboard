using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;

public interface IEndOfLifeService : IQueryService
{
    Task<IReadOnlyList<EolInfoDto>> GetAll(EolSubjectKind kind, CancellationToken ctx = default);
    Task<IReadOnlyList<EolInfoDto>> GetByName(EolSubjectKind kind, string name, CancellationToken ctx = default);
    Task<EolInfoDto?> GetByNameAndVersion(EolSubjectKind kind, string name, string version, CancellationToken ctx = default);

    // marks all products for refetch and wakes up the processing
    void RequestRebuild();
}
