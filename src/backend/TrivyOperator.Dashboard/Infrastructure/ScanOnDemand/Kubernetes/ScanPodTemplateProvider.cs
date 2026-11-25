using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;

// the optional pod template (ScanOnDemand:PodTemplateFile): read once, checked, kept as JSON so every pod starts from
// a fresh copy; never throws while loading, the errors are returned
// rules: kind Pod; at most one container and one init container (another container would be scanned too, another
// init container would run)
public sealed class ScanPodTemplateProvider(
    IOptions<ScanOnDemandOptions> options,
    ILogger<ScanPodTemplateProvider> logger)
{
    private readonly Lock gate = new();
    private bool loaded;
    private string? templateJson;
    private IReadOnlyList<string> loadErrors = [];

    // true when there is no template or the template is valid
    public bool TryLoad(out IReadOnlyList<string> errors)
    {
        lock (gate)
        {
            if (!loaded)
            {
                Load();
                loaded = true;
            }

            errors = loadErrors;
            return loadErrors.Count == 0;
        }
    }

    // a deep copy of the template; null when none is configured; throws when the template is invalid
    public V1Pod? CreateBase()
    {
        if (!TryLoad(out IReadOnlyList<string> errors))
        {
            throw new InvalidOperationException($"The scan pod template is invalid: {string.Join(" ", errors)}");
        }

        return templateJson is null ? null : KubernetesJson.Deserialize<V1Pod>(templateJson);
    }

    private void Load()
    {
        string path = options.Value.PodTemplateFile?.Trim() ?? string.Empty;

        if (path.Length == 0)
        {
            logger.LogInformation("Scan pods use the built-in spec (no ScanOnDemand:PodTemplateFile)");
            return;
        }

        V1Pod template;

        try
        {
            template = KubernetesYaml.Deserialize<V1Pod>(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            loadErrors = [$"ScanOnDemand:PodTemplateFile '{path}' could not be read as a Pod: {ex.Message}",];
            logger.LogError(ex, "The scan pod template {path} could not be read", path);
            return;
        }

        List<string> errors = [];

        if (!string.Equals(template.Kind, "Pod", StringComparison.Ordinal))
        {
            errors.Add($"The scan pod template must be of kind Pod, not '{template.Kind}'.");
        }

        if (template.Spec?.Containers is { Count: > 1 })
        {
            errors.Add("The scan pod template may have at most one container.");
        }

        if (template.Spec?.InitContainers is { Count: > 1 })
        {
            errors.Add("The scan pod template may have at most one init container.");
        }

        if (errors.Count > 0)
        {
            loadErrors = errors;
            logger.LogError("The scan pod template {path} is invalid: {errors}", path, string.Join(" ", errors));
            return;
        }

        templateJson = KubernetesJson.Serialize(template);
        logger.LogInformation("Scan pods use the template {path}", path);
    }
}
