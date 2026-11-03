# Architecture Decisions

## Language and framework

**Decision:** C# 13 on .NET 10 (`net10.0`) with PuppeteerSharp.

**Rationale:**
- Native async and strong typing.
- PuppeteerSharp 19.0.0 is pinned and is the supported browser integration.
- Current APIs are standard ASP.NET Core and .NET dependency injection.
- The project is developed and verified on Windows.

**Current status:** Verified.

---

## Browser engine

**Decision:** Chromium through PuppeteerSharp CDP.

**Rationale:**
- Standards-compliant rendering.
- Mature CDP access for navigation, DOM, screenshots, and JavaScript.
- Headless and headed modes are supported through `SessionOptions.Headless`.

**Alternatives considered:**
- CEF: more complex setup.
- WebView2: Windows-only and not browser-automation oriented.
- Playwright: not chosen for this iteration.

**Implementation:**
- One Chromium browser per session.
- One active page per session.
- Browser and page references are owned by `BrowserManager` and closed on session close.

**Current status:** Verified.

---

## Browser cache

**Decision:** Download Chromium on first use into a cache directory.

**Rationale:** The repository should not commit Chromium binaries.

**Implementation:**
- Default cache path is `.agentbrowser-cache` under the host application's output directory (`AppContext.BaseDirectory`).
- Set `AGENTBROWSER_BROWSER_CACHE` to override it.
- Concurrent downloads are guarded by a shared semaphore.
- The installed Chrome executable path is passed explicitly to PuppeteerSharp.

**Current status:** Verified.

---

## API architecture

**Decision:** REST API for commands and WebSocket for events.

**Rationale:**
- REST is easy to call and debug.
- WebSocket is appropriate for console, load, and page-error events.
- Language-agnostic for agents.

**Current status:** Verified for REST. WebSocket supports `console`, `load-complete`, and `error` only.

---

## MCP server

**Decision:** Use the official `ModelContextProtocol` C# SDK.

**Rationale:**
- Standard initialize, tools/list, and tools/call handshake.
- Avoids a fragile hand-rolled protocol implementation.
- Tool discovery can be tested locally.

**Implementation:**
- stdio transport.
- Logs to stderr; stdout contains protocol frames only.
- Tools live in `src/AgentBrowser.MCP/Tools/BrowserTools.cs`.

**Current status:** Verified.

---

## Session management

**Decision:** In-memory sessions only.

**Rationale:**
- Simplest correct MVP.
- Avoids half-implemented persistence.
- Session IDs are unique per process.

**Consequences:**
- No session survives restart.
- No multi-instance coordination.
- No SQLite or distributed store.

**Current status:** Verified.

---

## Single-tab scope

**Decision:** One active page per session.

**Rationale:** Multi-tab behavior is not required for the MVP and is not assumed by extraction, screenshots, or JavaScript execution.

**Consequences:** Multi-tab support requires explicit design and is not claimed.

**Current status:** Verified.

---

## Data extraction

**Decision:** Extract hidden data with page-side JavaScript.

**Rationale:**
- Hidden data lives in meta tags, Open Graph, JSON-LD, `data-*`, and `itemprop` attributes.
- JavaScript can normalize values before serialization.
- Extraction is typed and testable.

**Current status:** Verified for metadata, prices, images, forms, and links.

---

## Error handling

**Decision:** Custom exception hierarchy and a single middleware mapping exceptions to HTTP responses.

**Rationale:**
- Predictable 400/404/500/503 responses.
- No stack traces in production error responses.
- Controllers should not contain error-handling boilerplate.

**Hierarchy:**
- `SessionNotFoundException` → 404
- `NavigationException`, `SelectorException`, `ScriptExecutionException`, `ArgumentException` → 400
- `BrowserLaunchException` → 503

**Current status:** Verified.

---

## Dependency injection

**Decision:** Microsoft.Extensions.DependencyInjection.

**Rationale:** Standard for ASP.NET Core and compatible with the MCP host.

**Current status:** Verified.

---

## Project structure

**Decision:** Layered projects with one direction of dependencies.

```text
Shared -> Browser -> Core -> Api / MCP
```

**Current status:** Verified.

---

## License

**Decision:** Undecided.

**Rationale:** This repository does not contain a `LICENSE` file and no copyright holder has been selected.

**Consequences:** Documentation must not claim an MIT or other license.

---

## Archived decisions

### Python vs C#

- Python was rejected for the initial implementation because of performance and deployment trade-offs.

### FastAPI vs ASP.NET Core

- ASP.NET Core was selected.

### PuppeteerSharp vs Playwright

- PuppeteerSharp is used. Playwright is not integrated.
