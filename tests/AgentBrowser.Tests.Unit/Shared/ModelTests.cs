using AgentBrowser.Shared.Models;

namespace AgentBrowser.Tests.Unit.Shared;

public class ModelTests
{
    [Fact]
    public void ClickOptions_DefaultsToLeftSingleClick()
    {
        var options = new ClickOptions();
        Assert.Equal(0, options.Button);
        Assert.Equal(1, options.ClickCount);
    }

    [Fact]
    public void ScreenshotType_HasExpectedMembers()
    {
        Assert.Equal(new[] { "Viewport", "Element", "FullPage" },
            Enum.GetNames<ScreenshotType>());
    }
}
