using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;

public interface IEndOfLifeService
{
    Task<IReadOnlyList<EolInfoDto>> GetAll(EolSubjectKind kind, CancellationToken ctx = default);
    Task<IReadOnlyList<EolInfoDto>> GetByName(EolSubjectKind kind, string name, CancellationToken ctx = default);
    Task<EolInfoDto?> GetByNameAndVersion(EolSubjectKind kind, string name, string version, CancellationToken ctx = default);

    // marks all products for refetch and wakes up the processing; false when the feature is disabled
    bool RequestRebuild();
}
