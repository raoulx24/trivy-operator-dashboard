using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;

public interface INvdService
{
    // null when the id is not a CVE, or not a CVE subject (not in the current vulnerability reports)
    Task<NvdCveDto?> GetById(string cveId, CancellationToken ctx = default);
}
