# AgentBrowser Specification

> A controlled browser-control daemon for AI agents, backed by Chromium.

## 1. Purpose

AgentBrowser gives agents fine-grained control over a single Chromium page exposed through a local API. It supports navigation, DOM interaction, JavaScript execution, screenshots, hidden-data extraction, and real-time console/load/error events.

The current implementation is a single-tab MVP. Multi-tab browsing is out of scope.

## 2. Architecture

```text
Agent / application
        |
        | HTTP, WebSocket, or MCP
        v
ASP.NET Core API / MCP server
        |
        | Core services
        v
Browser layer
        |
        | PuppeteerSharp / CDP
        v
Chromium
```

### 2.1 Layers

- `AgentBrowser.Shared`: domain models, requests, responses, exceptions.
- `AgentBrowser.Browser`: Chromium startup and page operations.
- `AgentBrowser.Core`: service adapters and extraction logic.
- `AgentBrowser.Api`: REST controllers, middleware, and WebSocket streaming.
- `AgentBrowser.MCP`: MCP server built with the official C# SDK.

## 3. Technology stack

| Layer | Technology |
|-------|------------|
| Language | C# 13, .NET 10 (`net10.0`) |
| Browser engine | PuppeteerSharp 19.0.0 |
| API | ASP.NET Core |
| MCP | ModelContextProtocol 2.3.1 |
| Tests | xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing |
| Browser tests | Local `data:` URLs; no public network access |

## 4. HTTP API

### 4.1 Session management

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/session` | Create a session |
| GET | `/session` | List active sessions |
| GET | `/session/{id}` | Get session state |
| DELETE | `/session/{id}` | Close a session |

### 4.2 Navigation

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/session/{sessionId}/navigate` | Navigate to an absolute URL |
| GET | `/session/{sessionId}/html` | Get rendered HTML |
| GET | `/session/{sessionId}/content` | Get visible body text |

### 4.3 DOM and JavaScript

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/session/{sessionId}/dom/click` | Click an element |
| POST | `/session/{sessionId}/dom/type` | Type into an element |
| POST | `/session/{sessionId}/dom/execute` | Execute JavaScript |
| GET | `/session/{sessionId}/dom/wait` | Wait for a selector |

### 4.4 Screenshots

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/session/{sessionId}/screenshot` | Capture screenshot |
| GET | `/session/{sessionId}/screenshot/base64` | Capture base64 screenshot |

### 4.5 Extraction

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/session/{sessionId}/extract/metadata` | Extract metadata |
| GET | `/session/{sessionId}/extract/prices` | Extract prices |
| GET | `/session/{sessionId}/extract/images` | Extract images |
| GET | `/session/{sessionId}/extract/forms` | Extract forms |
| GET | `/session/{sessionId}/extract/links` | Extract links |

### 4.6 Health

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/health` | Health check |

## 5. WebSocket events

Endpoint: `/ws/session/{sessionId}`

Supported event types:

- `console`
- `load-complete`
- `error`

DOM mutation and network interception events are not implemented.

## 6. MCP tools

The MCP server uses stdio and standard MCP `initialize`, `tools/list`, and `tools/call`. Logs are written to stderr; stdout contains protocol frames only.

```text
browse_create_session(bool headless = true)
browse_close_session(string sessionId)
browse_navigate(string sessionId, string url, string? waitUntil = null, int? timeout = null)
browse_click(string sessionId, string selector, int delay = 0, int button = 0, int clickCount = 1)
browse_type(string sessionId, string selector, string text, int delay = 0, bool clearFirst = true)
browse_get_html(string sessionId)
browse_get_screenshot(string sessionId, bool fullPage = false, string format = "png", int? quality = null)
browse_execute_js(string sessionId, string script)
browse_get_metadata(string sessionId)
browse_get_prices(string sessionId)
browse_get_images(string sessionId)
browse_get_forms(string sessionId)
browse_get_links(string sessionId)
```

## 7. Core data models

- `Session`: session metadata and one active tab.
- `Tab`: URL, title, and status.
- `NavigateRequest`: `Url`, `Timeout`, `WaitUntil`.
- `ScreenshotRequest`: `Type`, `Selector`, `FullPage`, `Format`, `Quality`.
- `PageMetadata`: title, description, canonical URL, Open Graph, Twitter card, JSON-LD, and meta tags.
- `PriceInfo`, `ImageInfo`, `FormInfo`, `LinkInfo`: extraction results.

## 8. Error behavior

Known browser and request errors are mapped to stable HTTP responses:

- Unknown session: 404
- Invalid navigation URL, selector, screenshot type, or format: 400
- Chromium launch failure: 503
- Unknown error: 500 with a generic message

## 9. Known limitations

- One active page per session; no multi-tab support.
- No network interception, request modification, mocking, or blocking.
- No DOM mutation events.
- No persistence, authentication, authorization, or rate limiting.
- No multi-instance scaling.
- No Docker or PDF generation.
- License is intentionally undecided; this repository has no `LICENSE` file.

## 10. Implementation status

The implemented and locally verified MVP covers sections 4-8. Any feature outside this document is not supported and must not be advertised as functional.
