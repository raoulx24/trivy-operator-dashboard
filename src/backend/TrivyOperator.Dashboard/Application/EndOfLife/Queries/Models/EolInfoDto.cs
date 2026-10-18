using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;

public sealed record EolInfoDto(
    EolSubjectKind Kind,
    string Name,
    string Version,
    EolStatus Status,
    EolNotApplicableReason? NotApplicableReason,
    string? ErrorMessage,
    string? Product,
    string? ProductLabel,
    string? ProductLink,
    string? ReleasePolicyLink,
    string? EolLabel,
    string? EoasLabel,
    string? EoesLabel,
    EolReleaseDto? Release,
    DateTime ResolvedAt);

public sealed record EolReleaseDto(
    string Cycle,
    string? Codename,
    string Label,
    DateOnly? ReleaseDate,
    bool IsLts,
    DateOnly? LtsFrom,
    bool IsEol,
    DateOnly? EolFrom,
    bool IsMaintained,
    bool? IsEoas,
    DateOnly? EoasFrom,
    bool? IsEoes,
    DateOnly? EoesFrom,
    string? LatestVersion,
    DateOnly? LatestDate,
    string? LatestLink);

public static class EolInfoDtoExtensions
{
    public static EolInfoDto ToEolInfoDto(this EolInfo info) =>
        new(
            Kind: info.Subject.Kind,
            Name: info.Subject.Name,
            Version: info.Subject.Version,
            Status: info.Status,
            NotApplicableReason: info.NotApplicableReason,
            ErrorMessage: info.ErrorMessage,
            Product: info.Product?.Id.Value,
            ProductLabel: info.Product?.Label,
            ProductLink: info.Product?.Link.InitialValue,
            ReleasePolicyLink: info.Product?.ReleasePolicyLink.InitialValue,
            EolLabel: info.Product?.Labels.Eol,
            EoasLabel: info.Product?.Labels.Eoas,
            EoesLabel: info.Product?.Labels.Eoes,
            Release: info.Release?.ToEolReleaseDto(),
            ResolvedAt: info.ResolvedAt.Value);

    public static EolReleaseDto ToEolReleaseDto(this EolRelease release) =>
        new(
            Cycle: release.Cycle.Value,
            Codename: release.Codename,
            Label: release.Label,
            ReleaseDate: release.ReleaseDate,
            IsLts: release.IsLts,
            LtsFrom: release.LtsFrom,
            IsEol: release.IsEol,
            EolFrom: release.EolFrom,
            IsMaintained: release.IsMaintained,
            IsEoas: release.IsEoas,
            EoasFrom: release.EoasFrom,
            IsEoes: release.IsEoes,
            EoesFrom: release.EoesFrom,
            LatestVersion: release.Latest?.Name,
            LatestDate: release.Latest?.Date,
            LatestLink: release.Latest?.Link.InitialValue);
}
