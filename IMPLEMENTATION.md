# AgentBrowser Implementation Map

> This document describes what is implemented and verified, not aspirational work.

See [docs/VERIFICATION.md](docs/VERIFICATION.md) for the latest recorded verification run.

## Current implementation

### Session lifecycle

- `PuppeteerBrowserEngine` downloads and launches Chromium.
- `BrowserManager` owns the browser and page for each session.
- One active page per session; session IDs and tab IDs are independent.
- Session create, get, list, and close are implemented.
- Browser and page objects are closed on session close and on shutdown.

### Navigation and content

- Navigation with `NavigationOptions`.
- HTML retrieval.
- Body text retrieval.
- Tab URL, title, and status are updated after navigation.
- Navigation failures are mapped to `NavigationException`.

### DOM and JavaScript

- Click, type, wait for selector, and JavaScript evaluation.
- Selector failures are mapped to `SelectorException`.
- Script failures are mapped to `ScriptExecutionException`.

### Screenshots

- Viewport, full-page, and element screenshots.
- PNG, JPEG, and WebP.
- Base64 responses from the API and MCP.

### Extraction

- Metadata: meta tags, Open Graph, Twitter card, canonical URL, JSON-LD.
- Prices: `data-price`, `itemprop="price"`, and visible currency patterns.
- Images: src, alt, title, dimensions, captions, parent context.
- Forms: action, method, fields, labels, required attributes.
- Links: absolute href, text, title, rel, target.

### WebSocket events

- `/ws/session/{sessionId}`.
- Supported: `console`, `load-complete`, `error`.
- Not implemented: DOM mutation and network interception.

### MCP

- Official `ModelContextProtocol` C# SDK.
- Standard `initialize`, `tools/list`, and `tools/call`.
- Logs to stderr; stdout contains protocol frames only.
- 13 tools listed in README and SPEC.

## Source layout

```text
src/
  AgentBrowser.Api/
    Controllers/       HTTP endpoints
    Middleware/        ExceptionHandlingMiddleware, WebSocketMiddleware
    WebSockets/        WebSocket connection and payload handling
  AgentBrowser.Browser/
    Browser/           BrowserManager, engine seam, browser events
  AgentBrowser.Core/
    Extraction/        ExtractionService and extraction scripts
    Services/          Service adapters
    Extensions/        Dependency injection
  AgentBrowser.MCP/
    Tools/             MCP tools
  AgentBrowser.Shared/
    Exceptions/        Browser/session/navigation/selector/script errors
    Models/            Models, requests, responses, extraction records
```

## Test coverage

- Unit tests: model, browser manager, services, API controllers, MCP tools, WebSocket payloads.
- Integration tests: API endpoints with a fake browser, real browser extraction tests with local `data:` URLs, WebSocket endpoint, MCP stdio protocol.
- Browser tests are marked `Category=Browser`.
- MCP process test is marked `Category=Mcp`.

## Open items

- Multiple tabs per session.
- Network interception, request modification, mocking, and blocking.
- DOM mutation events.
- Session persistence, authentication, authorization, and rate limiting.
- Docker, PDF, and a license file.
