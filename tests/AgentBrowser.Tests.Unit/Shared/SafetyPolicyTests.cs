using AgentBrowser.Shared.Models;

namespace AgentBrowser.Tests.Unit.Shared;

public class SafetyPolicyTests
{
    [Fact]
    public void IsAllowedNavigation_AllowsDataUrls()
    {
        var allowed = SafetyPolicy.IsAllowedNavigation(new Uri("data:text/html,hello"), out var reason);
        Assert.True(allowed);
        Assert.Empty(reason);
    }

    [Fact]
    public void IsAllowedNavigation_BlocksFileUrls()
    {
        var allowed = SafetyPolicy.IsAllowedNavigation(new Uri("file:///C:/secret.txt"), out var reason);
        Assert.False(allowed);
        Assert.Contains("file", reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsAllowedNavigation_WildcardAllowList()
    {
        var allowed = SafetyPolicy.IsAllowedNavigation(
            new Uri("https://foo.example.com"), out var reason,
            new[] { "*.example.com" });

        Assert.True(allowed);
        Assert.Empty(reason);
    }

    [Fact]
    public void IsAllowedNavigation_BlocksApexForWildcardAllow()
    {
        var allowed = SafetyPolicy.IsAllowedNavigation(
            new Uri("https://example.com"), out var reason,
            new[] { "*.example.com" });

        Assert.False(allowed);
        Assert.NotEmpty(reason);
    }

    [Fact]
    public void IsAllowedNavigation_BlocksPrivateIpsWhenConfigured()
    {
        var allowed = SafetyPolicy.IsAllowedNavigation(
            new Uri("http://127.0.0.1/admin"), out var reason,
            blockPrivateNetworks: true);

        Assert.False(allowed);
        Assert.NotEmpty(reason);
    }
}
