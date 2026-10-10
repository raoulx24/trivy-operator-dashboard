using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

public static class ExposedSecretMappingExtensions
{
    public static ExposedSecretReport ToVExposedSecretReport(
        this ExposedSecretReportCr cr,
        ExposedSecretReport? existing)
    {
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        ContainerName container = cr.Metadata.ToContainerName();
        ImageMeta imageMeta = cr.Report.Artifact.ToImageMeta(cr.Report.Registry);
        Digest digest = cr.Report.Artifact.ToDigest();

        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow);

        ReportImageOccurrence occurrence = new(metadata, container, imageMeta);

        // Different digests represent different report identities.
        if (existing?.ImageDigest != digest)
            existing = null;

        // Existing report wins: preserve its expensive secret details.
        if (existing is not null && existing.LastSeenAt > lastSeenAt)
        {
            ExposedSecretReport incomingHeader = existing with
            {
                Occurrences = [occurrence],
                LastSeenAt = lastSeenAt,
                Secrets = [],
            };

            return existing.MergeFrom(incomingHeader);
        }

        // Incoming report wins: materialize secret details.
        SeverityCounters severityCounters = cr.Report.Summary.ToSeverityCounters();
        Scanner scanner = cr.Report.Scanner.ToScanner();

        List<Secret> secrets = [.. cr.Report.Secrets.Select(ToSecret)];

        ExposedSecretReport incoming = new(
            [occurrence,],
            digest,
            lastSeenAt,
            scanner,
            severityCounters,
            secrets);

        return existing is null ? incoming : incoming.MergeFrom(existing);
    }
    
    private static Rule ToRule(this SecretCr cr)
    {
        return new Rule(
            new Category(cr.Category),
            new RuleId(cr.RuleId),
            new Severity(cr.SeverityCr.ToString()),
            new Title(cr.Title));
    }
    
    private static Secret ToSecret(this SecretCr cr)
    {
        return new Secret(
            cr.ToRule(),
            new Match(cr.Match),
            new Target(cr.Target));
    }
}