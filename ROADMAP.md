# AgentBrowser Roadmap

> Reflects the latest verified branch. This is not an aspirational wishlist.

## Done

- [x] Chromium session lifecycle without multi-tab support.
- [x] Navigation, rendered HTML, body text, and Markdown.
- [x] Click, type, wait for selector, JavaScript execution.
- [x] Viewport, full-page, and element screenshots.
- [x] Hidden data extraction: metadata, prices, images, forms, links, invoice.
- [x] Browser telemetry: console, load-complete, page-error.
- [x] Session health and browser recycle endpoint.
- [x] WebSocket event streaming.
- [x] MCP stdio server via the official SDK.
- [x] API diagnostics, capabilities manifest, correlation IDs, request size limits.
- [x] Optional API-key authentication and CORS allowlist.
- [x] Host/scheme safety policy for navigation.
- [x] Optional SQLite operation audit log.
- [x] Security, contributing, and packaging files.

## Not started / intentionally deferred

- [ ] Network interception, request modification, or blocking.
- [ ] Multi-tab support.
- [ ] DOM mutation events.
- [ ] Production Docker release.
- [ ] Multi-instance coordination.
- [ ] License selection.

## Next

1. Browser pool and crash recovery.
2. Structured output schema for every endpoint and MCP tool.
3. Contract tests for the documented routes and MCP tools.
