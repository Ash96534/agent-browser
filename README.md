# AgentBrowser

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![PuppeteerSharp 19.0.0](https://img.shields.io/badge/PuppeteerSharp-19.0.0-2671A9?logo=googlechrome)
![License: Undecided](https://img.shields.io/badge/license-undecided-lightgrey)
![Status: MVP](https://img.shields.io/badge/status-MVP-blue)

> A local browser-control service for AI agents, backed by Chromium.

AgentBrowser gives autonomous AI agents fine-grained, programmatic control over a single Chromium page through a REST API, a WebSocket event stream, and an MCP stdio server. It is intentionally **single-tab**: one session has one active page. The goal is not to replace a human browser, but to give agents reliable, observable, and scriptable control over Chromium for navigation, extraction, DOM interaction, screenshots, and page-health monitoring.

## Badges

[![build](https://github.com/Ash96534/agent-browser/actions/workflows/ci.yml/badge.svg)](https://github.com/Ash96534/agent-browser/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PuppeteerSharp](https://img.shields.io/badge/PuppeteerSharp-19.0.0-2671A9?logo=googlechrome)](https://github.com/hardkoded/puppeteer-sharp)
[![MCP SDK](https://img.shields.io/badge/ModelContextProtocol-2.3.1-blue)](https://github.com/modelcontextprotocol/csharp-sdk)

## Table of contents

- [Features](#features)
- [Requirements](#requirements)
- [Quick start](#quick-start)
- [Configuration](#configuration)
- [HTTP API](#http-api)
  - [Session management](#session-management)
  - [Navigation](#navigation)
  - [DOM and JavaScript](#dom-and-javascript)
  - [Advanced DOM](#advanced-dom)
  - [Screenshots](#screenshots)
  - [Extraction](#extraction)
  - [Browser telemetry and health](#browser-telemetry-and-health)
  - [Meta](#meta)
- [WebSocket events](#websocket-events)
- [MCP tools](#mcp-tools)
- [Safety policy](#safety-policy)
- [Authentication](#authentication)
- [Packaging](#packaging)
- [Security](#security)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## Features

### Browser control
- Create, list, get, and close browser sessions
- One Chromium browser per session, one active page per session
- Session health check and restart for crashed or closed browsers
- Navigation to absolute URLs with configurable wait states (`Load`, `DOMContentLoaded`, `NetworkIdle0`, `NetworkIdle2`)
- Rendered HTML and visible body text retrieval
- Content output in `text`, `html`, or `markdown` formats

### DOM interaction
- Click elements by CSS selector with configurable button, delay, and click count
- Type text into elements with optional clear-first and keystroke delay
- Execute arbitrary JavaScript expressions and return typed results
- Wait for a selector with configurable timeout
- Fill form fields by visible label text
- List visible, actionable elements with tag, type, role, text, selector, and URL

### Screenshots
- Viewport, full-page, and element screenshots
- PNG, JPEG, and WebP formats
- Quality control for lossy formats
- Optional `maxBytes` guard to prevent oversized captures

### Hidden-data extraction
- Metadata: `<title>`, `<meta description>`, `<link rel="canonical">`, Open Graph tags, Twitter Card tags, JSON-LD, `<meta>` tags
- Prices: `data-price`, `itemprop="price"`, and visible currency patterns
- Images: src, alt, title, natural dimensions, captions, parent context
- Forms: action, method, fields, labels, placeholders, required attributes
- Links: absolute href, text, title, rel, target, same-origin filtering
- Invoice schema: vendor, invoice number, issue/due dates, total, currency, source (from JSON-LD)

### Browser telemetry
- Console log messages per session
- Page error messages per session
- Session health: active, browser-closed, page-closed
- Diagnostics endpoint: uptime, active sessions, Chromium availability

### MCP server
- stdio transport using the official `ModelContextProtocol` C# SDK
- Tools for session lifecycle, navigation, DOM interaction, screenshots, extraction, telemetry, health, and restart
- Logs go to stderr; stdout contains only MCP protocol frames

### Safety and security
- API key authentication (optional, via `AGENTBROWSER_API_KEY`)
- CORS allowlist (denied by default)
- Request size limits (default 1 MB)
- Blocked URL schemes (`file`, `chrome`, `about` by default)
- Optional private-network blocking
- Correlation IDs on every response

## Requirements

- .NET 10 SDK (10.0.301 or later)
- Chromium, downloaded automatically on first use by PuppeteerSharp
- Windows is the verified platform
- Sufficient disk space and system dependencies for Chromium

## Quick start

Clone and build:

```bash
git clone https://github.com/Ash96534/agent-browser.git
cd agent-browser

dotnet restore AgentBrowser.sln
dotnet build AgentBrowser.sln --nologo -warnaserror
```

Run the API server:

```bash
dotnet run --project src/AgentBrowser.Api
```

The API listens on `http://127.0.0.1:5000` by default.

Health check:

```bash
curl http://localhost:5000/health
```

Create a session, navigate, and extract prices in one flow:

```bash
# Create a session
SESSION=$(curl -s -X POST http://localhost:5000/session \
  -H "Content-Type: application/json" \
  -d '{"headless":true}' | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")

# Navigate
curl -s -X POST http://localhost:5000/session/$SESSION/navigate \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com"}'

# Extract prices
curl -s http://localhost:5000/session/$SESSION/extract/prices

# Close the session
curl -s -X DELETE http://localhost:5000/session/$SESSION
```

Run the MCP server:

```bash
dotnet run --project src/AgentBrowser.MCP
```

MCP communicates over stdio. Logs go to stderr; stdout contains only JSON-RPC protocol frames.

## Configuration

All configuration is via environment variables. None are required for local development.

| Variable | Description | Default |
|----------|-------------|---------|
| `AGENTBROWSER_API_KEY` | Enable API-key authentication. Require `X-Api-Key` or `Authorization: Bearer` on every request. | unset |
| `AGENTBROWSER_ALLOWED_HOSTS` | Comma-separated host allowlist for navigation. `*` allows all hosts. | unset (no restriction) |
| `AGENTBROWSER_BLOCKED_SCHEMES` | Comma-separated scheme blocklist. | `file,chrome,about` |
| `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS` | `true` to block private IP addresses and `localhost`. | unset |
| `AGENTBROWSER_CORS_ORIGINS` | Comma-separated CORS origins. CORS is disabled by default. | unset |
| `AGENTBROWSER_MAX_REQUEST_BYTES` | Maximum HTTP request body size in bytes. | `1000000` |
| `AGENTBROWSER_PERSISTENCE` | Set `true` to enable SQLite operation audit log. | unset |
| `AGENTBROWSER_SQLITE_PATH` | Override the SQLite operation database path. | `<output dir>/agentbrowser-operations.db` |
| `AGENTBROWSER_BROWSER_CACHE` | Override the Chromium cache path. | `<output dir>/.agentbrowser-cache` |

Read [SECURITY.md](SECURITY.md) for the full threat model.

## HTTP API

### Session management

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/session` | Create a new browser session |
| `GET` | `/session` | List all active sessions |
| `GET` | `/session/{id}` | Get session state |
| `DELETE` | `/session/{id}` | Close a session |

### Navigation

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/session/{sessionId}/navigate` | Navigate to an absolute URL |
| `GET` | `/session/{sessionId}/html` | Get rendered HTML |
| `GET` | `/session/{sessionId}/content?format=text\|html\|markdown` | Get page content |

### DOM and JavaScript

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/session/{sessionId}/dom/click` | Click an element by CSS selector |
| `POST` | `/session/{sessionId}/dom/type` | Type text into an element |
| `POST` | `/session/{sessionId}/dom/execute` | Execute JavaScript |
| `GET` | `/session/{sessionId}/dom/wait?selector=...&timeout=...` | Wait for a selector |
| `POST` | `/session/{sessionId}/dom/fill` | Fill fields by visible label text |

### Advanced DOM

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/session/{sessionId}/clickable-elements` | List visible, actionable elements |
| `POST` | `/session/{sessionId}/click-safe` | Click an element only with explicit `confirm`; purchase/payment labels are refused |

### Screenshots

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/session/{sessionId}/screenshot` | Capture a screenshot (PNG, JPEG, WebP) |
| `GET` | `/session/{sessionId}/screenshot/base64` | Capture a screenshot as base64 |

Screenshot query parameters: `type` (`viewport`, `fullpage`, `element`), `selector` (for element), `fullPage`, `format` (`png`, `jpeg`, `jpg`, `webp`), `quality` (0–100), `maxBytes`.

### Extraction

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/session/{sessionId}/extract/metadata` | Metadata (title, description, canonical, Open Graph, Twitter, JSON-LD, meta tags) |
| `GET` | `/session/{sessionId}/extract/prices?max=...` | Prices from `data-price`, `itemprop`, and currency patterns |
| `GET` | `/session/{sessionId}/extract/images?max=...` | Images with alt, title, dimensions, captions, parent context |
| `GET` | `/session/{sessionId}/extract/forms?max=...` | Forms with action, method, fields, labels |
| `GET` | `/session/{sessionId}/extract/links?max=...&sameOrigin=true` | Links with absolute href, text, title, rel, target |
| `GET` | `/session/{sessionId}/extract/invoice` | Invoice fields from JSON-LD |

### Browser telemetry and health

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/session/{sessionId}/console` | Recent console log messages |
| `GET` | `/session/{sessionId}/errors` | Recent page error messages |
| `GET` | `/session/{sessionId}/health` | Session health: active, browser-closed, page-closed |
| `POST` | `/session/{sessionId}/restart` | Recycle the browser and page, preserving session ID |

### Meta

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/health` | Service health |
| `GET` | `/capabilities` | API version, feature manifest, endpoint list |
| `GET` | `/diagnostics` | Uptime, active sessions, Chromium status |
| `GET` | `/docs` | Lightweight HTML API overview |

## WebSocket events

Endpoint: `/ws/session/{sessionId}`

| Event type | Description |
|------------|-------------|
| `console` | Console log message from the page |
| `load-complete` | Page load completed |
| `error` | Page error |

DOM mutation and network events are not implemented.

## MCP tools

The MCP server uses stdio and the standard `initialize`, `tools/list`, and `tools/call` handshake. Logs go to stderr; stdout contains only protocol frames.

| Tool | Description |
|------|-------------|
| `browse_create_session` | Create a new browser session |
| `browse_close_session` | Close a browser session |
| `browse_close_all_sessions` | Close every active session |
| `browse_list_sessions` | List active sessions |
| `browse_healthcheck` | Service diagnostics |
| `browse_get_session_health` | Session health state |
| `browse_restart_session` | Recycle a session's browser |
| `browse_navigate` | Navigate to a URL |
| `browse_click` | Click an element |
| `browse_type` | Type text into an element |
| `browse_get_html` | Get rendered HTML |
| `browse_get_screenshot` | Capture a screenshot |
| `browse_execute_js` | Execute JavaScript |
| `browse_get_metadata` | Extract metadata |
| `browse_get_prices` | Extract prices |
| `browse_get_images` | Extract images |
| `browse_get_forms` | Extract forms |
| `browse_get_links` | Extract links |
| `browse_get_invoice` | Extract invoice fields |
| `browse_get_console_logs` | Get console logs |
| `browse_get_page_errors` | Get page errors |
| `browse_clickable_elements` | List clickable elements |
| `browse_fill_by_label` | Fill fields by label |
| `browse_visual_diff` | Compare current screenshot to previous |
| `browse_click_safe` | Confirm-gated click; payment/purchase labels blocked |

## Safety policy

Navigation URLs are validated before the browser loads them:

- Must be absolute URLs.
- `data:` URLs are allowed for local testing.
- `file:`, `chrome:`, and `about:` schemes are blocked by default.
- Optional host allowlist via `AGENTBROWSER_ALLOWED_HOSTS`.
- Optional private-network blocking via `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS`.

Invalid or blocked navigation returns `400 Bad Request` with an error kind of `NavigationFailure`.

## Authentication

Set `AGENTBROWSER_API_KEY` to enable authentication. Provide the key via the `X-Api-Key` header or an `Authorization: Bearer <key>` header. Without a key configured, no authentication is required.

## Packaging

See:

- [packaging/Dockerfile](packaging/Dockerfile) — container image
- [packaging/agentbrowser.service](packaging/agentbrowser.service) — systemd unit
- [packaging/publish.ps1](packaging/publish.ps1) — .NET publish script

A PowerShell helper is included at [scripts/agentbrowser.ps1](scripts/agentbrowser.ps1):

```powershell
pwsh ./scripts/agentbrowser.ps1 -Command session-create
```

## Security

AgentBrowser is a local browser-control service. It is not a public-facing service and must not be exposed to the internet without careful consideration.

Read [SECURITY.md](SECURITY.md) for the full threat model, configuration guidance, and safety restrictions.

## Troubleshooting

| Problem | Likely cause | Fix |
|---------|--------------|-----|
| `503 BrowserLaunchException` | Chromium not found or cannot launch | Verify `AGENTBROWSER_BROWSER_CACHE` and system dependencies; delete the cache and retry |
| `401 Unauthorized` | API key required | Set `AGENTBROWSER_API_KEY` and provide it on requests |
| `400 NavigationFailure` | Blocked scheme or host | Check URL scheme and `AGENTBROWSER_ALLOWED_HOSTS` |
| `413 Request entity too large` | Request body exceeds limit | Reduce payload or raise `AGENTBROWSER_MAX_REQUEST_BYTES` |
| Session missing | Session closed or expired | Create a new session |

Browser tests use local `data:` URLs and do not access the public network. They require a downloaded Chromium binary.

## Contributing

Build and test before contributing:

```bash
dotnet build AgentBrowser.sln --nologo -warnaserror
dotnet test AgentBrowser.sln --no-build --nologo
```

Browser tests require Chromium and run against local `data:` URLs only.

See [CONTRIBUTING.md](CONTRIBUTING.md) for the contribution guide, code style, and verification checklist.

## License

This project does not yet contain a `LICENSE` file. Do not publish copies or derivative works until a license is selected.
