using TrivyOperator.Dashboard.Application.Shared.Details.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Domain.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.SbomDetails.KubernetesEventProcessors;

// feeds the purl index from the sbom reports of the events: the purls store and the image purls store
// - the purls go first; the image keeps the stored (canonical) purl instances, so a purl string is held once
// - a purl found in several components of the image (e.g. on several paths) is kept once, with the licenses of all
// - components without a valid purl are skipped
// - nothing happens on delete (see DetailsCollector); the housekeeping removes the images no longer in the sbom cache
public sealed class PurlDetailsCollector<TReport>(
    IDetailRepository<PurlDetail, Purl> purlDetails,
    IDetailStore<ImagePurls, Digest> imagePurls,
    ILogger<PurlDetailsCollector<TReport>> logger
) : DetailsCollector<TReport, Digest>(logger)
    where TReport : class, ISbomReport<TReport, Digest>
{
    protected override async Task Collect(TReport report, CancellationToken ctx)
    {
        Dictionary<Purl, List<License>> licensesByPurl = [];

        foreach (Component component in report.Components)
        {
            if (component.Purl is not { IsValid: true } purl)
            {
                continue;
            }

            if (!licensesByPurl.TryGetValue(purl, out List<License>? licenses))
            {
                licenses = [];
                licensesByPurl[purl] = licenses;
            }

            licenses.AddRange(component.Licenses);
        }

        foreach ((Purl purl, List<License> licenses) in licensesByPurl)
        {
            await purlDetails.Upsert(PurlDetail.Create(purl, licenses), report.LastSeenAt, ctx);
        }

        IReadOnlyDictionary<Purl, PurlDetail> stored = await purlDetails.GetDetails(licensesByPurl.Keys, ctx);

        // a purl removed meanwhile by the housekeeping (not expected, it was just touched) keeps the report's instance
        ImagePurls image = ImagePurls.Create(
            report.Id,
            licensesByPurl.Keys.Select(x => stored.TryGetValue(x, out PurlDetail? detail) ? detail.Id : x));

        await imagePurls.Upsert(image, report.LastSeenAt, ctx);
    }
}
