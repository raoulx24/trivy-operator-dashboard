using k8s;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TrivyOperator.Dashboard.Application.Services.Options;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Infrastructure.Clients;

public class KubernetesMultiClientFactory : IKubernetesClientFactory
{
    private readonly IDictionary<string, Kubernetes> clients;
    private readonly ILogger<KubernetesMultiClientFactory> logger;
    private readonly string currentContextName = "default";

    static KubernetesMultiClientFactory()
    {
        KubernetesJson.AddJsonOptions(ConfigureJsonSerializerOptions);
    }

    public KubernetesMultiClientFactory(
        IOptions<KubernetesOptions> options,
        ILogger<KubernetesMultiClientFactory> logger)
    {
        this.logger = logger;
        clients = new Dictionary<string, Kubernetes>(StringComparer.OrdinalIgnoreCase);

        string kubeConfigFileName = options.Value.KubeConfigFileName;

        try
        {
            var kubeConfig = !string.IsNullOrWhiteSpace(kubeConfigFileName) && File.Exists(kubeConfigFileName)
                ? KubernetesClientConfiguration.LoadKubeConfig(kubeConfigFileName)
                : KubernetesClientConfiguration.LoadKubeConfig();

            // Initialize a client for each context
            foreach (var ctx in kubeConfig.Contexts)
            {
                try
                {
                    var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                        kubeConfigFileName, ctx.Name);

                    config.AddJsonOptions(ConfigureJsonSerializerOptions);
                    clients[ctx.Name] = new Kubernetes(config);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to initialize Kubernetes client for context {Context}", ctx.Name);
                }
            }

            currentContextName ??= kubeConfig.CurrentContext;

            // Fallback: if no contexts were loaded, use default config
            if (clients.Count == 0)
            {
                KubernetesClientConfiguration? defaultConfig = KubernetesClientConfiguration.IsInCluster()
                    ? KubernetesClientConfiguration.InClusterConfig()
                    : KubernetesClientConfiguration.BuildConfigFromConfigFile();

                defaultConfig.AddJsonOptions(ConfigureJsonSerializerOptions);
                clients[currentContextName] = new Kubernetes(defaultConfig);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not initialize Kubernetes clients from kubeconfig.");
            throw;
        }
    }

    public Kubernetes GetClient(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            throw new ArgumentNullException(nameof(context));

        if (clients.TryGetValue(context, out var client))
            return client;

        throw new InvalidOperationException($"Unknown Kubernetes context: {context}");
    }

    private static void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Insert(0, new DateTimeJsonConverter());
        jsonSerializerOptions.Converters.Insert(0, new DateTimeNullableJsonConverter());
    }

    public IEnumerable<string> GetContexts() => [.. clients.Keys];
    public string GetCurrentContext() => currentContextName;
}
