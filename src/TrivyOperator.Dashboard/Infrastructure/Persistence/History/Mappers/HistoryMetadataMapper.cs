using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Persistence.History.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.History.Mappers;

public static class HistoryMetadataMapper
{
    public static HistoryMetadataPersistenceModel ToPersistenceModel(this HistoryMetadata source)
    {
        return new HistoryMetadataPersistenceModel(
            source.NamespaceNames.Select(x => x.Value).ToArray(),
            source.ImageMeta.Registry.Value,
            source.ImageMeta.Repo.Value,
            source.ImageMeta.Tag.Value,
            source.Current.Values.ToArray(),
            source.AddedCvesDeltas.Values.ToArray(),
            source.DroppedCvesDeltas.Values.ToArray()
        );
    }

    public static HistoryMetadata ToDomain(this HistoryMetadataPersistenceModel source)
    {
        return new HistoryMetadata(
            source.NamespaceNames.Select(x => new NamespaceName(x)),
            new ImageMeta(
                new ImageRegistry(source.Registry),
                new ImageRepository(source.Repository),
                new ImageTag(source.Tag)
            ),
            new SeverityCounters(source.Current),
            new SeverityCounters(source.AddedCvesDeltas),
            new SeverityCounters(source.DroppedCvesDeltas)
        );
    }
}
