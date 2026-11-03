using AgentBrowser.Core.Extraction;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentBrowser.Tests.Integration.Extraction;

[Trait("Category", "Browser")]
public class ExtractionPriceTests
{
    private const string Fixture = """
        <!doctype html>
        <html>
        <body>
          <div class="product" data-context="product-card">
            <span class="price" data-price="19.99">$19.99</span>
            <span itemprop="price" content="24.50">$24.50</span>
          </div>
        </body>
        </html>
        """;

    [Fact]
    public async Task ExtractPricesAsync_ReturnsDataPriceAndItempropPrices()
    {
        await using var manager = await BrowserExtractionHarness.CreateNavigatedManagerAsync(Fixture);
        var sessionId = (await manager.GetAllSessionsAsync()).Single().Id;
        var service = new ExtractionService(manager, NullLogger<ExtractionService>.Instance);

        var prices = await service.ExtractPricesAsync(sessionId);

        Assert.Contains(prices, price => price.Value == 19.99m && price.Context == "product-card");
        Assert.Contains(prices, price => price.Value == 24.50m);
    }
}
