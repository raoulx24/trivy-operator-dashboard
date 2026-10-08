using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record ReportImageOccurrence(ReportMetadata Metadata, ContainerName Container, ImageMeta ImageMeta)
{
    public ReportImageOccurrence() : this(new ReportMetadata(), new ContainerName(), new ImageMeta())
    { }
    
    public IReadOnlyList<ReportImageOccurrence> MergeInto(IReadOnlyList<ReportImageOccurrence>? others)
    {
        if (others is null)
            return [this,];

        List<ReportImageOccurrence> result = [.. others,];

        int index = result.FindIndex(x => x.Metadata.Uid == Metadata.Uid);

        if (index < 0)
        {
            result.Add(this);
        }
        else if (result[index].Metadata.CreationTimestamp < Metadata.CreationTimestamp)
        {
            result[index] = this;
        }

        return result;
    }
}
