namespace AgentBrowser.Shared.Models;

/// <summary>
/// URL and scheme safety policy for navigation. Configured from command-line environment variables or test-provided values.
/// </summary>
public static class SafetyPolicy
{
    /// <summary>
    /// Default blocked URI schemes.
    /// </summary>
    public static readonly string[] DefaultBlockedSchemes = ["file", "chrome", "about"];

    /// <summary>
    /// Validates a navigation URI and returns false with a reason when blocked.
    /// </summary>
    public static bool IsAllowedNavigation(Uri uri, out string reason, string[]? allowedHosts = null, string[]? blockedSchemes = null, bool blockPrivateNetworks = false)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (uri.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase))
        {
            reason = string.Empty;
            return true;
        }

        blockedSchemes ??= GetList("AGENTBROWSER_BLOCKED_SCHEMES", DefaultBlockedSchemes);
        if (blockedSchemes.Any(scheme => scheme.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase)))
        {
            reason = $"URI scheme '{uri.Scheme}' is blocked.";
            return false;
        }

        allowedHosts ??= GetList("AGENTBROWSER_ALLOWED_HOSTS", null);
        if (allowedHosts.Length > 0 && !MatchesAllowedHost(uri.Host, allowedHosts))
        {
            reason = $"Host '{uri.Host}' is not in AGENTBROWSER_ALLOWED_HOSTS.";
            return false;
        }

        if (blockPrivateNetworks && IsPrivateHost(uri.Host))
        {
            reason = $"Host '{uri.Host}' is private or local and AGENTBROWSER_BLOCK_PRIVATE_NETWORKS is enabled.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static string[] GetList(string variable, string[]? defaults)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaults ?? Array.Empty<string>();
        }

        var separators = new[] { ',', ';', ' ' };
        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => item.Length > 0)
            .ToArray();
    }

    private static bool MatchesAllowedHost(string host, IEnumerable<string> allowedHosts)
    {
        if (allowedHosts.Any(host => host.Equals("*", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return allowedHosts.Any(allowed =>
        {
            if (allowed.StartsWith("*.", StringComparison.OrdinalIgnoreCase))
            {
                var suffix = allowed[1..];
                return host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                    host.Length > suffix.Length;
            }

            return host.Equals(allowed, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static bool IsPrivateHost(string host)
    {
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!System.Net.IPAddress.TryParse(host, out var address))
        {
            return false;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 10 ||
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168) ||
                bytes[0] == 127 ||
                (bytes[0] == 169 && bytes[1] == 254) ||
                bytes[0] == 0;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            return address.IsIPv6LinkLocal ||
                address.IsIPv6SiteLocal ||
                address.Equals(System.Net.IPAddress.IPv6Loopback) ||
                (bytes[0] & 0xfe) == 0xfc;
        }

        return false;
    }
}
