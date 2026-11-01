using System.Collections.Immutable;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

public static class SbomMappingExtensions
{
    internal static SbomReport ToSbom(
        this SbomReportCr cr,
        SbomReport? existing)
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
        if (existing is not null && existing.ImageDigest != digest)
            existing = null;

        // Existing report wins: preserve its expensive SBOM graph.
        if (existing is not null && existing.LastSeenAt > lastSeenAt)
        {
            SbomReport incomingHeader = existing with
            {
                Occurrences = [occurrence,],
                LastSeenAt = lastSeenAt,
                Layers = [],
                Components = [],
            };

            return existing.MergeFrom(incomingHeader);
        }

        // Incoming report wins: materialize the SBOM graph.
        SbomSummary summary = cr.Report.Summary.ToSbomSummary();
        Scanner scanner = cr.Report.Scanner.ToScanner();
        SbomMetadata sbomMetadata = ToSbomMetadata(cr.Report.Components);

        List<ComponentCr> allComponents = CollectAllComponents(cr.Report);
        Dictionary<string, ComponentId> idMap = BuildIdMap(allComponents);

        Dictionary<ComponentId, ImmutableArray<ComponentId>> sbomComponents =
            BuildDependencyLookup(cr.Report.Components, idMap);

        List<Component> components = BuildComponents(
            allComponents,
            idMap,
            sbomComponents);

        ComponentId root = ResolveRootNode(cr.Report, idMap);

        List<ImageLayer> layers = ExtractLayers(components, root);

        SbomReport incoming = new(
            [occurrence,],
            digest,
            lastSeenAt,
            scanner,
            summary,
            sbomMetadata,
            root,
            layers,
            components);

        return existing is null ? incoming : incoming.MergeFrom(existing);
    }
    
    public static ClusterSbomReport ToClusterSbom(
        this ClusterSbomReportCr cr,
        ClusterSbomReport? existing)
    {
        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow);

        // Map only the inexpensive occurrence before deciding which report wins.
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        ContainerName container = cr.Metadata.ToContainerName();
        ImageMeta imageMeta = cr.Report.Artifact.ToImageMeta(cr.Report.Registry);

        ReportImageOccurrence occurrence = new(metadata, container, imageMeta);

        // Existing report wins: preserve expensive SBOM details.
        if (existing is not null && existing.LastSeenAt > lastSeenAt)
        {
            ClusterSbomReport incomingHeader = existing with
            {
                Occurrence = occurrence,
                LastSeenAt = lastSeenAt,
                Components = [],
            };

            return existing.MergeFrom(incomingHeader);
        }

        // Incoming report wins: materialize expensive SBOM details.
        Scanner scanner = cr.Report.Scanner.ToScanner();
        SbomSummary summary = cr.Report.Summary.ToSbomSummary();
        SbomMetadata sbomMetadata = ToSbomMetadata(cr.Report.Components);

        List<ComponentCr> allComponents = CollectAllComponents(cr.Report);
        Dictionary<string, ComponentId> idMap = BuildIdMap(allComponents);

        Dictionary<ComponentId, ImmutableArray<ComponentId>> sbomComponents =
            BuildDependencyLookup(cr.Report.Components, idMap);

        List<Component> components = BuildComponents(
            allComponents,
            idMap,
            sbomComponents);

        ComponentId root = ResolveRootNode(cr.Report, idMap);

        ClusterSbomReport incoming = new(
            occurrence,
            lastSeenAt,
            scanner,
            summary,
            sbomMetadata,
            root,
            components);

        return existing is null ? incoming : incoming.MergeFrom(existing);
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
    
    private static List<ComponentProperty> ToProperties(ComponentCr source)
    {
        PropertyCr[]? props = source.Properties;
        if (props is null)
            return [];

        // A list, not a dictionary: CycloneDX allows repeated names (e.g. one DiffID per image layer).
        List<ComponentProperty> result = new(props.Length);

        for (int i = 0; i < props.Length; i++)
        {
            PropertyCr p = props[i];
            string key = p.Name;

            if (key.StartsWith("aquasecurity:trivy:", StringComparison.Ordinal))
                key = key["aquasecurity:trivy:".Length..];

            result.Add(new ComponentProperty(key, p.Value));
        }

        return result;
    }

    // Moves the layer properties out of the components into a per-image layer table:
    // the root's DiffID list gives the layer order, the components' LayerDiffID/LayerDigest
    // give each layer's digest and become a LayerIndex on the component.
    private static List<ImageLayer> ExtractLayers(List<Component> components, ComponentId root)
    {
        List<ImageLayer> layers = [];
        Dictionary<Digest, int> indexByDiffId = new();

        int rootIndex = components.FindIndex(c => c.Id == root);
        if (rootIndex >= 0)
        {
            Component rootComponent = components[rootIndex];
            List<ComponentProperty> remaining = new(rootComponent.Properties.Count);

            foreach (ComponentProperty p in rootComponent.Properties)
            {
                if (p.Name != LayerPropertyNames.DiffId)
                {
                    remaining.Add(p);
                    continue;
                }

                Digest diffId = new(p.Value);
                if (indexByDiffId.TryAdd(diffId, layers.Count))
                    layers.Add(new ImageLayer(diffId, new Digest(null)));
            }

            components[rootIndex] = rootComponent with { Properties = remaining, };
        }

        for (int i = 0; i < components.Count; i++)
        {
            if (i == rootIndex)
                continue;

            Component component = components[i];

            string? diffIdValue = null;
            string? digestValue = null;
            foreach (ComponentProperty p in component.Properties)
            {
                if (p.Name == LayerPropertyNames.LayerDiffId)
                    diffIdValue ??= p.Value;
                else if (p.Name == LayerPropertyNames.LayerDigest)
                    digestValue ??= p.Value;
            }

            // Without a DiffID there is no layer to point at; keep the properties as they are.
            if (diffIdValue is null)
                continue;

            Digest diffId = new(diffIdValue);
            if (!indexByDiffId.TryGetValue(diffId, out int layerIndex))
            {
                layerIndex = layers.Count;
                indexByDiffId[diffId] = layerIndex;
                layers.Add(new ImageLayer(diffId, new Digest(null)));
            }

            // DiffID -> digest is 1:1 within an image, so the first digest seen is kept.
            if (digestValue is not null && !layers[layerIndex].Digest.IsValid)
                layers[layerIndex] = layers[layerIndex] with { Digest = new Digest(digestValue), };

            components[i] = component with
            {
                Properties =
                [
                    .. component.Properties.Where(static p =>
                        p.Name != LayerPropertyNames.LayerDiffId && p.Name != LayerPropertyNames.LayerDigest),
                ],
                LayerIndex = layerIndex,
            };
        }

        return layers;
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
                    : new ComponentId(Guid.NewGuid().ToString());
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
                dependsOn.IsDefault ? ImmutableArray<ComponentId>.Empty : dependsOn,
                null));
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