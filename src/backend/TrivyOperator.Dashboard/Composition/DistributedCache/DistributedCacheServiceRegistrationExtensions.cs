using System.Globalization;
using System.Text.RegularExpressions;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Abstractions;

namespace TrivyOperator.Dashboard.Composition.DistributedCache;

public static partial class DistributedCacheServiceRegistrationExtensions
{
    private const string RetryOptionsPath = "History:DistributedCache:RetryOptions";

    // the Redis/Valkey client; idempotent, so every feature that persists there can call it
    public static IServiceCollection AddDistributedCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services.Any(x => x.ServiceType == typeof(DistributedCacheConnectionProvider)))
        {
            return services;
        }

        services.Configure<DistributedCacheClientOptions>(configuration.GetSection("History").GetSection("DistributedCache"));

        // bound by hand: the deployments (Helm, static manifest) set the delays as "50ms" / "1s", which the
        // configuration binder cannot turn into a TimeSpan; both that and the TimeSpan format ("00:00:00.050") are accepted
        // ValidateOnStart: a bad value stops the app at startup, not at the first Redis call
        IConfigurationSection retrySection = configuration.GetSection(RetryOptionsPath);

        services.AddOptions<DistributedCacheRetryOptions>()
            .Configure(options =>
            {
                options.MaxRetries = retrySection.GetValue("MaxRetries", options.MaxRetries);
                options.InitialDelay = ParseDuration(retrySection["InitialDelay"], options.InitialDelay, "InitialDelay");
                options.MaxDelay = ParseDuration(retrySection["MaxDelay"], options.MaxDelay, "MaxDelay");
            })
            .ValidateOnStart();

        // one instance: the hosted service connects at startup, the client factory uses the same connection
        services.AddSingleton<DistributedCacheConnectionProvider>();
        services.AddHostedService(sp => sp.GetRequiredService<DistributedCacheConnectionProvider>());

        services.AddSingleton<IDistributedCacheClientFactory, DistributedCacheClientFactory>();
        services.AddSingleton<ICacheEntityCodecFactory, CacheEntityCodecFactory>();
        services.AddSingleton<IDistributedCacheExecutor, DistributedCacheExecutor>();

        return services;
    }

    // empty: the default; "50ms", "1s", "2m", "1h" (decimals allowed); or a TimeSpan ("00:00:00.050")
    private static TimeSpan ParseDuration(string? value, TimeSpan defaultValue, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        string trimmed = value.Trim();
        Match match = DurationRegex().Match(trimmed);

        if (match.Success)
        {
            double amount = double.Parse(match.Groups["amount"].Value, CultureInfo.InvariantCulture);

            return match.Groups["unit"].Value switch
            {
                "ms" => TimeSpan.FromMilliseconds(amount),
                "s" => TimeSpan.FromSeconds(amount),
                "m" => TimeSpan.FromMinutes(amount),
                _ => TimeSpan.FromHours(amount),
            };
        }

        if (TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out TimeSpan timeSpan))
        {
            return timeSpan;
        }

        throw new InvalidOperationException(
            $"{RetryOptionsPath}:{name} '{value}' is not a duration (e.g. 50ms, 1s or 00:00:00.050).");
    }

    [GeneratedRegex(@"^(?<amount>\d+(?:\.\d+)?)(?<unit>ms|s|m|h)$")]
    private static partial Regex DurationRegex();
}
