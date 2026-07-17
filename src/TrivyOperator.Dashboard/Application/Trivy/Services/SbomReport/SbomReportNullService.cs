using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport;

public class SbomReportNullService : ISbomReportService
{
    public Task<IEnumerable<SbomReportDto>> GetSbomReportDtos(string? namespaceName = null) =>
        Task.FromResult<IEnumerable<SbomReportDto>>([]);

    public Task<IEnumerable<SbomReportImageDto>> GetSbomReportImageDtos(
        string? digest = null,
        string? namespaceName = null
    ) =>
        Task.FromResult<IEnumerable<SbomReportImageDto>>([]);

    public Task<SbomReportDto?> GetFullSbomReportDtoByUid(string uid) =>
        Task.FromResult<SbomReportDto?>(null);

    public Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(string? namespaceName = null) =>
        Task.FromResult<IEnumerable<SbomReportImageMinimalDto>>([]);

    public Task<SbomReportDto?> GetFullSbomReportDtoByUidNamespace(string uid, string namespaceName) =>
        Task.FromResult<SbomReportDto?>(null);

    public Task<SbomReportDto?> GetFullSbomReportDtoByDigestNamespace(string digest, string namespaceName) =>
        Task.FromResult<SbomReportDto?>(null);

    public Task<CycloneDxBom?> GetCycloneDxBomByDigestNamespace(
        string digest,
        string namespaceName,
        IEnumerable<OldSbomReportCr>? cachedValues = null
    ) =>
        Task.FromResult<CycloneDxBom?>(null);

    public Task<SpdxBom?> GetSpdxBomByDigestNamespace(string digest, string namespaceName) =>
        Task.FromResult<SpdxBom?>(null);

    public Task<string> CreateCycloneDxExportZipFile(SbomReportExportDto[] exportSboms, string fileType = "json") =>
        Task.FromResult(string.Empty);

    public void CleanupFile(string fileName) { }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([]);

    public Task<IEnumerable<SbomReportImageResourceDto>> GetSbomReportImageResourceDtosByDigestAndNamespace(
        string digest,
        string namespaceName
    ) =>
        Task.FromResult<IEnumerable<SbomReportImageResourceDto>>([]);
}
