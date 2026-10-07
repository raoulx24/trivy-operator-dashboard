using Serilog;
using Serilog.Extensions.Logging;
using System.Runtime.InteropServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TrivyOperator.Dashboard.Composition.Configuration;

public static class TrivyDashboardConfigurationReader
{
    public static IConfiguration CreateConfiguration()
    {
        IConfigurationBuilder configurationBuilder = 
            new ConfigurationBuilder().AddJsonFile("appsettings.json", true)
                .AddJsonFile("serilog.config.json", true)
                .AddEnvironmentVariables();
        IConfiguration configuration = configurationBuilder.Build();
        string? tempFolder = configuration.GetSection("FileExport")["TempFolder"];
        if (!string.IsNullOrEmpty(tempFolder))
        {
            return configuration;
        }

        Dictionary<string, string?> inMemorySettings = new()
        {
            {
                "FileExport:TempFolder", Path.GetTempPath()
            },
        };
        configurationBuilder.AddInMemoryCollection(inMemorySettings);
        configuration = configurationBuilder.Build();

        return configuration;
    }
    
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

            string reportName = child.Key[..^"CrSubpath".Length];

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
    
    internal static bool LoadUseGithub(this IConfiguration config)
        => config.GetValue<bool>("GitHub:ServerCheckForUpdates");
    
    internal static int GetPort(this IConfiguration configuration, string key, int defaultPort)
    {
        string? value = configuration[key];

        return int.TryParse(value, out int port) && port is >= 1024 and <= 65535
            ? port
            : defaultPort;
    }
}
