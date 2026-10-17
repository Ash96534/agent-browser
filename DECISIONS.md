# Architecture Decisions

## Language & Framework

**Decision:** Use C# (.NET 8) with PuppeteerSharp for browser automation

**Rationale:**
- 5-10x better throughput than Python/FastAPI
- Native performance for high-frequency agent calls
- Strong async/await support for concurrent operations
- Good CDP wrapper ecosystem (PuppeteerSharp v19.0.0)
- Faster development compared to C++ while still being compiled
- Excellent tooling and debugging support in VS Code
- Strong type safety reduces runtime errors

**Date:** 2025-10-16

**Implementation:**
- Language: C# (.NET 8)
- Browser Engine: PuppeteerSharp 19.0.0
- API Framework: ASP.NET Core
- DI Container: Microsoft.Extensions.DependencyInjection

---

## Browser Engine

**Decision:** Chromium via PuppeteerSharp (CDP)

**Rationale:**
- Standards-compliant rendering (blink engine)
- Mature CDP protocol with full browser internals access
- Headless and headed modes supported
- Good debugging tools
- Active maintenance and community support
- Production-ready for automation use cases

**Alternatives considered:**
- CEF (Chromium Embedded Framework) - too low-level, more complex setup
- Firefox via Marionette - less mature CDP support
- WebView2 - Windows-only, less automation-focused

**Implementation:**
- Package: PuppeteerSharp 19.0.0
- Usage: `Puppeteer.LaunchAsync()` for browser creation
- Session Management: In-memory with optional SQLite persistence

---

## API Architecture

**Decision:** REST API with WebSocket for real-time events

**Rationale:**
- REST for stateless operations (navigate, extract, query)
- WebSocket for streaming (console logs, network events, DOM updates)
- Simple to integrate with any agent language (Python, Node.js, etc.)
- Easy to debug with curl/Postman
- Built-in ASP.NET Core support for both protocols

**Alternatives considered:**
- gRPC - more complex, overkill for this use case
- Pure WebSocket - harder to debug/test
- SignalR - good but WebSocket is simpler for our needs

**Implementation:**
- REST Endpoints: ASP.NET Core controllers
- WebSocket Endpoints: `/ws/session/{id}`
- Message Format: JSON

---

## Session Management

**Decision:** In-memory sessions with optional SQLite persistence

**Rationale:**
- Fast for single-instance deployments
- SQLite option for multi-instance/scaling
- Simple state model per session
- Easy to implement and maintain
- Can be extended with Redis for distributed caching

**Session State:**
```csharp
public class Session
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public SessionStatus Status { get; set; }
    public List<Tab> Tabs { get; set; } = new();
    public Viewport Viewport { get; set; } = new();
    public string UserAgent { get; set; } = string.Empty;
    public List<Cookie> Cookies { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

---

## Project Structure

**Decision:** Monorepo with clear layer separation

```
src/
  AgentBrowser.Api/      # REST API (controllers, middleware, WebSocket)
  AgentBrowser.Core/     # Business logic, extraction
  AgentBrowser.Browser/  # Browser engine (PuppeteerSharp wrapper)
  AgentBrowser.MCP/      # MCP server
  AgentBrowser.Shared/   # Shared models
```

**Rationale:**
- Clear separation of concerns
- Testable layers
- Can be packaged as libraries if needed
- Dependencies flow: Shared -> Browser -> Core -> API

**Project Dependencies:**
- `AgentBrowser.Shared` - No dependencies (core models)
- `AgentBrowser.Browser` - References Shared + PuppeteerSharp
- `AgentBrowser.Core` - References Shared + Browser
- `AgentBrowser.Api` - References Core + Shared

---

## Extension Methods Pattern

**Decision:** Use extension methods for browser operations

**Rationale:**
- Cleaner API for session/tab operations
- Easy to test and mock
- Follows .NET conventions
- Allows adding functionality without modifying core types

**Example:**
```csharp
// Instead of: session.GetTab(tabId).Click(selector)
// Use: session.GetTabAsync(tabId).Result.ClickAsync(selector)

public static class TabExtensions
{
    public static async Task ClickAsync(this Tab tab, string selector, ClickOptions? options = null)
    {
        // Implementation
    }
}
```

---

## Dependency Injection

**Decision:** Microsoft.Extensions.DependencyInjection

**Rationale:**
- Standard .NET DI container
- Well-documented and supported
- Works seamlessly with ASP.NET Core
- Easy to configure and test

**Service Registration:**
```csharp
services.AddSingleton<IBrowserManager, BrowserManager>();
services.AddScoped<ISessionService, SessionService>();
services.AddScoped<INavigationService, NavigationService>();
services.AddScoped<IDomService, DomService>();
services.AddScoped<IScreenshotService, ScreenshotService>();
```

---

## Error Handling Strategy

**Decision:** Custom exception hierarchy with error mapping

**Rationale:**
- Clear error messages for debugging
- Consistent error responses to API clients
- Easy to unit test error scenarios
- Can map low-level exceptions to domain-specific ones

**Exception Hierarchy:**
- `BrowserException` (base)
  - `SessionNotFoundException`
  - `TabNotFoundException`
  - `NavigationException`
  - `SelectorException`
  - `ScriptExecutionException`

---

## Data Extraction Philosophy

**Decision:** Unified extraction API for "hidden" data

**Rationale:**
- Hidden data lives in:
  - Developer tooling/attributes (`data-*`, `aria-*`)
  - Structured data (JSON-LD, Microdata)
  - Meta tags (OG, Twitter Cards)
  - Implicit context (nearby elements, parent relationships)
- Expose these through a unified API rather than raw HTML access
- Support both simple selectors and schema-based extraction

**Extraction Types:**
- Metadata (OG, Twitter, JSON-LD, Microdata)
- Price data (`data-price`, Schema.org)
- Image context (alt, aria-label, nearby text)
- Form data (fields, labels, validation)
- Links (href, rel, target)

---

## Headless vs Headed Mode

**Decision:** Support both modes, default to headless

**Rationale:**
- Headless for production/automated use (no UI overhead)
- Headed for debugging/development (visual inspection)
- Configurable via `SessionOptions.Headless`

**Implementation:**
```csharp
var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = options.Headless  // true for headless, false for headed
});
```

---

## Configuration Strategy

**Decision:** Configuration classes with options pattern

**Rationale:**
- Strongly-typed configuration
- Easy to validate
- Supports multiple environments (Development, Production)
- Easy to inject into services

**Configuration Classes:**
```csharp
public class BrowserSettings
{
    public bool Headless { get; set; } = true;
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
}

public class SessionSettings
{
    public int MaxSessions { get; set; } = 100;
    public int IdleTimeoutMinutes { get; set; } = 30;
}
```

---

## Next Major Decision Points

1. **MCP Server Transport**
   - stdio (standard input/output)
   - HTTP (for network-based communication)
   - Recommendation: Start with stdio, add HTTP later

2. **Authentication Approach**
   - API key-based
   - Token-based (JWT)
   - OAuth2 for multi-tenant
   - Recommendation: API key for prototype, expand later

3. **Multi-Instance Scaling**
   - Redis for distributed session state
   - Message queue for load balancing
   - Recommendation: Start with single-instance, add Redis later

4. **Build System**
   - GitHub Actions for CI/CD
   - Docker for containerization
   - Recommendation: Start with basic GitHub Actions, expand later

---

## Breaking Changes

None yet. All decisions are backward-compatible with current implementation.

---

## Archived Decisions

### Python vs C#
- **Python** - Slower, good for prototyping
- **C#** - Faster, production-ready
- **Decision:** C# for performance-critical agent interactions

### FastAPI vs ASP.NET Core
- **FastAPI** - Python, async-first, good docs
- **ASP.NET Core** - C#, high-performance, mature
- **Decision:** ASP.NET Core for production performance

### PuppeteerSharp vs Playwright
- **PuppeteerSharp** - Direct C# port of Node.js Puppeteer
- **Playwright** - Multi-language, newer but less C# maturity
- **Decision:** PuppeteerSharp for C# ecosystem fit

---

**Last Updated:** 2026-10-16  
**Current Status:** Phase 1 Complete - Architecture validated and implemented
