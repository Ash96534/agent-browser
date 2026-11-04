# Changelog

## Unreleased — working MVP

### Added

- Single-page Chromium sessions for AI agents.
- REST endpoints for sessions, navigation, HTML/content, DOM operations, JavaScript, screenshots, and hidden-data extraction.
- MCP stdio server using the official C# SDK.
- Browser events: console, load-complete, page-error.
- `/diagnostics` and `/capabilities` for runtime health and API feature manifests.
- Correlation IDs and API version headers.
- Optional API-key authentication and CORS allow-list.
- Request size limits.
- Exact error contracts: `ErrorKind`, `TraceId`, `Retryable`.
- SQLite operation audit log, opt-in via `AGENTBROWSER_PERSISTENCE=true`.
- Optional `AGENTBROWSER_ALLOWED_HOSTS`, `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS`, and `AGENTBROWSER_BLOCKED_SCHEMES`.
- Markdown content retrieval and link/image/form/price limits.
- Clickable element discovery, label-based form fill, confirm-gated safe clicks, and invoice extraction.
- Screenshot maximum-byte enforcement and image metadata in the response.
- Browser smoke tests using local `data:` URLs.

### Supported

- Windows, .NET 10, PuppeteerSharp 19.0.0.

### Not supported

- Multi-tab sessions.
- Network interception, request modification, mocking, or blocking.
- DOM mutation events.
- Persistent sessions across process restarts.
- Multi-instance coordination.
- Production Docker deployment or service management.
