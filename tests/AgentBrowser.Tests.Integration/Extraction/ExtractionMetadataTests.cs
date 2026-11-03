using AgentBrowser.Core.Extraction;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentBrowser.Tests.Integration.Extraction;

[Trait("Category", "Browser")]
public class ExtractionMetadataTests
{
    private const string Fixture = """
        <!doctype html>
        <html>
        <head>
          <title>Smoke Store</title>
          <meta name="description" content="Fine smoke goods">
          <link rel="canonical" href="https://example.com/smoke">
          <meta property="og:title" content="OG Title">
          <meta property="og:type" content="website">
          <meta name="twitter:card" content="summary">
          <script type="application/ld+json">{"@context":"https://schema.org","@type":"Product","name":"Smoke Pipe"}</script>
          <script type="application/ld+json">{this is not valid json</script>
        </head>
        <body></body>
        </html>
        """;

    [Fact]
    public async Task ExtractMetadataAsync_ReturnsMetadataAndJsonLd()
    {
        await using var manager = await BrowserExtractionHarness.CreateNavigatedManagerAsync(Fixture);
        var sessionId = (await manager.GetAllSessionsAsync()).Single().Id;
        var service = new ExtractionService(manager, NullLogger<ExtractionService>.Instance);

        var metadata = await service.ExtractMetadataAsync(sessionId);

        Assert.Equal("Smoke Store", metadata.Title);
        Assert.Equal("Fine smoke goods", metadata.Description);
        Assert.Equal("https://example.com/smoke", metadata.CanonicalUrl);
        Assert.Equal("OG Title", metadata.OpenGraph["og:title"]);
        Assert.Equal("website", metadata.OpenGraph["og:type"]);
        Assert.Equal("summary", metadata.Twitter["twitter:card"]);
        Assert.Single(metadata.JsonLd);
        Assert.Equal("Product", metadata.JsonLd[0].GetProperty("@type").GetString());
        Assert.Contains(metadata.MetaTags, tag =>
            tag.Name == "description" && tag.Content == "Fine smoke goods");
    }
}
