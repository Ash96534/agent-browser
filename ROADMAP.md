# AgentBrowser Roadmap

> Status reflects the latest verified branch, not the original ambition.

## Current: Working MVP

### Implemented and verified

- [x] Chromium session lifecycle
- [x] Single-tab navigation, HTML, and body text
- [x] Click, type, wait, and JavaScript execution
- [x] Viewport, full-page, and element screenshots
- [x] Extraction of metadata, prices, images, forms, and links
- [x] WebSocket console, load-complete, and page-error events
- [x] REST API with consistent error mapping
- [x] MCP server over stdio using the official C# SDK
- [x] Unit and integration test coverage

### Explicitly out of scope for the MVP

- [ ] Multi-tab support
- [ ] Network interception, request modification, mocking, or blocking
- [ ] DOM mutation events
- [ ] Session persistence
- [ ] API authentication, authorization, and rate limiting
- [ ] Multi-instance scaling
- [ ] Docker and PDF generation
- [ ] License selection

## Next priorities

1. GitHub Actions CI for Windows: build, unit tests, integration tests, browser tests, and MCP test.
2. Open source readiness: choose a license and make public claims consistent.
2. Observability: structured logging, request logging, and a resource monitoring endpoint.
3. Safety: CORS policy, input limits, request size limits, and defensive error responses.
4. Testing: browser-backed end-to-end tests for API, WebSocket, and MCP tools.
5. Performance: benchmark navigation and extraction before making performance claims.
6. Docker: reproducible image with the verified Chromium cache path.

## Backlog

- Multi-tab support.
- Network interception and blocking.
- DOM mutation events.
- Session persistence with SQLite.
- API keys and rate limiting.
- OpenAPI documentation and examples.
- Local browser cache eviction and monitoring.

## Notes

The original plan described a broader vision. That vision is not implemented, and this roadmap is not a commitment to build it in any particular order.
