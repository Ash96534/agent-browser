# AgentBrowser Working MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn AgentBrowser into a verified, honestly documented single-tab browser-control MVP for AI agents, with real Chromium operations, a working REST surface, real extraction, standard MCP over stdio, tests, and CI.

**Architecture:** Keep the existing Shared -> Browser -> Core -> API layering. The Browser layer owns Chromium through PuppeteerSharp. A new thin `IBrowserEngine` seam makes session lifecycle unit-testable while `PuppeteerBrowserEngine` performs the real launch. Core services become thin adapters over `IBrowserManager`, API controllers expose those services, and an official MCP SDK server exposes the same operations over stdio.

**Tech Stack:** C# 13, .NET 10 (`net10.0`), ASP.NET Core, PuppeteerSharp 19.0.0, xUnit, Moq 4.21.0, Microsoft.AspNetCore.Mvc.Testing 10.0.14, ModelContextProtocol 2.3.1.

**Spec:** `SPEC.md` as amended by the "Working MVP scope" section below. Where `SPEC.md` claims a feature outside that scope, this plan is authoritative.

## Working MVP Scope

**Supported and must work end to end:**
- One Chromium browser per session; exactly one active page per session.
- Session create, get, list, close.
- Navigate, get rendered HTML, get body text.
- Click, type, execute JavaScript, wait for selector.
- Screenshot viewport, full page, and element as PNG or JPEG, returned as base64.
- Extraction: metadata, prices, images, forms, links.
- WebSocket console, load-complete, and page-error events.
- MCP stdio server with standard initialize, tools/list, and tools/call.

**Explicitly not supported in this MVP (docs must say so):**
- Multi-tab behavior.
- Network interception, request modification, mocking, or blocking.
- DOM mutation event streaming.
- SQLite or other session persistence.
- API authentication, authorization, rate limiting, or multi-instance scaling.
- Docker and PDF generation.
- Playwright, Firefox, or WebKit backends.

## Global Constraints

- All files must stay inside `D:/KIRO_PROJ`; no writes outside that root.
- Target framework is `net10.0`. Do not retarget to .NET 8.
- Pin `PuppeteerSharp` at `19.0.0`.
- Use nullable reference types and implicit usings.
- Never commit directly to `master`. Work on `fix/working-mvp`, branch from `master`.
- Use semantic commit messages: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`.
- Every task ends with a build and a test run. Commit only after both pass.
- Public types and public methods get XML documentation.
- Browser and API integration tests must use local `data:` URLs or a local test server, never the public internet.
- Do not claim unsupported behavior in README, SPEC, ROADMAP, IMPLEMENTATION, DECISIONS, or AGENTS.md.
- Keep one canonical solution file: `AgentBrowser.sln`.

## Review Focus

These are the five likely user-facing failure modes the happy-path tests can miss. Each one is pinned to a task below.

1. Chromium is missing or fails to launch. Expected: no half-created session, a clear `BrowserLaunchException`, and HTTP 503 with an actionable message.
2. Any endpoint receives an unknown or already-closed session ID. Expected: HTTP 404, never an unhandled 500.
3. Invalid URL, selector, screenshot format, or timeout. Expected: HTTP 400 with the reason, no browser process leak.
4. A session is closed and a later operation is attempted. Expected: the operation fails with `SessionNotFoundException`, and the browser is still disposed without a leak.
5. An MCP client closes stdin or a tool throws. Expected: clean process exit or a JSON-RPC error frame; stdout contains protocol frames only.

---

### Task 1: Reconcile the unmerged WebSocket branch

**Files:**
- Merge: `feature/websocket-support` into a new branch from `master`
- Verify: `src/AgentBrowser.Api/Controllers/ExtractionController.cs`
- Verify: `src/AgentBrowser.Api/WebSockets/WebSocketHandler.cs`
- Verify: `src/AgentBrowser.Api/Middleware/WebSocketMiddleware.cs`
- Verify: `src/AgentBrowser.Core/Extraction/ExtractionService.cs`

**Interfaces:**
- Consumes: `feat: Add WebSocket support with event streaming` and `feat: Add extraction service for hidden data` from `feature/websocket-support`.
- Produces: a clean baseline containing the extraction controller, WebSocket scaffolding, and extraction service registration. This task does not claim those features work yet.

- [ ] **Step 1: Confirm the baseline is clean**

Run:
```powershell
git status --short --branch
git log --oneline -3
```
Expected: `master` is clean and points at `770e964`.

- [ ] **Step 2: Create the working branch and inspect the unmerged work**

Run:
```powershell
git switch -c fix/working-mvp master
git log --oneline master..feature/websocket-support
git diff --stat master...feature/websocket-support
```
Expected: two commits and seven changed paths, including `ExtractionController.cs` and the WebSocket files.

- [ ] **Step 3: Merge the branch without fast-forward**

Run:
```powershell
git merge --no-ff feature/websocket-support -m "chore: merge websocket and extraction scaffolding"
```
Expected: merge succeeds with no conflicts.

- [ ] **Step 4: Verify the merged baseline builds and tests run**

Run:
```powershell
dotnet build AgentBrowser.sln --nologo
dotnet test AgentBrowser.sln --no-build --nologo
```
Expected: build succeeds with 0 errors and 1 known warning (`CS8603` in the old MCP server); the placeholder unit test passes. Task 2 removes this warning.

- [ ] **Step 5: Confirm dead code is not advertised yet**

Read `src/AgentBrowser.Api/Program.cs` and confirm it does not call `UseMiddleware<WebSocketMiddleware>()`. Do not add it yet. The WebSocket endpoint must not be advertised until Task 10 makes it functional.

- [ ] **Step 6: Commit and push the baseline**

Run:
```powershell
git push -u origin fix/working-mvp
```
Expected: the new branch is pushed. The merge commit is the commit.

---

### Task 2: Clean scaffolding, consolidate duplicate models, and add the test harness

**Files:**
- Delete: `src/AgentBrowser.Shared/Class1.cs`
- Delete: `src/AgentBrowser.Browser/Class1.cs`
- Delete: `src/AgentBrowser.Core/Class1.cs`
- Delete: `src/AgentBrowser.MCP/Class1.cs`
- Delete: `AgentBrowser.slnx`
- Modify: `src/AgentBrowser.Shared/Models/Requests/ClickRequest.cs` (remove duplicate `ClickOptions`)
- Modify: `src/AgentBrowser.Shared/Models/Requests/ScreenshotRequest.cs` (remove duplicate `ScreenshotType`)
- Modify: `src/AgentBrowser.Shared/Models/Options.cs` (single source for `ClickOptions`, `ScreenshotType`, `ScreenshotOptions`, `TypeOptions`)
- Modify: `src/AgentBrowser.Api/Controllers/ScreenshotController.cs` (use `AgentBrowser.Shared.Models.ScreenshotType`)
- Modify: `src/AgentBrowser.Api/AgentBrowser.Api.csproj` (remove unused `AgentBrowser.MCP` project reference)
- Modify: `tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj` (add `Moq` 4.21.0)
- Modify: `tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj` (add `Moq` 4.21.0, `Microsoft.AspNetCore.Mvc.Testing` 10.0.14, and project references to Api, Core, Browser, Shared)
- Create: `global.json` pinning SDK `10.0.301` with `rollForward: latestFeature`
- Delete: `src/AgentBrowser.MCP/McpServer.cs`
- Modify: `src/AgentBrowser.MCP/Program.cs` (temporary no-op entry point until Task 11)
- Replace: `tests/AgentBrowser.Tests.Unit/UnitTest1.cs` with a temporary smoke test that asserts the solution namespace loads
- Replace: `tests/AgentBrowser.Tests.Integration/UnitTest1.cs` with a temporary smoke test that asserts the test assembly loads

**Interfaces:**
- Consumes: nothing from earlier tasks.
- Produces: one `ClickOptions` type and one `ScreenshotType` type in `AgentBrowser.Shared.Models`; test projects ready for Moq and `WebApplicationFactory<Program>`; the MCP project compiles without the obsolete hand-rolled server until Task 11 replaces it.

- [ ] **Step 1: Write the failing model-usage tests**

Add `tests/AgentBrowser.Tests.Unit/Shared/ModelTests.cs`:
```csharp
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
```

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --nologo
```
Expected: FAIL because `ClickOptions` and `ScreenshotType` are ambiguous or missing from `AgentBrowser.Shared.Models`.

- [ ] **Step 3: Consolidate the duplicate types**

Remove `ClickOptions` from `src/AgentBrowser.Shared/Models/Requests/ClickRequest.cs` and remove `ScreenshotType` from `src/AgentBrowser.Shared/Models/Requests/ScreenshotRequest.cs`. Keep the definitions in `src/AgentBrowser.Shared/Models/Options.cs`. Update every `using` and type reference so the solution builds.

- [ ] **Step 4: Add the test packages and SDK pin**

Add the exact package versions listed in Files. Set `global.json`:
```json
{
  "sdk": {
    "version": "10.0.301",
    "rollForward": "latestFeature"
  }
}
```
Delete the four `Class1.cs` files, `AgentBrowser.slnx`, and `src/AgentBrowser.MCP/McpServer.cs`. Replace `src/AgentBrowser.MCP/Program.cs` with:
```csharp
Console.Error.WriteLine("AgentBrowser MCP server is being rebuilt on the official SDK.");
```
Remove the unused MCP project reference from the API project. This keeps the solution buildable until Task 11 adds the real MCP host.

- [ ] **Step 5: Run build and tests**

Run:
```powershell
dotnet build AgentBrowser.sln --nologo
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build --nologo
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo
```
Expected: build has 0 warnings; both temporary smoke tests pass.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "chore: clean scaffolding and add test harness"
```

---

### Task 3: Add the browser engine seam and real session lifecycle

**Files:**
- Create: `src/AgentBrowser.Browser/Browser/IBrowserEngine.cs`
- Create: `src/AgentBrowser.Browser/Browser/PuppeteerBrowserEngine.cs`
- Create: `src/AgentBrowser.Shared/Exceptions/BrowserException.cs`
- Create: `src/AgentBrowser.Shared/Exceptions/SessionNotFoundException.cs`
- Create: `src/AgentBrowser.Shared/Exceptions/BrowserLaunchException.cs`
- Create: `src/AgentBrowser.Browser/Browser/BrowserEvent.cs`
- Create: `src/AgentBrowser.Browser/Browser/IBrowserEventSink.cs`
- Modify: `src/AgentBrowser.Browser/Browser/IBrowserManager.cs`
- Modify: `src/AgentBrowser.Browser/Browser/BrowserManager.cs`
- Modify: `src/AgentBrowser.Core/Extensions/ServiceCollectionExtensions.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Browser/BrowserManagerSessionTests.cs`
- Delete: `tests/AgentBrowser.Tests.Unit/UnitTest1.cs`

**Interfaces:**
- Consumes: `IBrowserEngine`, `BrowserManager`.
- Produces:
```csharp
public interface IBrowserManager : IAsyncDisposable
{
    Task<Session> CreateSessionAsync(SessionOptions options, CancellationToken cancellationToken = default);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<bool> CloseSessionAsync(string sessionId);
    Task<IReadOnlyList<Session>> GetAllSessionsAsync();
}
```
```csharp
public interface IBrowserEngine
{
    Task<IBrowser> LaunchAsync(LaunchOptions options, CancellationToken cancellationToken = default);
}
```
```csharp
public abstract record BrowserEvent(string SessionId, DateTimeOffset Timestamp);

public interface IBrowserEventSink
{
    ValueTask PublishAsync(BrowserEvent browserEvent, CancellationToken cancellationToken = default);
}

public sealed class NullBrowserEventSink : IBrowserEventSink
{
    public static NullBrowserEventSink Instance { get; } = new();
    public ValueTask PublishAsync(BrowserEvent browserEvent, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
```
Later tasks add navigation, DOM, screenshot, and extraction methods to `IBrowserManager` as those behaviors are tested.

- [ ] **Step 1: Write the failing session tests**

Add `tests/AgentBrowser.Tests.Unit/Browser/BrowserManagerSessionTests.cs`:
```csharp
using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PuppeteerSharp;

namespace AgentBrowser.Tests.Unit.Browser;

public class BrowserManagerSessionTests
{
    [Fact]
    public async Task CreateSessionAsync_LaunchesBrowserAndStoresSession()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), null)).Returns(Task.CompletedTask);

        var browser = new Mock<IBrowser>();
        browser.Setup(b => b.NewPageAsync()).ReturnsAsync(page.Object);

        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(browser.Object);

        var manager = new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance);

        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.NotEmpty(session.Id);
        Assert.Single(session.Tabs);
        Assert.Equal(SessionStatus.Active, session.Status);
        browser.Verify(b => b.NewPageAsync(), Times.Once);
        page.Verify(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>()), Times.Once);
    }

    [Fact]
    public async Task GetSessionAsync_UnknownId_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(await manager.GetSessionAsync("missing"));
    }

    [Fact]
    public async Task CloseSessionAsync_ClosesBrowserAndRemovesSession()
    {
        var (manager, browser, _) = CreateManagerWithBrowser();
        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.True(await manager.CloseSessionAsync(session.Id));
        Assert.Null(await manager.GetSessionAsync(session.Id));
        browser.Verify(b => b.CloseAsync(), Times.Once);
    }

    [Fact]
    public async Task CloseSessionAsync_UnknownId_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(await manager.CloseSessionAsync("missing"));
    }

    [Fact]
    public async Task CreateSessionAsync_WhenLaunchFails_ThrowsBrowserLaunchException()
    {
        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("chromium missing"));

        var manager = new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance);

        await Assert.ThrowsAsync<BrowserLaunchException>(
            () => manager.CreateSessionAsync(new SessionOptions()));
        Assert.Empty(await manager.GetAllSessionsAsync());
    }

    private static BrowserManager CreateManager() => CreateManagerWithBrowser().Manager;

    private static (BrowserManager Manager, Mock<IBrowser> Browser, Mock<IPage> Page) CreateManagerWithBrowser()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), null)).Returns(Task.CompletedTask);
        var browser = new Mock<IBrowser>();
        browser.Setup(b => b.NewPageAsync()).ReturnsAsync(page.Object);
        browser.Setup(b => b.CloseAsync()).Returns(Task.CompletedTask);
        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(browser.Object);
        return (new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance), browser, page);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~BrowserManagerSessionTests" --nologo
```
Expected: FAIL to compile because `IBrowserEngine`, `NullBrowserEventSink`, and the new `BrowserManager` constructor do not exist.

- [ ] **Step 3: Implement the engine seam and session handle**

Implement `PuppeteerBrowserEngine` so it downloads Chromium once under a `SemaphoreSlim`, then calls `Puppeteer.LaunchAsync(options)`. Create `BrowserEvent`, `IBrowserEventSink`, and `NullBrowserEventSink`, but `BrowserManager` does not publish events yet. Implement `BrowserManager` with a private `SessionHandle` holding `Session`, `IBrowser`, `IPage`, and a per-session `SemaphoreSlim`. `CloseSessionAsync` removes the handle, calls `browser.CloseAsync()`, and disposes the handle even if closing throws. `DisposeAsync` closes every remaining session. Throw `BrowserLaunchException` when launch fails and `SessionNotFoundException` when an operation cannot find a live session.

- [ ] **Step 4: Register the new services**

In `ServiceCollectionExtensions`:
```csharp
services.AddSingleton<IBrowserEngine, PuppeteerBrowserEngine>();
services.AddSingleton<IBrowserEventSink>(NullBrowserEventSink.Instance);
services.AddSingleton<IBrowserManager, BrowserManager>();
```

- [ ] **Step 5: Run the session tests and the full unit suite**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: all tests pass and build has 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: add real browser session lifecycle"
```

---

### Task 4: Implement real navigation, HTML, and text content

**Files:**
- Create: `src/AgentBrowser.Shared/Exceptions/NavigationException.cs`
- Create: `src/AgentBrowser.Shared/Models/PageNavigationResult.cs`
- Modify: `src/AgentBrowser.Browser/Browser/IBrowserManager.cs`
- Modify: `src/AgentBrowser.Browser/Browser/BrowserManager.cs`
- Modify: `src/AgentBrowser.Core/Services/INavigationService.cs`
- Modify: `src/AgentBrowser.Core/Services/NavigationService.cs`
- Modify: `src/AgentBrowser.Api/Controllers/NavigationController.cs`
- Modify: `src/AgentBrowser.Shared/Models/Responses/NavigateResponse.cs` (add `Title` population on success)
- Create: `tests/AgentBrowser.Tests.Unit/Browser/BrowserManagerNavigationTests.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Core/NavigationServiceTests.cs`

**Interfaces:**
- Consumes: `IBrowserManager` from Task 3.
- Adds to `IBrowserManager`:
```csharp
Task<PageNavigationResult> NavigateAsync(string sessionId, NavigateRequest request, CancellationToken cancellationToken = default);
Task<string> GetHtmlAsync(string sessionId, CancellationToken cancellationToken = default);
Task<string> GetContentTextAsync(string sessionId, CancellationToken cancellationToken = default);
```
```csharp
public sealed record PageNavigationResult(string Url, string Title);
```
- Produces:
```csharp
public interface INavigationService
{
    Task<NavigateResponse> NavigateAsync(string sessionId, NavigateRequest request, CancellationToken cancellationToken = default);
    Task<HtmlResponse> GetHtmlAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<HtmlResponse> GetContentAsync(string sessionId, CancellationToken cancellationToken = default);
}
```
`GetContentAsync` returns the page body text in `HtmlResponse.Html` for backward compatibility with the existing endpoint, and the README will describe it as text content.

- [ ] **Step 1: Write the failing navigation tests**

Add `tests/AgentBrowser.Tests.Unit/Browser/BrowserManagerNavigationTests.cs`. Reuse the mock helper from Task 3; expose both `manager` and `page` from a `CreateManagerWithPage()` local helper.
```csharp
[Fact]
public async Task NavigateAsync_UpdatesTabUrlTitleAndStatus()
{
    var (manager, page) = CreateManagerWithPage();
    page.Setup(p => p.GoToAsync("data:text/html,<title>Hello</title><h1>Hi</h1>",
            It.IsAny<NavigationOptions>()))
        .ReturnsAsync((IResponse?)null);
    page.Setup(p => p.GetTitleAsync()).ReturnsAsync("Hello");
    page.SetupGet(p => p.Url).Returns("data:text/html,<title>Hello</title><h1>Hi</h1>");

    var session = await manager.CreateSessionAsync(new SessionOptions());
    var result = await manager.NavigateAsync(session.Id,
        new NavigateRequest { Url = "data:text/html,<title>Hello</title><h1>Hi</h1>" });

    Assert.Equal("Hello", result.Title);
    Assert.Equal(TabStatus.Loaded, session.Tabs[0].Status);
    Assert.Equal("Hello", session.Tabs[0].Title);
}

[Fact]
public async Task NavigateAsync_WhenPageThrows_WrapsInNavigationException()
{
    var (manager, page) = CreateManagerWithPage();
    page.Setup(p => p.GoToAsync(It.IsAny<string>(), It.IsAny<NavigationOptions>()))
        .ThrowsAsync(new InvalidOperationException("bad url"));
    var session = await manager.CreateSessionAsync(new SessionOptions());

    await Assert.ThrowsAsync<NavigationException>(() =>
        manager.NavigateAsync(session.Id, new NavigateRequest { Url = "not-a-url" }));
}

[Fact]
public async Task GetHtmlAsync_ReturnsPageContent()
{
    var (manager, page) = CreateManagerWithPage();
    page.Setup(p => p.GetContentAsync()).ReturnsAsync("<html>ok</html>");
    var session = await manager.CreateSessionAsync(new SessionOptions());

    Assert.Equal("<html>ok</html>", await manager.GetHtmlAsync(session.Id));
}

[Fact]
public async Task GetContentTextAsync_ReturnsBodyText()
{
    var (manager, page) = CreateManagerWithPage();
    page.Setup(p => p.EvaluateExpressionAsync<string>(It.IsAny<string>()))
        .ReturnsAsync("Hello");
    var session = await manager.CreateSessionAsync(new SessionOptions());

    Assert.Equal("Hello", await manager.GetContentTextAsync(session.Id));
}
```

Add `tests/AgentBrowser.Tests.Unit/Core/NavigationServiceTests.cs` using `Mock<IBrowserManager>` to verify the service maps `PageNavigationResult` to `NavigateResponse` with `Success = true`, `Url`, `Title`, and a non-default `Timestamp`.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~Navigation" --nologo
```
Expected: FAIL because navigation methods are still stubs.

- [ ] **Step 3: Implement `BrowserManager.NavigateAsync` and content methods**

Use `page.GoToAsync(request.Url, new NavigationOptions { Timeout = request.Timeout ?? 30000, WaitUntil = Map(request.WaitUntil) })`. Map `WaitUntil.Load`, `DOMContentLoaded`, `NetworkIdle0`, and `NetworkIdle2` to the PuppeteerSharp enum. Update the tab URL, title, and status after navigation. Wrap Puppeteer exceptions in `NavigationException`. `GetHtmlAsync` calls `page.GetContentAsync()`. `GetContentTextAsync` calls `page.EvaluateExpressionAsync<string>("document.body?.innerText ?? ''")`.

- [ ] **Step 4: Update the service and controller**

Make `NavigationService` a thin adapter. Change `NavigationController` to call `NavigateAsync(sessionId, request)` and `GetHtmlAsync(sessionId)` without the fake tab ID. Keep the route shapes unchanged.

- [ ] **Step 5: Run the tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: navigation tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: implement real navigation and content retrieval"
```

---

### Task 5: Implement real DOM interactions, JavaScript, and waits

**Files:**
- Create: `src/AgentBrowser.Shared/Exceptions/SelectorException.cs`
- Create: `src/AgentBrowser.Shared/Exceptions/ScriptExecutionException.cs`
- Modify: `src/AgentBrowser.Browser/Browser/IBrowserManager.cs`
- Modify: `src/AgentBrowser.Browser/Browser/BrowserManager.cs`
- Modify: `src/AgentBrowser.Core/Services/IDomService.cs`
- Modify: `src/AgentBrowser.Core/Services/DomService.cs`
- Modify: `src/AgentBrowser.Api/Controllers/DomController.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Browser/BrowserManagerDomTests.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Core/DomServiceTests.cs`

**Interfaces:**
- Consumes: `IBrowserManager` from Task 3.
- Adds to `IBrowserManager`:
```csharp
Task<T> EvaluateExpressionAsync<T>(string sessionId, string script, CancellationToken cancellationToken = default);
Task ClickAsync(string sessionId, string selector, ClickOptions options, CancellationToken cancellationToken = default);
Task TypeAsync(string sessionId, string selector, string text, int delay, bool clearFirst, CancellationToken cancellationToken = default);
Task WaitForSelectorAsync(string sessionId, string selector, int timeout, CancellationToken cancellationToken = default);
```
- Produces:
```csharp
public interface IDomService
{
    Task ClickAsync(string sessionId, ClickRequest request, CancellationToken cancellationToken = default);
    Task TypeAsync(string sessionId, TypeRequest request, CancellationToken cancellationToken = default);
    Task<T> ExecuteJsAsync<T>(string sessionId, ExecuteJsRequest request, CancellationToken cancellationToken = default);
    Task WaitForSelectorAsync(string sessionId, string selector, int timeout = 30000, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 1: Write the failing DOM tests**

Add tests that verify:
- `ClickAsync` calls `page.ClickAsync(selector, It.IsAny<PuppeteerSharp.Input.ClickOptions>())`.
- `TypeAsync` clears the field first and calls `page.TypeAsync(selector, text, It.IsAny<TypeOptions>())`.
- `WaitForSelectorAsync` calls `page.WaitForSelectorAsync(selector, It.IsAny<WaitForSelectorOptions>())`.
- A thrown selector timeout becomes `SelectorException`.
- `ExecuteJsAsync<int>` returns the value produced by `EvaluateExpressionAsync<int>`.
- A thrown script error becomes `ScriptExecutionException`.

Use the same mock `IPage` and `IBrowserEngine` helpers as Task 3.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~Dom" --nologo
```
Expected: FAIL because DOM operations are stubs.

- [ ] **Step 3: Implement the Browser methods**

Map `ClickOptions.Button` to `MouseButton.Left`, `Middle`, or `Right`. Map `ClickOptions.Delay` and `ClickCount`. For `ClearFirst`, evaluate `sel => { const el = document.querySelector(sel); if (el) el.value = ''; }` before typing. Wrap selector and script failures in the new exceptions.

- [ ] **Step 4: Update `DomService` and `DomController`**

Remove the `Convert.ChangeType` path. `DomService.ExecuteJsAsync<T>` must call `IBrowserManager.EvaluateExpressionAsync<T>`. Controllers must pass only `sessionId`.

- [ ] **Step 5: Run the tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: all unit tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: implement real DOM and JavaScript operations"
```

---

### Task 6: Implement real screenshots

**Files:**
- Modify: `src/AgentBrowser.Browser/Browser/IBrowserManager.cs`
- Modify: `src/AgentBrowser.Browser/Browser/BrowserManager.cs`
- Modify: `src/AgentBrowser.Core/Services/IScreenshotService.cs`
- Modify: `src/AgentBrowser.Core/Services/ScreenshotService.cs`
- Modify: `src/AgentBrowser.Api/Controllers/ScreenshotController.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Core/ScreenshotServiceTests.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Browser/BrowserManagerScreenshotTests.cs`

**Interfaces:**
- Consumes: `IBrowserManager`.
- Adds to `IBrowserManager`:
```csharp
Task<byte[]> ScreenshotAsync(string sessionId, ScreenshotOptions options, CancellationToken cancellationToken = default);
```
- Changes `IScreenshotService` to:
```csharp
Task<ScreenshotResponse> CaptureAsync(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken = default);
Task<string> CaptureBase64Async(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken = default);
```
- Produces: screenshots over base64 instead of empty strings.

- [ ] **Step 1: Write the failing screenshot tests**

Verify:
- `BrowserManager.ScreenshotAsync` calls `page.ScreenshotDataAsync` and returns the exact byte array.
- `ScreenshotService.CaptureBase64Async` returns `Convert.ToBase64String(bytes)`.
- A missing element selector throws `SelectorException`.
- A JPEG request sets `ScreenshotOptions.Type` to `PuppeteerSharp.ScreenshotType.Jpeg` and passes `Quality`.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~Screenshot" --nologo
```
Expected: FAIL because screenshots still return empty data.

- [ ] **Step 3: Implement screenshots**

Use `page.ScreenshotDataAsync` for viewport and full-page screenshots. For element screenshots, call `page.QuerySelectorAsync(selector)`, throw `SelectorException` if null, and use `IElementHandle.ScreenshotDataAsync`. Map `AgentBrowser.Shared.Models.ScreenshotType` to `PuppeteerSharp.ScreenshotType.Png` or `Jpeg`. Throw `ArgumentException` for unsupported formats.

- [ ] **Step 4: Update the controller**

Keep both routes. `GET /session/{id}/screenshot` returns `ScreenshotResponse`; `GET /session/{id}/screenshot/base64` returns `{ "base64": "..." }`.

- [ ] **Step 5: Run tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: all unit tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: implement real screenshots"
```

---

### Task 7: Add API error mapping and integration tests

**Files:**
- Create: `src/AgentBrowser.Api/Middleware/ExceptionHandlingMiddleware.cs`
- Modify: `src/AgentBrowser.Api/Program.cs`
- Modify: `src/AgentBrowser.Api/Controllers/SessionController.cs`
- Modify: `src/AgentBrowser.Api/Controllers/NavigationController.cs`
- Modify: `src/AgentBrowser.Api/Controllers/DomController.cs`
- Modify: `src/AgentBrowser.Api/Controllers/ScreenshotController.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Api/ApiFactory.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Api/SessionApiTests.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Api/NavigationApiTests.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Api/DomApiTests.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Api/ScreenshotApiTests.cs`
- Delete: `tests/AgentBrowser.Tests.Integration/UnitTest1.cs`

**Interfaces:**
- Consumes: all services from Tasks 3-6.
- Produces: consistent error responses:
  - `SessionNotFoundException` -> 404
  - `NavigationException`, `SelectorException`, `ScriptExecutionException`, `ArgumentException` -> 400
  - `BrowserLaunchException` -> 503
  - unknown exception -> 500 with a generic message

- [ ] **Step 1: Write the failing API tests**

`ApiFactory` creates `WebApplicationFactory<Program>` and replaces `IBrowserManager` with a `Mock<IBrowserManager>`. Tests:
```csharp
[Fact]
public async Task CreateSession_ReturnsCreatedSession()
{
    var client = factory.CreateClient();
    var response = await client.PostAsJsonAsync("/session", new SessionOptions());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<SessionResponse>();
    Assert.NotNull(body);
    Assert.NotEmpty(body!.Id);
}

[Fact]
public async Task GetSession_UnknownId_ReturnsNotFound()
{
    var client = factory.CreateClient();
    var response = await client.GetAsync("/session/missing");
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```
Add equivalent tests for navigation, HTML, click, screenshot, and a fake `BrowserLaunchException` returning 503. For the 503 test, configure the fake `IBrowserManager.CreateSessionAsync` to throw `BrowserLaunchException`.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~Api" --nologo
```
Expected: FAIL because `Program` is not public, the middleware does not exist, and controllers are not mapped consistently.

- [ ] **Step 3: Add `public partial class Program` and the middleware**

At the end of `Program.cs`:
```csharp
public partial class Program { }
```
Register the middleware before `MapControllers()`. Remove `app.UseAuthorization()` because authentication is out of scope. Remove unused `using AgentBrowser.Browser.Browser;`. Remove `try/catch` blocks from the four controllers and let the middleware map exceptions.

- [ ] **Step 4: Implement error mapping**

The middleware reads `HttpContext.Response`, sets the status code and content type, and writes `ErrorResponse`. It must not leak stack traces unless the environment is Development.

- [ ] **Step 5: Run integration tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~Api" --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: all API tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: add API error mapping and integration tests"
```

---

### Task 8: Implement real metadata and price extraction

**Files:**
- Modify: `src/AgentBrowser.Browser/Browser/IBrowserManager.cs`
- Modify: `src/AgentBrowser.Browser/Browser/BrowserManager.cs`
- Create: `src/AgentBrowser.Shared/Models/Extraction/PageMetadata.cs`
- Create: `src/AgentBrowser.Shared/Models/Extraction/PriceInfo.cs`
- Modify: `src/AgentBrowser.Core/Extraction/ExtractionService.cs`
- Create: `src/AgentBrowser.Core/Extraction/ExtractionScripts.cs`
- Modify: `src/AgentBrowser.Api/Controllers/ExtractionController.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Extraction/ExtractionMetadataTests.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Extraction/ExtractionPriceTests.cs`

**Interfaces:**
- Adds to `IBrowserManager`:
```csharp
Task<string> EvaluateJsonFunctionAsync(string sessionId, string function, object[]? args = null, CancellationToken cancellationToken = default);
```
Returns a JSON string; the implementation may use `page.EvaluateFunctionAsync<string>` or serialize the returned `JsonElement`. Callers deserialize the string.
- Produces:
```csharp
public sealed record PageMetadata(
    string? Title,
    string? Description,
    string? CanonicalUrl,
    IReadOnlyDictionary<string, string> OpenGraph,
    IReadOnlyDictionary<string, string> Twitter,
    IReadOnlyList<JsonElement> JsonLd,
    IReadOnlyList<MetaTag> MetaTags);

public sealed record MetaTag(string? Name, string? Property, string? Content);

public sealed record PriceInfo(
    decimal? Value,
    string? Currency,
    string? CurrencySymbol,
    string? Context,
    string? Selector);
```

- [ ] **Step 1: Write the failing extraction tests**

Create a fixture HTML string containing a title, description meta, canonical link, Open Graph tags, Twitter card tags, one valid JSON-LD block, one malformed JSON-LD block, `data-price="19.99"`, and `<span itemprop="price" content="24.50">`. Navigate to `data:text/html,...` with the real browser and assert:
- Metadata title and description match.
- `og:title` and `twitter:card` are present.
- The valid JSON-LD block is parsed and the malformed one is ignored.
- Prices include 19.99 and 24.50 with the expected context.
Mark these tests `[Trait("Category", "Browser")]`.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~ExtractionMetadata|FullyQualifiedName~ExtractionPrice" --nologo
```
Expected: FAIL because extraction still returns hard-coded example data.

- [ ] **Step 3: Implement the extraction models and scripts**

Put the JavaScript in `ExtractionScripts` as `const string` raw string literals. `Metadata` returns an object with `title`, `description`, `canonicalUrl`, `openGraph`, `twitter`, `jsonLd`, and `metaTags`. `Prices` scans `[data-price]`, `[itemprop="price"]`, and elements whose class or text contains a currency pattern. Use `EvaluateJsonFunctionAsync` and `JsonSerializer.Deserialize` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.

- [ ] **Step 4: Update `ExtractionService` and the controller**

`ExtractMetadataAsync` returns `PageMetadata`. `ExtractPricesAsync` returns `IReadOnlyList<PriceInfo>`. Controller routes stay `GET /session/{sessionId}/extract/metadata` and `/prices`.

- [ ] **Step 5: Run browser extraction tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~ExtractionMetadata|FullyQualifiedName~ExtractionPrice" --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: implement metadata and price extraction"
```

---

### Task 9: Implement image, form, and link extraction

**Files:**
- Create: `src/AgentBrowser.Shared/Models/Extraction/ImageInfo.cs`
- Create: `src/AgentBrowser.Shared/Models/Extraction/FormInfo.cs`
- Create: `src/AgentBrowser.Shared/Models/Extraction/LinkInfo.cs`
- Modify: `src/AgentBrowser.Core/Extraction/ExtractionService.cs`
- Modify: `src/AgentBrowser.Core/Extraction/ExtractionScripts.cs`
- Modify: `src/AgentBrowser.Api/Controllers/ExtractionController.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Extraction/ExtractionContentTests.cs`

**Interfaces:**
- Consumes: the extraction pipeline from Task 8.
- Produces:
```csharp
public sealed record ImageInfo(string? Src, string? Alt, string? Title, int? Width, int? Height, string? Caption, string? ParentContext);
public sealed record FormFieldInfo(string? Name, string? Type, string? Value, string? Placeholder, bool Required, string? Label);
public sealed record FormInfo(string? Action, string? Method, IReadOnlyList<FormFieldInfo> Fields);
public sealed record LinkInfo(string? Href, string? Text, string? Title, string? Rel, string? Target);
```

- [ ] **Step 1: Write the failing content extraction tests**

Extend the fixture with:
- An `<img src="..." alt="..." title="..." width="..." height="...">` inside `<figure><figcaption>Caption</figcaption></figure>`.
- A form with `action`, `method`, a required email input, a password input, and labels.
- Links with `href`, text, `rel`, and `target`.

Assert exact values for all three extractors. Mark the tests `[Trait("Category", "Browser")]`.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~ExtractionContent" --nologo
```
Expected: FAIL because the three extractors still return example data.

- [ ] **Step 3: Implement the three scripts and service methods**

Images: use `naturalWidth`/`naturalHeight`, nearest `figcaption`, and the closest `article`, `section`, `[data-product]`, or `li` as `ParentContext`. Forms: include all inputs, textareas, and selects; resolve labels through `labels` or `aria-label`. Links: skip empty hrefs and resolve `href` to an absolute URL.

- [ ] **Step 4: Update controller routes**

Keep `GET /session/{sessionId}/extract/images`, `/forms`, and `/links`. Return empty arrays for pages with no matches, never example data.

- [ ] **Step 5: Run tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~ExtractionContent" --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: implement image, form, and link extraction"
```

---

### Task 10: Make WebSocket event streaming real

**Files:**
- Modify: `src/AgentBrowser.Browser/Browser/BrowserEvent.cs` (add console, load-complete, and page-error records)
- Modify: `src/AgentBrowser.Browser/Browser/BrowserManager.cs`
- Modify: `src/AgentBrowser.Api/WebSockets/WebSocketHandler.cs`
- Modify: `src/AgentBrowser.Api/Middleware/WebSocketMiddleware.cs`
- Modify: `src/AgentBrowser.Api/Program.cs`
- Modify: `src/AgentBrowser.Core/Extensions/ServiceCollectionExtensions.cs`
- Create: `tests/AgentBrowser.Tests.Unit/WebSockets/WebSocketHandlerTests.cs`
- Create: `tests/AgentBrowser.Tests.Integration/WebSockets/WebSocketEndpointTests.cs`

**Interfaces:**
- Consumes: `IBrowserEventSink`, `BrowserEvent`, and `NullBrowserEventSink` from Task 3, plus `WebSocketHandler`.
- Produces the concrete event records:
```csharp
public sealed record BrowserConsoleEvent(string SessionId, string Level, string Message, DateTimeOffset Timestamp) : BrowserEvent(SessionId, Timestamp);
public sealed record BrowserLoadCompleteEvent(string SessionId, string Url, DateTimeOffset Timestamp) : BrowserEvent(SessionId, Timestamp);
public sealed record BrowserErrorEvent(string SessionId, string Message, DateTimeOffset Timestamp) : BrowserEvent(SessionId, Timestamp);
```

- [ ] **Step 1: Write the failing WebSocket tests**

Unit test: `WebSocketHandler.PublishAsync` serializes a `BrowserConsoleEvent` and only sends it to the matching session ID. Use a fake `System.Net.WebSockets.WebSocket` subclass or a `Mock<WebSocket>` with `State = Open`; verify `SendAsync` receives JSON containing `"type":"console"` and the message text.

Integration test: start `WebApplicationFactory<Program>` with a fake `IBrowserManager`, connect to `/ws/session/test-session` using `factory.Server.CreateWebSocketClient()`, resolve `IBrowserEventSink` from the factory services, publish a console event, read one text frame, and assert the payload.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~WebSocket" --nologo
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~WebSocket" --nologo
```
Expected: FAIL because the sink and endpoint are not wired.

- [ ] **Step 3: Implement the event sink and page subscriptions**

`BrowserManager` accepts `IBrowserEventSink` and subscribes to `page.Console`, `page.Load`, and `page.PageError` when a session is created. Publish fire-and-forget tasks through a helper that catches and logs sink failures. Console events map `ConsoleMessage.Type` and `ConsoleMessage.Text`.

- [ ] **Step 4: Wire the endpoint**

`WebSocketHandler` implements `IBrowserEventSink`, keeps `WebSocket` instances keyed by session ID, and removes them on close or send failure. In `Program.cs`:
```csharp
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IBrowserEventSink>(sp => sp.GetRequiredService<WebSocketHandler>());
```
Add:
```csharp
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();
```
before `MapControllers()`.

- [ ] **Step 5: Run tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --nologo
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~WebSocket" --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: all tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: stream browser events over WebSocket"
```

---

### Task 11: Replace the hand-rolled MCP server with the official SDK

**Files:**
- Modify: `src/AgentBrowser.MCP/AgentBrowser.MCP.csproj` (add `ModelContextProtocol` 2.3.1; align `Microsoft.Extensions.Hosting` to 10.0.14 if restore asks for it)
- Modify: `src/AgentBrowser.MCP/Program.cs`
- Create: `src/AgentBrowser.MCP/Tools/BrowserTools.cs`
- Create: `tests/AgentBrowser.Tests.Unit/Mcp/BrowserToolsTests.cs`
- Create: `tests/AgentBrowser.Tests.Integration/Mcp/StdioProtocolTests.cs`

**Interfaces:**
- Consumes: `ISessionService`, `INavigationService`, `IDomService`, `IScreenshotService`, `ExtractionService`.
- Produces these MCP tools:
  - `browse_create_session(bool headless = true)`
  - `browse_close_session(string sessionId)`
  - `browse_navigate(string sessionId, string url, string? waitUntil = null, int? timeout = null)`
  - `browse_click(string sessionId, string selector, int delay = 0, int button = 0, int clickCount = 1)`
  - `browse_type(string sessionId, string selector, string text, int delay = 0, bool clearFirst = true)`
  - `browse_get_html(string sessionId)`
  - `browse_get_screenshot(string sessionId, bool fullPage = false, string format = "png", int? quality = null)`
  - `browse_execute_js(string sessionId, string script)`
  - `browse_get_metadata(string sessionId)`
  - `browse_get_prices(string sessionId)`
  - `browse_get_images(string sessionId)`
  - `browse_get_forms(string sessionId)`
  - `browse_get_links(string sessionId)`

- [ ] **Step 1: Write the failing tool tests**

`BrowserToolsTests` builds a `ServiceCollection` with fake services and resolves `IServiceProvider`. Tests:
- `browse_create_session` calls `ISessionService.CreateSessionAsync` and returns its session ID.
- `browse_navigate` validates that an unknown session ID propagates `SessionNotFoundException`.
- `browse_get_prices` calls `ExtractionService.ExtractPricesAsync` with the supplied session ID.
- `browse_click` passes selector and options through to `IDomService.ClickAsync`.

`StdioProtocolTests` starts the MCP process, writes newline-delimited JSON-RPC:
```json
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}
```
Then sends `tools/list` and asserts the response contains `browse_create_session` and `browse_navigate`. Close stdin, wait for exit, and assert every non-empty stdout line parses as JSON. Mark `[Trait("Category", "Mcp")]`.

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~BrowserTools" --nologo
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~StdioProtocol" --nologo
```
Expected: FAIL because the official SDK and tools do not exist.

- [ ] **Step 3: Rewrite the MCP host**

Use:
```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services.AddAgentBrowserCore();
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();
```
No `Console.WriteLine` on stdout. No custom JSON-RPC loop. No manual `StreamReader`.

- [ ] **Step 4: Implement the tools**

Use `[McpServerToolType]` and `[McpServerTool]` attributes. Each tool method takes `IServiceProvider services` as its first parameter so the SDK binds request services and keeps the JSON schema clean. Resolve scoped services inside each call. Never create a session implicitly in `browse_navigate`; require a session ID and return a clear tool error if it is missing.

- [ ] **Step 5: Run MCP tests and build**

Run:
```powershell
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --filter "FullyQualifiedName~BrowserTools" --nologo
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --filter "FullyQualifiedName~StdioProtocol" --nologo
dotnet build AgentBrowser.sln --nologo
```
Expected: tests pass, build 0 warnings.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat: replace MCP server with official SDK"
```

---

### Task 12: Make the documentation match the verified implementation

**Files:**
- Modify: `README.md`
- Modify: `SPEC.md`
- Modify: `IMPLEMENTATION.md`
- Modify: `ROADMAP.md`
- Modify: `DECISIONS.md`
- Modify: `AGENTS.md`
- Create: `docs/VERIFICATION.md` (initial version; Task 13 fills in results)

**Interfaces:**
- Consumes: the verified behavior from Tasks 1-11.
- Produces: docs that describe only supported behavior and explicitly list unsupported features.

- [ ] **Step 1: Write a documentation checklist test**

Add `docs/VERIFICATION.md` with a checklist:
```markdown
# Verification

- [ ] `dotnet build AgentBrowser.sln` succeeds with 0 warnings.
- [ ] `dotnet test tests/AgentBrowser.Tests.Unit` succeeds.
- [ ] `dotnet test tests/AgentBrowser.Tests.Integration --filter "Category!=Browser&Category!=Mcp"` succeeds.
- [ ] `dotnet test tests/AgentBrowser.Tests.Integration --filter "Category=Browser"` succeeds.
- [ ] `dotnet test tests/AgentBrowser.Tests.Integration --filter "Category=Mcp"` succeeds.
- [ ] Every HTTP route in README exists in `src/AgentBrowser.Api/Controllers`.
- [ ] Every MCP tool in README exists in `src/AgentBrowser.MCP/Tools/BrowserTools.cs`.
- [ ] Unsupported features are listed explicitly.
```
This file is the acceptance checklist, not an automated test.

- [ ] **Step 2: Run the checklist against the code**

Run:
```powershell
rg "\[Http(Get|Post|Delete|Put|Patch)" src/AgentBrowser.Api/Controllers
rg "\[McpServerTool" src/AgentBrowser.MCP/Tools
dotnet build AgentBrowser.sln --nologo
dotnet test AgentBrowser.sln --no-build --nologo
```
Expected: commands succeed; record the actual route and tool names for the docs.

- [ ] **Step 3: Rewrite the status sections**

In every affected document:
- Change `.NET 8` to `.NET 10`.
- Replace "Phase 2/3/4 complete" claims with a table of verified, in-progress, and unsupported features.
- List all actual HTTP routes and MCP tools.
- State that sessions are single-tab and Chromium-per-session.
- State that network interception, DOM mutation streaming, persistence, auth, Docker, and PDF are not implemented.
- Remove claims that the project is "production ready" or "5-10x faster" unless there is a benchmark in the repo.
- Do not add a license section unless a real `LICENSE` file exists. If the file is missing, mark the license as undecided.

- [ ] **Step 4: Verify documentation links and commands**

Run:
```powershell
rg "dotnet run|dotnet test|dotnet build" README.md docs/VERIFICATION.md
```
Expected: every documented command is one that has actually been run during this plan.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "docs: align documentation with verified MVP"
```

---

### Task 13: Add CI, run the full verification, and prepare the branch for review

**Files:**
- Create: `.github/workflows/ci.yml`
- Modify: `docs/VERIFICATION.md`
- Modify: `README.md` (only if verification uncovers a wrong command)

**Interfaces:**
- Consumes: all previous tasks.
- Produces: a CI workflow and a dated verification record.

- [ ] **Step 1: Create the CI workflow**

Create `.github/workflows/ci.yml`:
```yaml
name: ci
on:
  push:
    branches-ignore: [master]
  pull_request:

jobs:
  build-and-test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x
      - run: dotnet restore AgentBrowser.sln --nologo
      - run: dotnet build AgentBrowser.sln --no-restore --nologo -warnaserror
      - run: dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build --nologo
      - run: dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category!=Browser&Category!=Mcp"
      - run: dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Browser"
      - run: dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Mcp"

  api-smoke:
    runs-on: windows-latest
    needs: build-and-test
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x
      - run: dotnet build src/AgentBrowser.Api/AgentBrowser.Api.csproj --nologo
      - name: Start API
        shell: pwsh
        run: |
          Start-Process dotnet -ArgumentList "run --project src/AgentBrowser.Api --no-build --urls http://127.0.0.1:5080" -PassThru
          Start-Sleep -Seconds 8
      - name: Smoke test
        shell: pwsh
        run: |
          $health = Invoke-RestMethod http://127.0.0.1:5080/health
          if ($health.status -ne "healthy") { throw "health failed" }
```

- [ ] **Step 2: Run the full local verification**

Run:
```powershell
dotnet clean AgentBrowser.sln --nologo
dotnet restore AgentBrowser.sln --nologo
dotnet build AgentBrowser.sln --nologo -warnaserror
dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build --nologo
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category!=Browser&Category!=Mcp"
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Browser"
dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Mcp"
```
Expected: every command exits 0.

- [ ] **Step 3: Run the end-to-end REST smoke test**

Start the API:
```powershell
$api = Start-Process dotnet -ArgumentList "run --project src/AgentBrowser.Api --no-build --urls http://127.0.0.1:5080" -PassThru
Start-Sleep -Seconds 8
$session = Invoke-RestMethod -Method Post -Uri http://127.0.0.1:5080/session `
  -ContentType "application/json" -Body '{"headless":true}'
$html = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5080/session/$($session.id)/navigate" `
  -ContentType "application/json" -Body '{"url":"data:text/html,<title>Smoke</title><h1>Hello</h1><p data-price=\"19.99\">Price</p>"}'
$content = Invoke-RestMethod -Uri "http://127.0.0.1:5080/session/$($session.id)/html"
$prices = Invoke-RestMethod -Uri "http://127.0.0.1:5080/session/$($session.id)/extract/prices"
if ($html.title -ne "Smoke") { throw "navigation failed" }
if ($content.html -notmatch "Hello") { throw "html failed" }
if (@($prices).Count -lt 1) { throw "extraction failed" }
Invoke-RestMethod -Method Delete -Uri "http://127.0.0.1:5080/session/$($session.id)"
Stop-Process -Id $api.Id -Force
```
Expected: no exception; session closes; API process stops.

- [ ] **Step 4: Run the MCP stdio smoke test**

Run:
```powershell
@'
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"smoke","version":"1.0"}}}
{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
'@ | Set-Content -Encoding utf8 mcp-input.txt
$mcp = Start-Process dotnet -ArgumentList "run --project src/AgentBrowser.MCP --no-build" -PassThru `
  -RedirectStandardInput "mcp-input.txt" -RedirectStandardOutput "mcp-output.txt" -RedirectStandardError "mcp-error.txt"
Wait-Process -Id $mcp.Id -Timeout 30
$lines = Get-Content "mcp-output.txt" | Where-Object { $_.Trim() }
if (-not ($lines -match "browse_create_session")) { throw "tools/list missing browse_create_session" }
$lines | ForEach-Object { $_ | ConvertFrom-Json | Out-Null }
Remove-Item "mcp-input.txt", "mcp-output.txt", "mcp-error.txt" -ErrorAction SilentlyContinue
```
Expected: every non-empty stdout line is valid JSON, `browse_create_session` is present, and stdout contains no startup banner. If the process does not exit on stdin EOF, stop it after 10 seconds and still validate the captured frames.

- [ ] **Step 5: Record the results and commit**

Fill in `docs/VERIFICATION.md` with the date, commands run, and observed results. Then:
```powershell
git add -A
git commit -m "test: add CI and record full verification"
git push -u origin fix/working-mvp
```
Expected: branch pushed. Open a pull request against `master`; do not merge it without review.

---

## Plan Self-Review

**Spec coverage:** The plan covers every feature in the Working MVP scope: session lifecycle, navigation, HTML/content, DOM, JavaScript, waits, screenshots, all five extractors, WebSocket console/load/error events, MCP tools, tests, CI, and honest docs. Unsupported SPEC features are explicitly excluded.

**Step scan:** Every task has a failing test, a run that shows the failure, an implementation step with exact signatures, a passing run, and a commit. Tasks 1 and 13 are verification and CI tasks with concrete commands instead of new behavior.

**Type consistency:** `IBrowserManager` uses one set of signatures throughout. `PageNavigationResult`, `PageMetadata`, `PriceInfo`, `ImageInfo`, `FormInfo`, `LinkInfo`, and `BrowserEvent` are defined once and referenced consistently by services, controllers, and tools. `ScreenshotType` and `ClickOptions` live only in `AgentBrowser.Shared.Models`.

**Review Focus coverage:** Browser launch failure is pinned to Task 3 and Task 7. Unknown sessions to Task 7. Invalid inputs to Tasks 4, 5, 6, and 7. Close-during-operation to Task 3 and Task 7. MCP stdout/EOF to Task 11.

**Proportion:** The plan gives signatures, test names, assertions, and commands, not full implementations. Bodies are left to the implementer except where a browser API mapping or payload shape is easy to get wrong.
