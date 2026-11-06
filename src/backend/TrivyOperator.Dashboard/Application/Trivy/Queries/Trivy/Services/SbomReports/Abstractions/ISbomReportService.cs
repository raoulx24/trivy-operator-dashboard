using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.SbomReports.Abstractions;

public interface ISbomReportService : IQueryService
{
    Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(CancellationToken ctx = default);
    Task<SbomReportImageDto?> GetFullSbomReportImageDtoByDigest(string digest, CancellationToken ctx = default);

    Task<CycloneDxBom?> GetCycloneDxBomByDigest(string digest, CancellationToken ctx = default);
    Task<SpdxBom?> GetSpdxBomByDigest(string digest, CancellationToken ctx = default);

    Task<SbomExportFileDto?> CreateCycloneDxExportZipFile(
        SbomReportExportDto[] exportSboms,
        string fileType = "json",
        CancellationToken ctx = default
    );
    
    Task<string[]> GetActiveNamespaces(CancellationToken ctx = default);
}
