using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports.Abstractions;

public interface ISbomReportService
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
}
