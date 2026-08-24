using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports;

public class SbomReportNullService : ISbomReportService
{
    public Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<SbomReportImageMinimalDto>>([]);

    public Task<SbomReportImageDto?> GetFullSbomReportImageDtoByDigest(string digest, CancellationToken ctx = default) 
        => Task.FromResult<SbomReportImageDto?>(null);

    public Task<CycloneDxBom?> GetCycloneDxBomByDigest(string digest, CancellationToken ctx = default) 
        => Task.FromResult<CycloneDxBom?>(null);

    public Task<SpdxBom?> GetSpdxBomByDigest(string digest, CancellationToken ctx = default) 
        => Task.FromResult<SpdxBom?>(null);

    public Task<SbomExportFileDto?> CreateCycloneDxExportZipFile(
        SbomReportExportDto[] exportSboms,
        string fileType = "json",
        CancellationToken ctx = default)
            => Task.FromResult<SbomExportFileDto?>(null);
}
