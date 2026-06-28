using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public static class SbomMappingExtensions
{
    public static SbomReport ToSbom(this SbomReportCr sbomReportCr)
    {
        if (sbomReportCr.Report is null)
            throw new ArgumentNullException(nameof(sbomReportCr.Report));

        // fake - dt should be sbomReportCr.Metadata.CreationTimestamp
        var metadata = new ReportMetadata(new ResourceName(sbomReportCr.Metadata.Name), new Kind(sbomReportCr.Kind), new NamespaceName(sbomReportCr.Metadata.NamespaceProperty), new Timestamp(DateTime.UtcNow), new Guid(sbomReportCr.Metadata.Uid));
        
        var resourceName = new ResourceName(
            sbomReportCr.Metadata.Labels != null &&
            sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.name", out string? resourceNameInt)
                ? resourceNameInt : string.Empty);
        var resourceNamespace = new NamespaceName(
            sbomReportCr.Metadata.Labels != null &&
            sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.namespace", out string? resourceNamespaceInt)
                ? resourceNamespaceInt : string.Empty);
        var resourceKind = new Kind(
            sbomReportCr.Metadata.Labels != null &&
             sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.kind", out string? resourceKindInt)
                ? resourceKindInt : string.Empty);
        var resourceContainerName = new ContainerName(
            sbomReportCr.Metadata.Labels != null &&
            sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.container.name", out string? resourceContainerNameInt)
                ? resourceContainerNameInt : string.Empty);
        var resource = new Resource(resourceName, resourceKind, resourceNamespace, resourceContainerName);
        

        var imageUsage = new ImageUsage(new Digest(sbomReportCr.Report.Artifact.Digest), []);
        var scanner = new Scanner(new ScannerName(sbomReportCr.Report.Scanner.Name), new ScannerVendor(sbomReportCr.Report.Scanner.Vendor), new ScannerVersion(sbomReportCr.Report.Scanner.Version));
        
        // missing - fake now
        var summary = new Summary(0, 0, 0, 0, null, null);

        var sn = new SbomSerialNumber(sbomReportCr.Report.CdxComponents.SerialNumber);
        
        // fake now - datatime is not right - sbomReportCr.Report.CdxComponents.timestamp
        var sbomMeta = new SbomMetadata(sbomReportCr.Report.CdxComponents.BomFormat, sbomReportCr.Report.CdxComponents.SpecVersion, sbomReportCr.Report.CdxComponents.Version, DateTime.UtcNow);
        
        var cdxComponents = sbomReportCr.Report.CdxComponents;

        var components = cdxComponents.ChildComponents
            .Select(ToDomain)
            .ToArray();

        var componentMap = components.ToDictionary(c => c.Id);

        var dependencies = cdxComponents.Dependencies
            .SelectMany(d => d.DependsOn.Select(dep => (Parent: d.Ref, Child: dep)))
            .Select(d => new Dependency(new ComponentId(d.Parent), new ComponentId(d.Child)))
            .ToList();

        var graph = new DependencyGraph(dependencies);

        var root = ToDomain(cdxComponents.CdxMetadata.CdxComponent);

        var result = TrivyReportFactory.CreateSbomReport<SbomReport>(metadata, resource, imageUsage, scanner, summary, sn, root, components, graph, sbomMeta);

        return result;
    }
    
    private static SbomComponent ToDomain(CdxComponent source)
    {
        var licenses = source.Licenses?
                           .Select(l => new License(
                               l.License?.Id,
                               l.License?.Name,
                               TryUri(l.License?.Url)))
                           .ToArray()
                       ?? [];

        var properties = source.Properties
            .ToDictionary(p => p.Name, p => p.Value);

        return new SbomComponent(
            new ComponentId(source.BomRef),
            new ComponentName(source.Name),
            new ComponentVersion(source.Version),
            string.IsNullOrWhiteSpace(source.Purl) ? null : new Purl(source.Purl),
            new ComponentType(source.Type),
            ToSupplier(source.Supplier),
            licenses,
            properties);
    }
    
    private static Supplier? ToSupplier(CdxSupplier? source)
    {
        if (source is null)
            return null;

        return new Supplier(
            source.Name,
            source.Email,
            source.Phone);
    }
    
    private static Uri? TryUri(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri
            : null;
}