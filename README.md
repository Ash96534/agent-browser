# AgentBrowser

> A fully exposed, agent-centric web browser for autonomous AI agents - built for speed, visibility, and control.

AgentBrowser gives AI agents complete, fine-grained control over browser behavior with capabilities beyond traditional automation tools. Get access to hidden metadata, structured data extraction, and real-time DOM observation - all powered by C# for 5-10x better performance than Python alternatives.

## Features

- **High Performance** - C# (.NET 8) + PuppeteerSharp, 5-10x faster than Python
- **Full Browser Control** - Navigate, click, type, scroll, execute JavaScript
- **Extended Data Extraction** - Hidden metadata (OG, JSON-LD), prices, image context
- **Real-time Events** - WebSocket streaming for console, network, DOM updates
- **MCP Integration** - Native Model Context Protocol support for LLM agents
- **REST API** - 15+ endpoints for session, navigation, DOM, screenshot, extraction

## Documentation

| Document | Description |
|----------|-------------|
| [SPEC.md](SPEC.md) | Complete technical specification |
| [DECISIONS.md](DECISIONS.md) | Architecture decisions and rationale |
| [ROADMAP.md](ROADMAP.md) | Development roadmap and progress |
| [IMPLEMENTATION.md](IMPLEMENTATION.md) | Detailed task breakdown |

## Quick Start

```bash
# Clone the repository
git clone https://github.com/Ash96534/agent-browser.git
cd agent-browser

# Build the solution
dotnet build

# Run the API server
dotnet run --project src/AgentBrowser.Api

# Run the MCP server
dotnet run --project src/AgentBrowser.MCP
```

## API Endpoints

### Session Management
```
POST   /session              - Create new browser session
GET    /session/{id}         - Get session state
DELETE /session/{id}         - Close session
GET    /session              - List all active sessions
```

### Navigation
```
POST   /session/{id}/navigate   - Navigate to URL
GET    /session/{id}/html       - Get rendered HTML
GET    /session/{id}/content    - Get page content
```

### DOM Operations
```
POST   /session/{id}/dom/click      - Click element
POST   /session/{id}/dom/type       - Type text
POST   /session/{id}/dom/execute    - Execute JavaScript
GET    /session/{id}/dom/wait       - Wait for selector
```

### Screenshot
```
GET    /session/{id}/screenshot        - Capture screenshot
GET    /session/{id}/screenshot/base64 - Capture as base64
```

### Extraction (Hidden Data)
```
GET    /session/{id}/extract/metadata - Extract metadata (OG, JSON-LD)
GET    /session/{id}/extract/prices   - Extract prices
GET    /session/{id}/extract/images   - Extract images with context
GET    /session/{id}/extract/forms    - Extract forms
GET    /session/{id}/extract/links    - Extract links
```

### Health Check
```
GET    /health - Health check
```

## MCP Tools

AgentBrowser exposes 11 MCP tools for LLM agent integration:

| Tool | Description |
|------|-------------|
| `browse_navigate` | Navigate to URL |
| `browse_click` | Click element by selector |
| `browse_type` | Type text into element |
| `browse_get_html` | Get rendered HTML |
| `browse_get_screenshot` | Get base64 screenshot |
| `browse_execute_js` | Execute JavaScript |
| `browse_extract_schema` | Extract data by schema |
| `browse_get_metadata` | Extract metadata (OG, JSON-LD) |
| `browse_get_prices` | Extract prices |
| `browse_get_images` | Extract images with context |
| `browse_get_forms` | Extract forms |

## Architecture

```
+-------------------------------------------------------------+
|                     AgentBrowser                            |
+-------------------------------------------------------------+
|  +-----------+   +-----------+   +------------------+      |
|  | Agent API |   | WebSocket |   |   MCP Server     |      |
|  |(REST/HTTP)|   | Real-time |   |(LLM Integration) |      |
|  +-----+-----+   +-----+-----+   +--------+---------+      |
|        |               |                  |                |
|  +-----v----------------v------------------v----------+    |
|  |              Control Plane (Orchestrator)          |    |
|  |  - Session management    - Command routing         |    |
|  +--------------------------+-------------------------+    |
|                             |                              |
|  +--------------------------v--------------------------+    |
|  |              Browser Engine (Chromium via CDP)     |    |
|  |  - Rendering         - Network interception        |    |
|  |  - JavaScript exec   - DOM access                  |    |
|  +----------------------------------------------------+    |
+-------------------------------------------------------------+
```

## Project Structure

```
agent-browser/
+-- src/
|   +-- AgentBrowser.Api/          # REST API controllers
|   |   +-- Controllers/
|   |   |   +-- SessionController.cs
|   |   |   +-- NavigationController.cs
|   |   |   +-- DomController.cs
|   |   |   +-- ScreenshotController.cs
|   |   |   +-- ExtractionController.cs
|   |   +-- WebSockets/            # WebSocket handlers
|   +-- AgentBrowser.Core/         # Business logic
|   |   +-- Services/              # Core services
|   |   +-- Extraction/            # Extraction service
|   |   +-- Extensions/
|   +-- AgentBrowser.Browser/      # Browser engine
|   |   +-- Browser/
|   |   |   +-- IBrowserManager.cs
|   |   |   +-- BrowserManager.cs
|   |   +-- Extensions/
|   +-- AgentBrowser.MCP/          # MCP server
|   |   +-- McpServer.cs
|   |   +-- Program.cs
|   +-- AgentBrowser.Shared/       # Shared models
|       +-- Models/
+-- tests/
|   +-- AgentBrowser.Tests.Unit/
|   +-- AgentBrowser.Tests.Integration/
+-- docker/
|   +-- Dockerfile
+-- AgentBrowser.sln
+-- README.md
+-- SPEC.md
+-- DECISIONS.md
+-- ROADMAP.md
+-- IMPLEMENTATION.md
```

## Technology Stack

| Layer | Technology | Rationale |
|-------|------------|-----------|
| Language | C# (.NET 8) | 5-10x faster than Python |
| Browser | Chromium via PuppeteerSharp | Standards-compliant, CDP access |
| API Server | ASP.NET Core | High-performance HTTP |
| MCP Server | Custom implementation | Native LLM integration |
| WebSocket | Built-in | Real-time event streaming |
| DI Container | Microsoft.Extensions | Standard .NET DI |

## Current Status

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1 | Complete | Browser engine layer, core services |
| Phase 2 | Complete | 15 REST endpoints + WebSocket |
| Phase 3 | Complete | Extraction service, 5 endpoints |
| Phase 4 | Complete | MCP server, 11 tools |
| Phase 5 | In Progress | Production readiness features |

### What's Working
- Full session management via REST API
- Navigation with wait states
- DOM operations (click, type, JS execution)
- Screenshot capture
- 5 extraction endpoints for hidden data
- WebSocket infrastructure with event types
- MCP server with 11 tools

### What's Next
- Console log streaming (Puppeteer console hook)
- Network request capture (request interception)
- Unit and integration tests
- API authentication (API key)
- Session persistence with SQLite

## Development

### Build
```bash
dotnet build
```

### Run API Server
```bash
dotnet run --project src/AgentBrowser.Api
```

API will be available at: `https://localhost:5000`

### Run MCP Server
```bash
dotnet run --project src/AgentBrowser.MCP
```

MCP server uses stdio transport for LLM integration.

### Run Tests
```bash
dotnet test
```

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m "feat: Add some feature"`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built for agents, by developers who understand what agents need.**
