using Microsoft.Extensions.Options;
using System.Collections.Frozen;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Services;

// the option lists are parsed once
// registry patterns: exact (myregistry.io, also with a port) or a leading wildcard (*.azurecr.io, matches subdomains only);
// compared to the normalized registry of the image (docker.io for Docker Hub); the deny list wins over the allow list
public sealed class ScanRequestValidator : IScanRequestValidator
{
    private const string DockerHubAlias = "index.docker.io";

    private readonly FrozenSet<ScanReportType> allowedReportTypes;
    private readonly string[] allowList;
    private readonly string[] denyList;

    public ScanRequestValidator(IOptions<ScanOnDemandOptions> options, ILogger<ScanRequestValidator> logger)
    {
        HashSet<ScanReportType> allowed = [];

        foreach (string value in Split(options.Value.AllowedReportTypes))
        {
            if (Enum.TryParse(value, true, out ScanReportType type) && Enum.IsDefined(type))
            {
                allowed.Add(type);
            }
            else
            {
                logger.LogWarning("ScanOnDemand:AllowedReportTypes contains an unknown report type: {reportType}", value);
            }
        }

        allowedReportTypes = allowed.ToFrozenSet();
        allowList = [.. Split(options.Value.RegistryAllowList).Select(NormalizePattern),];
        denyList = [.. Split(options.Value.RegistryDenyList).Select(NormalizePattern),];
    }

    public IReadOnlySet<ScanReportType> AllowedReportTypes => allowedReportTypes;

    public ScanRequestValidation Validate(string? imageReference, IReadOnlyCollection<ScanReportType> requested)
    {
        List<string> errors = [];

        if (!ImageReference.TryParse(imageReference, out ImageReference? image, out string? parseError))
        {
            errors.Add(parseError);
        }
        else
        {
            string registry = image.Registry.Value;

            if (denyList.Any(x => Matches(x, registry)))
            {
                errors.Add($"Registry '{registry}' is not allowed.");
            }
            else if (allowList.Length > 0 && !allowList.Any(x => Matches(x, registry)))
            {
                errors.Add($"Registry '{registry}' is not in the allowed registries.");
            }
        }

        FrozenSet<ScanReportType> reportTypes = requested.ToFrozenSet();

        if (allowedReportTypes.Count == 0)
        {
            errors.Add("No report type is allowed for scans on demand.");
        }
        else if (reportTypes.Count == 0)
        {
            errors.Add("Select at least one report type.");
        }
        else
        {
            ScanReportType[] notAllowed = [.. reportTypes.Where(x => !allowedReportTypes.Contains(x)),];

            if (notAllowed.Length > 0)
            {
                errors.Add($"Report types not allowed: {string.Join(", ", notAllowed)}.");
            }
        }

        return new ScanRequestValidation(image, reportTypes, errors);
    }

    // exact patterns match the registry with or without its port; wildcards match the host only
    private static bool Matches(string pattern, string registry)
    {
        string host = registry.Split(':', 2)[0];

        return pattern.StartsWith("*.", StringComparison.Ordinal)
            ? host.EndsWith(pattern[1..], StringComparison.Ordinal)
            : pattern == registry || pattern == host;
    }

    private static string NormalizePattern(string pattern)
    {
        string normalized = pattern.ToLowerInvariant();

        return normalized == DockerHubAlias ? ImageReference.DefaultRegistry : normalized;
    }

    private static string[] Split(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
