using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Models;
using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.Report.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services;

public class TrivyReportDependenciesService(
    IConcurrentDictionaryCache<ConfigAuditReportCr> carCache,
    IConcurrentDictionaryCache<ExposedSecretReportCr> esrCache,
    IConcurrentDictionaryCache<SbomReportCr> srCache,
    IConcurrentDictionaryCache<VulnerabilityReportCr> vrCache) : ITrivyReportDependenciesService
{
    public Task<TrivyReportDependencyDto?> GetTryvyReportDependencies(string imageDigest, string namespaceName)
    {
        ExposedSecretReportCr[] esrReports = GetTrivyReportsFromCache(esrCache, namespaceName, imageDigest);
        SbomReportCr[] srReports = GetTrivyReportsFromCache(srCache, namespaceName, imageDigest);
        VulnerabilityReportCr[] vrReports = GetTrivyReportsFromCache(vrCache, namespaceName, imageDigest);
        ConfigAuditReportCr[] carReports = carCache.TryGetValue(namespaceName, out var carCacheValue)
            ? [.. carCacheValue.Select(x => x.Value)] : [];

        TrivyReportImageDto? imageDto = GetTrivyReportImageDto([esrReports, srReports, vrReports], namespaceName);

        if (imageDto == null)
        {
            return Task.FromResult<TrivyReportDependencyDto?>(null);
        }

        IEnumerable<TrivyReportDependencyKubernetesResourceBindingDto> digestBindings = esrReports
            .Select(r => r.ToTrivyReportDependencyKubernetesResourceBindingDto())
            .Concat(srReports.Select(r => r.ToTrivyReportDependencyKubernetesResourceBindingDto()))
            .Concat(vrReports.Select(r => r.ToTrivyReportDependencyKubernetesResourceBindingDto()));

        Dictionary<TrivyReportDependencyKubernetesResourceDto, List<TrivyReportDependencyDetailDto>> groupedByResource = digestBindings
            .GroupBy(x => x.KubernetesResource)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.TrivyReportDependency).ToList()
            );

        // Add config audit reports only if their Kubernetes resource exists in image-based groupings
        foreach (ConfigAuditReportCr car in carReports)
        {
            TrivyReportDependencyKubernetesResourceBindingDto binding = car.ToTrivyReportDependencyKubernetesResourceBindingDto();
            TrivyReportDependencyKubernetesResourceDto resource = binding.KubernetesResource;

            TrivyReportDependencyKubernetesResourceDto? key = groupedByResource.Keys
                .FirstOrDefault(key => key.ResourceKind == resource.ResourceKind && key.ResourceName == resource.ResourceName);
            
            if (key != null)
            {
                List<TrivyReportDependencyDetailDto> reportList = groupedByResource[key];
                reportList.Add(binding.TrivyReportDependency);
            }
        }

        TrivyReportDependencyKubernetesResourceLinkDto[] finalLinks = [.. groupedByResource
            .Select(kvp => new TrivyReportDependencyKubernetesResourceLinkDto
            {
                KubernetesResource = kvp.Key,
                TrivyReportDependencies = [.. kvp.Value]

            })];

        TrivyReportDependencyDto x = new()
        {
            Image = imageDto,
            KubernetesDependencies = finalLinks,
        };

        return Task.FromResult<TrivyReportDependencyDto?>(x);
    }

    private static T[] GetTrivyReportsFromCache<T>(IConcurrentDictionaryCache<T> cache, string namespaceName, string imageDigest)
        where T : ITrivyReportWithImage
    {
        T[] result = [];
        if (cache.TryGetValue(namespaceName, out var reports))
        {
            result = [.. reports
                .Select(kvp => kvp.Value)
                .Where(tr => tr.ImageArtifact?.Digest == imageDigest)];
        }
        return result;
    }

    private static TrivyReportImageDto? GetTrivyReportImageDto(ITrivyReportWithImage[][] allReports, string namespaceName)
    {
        foreach (var reports in allReports)
        {
            ITrivyReportWithImage? report = reports.FirstOrDefault();

            if (report != null)
            {
                return new()
                {
                    NamespaceName = namespaceName,
                    ImageDigest = report.ImageArtifact?.Digest ?? string.Empty,
                    ImageName = report.ImageArtifact?.Repository ?? string.Empty,
                    ImageTag = report.ImageArtifact?.Tag ?? string.Empty,
                    ImageRepository = report.ImageRegistry?.Server ?? string.Empty,
                };
            }
        }

        return null;
    }

}
