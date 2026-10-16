# AgentBrowser

> A fully exposed, agent-centric web browser for autonomous web agents

AgentBrowser gives AI agents complete, fine-grained control over browser behavior with capabilities beyond traditional automation tools — including access to hidden metadata, structured data extraction, and real-time DOM observation.

## Features

- **Full Browser Control** — Navigate, click, type, scroll, execute JavaScript
- **Extended Data Extraction** — Access hidden metadata (OG tags, JSON-LD, data-* attributes, prices, image context)
- **Real-time Events** — WebSocket streaming for console logs, network traffic, DOM updates
- **MCP Integration** — Native Model Context Protocol support for LLM agents
- **High Performance** — Built on C# (.NET 8) + PuppeteerSharp for 5-10x better throughput than Python alternatives

## Quick Start

```bash
# Clone and build
dotnet build

# Run the server
dotnet run --project src/AgentBrowser.Api
```

## API

### Create Session
```bash
POST /session
```

### Navigate
```bash
POST /session/{id}/navigate
{ "url": "https://example.com" }
```

### Get Screenshot
```bash
GET /session/{id}/screenshot
```

### Extract Hidden Metadata
```bash
GET /session/{id}/extract/metadata
```

### Get Prices
```bash
GET /session/{id}/extract/prices
```

## Architecture

See [SPEC.md](SPEC.md) for detailed architecture and [DECISIONS.md](DECISIONS.md) for design decisions.

## Current Status

**Phase 1 (MVP) - Browser Engine Layer - COMPLETE ✅**

The core browser engine layer is fully implemented with:

- **Browser Engine (`AgentBrowser.Browser`)**
  - `IBrowserManager` interface with full API surface
  - `BrowserManager` implementation using PuppeteerSharp
  - Session management (create, close, list, get by ID)
  - Navigation, screenshot, DOM operations

- **Core Services (`AgentBrowser.Core`)**
  - `SessionService` - Session CRUD operations
  - `NavigationService` - URL navigation and HTML retrieval
  - `DomService` - DOM manipulation (click, type, JS execution)
  - `ScreenshotService` - Screenshot capture

- **Extensions**
  - `TabExtensions` - Extension methods for browser operations

**Projects:**
```
src/
  AgentBrowser.Api/        # REST API (ready for controllers)
  AgentBrowser.Core/       # Business logic (COMPLETED)
  AgentBrowser.Browser/    # Browser engine (COMPLETED)
  AgentBrowser.MCP/        # MCP server (ready)
  AgentBrowser.Shared/     # Domain models
tests/
  AgentBrowser.Tests.Unit/
  AgentBrowser.Tests.Integration/
```

**GitHub:** [https://github.com/Ash96534/agent-browser](https://github.com/Ash96534/agent-browser)

## Development Progress

### Completed
- ✅ .NET 8 solution structure
- ✅ PuppeteerSharp integration
- ✅ Domain models (Session, Tab, Viewport, Cookie, etc.)
- ✅ Request/Response models (NavigateRequest, ClickRequest, etc.)
- ✅ Browser engine layer (IBrowserManager, BrowserManager)
- ✅ Core services (Session, Navigation, DOM, Screenshot)
- ✅ Service dependency injection setup
- ✅ GitHub repository setup

### In Progress
- 🔄 API controllers and REST endpoints
- 🔄 WebSocket support for real-time events

### Next Steps
1. Create API controllers (SessionController, NavigationController, etc.)
2. Implement WebSocket handlers for real-time streaming
3. Add MCP server implementation
4. Build and test the complete system

See [IMPLEMENTATION.md](IMPLEMENTATION.md) for the detailed task list.

## License

MIT