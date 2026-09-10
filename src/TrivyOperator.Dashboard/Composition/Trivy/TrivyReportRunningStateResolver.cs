namespace TrivyOperator.Dashboard.Composition.Trivy;

public static class TrivyReportRunningStateResolver
{
    public static TrivyReportRunningState Resolve<TReport>(
        IConfiguration configuration)
    {
        Dictionary<string, bool> enabledReports =
            LoadEnabledTrivyReports(configuration);

        bool isReportEnabled =
            enabledReports.GetValueOrDefault(typeof(TReport).Name);

        if (!isReportEnabled)
        {
            return TrivyReportRunningState.Disabled;
        }

        bool useFileRepository =
            !string.IsNullOrWhiteSpace(
                configuration.GetValue<string>("FileRepository:BasePath"));

        if (useFileRepository)
        {
            Dictionary<string, bool> reportsInFileRepository =
                LoadTrivyReportsInFileRepo(configuration);

            bool isReportEnabledInFileRepository =
                reportsInFileRepository.GetValueOrDefault(typeof(TReport).Name);

            return isReportEnabledInFileRepository
                ? TrivyReportRunningState.FileRepository
                : TrivyReportRunningState.Disabled;
        }

        bool useDefaultContext =
            configuration.GetValue<bool>("Kubernetes:UseDefaultContext");

        return useDefaultContext
            ? TrivyReportRunningState.DefaultContext
            : TrivyReportRunningState.MultiContext;
    }

    // helpers
    
    private static Dictionary<string, bool> LoadEnabledTrivyReports(
        IConfiguration configuration)
    {
        Dictionary<string, bool> result =
            new(StringComparer.OrdinalIgnoreCase);

        IConfigurationSection section =
            configuration.GetSection("EnabledTrivyReports");

        foreach (IConfigurationSection child in section.GetChildren())
        {
            result[child.Key] = child.Get<bool>();
        }

        return result;
    }

    private static Dictionary<string, bool> LoadTrivyReportsInFileRepo(
        IConfiguration configuration)
    {
        Dictionary<string, bool> result =
            new(StringComparer.OrdinalIgnoreCase);

        IConfigurationSection section =
            configuration.GetSection("FileRepository");

        foreach (IConfigurationSection child in section.GetChildren())
        {
            if (!child.Key.EndsWith("Subpath", StringComparison.Ordinal))
            {
                continue;
            }

            string reportName =
                child.Key[..^"Subpath".Length];

            result[reportName] =
                !string.IsNullOrWhiteSpace(child.Value);
        }

        return result;
    }
}
