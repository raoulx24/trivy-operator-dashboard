using k8s;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Infrastructure.K8s;

public class KubernetesClientFactory : IKubernetesClientFactory
{
    private readonly IDictionary<string, Kubernetes> clients;
    private readonly ILogger<KubernetesClientFactory> logger;
    private readonly string currentContextName = "default";
    private K8SConfiguration? loadedKubeConfig;


    static KubernetesClientFactory()
    {
        KubernetesJson.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
    }

    public KubernetesClientFactory(
        IOptions<KubernetesOptions> options, 
        ILogger<KubernetesClientFactory> logger)
    {
        this.logger = logger;
        clients = new Dictionary<string, Kubernetes>(StringComparer.OrdinalIgnoreCase);

        if (options.Value.UseDefaultContext)
        {
            KubernetesClientConfiguration? defaultConfig = KubernetesClientConfiguration.IsInCluster()
                ? KubernetesClientConfiguration.InClusterConfig()
                : KubernetesClientConfiguration.BuildConfigFromConfigFile();
            defaultConfig.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
            currentContextName = defaultConfig.CurrentContext ?? currentContextName;
            clients[currentContextName] = new Kubernetes(defaultConfig);

            logger.LogInformation("Using default context - {contextName}", currentContextName);
        }
        else
        {
            string kubeConfigFileName = options.Value.KubeConfigFileName;

            if (!string.IsNullOrWhiteSpace(kubeConfigFileName) &&
                File.Exists(kubeConfigFileName))
            {
                SetContextsFromConfig(kubeConfigFileName);
                currentContextName = GetCurrentContextFromConfig(kubeConfigFileName);
            }
            else
            {
                if (KubernetesClientConfiguration.IsInCluster())
                {
                    KubernetesClientConfiguration defaultConfig = KubernetesClientConfiguration.InClusterConfig();
                    defaultConfig.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
                    clients[currentContextName] = new Kubernetes(defaultConfig);
                }
                else
                {
                    SetContextsFromConfig();
                    currentContextName = GetCurrentContextFromConfig();
                }
            }
            logger.LogInformation("Default context is {currentContext}. Got {contextCount} contexts: {contextList}",
                    currentContextName, clients.Count, string.Join(", ", clients.Keys));
        }
    }

    public Kubernetes GetClient(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            throw new ArgumentNullException(nameof(context));

        if (clients.TryGetValue(context, out Kubernetes? client))
            return client;

        throw new InvalidOperationException($"Unknown Kubernetes context: {context}");
    }

    public IEnumerable<string> GetContexts() => [.. clients.Keys];
    public string GetCurrentContext() => currentContextName;

    private void SetContextsFromConfig(string? kubeConfigFileName = null)
    {
        clients.Clear();

        loadedKubeConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeConfigFileName);

        foreach (Context? ctx in loadedKubeConfig.Contexts)
        {
            try
            {
                KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    kubeConfigFileName, ctx.Name);

                config.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
                clients[ctx.Name] = new Kubernetes(config);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to initialize Kubernetes client for context {Context}", ctx.Name);
            }
        }
    }

    private string GetCurrentContextFromConfig(string? kubeConfigFileName = null)
    {
        if (loadedKubeConfig is null)
            throw new InvalidOperationException("Kubeconfig was not loaded before calling GetCurrentContextFromConfig");

        if (clients.Count == 0)
        {
            throw new InvalidOperationException("No Kubernetes contexts found in the kubeconfig file");
        }
        return loadedKubeConfig.CurrentContext ?? clients.Keys.First();
    }
}
