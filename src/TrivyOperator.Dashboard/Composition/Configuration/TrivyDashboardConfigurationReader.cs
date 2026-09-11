namespace TrivyOperator.Dashboard.Composition.Configuration;

public static class TrivyDashboardConfigurationReader
{
    internal static Dictionary<string, bool> LoadEnabledTrivyReports(this IConfiguration configuration)
    {
        Dictionary<string, bool> result = new(StringComparer.OrdinalIgnoreCase);

        IConfigurationSection section = configuration.GetSection("EnabledTrivyReports");

        foreach (IConfigurationSection child in section.GetChildren())
        {
            result[child.Key] = child.Get<bool>();
        }

        return result;
    }

    internal static Dictionary<string, bool> LoadTrivyReportsInFileRepo(this IConfiguration configuration)
    {
        Dictionary<string, bool> result = new(StringComparer.OrdinalIgnoreCase);

        IConfigurationSection section = configuration.GetSection("FileRepository");

        foreach (IConfigurationSection child in section.GetChildren())
        {
            if (!child.Key.EndsWith("Subpath", StringComparison.Ordinal))
            {
                continue;
            }

            string reportName = child.Key[..^"Subpath".Length];

            result[reportName] = !string.IsNullOrWhiteSpace(child.Value);
        }

        return result;
    }
    
    internal static bool LoadUseDefaultContext(this IConfiguration config)
        => config.GetValue<bool>("Kubernetes:UseDefaultContext");
    
    internal static bool LoadUseFileRepository(this IConfiguration config)
        => !string.IsNullOrEmpty(config.GetValue<string>("FileRepository:BasePath"));
    
    internal static bool LoadUseStaticNamespaceService(this IConfiguration config)
        => !string.IsNullOrWhiteSpace(config.GetValue<string>("Kubernetes:NamespaceList"));
    
    internal static bool LoadUseHistory(this IConfiguration config)
        => config.GetValue<bool>("History:Enabled");
}
