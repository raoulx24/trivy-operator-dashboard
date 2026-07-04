

using System.Collections.Immutable;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public static class SbomMappingExtensions
{
    public static SbomReport ToSbom(this SbomReportCr cr, SbomReport? other)
    {
        if (cr.Report is null)
            throw new ArgumentNullException(nameof(cr.Report));

        // vo layer
        var metadata = cr.Metadata.ToReportMetadata();
        var resource = cr.Metadata.ToResource();
        var imageMeta = TrivySharedMappingExtensions.ToImageMeta(cr.Report.Artifact, cr.Report.Registry);
        var digest =  TrivySharedMappingExtensions.ToDigest(cr.Report.Artifact);
        var scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);

        var summary = new Summary(0, 0, 0, 0, null, null);
        var sbomMetadata = ToSbomMetadata(cr);

        var lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );
        
        var occurrence = new ReportImageOccurrence(
            metadata,
            resource,
            imageMeta);
        
        // check if other has same digest and ns
        if (other is not null &&
            (other.ImageDigest != digest ||
            (other.Occurrences.Count > 0 && other.Occurrences[0].Metadata.NamespaceName != metadata.NamespaceName)))
        {
            other = null;
        }

        // previous is newer -> keep it, only update occurrences
        if (other is not null &&
            other.LastSeenAt > lastSeenAt)
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

        // core sbom materialization
        var allComponents = CollectAllComponents(cr);
        var idMap = BuildIdMap(allComponents);

        var sbomComponents = BuildDependencyLookup(
            cr.Report.Components,
            idMap);

        var components = BuildComponents(
            allComponents,
            idMap,
            sbomComponents);

        var root = ResolveRootNode(cr, idMap);

        return new SbomReport(
            occurrences,
            digest,
            lastSeenAt,
            scanner,
            summary,
            sbomMetadata,
            root,
            components);
    }
    
    private static SbomMetadata ToSbomMetadata(SbomReportCr cr)
    {
        var c = cr.Report?.Components;

        if (c is null)
        {
            return new SbomMetadata(
                string.Empty,
                string.Empty,
                new SbomSerialNumber(string.Empty),
                0,
                new Timestamp(DateTime.MinValue));
        }

        return new SbomMetadata(
            c.BomFormat,
            c.SpecVersion,
            new SbomSerialNumber(c.SerialNumber),
            c.Version ?? 0,
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
        var result = new List<License>();

        var licenses = source.Licenses;
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
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        var props = source.Properties;
        if (props is null)
            return dict;

        for (int i = 0; i < props.Length; i++)
        {
            var p = props[i];
            var key = p.Name;

            if (key.StartsWith("aquasecurity:trivy:", StringComparison.Ordinal))
                key = key["aquasecurity:trivy:".Length..];

            dict[key] = p.Value;
        }

        return dict;
    }
    
    private static Uri? TryUri(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri
            : null;
    
    private static List<ComponentCr> CollectAllComponents(SbomReportCr cr)
    {
        var list = new List<ComponentCr>();

        var children = cr.Report?.Components?.ChildComponents;
        if (children is not null)
        {
            list.AddRange(children);
        }

        var root = cr.Report?.Components?.MetadataCr?.ComponentCr;
        if (root is not null)
        {
            list.Add(root);
        }

        return list;
    }
    
    private static Dictionary<string, ComponentId> BuildIdMap(List<ComponentCr> components)
    {
        var map = new Dictionary<string, ComponentId>(components.Count, StringComparer.Ordinal);

        for (int i = 0; i < components.Count; i++)
        {
            var c = components[i];
            var refId = c.BomRef;

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
        var result = new Dictionary<ComponentId, ImmutableArray<ComponentId>>();

        var deps = components.Dependencies;
        if (deps is null)
            return result;

        for (int i = 0; i < deps.Length; i++)
        {
            var d = deps[i];

            if (d.Ref is null || d.DependsOn is null) continue;

            if (!idMap.TryGetValue(d.Ref, out var fromId))
                continue;

            var buffer = new List<ComponentId>(d.DependsOn.Length);

            for (int j = 0; j < d.DependsOn.Length; j++)
            {
                var depRef = d.DependsOn[j];

                if (idMap.TryGetValue(depRef, out var toId))
                {
                    buffer.Add(toId);
                }
            }

            result[fromId] = buffer.Count == 0
                ? ImmutableArray<ComponentId>.Empty
                : buffer.ToImmutableArray();
        }

        return result;
    }
    
    private static List<Component> BuildComponents(
        List<ComponentCr> all,
        Dictionary<string, ComponentId> idMap,
        Dictionary<ComponentId, ImmutableArray<ComponentId>> deps)
    {
        var result = new List<Component>(all.Count);

        for (int i = 0; i < all.Count; i++)
        {
            var c = all[i];
            
            if (c.BomRef is null) continue;

            if (!idMap.TryGetValue(c.BomRef, out var id))
                continue;

            deps.TryGetValue(id, out var dependsOn);

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
    
    private static ComponentId ResolveRootNode(SbomReportCr cr, Dictionary<string, ComponentId> idMap)
    {
        var rootRef = cr.Report?.Components?.MetadataCr?.ComponentCr?.BomRef;

        if (string.IsNullOrWhiteSpace(rootRef))
            return new ComponentId();

        return idMap.TryGetValue(rootRef, out var id)
            ? id
            : new ComponentId();
    }
    
    private static Dictionary<string, ComponentId> CreateBomRefMap(
        IEnumerable<ComponentCr> components)
    {
        var map = new Dictionary<string, ComponentId>(StringComparer.Ordinal);

        foreach (var component in components)
        {
            var bomRef = component.BomRef;

            if (string.IsNullOrWhiteSpace(bomRef))
                continue;

            if (map.ContainsKey(bomRef))
                continue;

            map.Add(
                bomRef,
                Guid.TryParse(bomRef, out _)
                    ? new ComponentId(bomRef)
                    : new ComponentId(GuidUtils.GetDeterministicGuid(bomRef).ToString()));
        }

        return map;
    }
    
    private static ComponentId NormalizeComponentId(
        string? bomRef,
        IReadOnlyDictionary<string, ComponentId> map)
    {
        if (string.IsNullOrWhiteSpace(bomRef))
            return new ComponentId();

        return map.TryGetValue(bomRef, out var id)
            ? id
            : new ComponentId();
    }
}