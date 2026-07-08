using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;


namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public static class ExposedSecretMappingExtensions
{
    public static ExposedSecretReport ToVExposedSecretReport(this ExposedSecretReportCr cr, ExposedSecretReport? other)
    {
        // vo layer
        var metadata = cr.Metadata.ToReportMetadata();
        var resource = cr.Metadata.ToResource();
        var namespaceName = new NamespaceName(cr.Metadata.NamespaceProperty);
        var imageMeta = TrivySharedMappingExtensions.ToImageMeta(cr.Report.Artifact, cr.Report.Registry);
        var digest =  TrivySharedMappingExtensions.ToDigest(cr.Report.Artifact);
        var scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);

        var summary = TrivySharedMappingExtensions.ToSummary(cr.Report.Summary);

        var lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );
        
        var occurrence = new ReportImageOccurrence(metadata, resource, imageMeta);
        
        // check if other has same digest and ns
        if (TrivySharedMappingExtensions.HasOtherSameId(other, namespaceName, digest))
        {
            other = null;
        }

        // previous is newer -> keep it, only update occurrences
        if (other is not null && TrivySharedMappingExtensions.IsOtherNewer(other, lastSeenAt))
        {
            return other with
            {
                Occurrences = TrivySharedMappingExtensions.MergeOccurrences(
                    occurrence,
                    other.Occurrences,
                    currentWins: false),
            };
        }

        var occurrences = TrivySharedMappingExtensions.MergeOccurrences(occurrence, other?.Occurrences, currentWins: true);

        // core esr
        var secrets = cr.Report.Secrets.Select(ToSecret).ToList();

        return new ExposedSecretReport(
            occurrences,
            namespaceName,
            digest,
            lastSeenAt,
            scanner,
            summary,
            secrets);
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