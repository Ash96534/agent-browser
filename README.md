# AgentBrowser

> A local browser-control service for AI agents.

AgentBrowser gives agents a controlled Chromium instance behind a REST API, a WebSocket event stream, and an MCP stdio server. It is intentionally single-page: one session has one active tab.

## Status

Tested end-to-end on Windows with .NET 10.

Supported:

- Session create, get, list, and close.
- Navigation, rendered HTML, and visible body text or Markdown.
- Click, type, wait for selector, and JavaScript execution.
- Viewport, full-page, and element screenshots.
- Hidden-data extraction: metadata, prices, images, forms, links, invoice fields.
- Console, page-load, and page-error events.
- Diagnostics, capabilities manifest, request size limits, optional authentication.

Not supported yet:

- Multi-tab sessions.
- Network interception or request blocking.
- DOM mutation events.
- Persistent sessions across process restarts.
- Multi-instance coordination.
- Production-hardened Docker or service deployment.

## Requirements

- .NET 10 SDK 10.0.301 or later.
- Chromium, downloaded on first use.
- Windows is the verified platform.

## Quick start

```bash
git clone https://github.com/Ash96534/agent-browser.git
cd agent-browser

dotnet restore AgentBrowser.sln
dotnet build AgentBrowser.sln -c Release
dotnet run --project src/AgentBrowser.Api
```

Health check:

```bash
curl http://localhost:5000/health
```

## HTTP overview

GET `/health` — health check.

Sessions:

- `POST /session`
- `GET /session`
- `GET /session/{id}`
- `DELETE /session/{id}`

Navigation:

- `POST /session/{id}/navigate`
- `GET /session/{id}/html`
- `GET /session/{id}/content?format=text|html|markdown`

DOM and JavaScript:

- `POST /session/{id}/dom/click`
- `POST /session/{id}/dom/type`
- `POST /session/{id}/dom/execute`
- `GET /session/{id}/dom/wait?selector=...`
- `POST /session/{id}/dom/fill` — fills fields by visible label text

Screenshots:

- `GET /session/{id}/screenshot?type=viewport|fullpage|element&format=png|jpeg&quality=80&maxBytes=...`
- `GET /session/{id}/screenshot/base64?format=png&maxBytes=...`

Extraction:

- `GET /session/{id}/extract/metadata`
- `GET /session/{id}/extract/prices?max=...`
- `GET /session/{id}/extract/images?max=...`
- `GET /session/{id}/extract/forms?max=...`
- `GET /session/{id}/extract/links?max=...&sameOrigin=true`
- `GET /session/{id}/extract/invoice`

Page telemetry:

- `GET /session/{id}/console`
- `GET /session/{id}/errors`

Advanced:

- `GET /session/{id}/clickable-elements`
- `GET /session/{id}/visual-diff`
- `POST /session/{id}/click-safe` — confirm gated; purchase/payment labels are rejected

Meta:

- `GET /capabilities` — API and feature manifest
- `GET /diagnostics` — uptime, active sessions, Chromium status

WebSocket:

- `/ws/session/{sessionId}` — console, load-complete, and error events

## MCP tools

```text
browse_create_session
browse_close_session
browse_close_all_sessions
browse_list_sessions
browse_healthcheck
browse_navigate
browse_click
browse_type
browse_get_html
browse_get_screenshot
browse_execute_js
browse_get_metadata
browse_get_prices
browse_get_images
browse_get_forms
browse_get_links
browse_get_invoice
browse_get_console_logs
browse_get_page_errors
browse_clickable_elements
browse_fill_by_label
browse_visual_diff
browse_click_safe
```

MCP logs go to stderr. stdout contains only MCP protocol frames.

## Configuration

- `AGENTBROWSER_API_KEY` — enable API-key authentication.
- `AGENTBROWSER_ALLOWED_HOSTS` — comma-separated allowlist for navigation.
- `AGENTBROWSER_BLOCKED_SCHEMES` — comma-separated blocklist. Default `file,chrome,about`.
- `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS` — `true` to block private IP addresses.
- `AGENTBROWSER_CORS_ORIGINS` — comma-separated origins. CORS is denied by default.
- `AGENTBROWSER_MAX_REQUEST_BYTES` — maximum HTTP request size, default 1 MB.
- `AGENTBROWSER_PERSISTENCE` — set `true` to enable SQLite operation logs.
- `AGENTBROWSER_SQLITE_PATH` — override the operation database path.
- `AGENTBROWSER_BROWSER_CACHE` — override the Chromium cache path.

Read [SECURITY.md](SECURITY.md) for the full threat model.

## Packaging

See [packaging/Dockerfile](packaging/Dockerfile), [packaging/agentbrowser.service](packaging/agentbrowser.service), and [packaging/publish.ps1](packaging/publish.ps1).

A PowerShell helper is included at [scripts/agentbrowser.ps1](scripts/agentbrowser.ps1). Example:

```powershell
pwsh ./scripts/agentbrowser.ps1 -Command session-create
```

## Verification

Recorded runs are in [docs/VERIFICATION.md](docs/VERIFICATION.md).
