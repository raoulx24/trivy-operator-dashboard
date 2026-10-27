using TrivyOperator.Dashboard.Domain.Nvd.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Models;

// Status - Ok, NotListed (unknown to NVD) or Error (ErrorMessage says why, and when to retry if rate limited)
// FetchedAt - when NVD was asked (a cached Ok keeps its original time)
// the data fields are null unless Status is Ok
public sealed record NvdCveDto(
    string CveId,
    string Status,
    string? ErrorMessage,
    DateTime FetchedAt,
    string? VulnStatus,
    string? Description,
    DateTime? Published,
    DateTime? LastModified,
    IReadOnlyList<NvdCvssDto>? Cvss,
    IReadOnlyList<string>? Weaknesses,
    IReadOnlyList<NvdReferenceDto>? References);

// Source - nvd@nist.gov, a CNA or an ADP id (guid); IsPrimary - NVD's own score
public sealed record NvdCvssDto(
    string Version,
    string Source,
    bool IsPrimary,
    decimal BaseScore,
    int SeverityId,
    string VectorString);

public sealed record NvdReferenceDto(
    string Url,
    IReadOnlyList<string> Tags);

public static class NvdCveDtoExtensions
{
    public static NvdCveDto ToNvdCveDto(this CveSourceResult<NvdInfo> result, CveId id)
    {
        NvdInfo? info = result.Data;

        return new NvdCveDto(
            CveId: id.Value,
            Status: result.Status.Value,
            ErrorMessage: result.ErrorMessage,
            FetchedAt: result.FetchedAt.Value,
            VulnStatus: info?.Status.Value,
            Description: info?.Description,
            Published: info?.Published?.Value,
            LastModified: info?.LastModified?.Value,
            Cvss: info is null ? null : [.. info.Cvss.Select(static x => x.ToNvdCvssDto()),],
            Weaknesses: info?.Weaknesses,
            References: info is null ? null : [.. info.References.Select(static x => x.ToNvdReferenceDto()),]);
    }

    private static NvdCvssDto ToNvdCvssDto(this NvdCvss cvss) =>
        new(
            Version: cvss.Version,
            Source: cvss.Source,
            IsPrimary: cvss.IsPrimary,
            BaseScore: cvss.BaseScore,
            SeverityId: cvss.Severity.Rank,
            VectorString: cvss.VectorString);

    private static NvdReferenceDto ToNvdReferenceDto(this NvdReference reference) =>
        new(
            Url: reference.Url.InitialValue,
            Tags: reference.Tags);
}
