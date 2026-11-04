# AgentBrowser Specification

> Local browser control for authorized AI agents.

## Purpose

AgentBrowser exposes a controlled Chromium page through a REST API, a WebSocket event stream, and an MCP stdio server. It is intended for authorized local automation, testing, evidence capture, information extraction, and agent-guided navigation.

## Baseline contract

- .NET 10, PuppeteerSharp 19.0.0, ASP.NET Core, official MCP C# SDK.
- One active page per session.
- API key is optional. Localhost use is intended; no public exposure.
- Chromium is downloaded once per platform into a configured cache directory.

## HTTP API

### Session management

- `POST /session`
- `GET /session`
- `GET /session/{id}`
- `DELETE /session/{id}`

### Navigation

- `POST /session/{sessionId}/navigate`
- `GET /session/{sessionId}/html`
- `GET /session/{sessionId}/content?format=text|html|markdown`

### DOM and JavaScript

- `POST /session/{sessionId}/dom/click`
- `POST /session/{sessionId}/dom/type`
- `POST /session/{sessionId}/dom/execute`
- `GET /session/{sessionId}/dom/wait?selector=...&timeout=...`
- `POST /session/{sessionId}/dom/fill` — fills fields by visible label text

### Screenshots

- `GET /session/{sessionId}/screenshot?type=viewport|fullpage|element&format=png|jpeg|webp&quality=...&maxBytes=...&selector=...`
- `GET /session/{sessionId}/screenshot/base64?format=png&maxBytes=...`

### Extraction

- `GET /session/{sessionId}/extract/metadata`
- `GET /session/{sessionId}/extract/prices?max=...`
- `GET /session/{sessionId}/extract/images?max=...`
- `GET /session/{sessionId}/extract/forms?max=...`
- `GET /session/{sessionId}/extract/links?max=...&sameOrigin=...`
- `GET /session/{sessionId}/extract/invoice`

### Browser telemetry and health

- `GET /session/{sessionId}/console`
- `GET /session/{sessionId}/errors`
- `GET /session/{sessionId}/health`
- `POST /session/{sessionId}/restart`

### Advanced

- `GET /session/{sessionId}/clickable-elements`
- `GET /session/{sessionId}/visual-diff`
- `POST /session/{sessionId}/click-safe` — requires explicit `confirm=true`; purchase/payment labels are refused

### Meta

- `GET /health`
- `GET /capabilities`
- `GET /diagnostics`

## WebSocket events

`/ws/session/{sessionId}`

Supported event types:

- `console`
- `load-complete`
- `error`

DOM mutation and network events are not implemented.

## MCP tools

`POST /session` is used only as the browser creation entry point. MCP does not start a new session for every action; sessions must be created with `browse_create_session` and reused.

## Error contract

Every non-2xx response uses:

```json
{
  "kind": "SessionNotFound",
  "error": "Session 'x' not found",
  "message": "...",
  "traceId": "f...",
  "retryable": false
}
```

Known kinds:

`InvalidRequest`, `Configuration`, `SessionNotFound`, `NavigationFailure`, `SelectorNotFound`, `ScriptFailure`, `Timeout`, `ScreenshotFailure`, `BrowserLaunchFailure`, `BrowserCrashed`, `RequestTooLarge`, `Unauthorized`.

## Optional SQLite audit log

Enable: `AGENTBROWSER_PERSISTENCE=true`

Table: `operations(method, path, session_id, status_code, duration_ms, trace_id)`

Payload bodies are not persisted.

## Configuration

See README for `AGENTBROWSER_API_KEY`, `AGENTBROWSER_ALLOWED_HOSTS`, `AGENTBROWSER_BLOCKED_SCHEMES`, `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS`, `AGENTBROWSER_CORS_ORIGINS`, `AGENTBROWSER_MAX_REQUEST_BYTES`, `AGENTBROWSER_PERSISTENCE`, `AGENTBROWSER_SQLITE_PATH`, and `AGENTBROWSER_BROWSER_CACHE`.
