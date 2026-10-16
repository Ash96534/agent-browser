# Architecture Decisions

## Language & Framework

**Decision:** Use C# (.NET 8) with PuppeteerSharp for browser automation

**Rationale:**
- 5-10x better throughput than Python
- Native performance for high-frequency agent calls
- Strong async/await support for concurrent operations
- Good CDP wrapper ecosystem (PuppeteerSharp)
- Faster development compared to C++ while still being compiled

**Date:** 2025-10-16

---

## Browser Engine

**Decision:** Chromium via PuppeteerSharp (CDP)

**Rationale:**
- Standards-compliant rendering
- Mature CDP protocol with full browser internals access
- Headless and headed modes supported
- Good debugging tools
- Active maintenance

**Alternatives considered:**
- CEF (too low-level, more complex)
- Firefox via Marionette (less mature CDP support)
- WebView2 (Windows-only, less automation-focused)

---

## API Architecture

**Decision:** REST API with WebSocket for real-time events

**Rationale:**
- REST for stateless operations (navigate, extract, query)
- WebSocket for streaming (console logs, network events, DOM updates)
- Simple to integrate with any agent language

**Alternatives considered:**
- gRPC — more complex, overkill for this use case
- Pure WebSocket — harder to debug/test

---

## Session Management

**Decision:** In-memory sessions with optional SQLite persistence

**Rationale:**
- Fast for single-instance deployments
- SQLite option for multi-instance/scaling
- Simple state model per session

---

## Project Structure

**Decision:** Monorepo with clear layer separation

```
src/
  AgentBrowser.Api/      # HTTP/WebSocket server
  AgentBrowser.Core/     # Business logic, extraction
  AgentBrowser.Puppeteer/ # CDP wrapper
  AgentBrowser.MCP/      # MCP server
  AgentBrowser.Shared/   # Shared models
```

**Rationale:**
- Clear separation of concerns
- Testable layers
- Can be packaged as libraries if needed

---

## Next Major Decision Points

1. MCP server transport (stdio vs HTTP)
2. Authentication approach for agent sessions
3. Multi-instance scaling strategy
4. CI/CD pipeline