using k8s.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;

// the scan pod: the template as written (or the built-in defaults), then the fields the app owns
// - with a template, a container's securityContext / resources missing from it stay missing (the platform or a
//   LimitRange fills them); the built-in defaults apply only when there is no template
// - the init container's name, image and command always come from the settings: an init container that ends would let
//   the scanned image run
internal static class ScanPodSpecBuilder
{
    // on top of the timeout and the grace period, before the kubelet itself fails the pod (safety net if the app is gone)
    private const int DeadlineMarginSeconds = 300;

    public static V1Pod Build(V1Pod? template, ScanId id, ImageReference image, ScanOnDemandOptions options)
    {
        bool hasTemplate = template is not null;
        V1Pod pod = template ?? CreateDefault();

        pod.ApiVersion = "v1";
        pod.Kind = "Pod";
        pod.Status = null;

        // metadata: the template's labels and annotations are kept, the server-set fields are cleared
        V1ObjectMeta metadata = pod.Metadata ??= new V1ObjectMeta();
        metadata.Name = ScanPodLabels.PodName(id);
        metadata.GenerateName = null;
        metadata.NamespaceProperty = options.NamespaceName;
        metadata.Uid = null;
        metadata.ResourceVersion = null;
        metadata.CreationTimestamp = null;
        metadata.OwnerReferences = null;
        metadata.Labels ??= new Dictionary<string, string>();
        metadata.Labels[ScanPodLabels.ManagedBy] = ScanPodLabels.ManagedByValue;
        metadata.Labels[ScanPodLabels.ScanId] = id.Value;

        V1PodSpec spec = pod.Spec ??= new V1PodSpec();
        spec.RestartPolicy = "Never";
        spec.AutomountServiceAccountToken = false;
        spec.ActiveDeadlineSeconds = options.ScanTimeoutSeconds + options.ReportsGracePeriodSeconds + DeadlineMarginSeconds;

        // the settings win when set; otherwise the template's (or none)
        if (!string.IsNullOrWhiteSpace(options.ServiceAccountName))
        {
            spec.ServiceAccountName = options.ServiceAccountName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(options.ImagePullSecretName))
        {
            spec.ImagePullSecrets = [new V1LocalObjectReference { Name = options.ImagePullSecretName.Trim() },];
        }

        V1Container? initTemplate = spec.InitContainers?.FirstOrDefault();
        V1Container? targetTemplate = spec.Containers?.FirstOrDefault();

        spec.InitContainers =
        [
            new V1Container
            {
                Name = ScanPodLabels.InitContainerName,
                Image = options.InitContainerImage,
                Command = options.InitContainerCommand is { Length: > 0 } command ? [.. command,] : null,
                ImagePullPolicy = "IfNotPresent",
                SecurityContext = hasTemplate ? initTemplate?.SecurityContext : DefaultContainerSecurityContext(),
                Resources = hasTemplate ? initTemplate?.Resources : DefaultResources(),
            },
        ];

        spec.Containers =
        [
            new V1Container
            {
                Name = ScanPodLabels.TargetContainerName,
                Image = image.FullName,
                SecurityContext = hasTemplate ? targetTemplate?.SecurityContext : DefaultContainerSecurityContext(),
                Resources = hasTemplate ? targetTemplate?.Resources : DefaultResources(),
                Env = targetTemplate?.Env,
            },
        ];

        return pod;
    }

    private static V1Pod CreateDefault()
        => new()
        {
            Spec = new V1PodSpec
            {
                EnableServiceLinks = false,
                TerminationGracePeriodSeconds = 0,
                SecurityContext = DefaultPodSecurityContext(),
            },
        };

    // no runAsUser / runAsGroup / fsGroup: platforms like OpenShift assign their own and reject a fixed one
    private static V1PodSecurityContext DefaultPodSecurityContext()
        => new()
        {
            RunAsNonRoot = true,
            SeccompProfile = new V1SeccompProfile { Type = "RuntimeDefault" },
        };

    private static V1SecurityContext DefaultContainerSecurityContext()
        => new()
        {
            AllowPrivilegeEscalation = false,
            ReadOnlyRootFilesystem = true,
            Capabilities = new V1Capabilities { Drop = ["ALL",] },
        };

    private static V1ResourceRequirements DefaultResources()
        => new()
        {
            Requests = new Dictionary<string, ResourceQuantity>
            {
                ["cpu"] = new("1m"),
                ["memory"] = new("8Mi"),
            },
            Limits = new Dictionary<string, ResourceQuantity>
            {
                ["cpu"] = new("10m"),
                ["memory"] = new("16Mi"),
            },
        };
}
