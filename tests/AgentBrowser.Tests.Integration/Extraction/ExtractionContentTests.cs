using AgentBrowser.Core.Extraction;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentBrowser.Tests.Integration.Extraction;

[Trait("Category", "Browser")]
public class ExtractionContentTests
{
    private const string Fixture = """
        <!doctype html>
        <html>
        <body>
          <article>
            <figure>
              <img src="https://example.com/image.jpg" alt="Smoke pipe" title="Pipe"
                   width="800" height="600">
              <figcaption>River pipe</figcaption>
            </figure>
          </article>
          <form action="/submit" method="post">
            <label for="email">Email address</label>
            <input id="email" name="email" type="email" required placeholder="you@example.com">
            <label><input type="checkbox" name="opt-in" value="yes">Opt in</label>
            <input name="password" type="password" aria-label="Password">
            <select name="country"><option value="us">US</option></select>
            <textarea name="message" placeholder="Message"></textarea>
          </form>
          <a href="https://example.com" title="Example" rel="noopener" target="_blank">Example Link</a>
          <a href="">Empty Link</a>
        </body>
        </html>
        """;

    [Fact]
    public async Task ExtractImagesAsync_ReturnsSourceAltAndCaptionContext()
    {
        await using var manager = await BrowserExtractionHarness.CreateNavigatedManagerAsync(Fixture);
        var sessionId = (await manager.GetAllSessionsAsync()).Single().Id;
        var service = new ExtractionService(manager, NullLogger<ExtractionService>.Instance);

        var images = await service.ExtractImagesAsync(sessionId);

        var image = Assert.Single(images);
        Assert.Equal("https://example.com/image.jpg", image.Src);
        Assert.Equal("Smoke pipe", image.Alt);
        Assert.Equal("Pipe", image.Title);
        Assert.Equal(800, image.Width);
        Assert.Equal(600, image.Height);
        Assert.Equal("River pipe", image.Caption);
        Assert.Equal("article", image.ParentContext);
    }

    [Fact]
    public async Task ExtractFormsAsync_ReturnsActionMethodAndFields()
    {
        await using var manager = await BrowserExtractionHarness.CreateNavigatedManagerAsync(Fixture);
        var sessionId = (await manager.GetAllSessionsAsync()).Single().Id;
        var service = new ExtractionService(manager, NullLogger<ExtractionService>.Instance);

        var forms = await service.ExtractFormsAsync(sessionId);

        var form = Assert.Single(forms);
        Assert.Equal("/submit", form.Action);
        Assert.Equal("POST", form.Method);
        Assert.Contains(form.Fields, field => field.Name == "email" && field.Required &&
            field.Label == "Email address" && field.Placeholder == "you@example.com");
        Assert.Contains(form.Fields, field => field.Name == "password" && field.Label == "Password");
        Assert.Contains(form.Fields, field => field.Name == "country" && field.Type == "select");
        Assert.Contains(form.Fields, field => field.Name == "message" && field.Type == "textarea");
    }

    [Fact]
    public async Task ExtractLinksAsync_ReturnsAbsoluteLinksAndSkipsEmptyHref()
    {
        await using var manager = await BrowserExtractionHarness.CreateNavigatedManagerAsync(Fixture);
        var sessionId = (await manager.GetAllSessionsAsync()).Single().Id;
        var service = new ExtractionService(manager, NullLogger<ExtractionService>.Instance);

        var links = await service.ExtractLinksAsync(sessionId);

        var link = Assert.Single(links);
        Assert.Equal("https://example.com/", link.Href);
        Assert.Equal("Example Link", link.Text);
        Assert.Equal("Example", link.Title);
        Assert.Equal("noopener", link.Rel);
        Assert.Equal("_blank", link.Target);
    }
}
