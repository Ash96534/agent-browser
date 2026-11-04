# Architecture Decisions

> This file records decisions that have been made and tested. Unanswered items are open, not done.

## Language and runtime

**Decision:** C# 13 on .NET 10 (`net10.0`).

**Rationale:** Native async, strong typing, PuppeteerSharp 19.0.0 pinned.

## Browser engine

**Decision:** Chromium via PuppeteerSharp.

**Rationale:** Full CDP coverage, stable Windows support, good for deterministic extraction tests.

**Current status:** Implemented and verified.

## Browser cache

**Decision:** Chromium downloads to a cache directory, overridable with `AGENTBROWSER_BROWSER_CACHE`.

**Rationale:** No secrets or binaries committed to the repository.

**Current status:** Implemented.

## API architecture

**Decision:** REST for commands, WebSocket for events.

**Rationale:** Language-agnostic and easy to debug.

**Current status:** Implemented and verified.

## MCP server

**Decision:** Official `ModelContextProtocol` C# SDK, stdio transport.

**Rationale:** Standard initialize/tools/list/tools/call handshake; logs go to stderr.

**Current status:** Implemented and verified.

## Session management

**Decision:** One active page per session, in memory only.

**Rationale:** Simple, safe MVP scope.

**Current status:** Implemented and verified.

## Session health and restart

**Decision:** Add `GET /session/{id}/health` and `POST /session/{id}/restart`.

**Rationale:** Long-running agents need to know whether Chromium is alive and recycle it when it crashes.

**Current status:** Implemented and verified.

## Error contracts

**Decision:** `ErrorKind`, `TraceId`, `Error`, `Message`, `Retryable`.

**Current status:** Implemented; responses use consistent shape across API and MCP.

## Persistence

**Decision:** Optional SQLite operation audit log, off by default.

**Rationale:** Some consumers want an audit trail; most do not. Payloads are never persisted.

**Implementation:** Set `AGENTBROWSER_PERSISTENCE=true` and optionally `AGENTBROWSER_SQLITE_PATH`.

## Safety policy

**Decision:**

- `file:`, `chrome:` and `about:` URLs are blocked by default.
- Allowlist navigation with `AGENTBROWSER_ALLOWED_HOSTS`.
- Block private networks with `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS=true`.

**Implementation status:** Implemented.

## Authentication

**Decision:** Optional API key. CORS denied by default.

**Implementation status:** Implemented via `AGENTBROWSER_API_KEY` and `AGENTBROWSER_CORS_ORIGINS`.

## Rate limits

**Decision:** HTTP request size limit, no per-IP rate limiting yet.

## API versioning

**Decision:** `X-Api-Version: 1.0.0`, `schemaVersion: 1.0` in capabilities.

## Access control

**Decision:** None beyond API key. No roles or teams.

## Packaging

**Decision:** Dockerfile, systemd unit, and PowerShell publish script included. No official package manager release yet.

## Open decisions

- Browser pool and crash recovery.
- License selection.
- Multi-tab support.
- Network interception.
