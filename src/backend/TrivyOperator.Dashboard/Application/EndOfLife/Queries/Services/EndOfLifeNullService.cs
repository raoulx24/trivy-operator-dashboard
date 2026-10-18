using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services;

public class EndOfLifeNullService : IEndOfLifeService
{
    public Task<IReadOnlyList<EolInfoDto>> GetAll(EolSubjectKind kind, CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<EolInfoDto>>([]);

    public Task<IReadOnlyList<EolInfoDto>> GetByName(EolSubjectKind kind, string name, CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<EolInfoDto>>([]);

    public Task<EolInfoDto?> GetByNameAndVersion(
        EolSubjectKind kind,
        string name,
        string version,
        CancellationToken ctx = default) => Task.FromResult<EolInfoDto?>(null);

    public void RequestRebuild() { }
}
