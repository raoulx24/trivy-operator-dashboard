using TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services;

// the subjects (Trivy side) drive the result; subjects not processed yet are returned as Pending
public class EndOfLifeService(
    ICache<EolSubject, Timestamp> subjects,
    ICache<EolSubject, EolInfo> eolInfos,
    IEndOfLifeRefreshTrigger trigger) : IEndOfLifeService
{
    public Task<IReadOnlyList<EolInfoDto>> GetAll(EolSubjectKind kind, CancellationToken ctx = default) =>
        Task.FromResult(GetDtos(subject => subject.Kind == kind));

    public Task<IReadOnlyList<EolInfoDto>> GetByName(EolSubjectKind kind, string name, CancellationToken ctx = default)
    {
        EolSubject probe = new(kind, name, null);

        return Task.FromResult(GetDtos(subject => subject.Kind == kind && subject.Name == probe.Name));
    }

    public Task<EolInfoDto?> GetByNameAndVersion(
        EolSubjectKind kind,
        string name,
        string version,
        CancellationToken ctx = default)
    {
        EolSubject subject = new(kind, name, version);

        EolInfoDto? result = subjects.ContainsKey(subject)
            ? GetEolInfo(subject).ToEolInfoDto()
            : null;

        return Task.FromResult(result);
    }

    public bool RequestRebuild() => trigger.RequestRebuild();

    private IReadOnlyList<EolInfoDto> GetDtos(Func<EolSubject, bool> predicate) =>
    [
        .. subjects.Keys
            .Where(predicate)
            .OrderBy(static subject => subject.Name, StringComparer.Ordinal)
            .ThenBy(static subject => subject.Version, StringComparer.Ordinal)
            .Select(subject => GetEolInfo(subject).ToEolInfoDto()),
    ];

    private EolInfo GetEolInfo(EolSubject subject) =>
        eolInfos.TryGetValue(subject, out EolInfo? eolInfo) ? eolInfo : EolInfo.Pending(subject);
}
