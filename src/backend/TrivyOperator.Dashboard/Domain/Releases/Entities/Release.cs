using TrivyOperator.Dashboard.Domain.Releases.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Releases.Entities;

public sealed record Release(
    ReleaseId Id,
    ReleaseVersion Version,
    ReleaseName Name,
    ReleaseDescription Description,
    ResourceUrl Url,
    Timestamp PublishedAt,
    Timestamp CreatedAt,
    bool IsLatest)
{
    public Release MarkAsLatest() => this with { IsLatest = true, };

    public Release MarkAsNotLatest() => this with { IsLatest = false, };

    public Release()
        : this(
            new ReleaseId(),
            new ReleaseVersion(),
            new ReleaseName(),
            new ReleaseDescription(),
            new ResourceUrl(),
            new Timestamp(),
            new Timestamp(),
            false)
    { }
}
