﻿using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport;

public class SbomReportService(
    IConcurrentCache<string, IList<SbomReportCr>> cache,
    INamespacedResourceWatchDomainService<SbomReportCr, CustomResourceList<SbomReportCr>> domainService)
    : ISbomReportService
{
    public Task<IEnumerable<SbomReportDto>> GetSbomReportDtos(string? namespaceName = null)
    {
        IEnumerable<SbomReportDto> dtos = cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Select(cr => cr.ToSbomReportDto()));

        return Task.FromResult(dtos);
    }

    public async Task<SbomReportDto?> GetSbomReportDtoByUid(Guid uid)
    {
        SbomReportCr? sr = null;

        foreach (string namespaceName in cache.Keys)
        {
            if (cache.TryGetValue(namespaceName, out IList<SbomReportCr>? sbomReportCrs))
            {
                sr = sbomReportCrs.FirstOrDefault(x => x.Metadata.Uid == uid.ToString());

                if (sr is not null)
                {
                    break;
                }
            }
        }

        if (sr != null)
        {
            try
            {
                return (await domainService.GetResource(sr.Metadata.Name, sr.Metadata.NamespaceProperty))
                    .ToSbomReportDto();
            }
            catch { }
        }

        return null;
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult(cache.Where(x => x.Value.Any()).Select(x => x.Key));
}
