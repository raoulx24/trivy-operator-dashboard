using k8s;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Utils;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory;

public class KubernetesClientFactory : IKubernetesClientFactory
{
    private readonly Dictionary<ContextName, Kubernetes> clients;
    private readonly ILogger<KubernetesClientFactory> logger;
    private ContextName currentContextName = new();
    private K8SConfiguration? loadedKubeConfig;

    static KubernetesClientFactory()
    {
        KubernetesJson.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
    }

    public KubernetesClientFactory(IOptions<KubernetesOptions> options, ILogger<KubernetesClientFactory> logger)
    {
        this.logger = logger;
        clients = new Dictionary<ContextName, Kubernetes>();

        if (options.Value.UseDefaultContext)
        {
            InitializeWithDefaultContext(options.Value);
        }
        else
        {
            InitializeExplicitContexts(options.Value);
        }

        logger.LogInformation(
            "Current context: {context}. Loaded {count} contexts: {list}",
            currentContextName,
            clients.Count,
            string.Join(", ", clients.Keys)
        );
    }

    public Kubernetes GetClient(ContextName context)
    {
        if (clients.TryGetValue(context, out Kubernetes? client))
        {
            return client;
        }

        throw new InvalidOperationException($"Unknown Kubernetes context: {context}");
    }

    public IEnumerable<ContextName> GetContexts() => [.. clients.Keys,];
    public ContextName GetCurrentContext() => currentContextName;

    private void InitializeWithDefaultContext(KubernetesOptions options)
    {
        TryGetKubeConfigFile(options.KubeConfigFileName, out bool hasConfigFile);

        if (hasConfigFile)
        {
            try
            {
                LoadSingleDefaultContextFromFile(options.KubeConfigFileName);

                logger.LogInformation("Using default context from file: {contextName}", currentContextName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to load default context from kubeconfig file {kubeconfigFile}. Falling back.",
                    options.KubeConfigFileName
                );

                UseFallbackContext();
            }
        }
        else
        {
            UseFallbackContext();
        }
    }

    private void InitializeExplicitContexts(KubernetesOptions options)
    {
        TryGetKubeConfigFile(options.KubeConfigFileName, out bool hasConfigFile);

        if (hasConfigFile)
        {
            SetContextsFromConfig(options.KubeConfigFileName);
            currentContextName = GetCurrentContextFromConfig(options.KubeConfigFileName);
        }
        else
        {
            if (KubernetesClientConfiguration.IsInCluster())
            {
                KubernetesClientConfiguration? defaultConfig = KubernetesClientConfiguration.InClusterConfig();
                defaultConfig.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
                clients[currentContextName] = new Kubernetes(defaultConfig);
            }
            else
            {
                SetContextsFromConfig();
                currentContextName = GetCurrentContextFromConfig();
            }
        }
    }

    private void UseFallbackContext()
    {
        KubernetesClientConfiguration? defaultConfig = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();

        defaultConfig.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);

        currentContextName = defaultConfig.CurrentContext is null ? currentContextName
            : new ContextName(defaultConfig.CurrentContext);

        clients.Clear();
        clients[currentContextName] = new Kubernetes(defaultConfig);

        logger.LogInformation("Using fallback context - {contextName}", currentContextName);
    }

    private void LoadSingleDefaultContextFromFile(string kubeConfigFileName)
    {
        loadedKubeConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeConfigFileName);

        if (!loadedKubeConfig.Contexts.Any())
        {
            throw new InvalidOperationException("No contexts found in kubeconfig");
        }

        ContextName contextName = loadedKubeConfig.CurrentContext is null
            ? new ContextName(loadedKubeConfig.Contexts.First().Name)
            : new ContextName(loadedKubeConfig.CurrentContext);
            

        KubernetesClientConfiguration? config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
            kubeConfigFileName,
            contextName.Value
        );

        config.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);

        clients.Clear();
        clients[contextName] = new Kubernetes(config);

        currentContextName = contextName;
    }

    private void SetContextsFromConfig(string? kubeConfigFileName = null)
    {
        clients.Clear();

        loadedKubeConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeConfigFileName);

        foreach (Context? ctx in loadedKubeConfig.Contexts)
        {
            try
            {
                KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                    kubeConfigFileName,
                    ctx.Name
                );

                config.AddJsonOptions(JsonUtils.ConfigureJsonSerializerOptions);
                clients[new ContextName(ctx.Name)] = new Kubernetes(config);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to initialize Kubernetes client for context {Context}", ctx.Name);
            }
        }
    }

    private ContextName GetCurrentContextFromConfig(string? kubeConfigFileName = null)
    {
        if (loadedKubeConfig is null)
        {
            throw new InvalidOperationException("Kubeconfig was not loaded before calling GetCurrentContextFromConfig");
        }

        if (clients.Count == 0)
        {
            throw new InvalidOperationException("No Kubernetes contexts found in the kubeconfig file");
        }

        return loadedKubeConfig.CurrentContext is null 
            ? clients.Keys.First() 
            : new ContextName(loadedKubeConfig.CurrentContext);
    }

    private bool TryGetKubeConfigFile(string? kubeConfigFileName, out bool hasConfigFile)
    {
        if (string.IsNullOrWhiteSpace(kubeConfigFileName))
        {
            hasConfigFile = false;
            return false;
        }

        if (!File.Exists(kubeConfigFileName))
        {
            hasConfigFile = false;

            logger.LogWarning("Kubeconfig file {kubeconfigFile} does not exist. Falling back.", kubeConfigFileName);

            return false;
        }

        hasConfigFile = true;
        return true;
    }
}
