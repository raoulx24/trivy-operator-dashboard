using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport.Abstractions;

public interface ISbomReportService
{
    Task<IEnumerable<SbomReportDto>> GetSbomReportDtos(string? namespaceName = null);
    Task<IEnumerable<SbomReportImageDto>> GetSbomReportImageDtos(string? digest = null, string? namespaceName = null);
    Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(string? namespaceName = null);
    Task<SbomReportDto?> GetFullSbomReportDtoByUid(string uid);
    Task<SbomReportDto?> GetFullSbomReportDtoByUidNamespace(string uid, string namespaceName);
    Task<SbomReportDto?> GetFullSbomReportDtoByDigestNamespace(string digest, string namespaceName);

    Task<CycloneDxBom?> GetCycloneDxBomByDigestNamespace(
        string digest,
        string namespaceName,
        IEnumerable<SbomReportCr>? cachedValues = null
    );

    Task<SpdxBom?> GetSpdxBomByDigestNamespace(string digest, string namespaceName);
    Task<string> CreateCycloneDxExportZipFile(SbomReportExportDto[] exportSboms, string fileType = "json");
    void CleanupFile(string fileName);
    Task<IEnumerable<string>> GetActiveNamespaces();

    Task<IEnumerable<SbomReportImageResourceDto>> GetSbomReportImageResourceDtosByDigestAndNamespace(
        string digest,
        string namespaceName
    );
}
