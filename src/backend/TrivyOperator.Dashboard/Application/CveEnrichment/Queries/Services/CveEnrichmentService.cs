using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services;

// the subjects (Trivy side) drive the result; CVEs not processed yet by a source have that part null
public class CveEnrichmentService(
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveInfo> infos,
    ICveEnrichmentRefreshTrigger trigger) : ICveEnrichmentService
{
    public bool IsEnabled => true;

    public Task<IReadOnlyList<CveInfoDto>> GetAll(CancellationToken ctx = default)
    {
        IReadOnlyList<CveInfoDto> result =
        [
            .. subjects.ToArray()
                .OrderBy(static subject => subject.Key.Value, StringComparer.Ordinal)
                .Select(subject => GetCveInfo(subject.Key).ToCveInfoDto(subject.Key, subject.Value)),
        ];

        return Task.FromResult(result);
    }

    public Task<CveInfoDto?> GetById(string cveId, CancellationToken ctx = default)
    {
        CveId id = new(cveId);

        CveInfoDto? result = id.IsValid && subjects.TryGetValue(id, out Timestamp lastSeen)
            ? GetCveInfo(id).ToCveInfoDto(id, lastSeen)
            : null;

        return Task.FromResult(result);
    }

    public void RequestRebuild() => trigger.RequestRebuild();

    private CveInfo? GetCveInfo(CveId id) => infos.TryGetValue(id, out CveInfo? info) ? info : null;
}
