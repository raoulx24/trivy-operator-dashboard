namespace TrivyOperator.Dashboard.Application.EndOfLife.Options;

public class EndOfLifeOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://endoflife.date/api/v1";

    public int ProcessingIntervalInSeconds { get; init; } = 60;
    public int ProductTtlInMinutes { get; init; } = 1440;
    public int NotFoundTtlInMinutes { get; init; } = 1440;
    public int ErrorTtlInMinutes { get; init; } = 15;
    public int RequestTimeoutInSeconds { get; init; } = 30;

    // optional json file with an EndOfLife:ProductMappings section; added to configuration with reload on change
    public string? ProductMappingsFile { get; init; }

    // Trivy OS family -> endoflife.date product slug
    // defaults below; configuration entries are merged over them per key
    public Dictionary<string, string> ProductMappings { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "alma", "almalinux" },
        { "alpine", "alpine-linux" },
        { "amazon", "amazon-linux" },
        { "centos", "centos" },
        { "debian", "debian" },
        { "fedora", "fedora" },
        { "opensuse-leap", "opensuse" },
        { "opensuse.leap", "opensuse" },
        { "oracle", "oracle-linux" },
        { "photon", "photon" },
        { "redhat", "rhel" },
        { "rocky", "rocky-linux" },
        { "sles", "sles" },
        { "suse linux enterprise server", "sles" },
        { "ubuntu", "ubuntu" },
    };
}
