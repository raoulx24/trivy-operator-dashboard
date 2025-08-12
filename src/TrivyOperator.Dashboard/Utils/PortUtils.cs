﻿namespace TrivyOperator.Dashboard.Utils;

public static class PortUtils
{
    public static bool IsValidPort(int port)
    {
        return port >= 1024 && port <= 65535;
    }

    public static int? GetValidatedPort(string? portStr)
    {
        if (int.TryParse(portStr, out int port) && IsValidPort(port))
        {
            return port;
        }

        return null;
    }
}
