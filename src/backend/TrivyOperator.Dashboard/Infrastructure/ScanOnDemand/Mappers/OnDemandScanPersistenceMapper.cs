using System.Collections.Frozen;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Models;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Mappers;

public static class OnDemandScanPersistenceMapper
{
    public static OnDemandScanPersistenceModel ToPersistenceModel(this OnDemandScan domain)
        => new(
            domain.Id.Value,
            domain.Image.FullName,
            [.. domain.RequestedReportTypes.Select(static x => x.ToString()),],
            domain.RequestedBy,
            domain.CreatedAt.Value,
            domain.State.ToString(),
            domain.UpdatedAt.Value,
            domain.FinishedAt?.Value,
            domain.PodName.IsValid ? domain.PodName.Value : null,
            domain.ImageDigest.IsValid ? domain.ImageDigest.Value : null,
            [.. domain.ProducedReportTypes.Select(static x => x.ToString()),],
            domain.FailureMessage,
            domain.StartedAt?.Value);

    // throws FormatException when the stored record cannot be read back (id, image or state)
    public static OnDemandScan ToDomain(this OnDemandScanPersistenceModel dto)
    {
        ScanId id = new(dto.Id);

        if (!id.IsValid)
        {
            throw new FormatException($"'{dto.Id}' is not a valid scan id.");
        }

        if (!ImageReference.TryParse(dto.Image, out ImageReference? image, out string? error))
        {
            throw new FormatException($"Scan {dto.Id}: {error}");
        }

        if (!Enum.TryParse(dto.State, out ScanState state))
        {
            throw new FormatException($"Scan {dto.Id}: '{dto.State}' is not a valid state.");
        }

        return new OnDemandScan(id, image, ToReportTypes(dto.RequestedReportTypes), dto.RequestedBy, new Timestamp(dto.CreatedAt))
        {
            State = state,
            UpdatedAt = new Timestamp(dto.UpdatedAt),
            StartedAt = dto.StartedAt is null ? null : new Timestamp(dto.StartedAt.Value),
            FinishedAt = dto.FinishedAt is null ? null : new Timestamp(dto.FinishedAt.Value),
            PodName = new ResourceName(dto.PodName),
            ImageDigest = new Digest(dto.ImageDigest),
            ProducedReportTypes = ToReportTypes(dto.ProducedReportTypes),
            FailureMessage = dto.FailureMessage,
        };
    }

    // unknown names (e.g. a type removed later) are skipped
    private static FrozenSet<ScanReportType> ToReportTypes(string[]? values)
        => (values ?? [])
            .Select(static x => Enum.TryParse(x, out ScanReportType type) ? (ScanReportType?)type : null)
            .OfType<ScanReportType>()
            .ToFrozenSet();
}
