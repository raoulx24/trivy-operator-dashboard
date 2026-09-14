using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.Trivy;

internal static class TrivyReportCompositionResolver
{
    public static TrivyReportCompositionMode Resolve<TReport>(IConfiguration configuration)
    {
        Dictionary<string, bool> enabledReports = configuration.LoadEnabledTrivyReports();

        bool isReportEnabled = enabledReports.GetValueOrDefault(typeof(TReport).Name);

        if (!isReportEnabled)
        {
            return TrivyReportCompositionMode.Disabled;
        }

        bool useFileRepository = !string.IsNullOrWhiteSpace(configuration.GetValue<string>("FileRepository:BasePath"));

        if (useFileRepository)
        {
            Dictionary<string, bool> reportsInFileRepository = configuration.LoadTrivyReportsInFileRepo();

            bool isReportEnabledInFileRepository = reportsInFileRepository.GetValueOrDefault(typeof(TReport).Name);

            return isReportEnabledInFileRepository
                ? TrivyReportCompositionMode.FileRepository
                : TrivyReportCompositionMode.Disabled;
        }

        bool useDefaultContext = configuration.GetValue<bool>("Kubernetes:UseDefaultContext");

        return useDefaultContext
            ? TrivyReportCompositionMode.DefaultContext
            : TrivyReportCompositionMode.MultiContext;
    }
}

internal enum TrivyReportCompositionMode
{
    Disabled,
    FileRepository,
    MultiContext,
    DefaultContext,
}
