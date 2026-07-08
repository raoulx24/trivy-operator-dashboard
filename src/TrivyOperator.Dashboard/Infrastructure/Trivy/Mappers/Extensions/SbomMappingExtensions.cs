using System.Collections.Immutable;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

public static class SbomMappingExtensions
{
    internal static SbomReport ToSbom(this SbomReportCr cr, SbomReport? existing)
    {
        // vo layer
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        NamespaceName namespaceName = new NamespaceName(cr.Metadata.NamespaceProperty);
        Resource resource = cr.Metadata.ToResource();
        ImageMeta imageMeta = TrivySharedMappingExtensions.ToImageMeta(cr.Report.Artifact, cr.Report.Registry);
        Digest digest =  TrivySharedMappingExtensions.ToDigest(cr.Report.Artifact);
        SbomMetadata sbomMetadata = ToSbomMetadata(cr.Report.Components);

        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );
        
        ReportImageOccurrence occurrence = new ReportImageOccurrence(metadata, resource, imageMeta);
        
        // check if existing has same digest and ns
        if (TrivySharedMappingExtensions.HasOtherSameId(existing, namespaceName, digest))
        {
            existing = null;
        }

        // existing is newer -> keep it, only update occurrences
        if (existing is not null && TrivySharedMappingExtensions.IsOtherNewer(existing, lastSeenAt))
        {
            return existing with
            {
                Occurrences = TrivySharedMappingExtensions.MergeOccurrences(
                    occurrence,
                    existing.Occurrences,
                    currentWins: false),
            };
        }
        
        SbomSummary summary = cr.Report.Summary.ToSbomSummary();
        Scanner scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);
        IReadOnlyList<ReportImageOccurrence> occurrences = TrivySharedMappingExtensions.MergeOccurrences(occurrence, existing?.Occurrences, currentWins: true);

        // core sbom
        List<ComponentCr> allComponents = CollectAllComponents(cr.Report);
        Dictionary<string, ComponentId> idMap = BuildIdMap(allComponents);

        Dictionary<ComponentId, ImmutableArray<ComponentId>> sbomComponents = BuildDependencyLookup(
            cr.Report.Components,
            idMap);

        List<Component> components = BuildComponents(
            allComponents,
            idMap,
            sbomComponents);

        ComponentId root = ResolveRootNode(cr.Report, idMap);

        return new SbomReport(
            occurrences,
            namespaceName,
            digest,
            lastSeenAt,
            scanner,
            summary,
            sbomMetadata,
            root,
            components);
    }
    
    public static ClusterSbomReport ToClusterSbom(this ClusterSbomReportCr cr, ClusterSbomReport? existing)
    {
        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );

        // is other newer than current?
        if (existing is not null && lastSeenAt < existing.LastSeenAt)
            return existing;
        
        // vo layer
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        Resource resource = cr.Metadata.ToResource();
        ImageMeta imageMeta = TrivySharedMappingExtensions.ToImageMeta(cr.Report.Artifact, cr.Report.Registry);
        Scanner scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);

        SbomSummary summary = cr.Report.Summary.ToSbomSummary();
        SbomMetadata sbomMetadata = ToSbomMetadata(cr.Report.Components);

        ReportImageOccurrence occurrence = new ReportImageOccurrence(metadata, resource, imageMeta);
        
        // core sbom
        List<ComponentCr> allComponents = CollectAllComponents(cr.Report);
        Dictionary<string, ComponentId> idMap = BuildIdMap(allComponents);

        Dictionary<ComponentId, ImmutableArray<ComponentId>> sbomComponents = BuildDependencyLookup(
            cr.Report.Components,
            idMap);

        List<Component> components = BuildComponents(
            allComponents,
            idMap,
            sbomComponents);

        ComponentId root = ResolveRootNode(cr.Report, idMap);

        return new ClusterSbomReport(
            occurrence,
            lastSeenAt,
            scanner,
            summary,
            sbomMetadata,
            root,
            components);
    }
    
    private static SbomMetadata ToSbomMetadata(ComponentsCr cr)
    {
        return new SbomMetadata(
            cr.BomFormat,
            cr.SpecVersion,
            new SbomSerialNumber(cr.SerialNumber),
            cr.Version ?? 0,
            new Timestamp(DateTime.MinValue)); // keep as-is until real source exists
    }
    
    private static Supplier? ToSupplier(SupplierCr? source)
    {
        if (source is null)
            return null;

        return new Supplier(
            source.Name,
            source.Email,
            source.Phone);
    }
    
    private static List<License> ToLicenses(ComponentCr source)
    {
        List<License> result = [];

        LicenseContainerCr[]? licenses = source.Licenses;
        if (licenses is null)
            return result;

        for (int i = 0; i < licenses.Length; i++)
        {
            var l = licenses[i].License;
            if (l is null)
                continue;

            if (!string.IsNullOrWhiteSpace(l.Name))
            {
                result.Add(new License(
                    l.Id,
                    l.Name,
                    TryUri(l.Url)));
            }
        }

        return result;
    }
    
    private static Dictionary<string, string> ToProperties(ComponentCr source)
    {
        Dictionary<string, string> dict = new(StringComparer.Ordinal);

        PropertyCr[]? props = source.Properties;
        if (props is null)
            return dict;

        for (int i = 0; i < props.Length; i++)
        {
            PropertyCr p = props[i];
            string key = p.Name;

            if (key.StartsWith("aquasecurity:trivy:", StringComparison.Ordinal))
                key = key["aquasecurity:trivy:".Length..];

            dict[key] = p.Value;
        }

        return dict;
    }
    
    private static Uri? TryUri(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            ? uri
            : null;
    
    private static List<ComponentCr> CollectAllComponents(ReportCr cr)
    {
        List<ComponentCr> list = [];

        ComponentCr[]? children = cr.Components.ChildComponents;
        if (children is not null)
        {
            list.AddRange(children);
        }

        ComponentCr? root = cr.Components.MetadataCr?.ComponentCr;
        if (root is not null)
        {
            list.Add(root);
        }

        return list;
    }
    
    private static Dictionary<string, ComponentId> BuildIdMap(List<ComponentCr> components)
    {
        Dictionary<string, ComponentId> map = new(components.Count, StringComparer.Ordinal);

        for (int i = 0; i < components.Count; i++)
        {
            ComponentCr c = components[i];
            string? refId = c.BomRef;

            if (string.IsNullOrWhiteSpace(refId))
                continue;

            if (map.ContainsKey(refId))
                continue;

            map[refId] =
                Guid.TryParse(refId, out _)
                    ? new ComponentId(refId)
                    : new ComponentId(GuidUtils.GetDeterministicGuid(refId).ToString());
        }

        return map;
    }
    
    private static Dictionary<ComponentId, ImmutableArray<ComponentId>> BuildDependencyLookup(
        ComponentsCr components,
        Dictionary<string, ComponentId> idMap)
    {
        Dictionary<ComponentId, ImmutableArray<ComponentId>> result = new();

        DependencyCr[]? deps = components.Dependencies;
        if (deps is null)
            return result;

        for (int i = 0; i < deps.Length; i++)
        {
            var d = deps[i];

            if (d.Ref is null || d.DependsOn is null) continue;

            if (!idMap.TryGetValue(d.Ref, out var fromId))
                continue;

            List<ComponentId> buffer = new(d.DependsOn.Length);

            for (int j = 0; j < d.DependsOn.Length; j++)
            {
                var depRef = d.DependsOn[j];

                if (idMap.TryGetValue(depRef, out ComponentId toId))
                {
                    buffer.Add(toId);
                }
            }

            result[fromId] = buffer.Count == 0
                ? ImmutableArray<ComponentId>.Empty
                : [..buffer,];
        }

        return result;
    }
    
    private static List<Component> BuildComponents(
        List<ComponentCr> all,
        Dictionary<string, ComponentId> idMap,
        Dictionary<ComponentId, ImmutableArray<ComponentId>> deps)
    {
        List<Component> result = new List<Component>(all.Count);

        for (int i = 0; i < all.Count; i++)
        {
            ComponentCr c = all[i];
            
            if (c.BomRef is null) continue;

            if (!idMap.TryGetValue(c.BomRef, out ComponentId id))
                continue;

            deps.TryGetValue(id, out ImmutableArray<ComponentId> dependsOn);

            result.Add(new Component(
                id,
                new ComponentName(c.Name),
                new ComponentVersion(c.Version),
                new ComponentType(c.Type),
                string.IsNullOrWhiteSpace(c.Purl) ? null : new Purl(c.Purl),
                ToSupplier(c.Supplier),
                ToLicenses(c),
                ToProperties(c),
                dependsOn.IsDefault ? ImmutableArray<ComponentId>.Empty : dependsOn));
        }

        return result;
    }
    
    private static ComponentId ResolveRootNode(ReportCr cr, Dictionary<string, ComponentId> idMap)
    {
        string? rootRef = cr.Components.MetadataCr?.ComponentCr?.BomRef;

        if (string.IsNullOrWhiteSpace(rootRef))
            return new ComponentId();

        return idMap.TryGetValue(rootRef, out var id)
            ? id
            : new ComponentId();
    }
    
    private static SbomSummary ToSbomSummary(this SummaryCr source)
    {
        return new SbomSummary(
            source.ComponentsCount,
            source.DependenciesCount);
    }
}