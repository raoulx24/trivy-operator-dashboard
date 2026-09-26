using TrivyOperator.Dashboard.Application.Queries.Shared.Identity;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ExposedSecretReportMappings
{
    public static ExposedSecretReportImageDto ToDto(
        this ExposedSecretReport report)
    {
        return new ExposedSecretReportImageDto(
            Uid: DeterministicId.Create(report.ImageDigest.Value).ToString(),

            NamespaceNames:
            [
                .. report.Occurrences
                    .Select(static occurrence => occurrence.Metadata.NamespaceName.Value)
                    .Distinct()
                    .OrderBy(static namespaceName => namespaceName),
            ],

            Digest: report.ImageDigest.Value,

            ImageInfos: [.. report.Occurrences.Select(static occurrence => occurrence.ToImageInfoDto()),],

            Resources: [.. report.Occurrences.Select(static occurrence => occurrence.ToResourceInfoDto()),],

            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UnknownCount: report.SeverityCounters.UnknownCount,

            UpdateTimestamp: report.LastSeenAt.Value,

            Details: [.. report.Secrets.Select(static secret => secret.ToDto()),]
        );
    }

    public static IReadOnlyList<ExposedSecretReportDto> ToDtos(
        this ExposedSecretReport report)
    {
        return [.. report.Occurrences.Select(occurrence => occurrence.ToDto(report)),];
    }

    public static IReadOnlyList<ExposedSecretReportDenormalizedDto> ToDenormalizedDtos(
        this ExposedSecretReport report,
        IReadOnlySet<int>? excludedSeverityIds = null)
    {
        return
        [
            .. report.Occurrences.SelectMany(
                    _ => report.Secrets,
                    (occurrence, secret) => new
                    {
                        occurrence,
                        secret,
                    }
                )
                .Where(x =>
                    excludedSeverityIds is null ||
                    !excludedSeverityIds.Contains(x.secret.Rule.Severity.Rank)
                )
                .Select(x => x.secret.ToDenormalizedDto(report, x.occurrence)),
        ];
    }

    private static ExposedSecretReportDto ToDto(
        this ReportImageOccurrence occurrence,
        ExposedSecretReport report)
    {
        return new ExposedSecretReportDto(
            Uid: occurrence.Metadata.Uid.Value,

            NamespaceName: occurrence.Metadata.NamespaceName.Value,

            Digest: report.ImageDigest.Value,

            ImageNameAndTag: $"{occurrence.ImageMeta.Registry.Value}:{occurrence.ImageMeta.Tag.Value}",

            ImageRepository: occurrence.ImageMeta.Repo.Value,

            ResourceName: occurrence.Metadata.GetResourceName().Value,
            ResourceKind: occurrence.Metadata.GetResourceKind().Value,
            ResourceContainerName: occurrence.Container.Value ?? string.Empty,

            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UnknownCount: report.SeverityCounters.UnknownCount,

            UpdateTimestamp: report.LastSeenAt.Value,

            Details: [.. report.Secrets.Select(static secret => secret.ToDto()),]
        );
    }

    private static ExposedSecretReportDetailDto ToDto(
        this Secret secret)
    {
        Uid key = new(DeterministicId.Create(
            secret.Rule.Severity.Rank,
            secret.Rule.Category.Value,
            secret.Rule.RuleId.Value,
            secret.Target.Value));

        return new ExposedSecretReportDetailDto(
            Id: key.Value,
            MatchKey: key.Value,

            Category: secret.Rule.Category.Value,
            Match: secret.Match.Value,
            RuleId: secret.Rule.RuleId.Value,
            SeverityId: secret.Rule.Severity.Rank,
            Target: secret.Target.Value,
            Title: secret.Rule.Title.Value
        );
    }

    private static ExposedSecretReportDenormalizedDto ToDenormalizedDto(
        this Secret secret,
        ExposedSecretReport report,
        ReportImageOccurrence occurrence)
    {
        return new ExposedSecretReportDenormalizedDto(
            Uid: occurrence.Metadata.Uid.Value,

            ResourceName: occurrence.Metadata.GetResourceName().Value,
            ResourceNamespace: occurrence.Metadata.NamespaceName.Value,
            ResourceKind: occurrence.Metadata.GetResourceKind().Value,
            ResourceContainerName: occurrence.Container.Value ?? string.Empty,

            ImageName: occurrence.ImageMeta.Repo.Value,
            ImageTag: occurrence.ImageMeta.Tag.Value,
            ImageDigest: report.ImageDigest.Value,
            ImageRepository: occurrence.ImageMeta.Registry.Value,

            UpdateTimestamp: report.LastSeenAt.Value,

            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UnknownCount: report.SeverityCounters.UnknownCount,

            Category: secret.Rule.Category.Value,
            Match: secret.Match.Value,
            RuleId: secret.Rule.RuleId.Value,
            SeverityId: secret.Rule.Severity.Rank,
            Target: secret.Target.Value,
            Title: secret.Rule.Title.Value
        );
    }
}
