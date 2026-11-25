using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Net;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Composition.ScanOnDemand;

// runs after Build and before Run: the namespace is reserved before any watcher reads the namespace policy, and the
// runner (a hosted service) starts only after the checks
// a failed check turns the feature off (with the reason), never the app
public static class ScanOnDemandStartupChecks
{
    public static async Task RunScanOnDemandStartupChecksAsync(
        this WebApplication app,
        IConfiguration configuration,
        CancellationToken ctx = default)
    {
        if (!ScanOnDemandCompositionResolver.IsEnabled(configuration))
        {
            return;
        }

        IServiceProvider sp = app.Services;
        ScanOnDemandOptions options = sp.GetRequiredService<IOptions<ScanOnDemandOptions>>().Value;
        IScanOnDemandAvailability availability = sp.GetRequiredService<IScanOnDemandAvailability>();

        NamespaceName namespaceName = new(options.NamespaceName);

        if (namespaceName.IsClusterScoped)
        {
            SetUnavailable(availability, "ScanOnDemand:NamespaceName is empty.");
            return;
        }

        // first, whatever else fails: the scan namespace must never reach the main pipeline
        try
        {
            sp.GetRequiredService<INamespacePolicyProvider>().Reserve(namespaceName);
        }
        catch (InvalidOperationException ex)
        {
            SetUnavailable(availability, $"The scan namespace {namespaceName} could not be reserved: {ex.Message}");
            return;
        }

        List<string> errors = [];

        if (!sp.GetRequiredService<ScanPodTemplateProvider>().TryLoad(out IReadOnlyList<string> templateErrors))
        {
            errors.AddRange(templateErrors);
        }

        k8s.Kubernetes client = sp.GetRequiredService<IKubernetesClientFactory>()
            .GetClient(sp.GetRequiredService<IContextProvider>().GetDefaultContext());

        try
        {
            await CheckNamespaceAsync(client, namespaceName, errors, ctx);
            await CheckPermissionsAsync(client, namespaceName, sp.GetRequiredService<ICrdFactory>(), errors, ctx);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errors.Add($"The Kubernetes checks could not run: {ex.Message}");
        }

        if (errors.Count > 0)
        {
            SetUnavailable(availability, string.Join(" ", errors));
            return;
        }

        CompositionLogger.Logger?.LogInformation("Scan on demand is enabled, in namespace {namespaceName}", namespaceName);
    }

    private static async Task CheckNamespaceAsync(
        k8s.Kubernetes client,
        NamespaceName namespaceName,
        List<string> errors,
        CancellationToken ctx)
    {
        try
        {
            await client.CoreV1.ReadNamespaceAsync(namespaceName.Value, cancellationToken: ctx);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            errors.Add($"The scan namespace {namespaceName} does not exist.");
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Forbidden)
        {
            // not fatal: the permission checks below tell whether the namespace can be used
            CompositionLogger.Logger?.LogWarning(
                "The scan namespace {namespaceName} could not be read (forbidden); its existence is not checked",
                namespaceName);
        }
    }

    // SelfSubjectAccessReview needs no extra rights; one review per verb and resource
    private static async Task CheckPermissionsAsync(
        k8s.Kubernetes client,
        NamespaceName namespaceName,
        ICrdFactory crdFactory,
        List<string> errors,
        CancellationToken ctx)
    {
        List<(string Group, string Resource, string Verb)> checks =
        [
            ("", "pods", "create"),
            ("", "pods", "get"),
            ("", "pods", "list"),
            ("", "pods", "delete"),
        ];

        foreach (CustomResourceDefinition crd in ReportCrds(crdFactory))
        {
            checks.Add((crd.Group, crd.PluralName, "list"));
        }

        foreach ((string group, string resource, string verb) in checks)
        {
            V1SelfSubjectAccessReview review = await client.AuthorizationV1.CreateSelfSubjectAccessReviewAsync(
                new V1SelfSubjectAccessReview
                {
                    Spec = new V1SelfSubjectAccessReviewSpec
                    {
                        ResourceAttributes = new V1ResourceAttributes
                        {
                            NamespaceProperty = namespaceName.Value,
                            Group = group,
                            Resource = resource,
                            Verb = verb,
                        },
                    },
                },
                cancellationToken: ctx);

            if (review.Status?.Allowed != true)
            {
                string name = string.IsNullOrEmpty(group) ? resource : $"{resource}.{group}";
                errors.Add($"Missing permission: {verb} {name} in namespace {namespaceName}.");
            }
        }
    }

    // the reports a scan can collect; all of them, whatever the settings allow (cheap, and the ceiling may change)
    private static IEnumerable<CustomResourceDefinition> ReportCrds(ICrdFactory crdFactory)
    {
        foreach (ScanReportType type in Enum.GetValues<ScanReportType>())
        {
            yield return type switch
            {
                ScanReportType.VulnerabilityReport => crdFactory.Get<VulnerabilityReportCr>(),
                ScanReportType.ExposedSecretReport => crdFactory.Get<ExposedSecretReportCr>(),
                ScanReportType.SbomReport => crdFactory.Get<SbomReportCr>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }
    }

    private static void SetUnavailable(IScanOnDemandAvailability availability, string reason)
    {
        availability.SetUnavailable(reason);
        CompositionLogger.Logger?.LogError("Scan on demand is turned off: {reason}", reason);
    }
}
