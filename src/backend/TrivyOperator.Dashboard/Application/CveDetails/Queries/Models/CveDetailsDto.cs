using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;

// Status - Ok or Error (ErrorMessage says why, and when to retry if rate limited)
// (unknown to the provider is not a dto - see CveDetailsLookupOutcomes.NotListed)
// FetchedAt - when the provider was asked (a cached Ok keeps its original time)
// the data fields are null unless Status is Ok
public sealed record CveDetailsDto(
    string CveId,
    string Status,
    string? ErrorMessage,
    DateTime FetchedAt,
    string? Description,
    DateTime? Published,
    DateTime? LastModified,
    IReadOnlyList<CveDetailsCvssDto>? Cvss,
    IReadOnlyList<string>? Weaknesses,
    IReadOnlyList<CveDetailsReferenceDto>? References);

// Source - who scored it: nvd@nist.gov, a CNA or an ADP
public sealed record CveDetailsCvssDto(
    string Version,
    string Source,
    decimal BaseScore,
    int SeverityId,
    string VectorString);

public sealed record CveDetailsReferenceDto(
    string Url,
    IReadOnlyList<string> Tags);

public static class CveDetailsDtoExtensions
{
    public static CveDetailsDto ToCveDetailsDto(this CveSourceResult<CveDetailsInfo> result, CveId id)
    {
        CveDetailsInfo? info = result.Data;

        return new CveDetailsDto(
            CveId: id.Value,
            Status: result.Status.Value,
            ErrorMessage: result.ErrorMessage,
            FetchedAt: result.FetchedAt.Value,
            Description: info?.Description,
            Published: info?.Published?.Value,
            LastModified: info?.LastModified?.Value,
            Cvss: info is null ? null : [.. info.Cvss.Select(static x => x.ToCveDetailsCvssDto()),],
            Weaknesses: info?.Weaknesses,
            References: info is null ? null : [.. info.References.Select(static x => x.ToCveDetailsReferenceDto()),]);
    }

    private static CveDetailsCvssDto ToCveDetailsCvssDto(this CveDetailsCvss cvss) =>
        new(
            Version: cvss.Version,
            Source: cvss.Source,
            BaseScore: cvss.BaseScore,
            SeverityId: cvss.Severity.Rank,
            VectorString: cvss.VectorString);

    private static CveDetailsReferenceDto ToCveDetailsReferenceDto(this CveDetailsReference reference) =>
        new(
            Url: reference.Url.InitialValue,
            Tags: reference.Tags);
}
