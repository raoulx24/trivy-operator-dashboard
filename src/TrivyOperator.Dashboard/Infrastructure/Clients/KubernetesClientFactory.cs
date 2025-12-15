using k8s;
using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Infrastructure.Clients;

public class KubernetesClientFactory : IKubernetesClientFactory
{
    private readonly Kubernetes kubernetesClient;
    private readonly string contextName;
    private static readonly string currentContextName = "default";

    static KubernetesClientFactory()
    {
        KubernetesJson.AddJsonOptions(ConfigureJsonSerializerOptions);
    }

    public KubernetesClientFactory(ILogger<KubernetesClientFactory> logger)
    {
        KubernetesClientConfiguration? defaultConfig = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();
        defaultConfig.AddJsonOptions(ConfigureJsonSerializerOptions);
        contextName = defaultConfig.CurrentContext ?? currentContextName;
        kubernetesClient = new Kubernetes(defaultConfig);
    }

    public Kubernetes GetClient(string contextName)
    {
        if (contextName == this.contextName) 
            return kubernetesClient;

        throw new ArgumentException(
            $"Unsupported Kubernetes context '{contextName}'. This client factory only supports context '{this.contextName}'.",
            nameof(contextName));
    }

    public IEnumerable<string> GetContexts() => [contextName];

    public string GetCurrentContext() => contextName;

    private static void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Insert(0, new DateTimeJsonConverter());
        jsonSerializerOptions.Converters.Insert(0, new DateTimeNullableJsonConverter());
    }
}
