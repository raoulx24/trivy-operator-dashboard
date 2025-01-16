﻿using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Trivy;

namespace TrivyOperator.Dashboard.Utils;

public class VarUtils
{
    public const string DefaultCacheRefreshKey = "generic.Key";

    public static string GetCacheRefreshKey(IKubernetesObject<V1ObjectMeta>? kubernetesObject) =>
        kubernetesObject?.Namespace() ?? DefaultCacheRefreshKey;

    public static List<int>? GetExcludedSeverityIdsFromStringList(string? excludedSeverities)
    {
        List<int> excludedSeverityIds = [];
        List<int> knownSeverityIds = [.. (int[])Enum.GetValues(typeof(TrivySeverity))];

        if (!string.IsNullOrWhiteSpace(excludedSeverities))
        {
            string[] excludedStringSeverities = excludedSeverities.Split(',');
            foreach (string excludedSeverity in excludedStringSeverities)
            {
                if (int.TryParse(excludedSeverity, out int vulnerabilityId))
                {
                    if (!knownSeverityIds.Contains(vulnerabilityId))
                    {
                        return null;
                    }

                    excludedSeverityIds.Add(vulnerabilityId);
                }
                else
                {
                    return null;
                }
            }
        }

        return excludedSeverityIds;
    }
}
