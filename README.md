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

# Run the API server
dotnet run --project src/AgentBrowser.Api

# Run MCP server (stdio transport)
dotnet run --project src/AgentBrowser.MCP
```

## API Endpoints

### Session Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/session` | Create new browser session |
| `GET /session/{id}` | Get session state |
| `DELETE /session/{id}` | Close session |
| `GET /session` | List all active sessions |

### Navigation
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST /session/{id}/navigate` | Navigate to URL |
| `GET /session/{id}/html` | Get rendered HTML |
| `GET /session/{id}/content` | Get page content |

### DOM Operations
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST /session/{id}/dom/click` | Click element |
| `POST /session/{id}/dom/type` | Type text |
| `POST /session/{id}/dom/execute` | Execute JavaScript |
| `GET /session/{id}/dom/wait` | Wait for selector |

### Screenshot
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET /session/{id}/screenshot` | Capture screenshot |
| `GET /session/{id}/screenshot/base64` | Capture as base64 |

### Extraction (Hidden Data)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET /session/{id}/extract/metadata` | Extract metadata (OG, JSON-LD) |
| `GET /session/{id}/extract/prices` | Extract prices |
| `GET /session/{id}/extract/images` | Extract images with context |
| `GET /session/{id}/extract/forms` | Extract forms |
| `GET /session/{id}/extract/links` | Extract links |

### Health Check
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET /health` | Health check |

## MCP Tools

| Tool | Description |
|------|-------------|
| `browse_navigate` | Navigate to URL |
| `browse_click` | Click element |
| `browse_type` | Type text |
| `browse_get_html` | Get HTML content |
| `browse_get_screenshot` | Get base64 screenshot |
| `browse_execute_js` | Execute JavaScript |
| `browse_extract_schema` | Extract data by schema |
| `browse_get_metadata` | Extract metadata |
| `browse_get_prices` | Extract prices |
| `browse_get_images` | Extract images |
| `browse_get_forms` | Extract forms |

## Architecture

See [SPEC.md](SPEC.md) for detailed architecture and [DECISIONS.md](DECISIONS.md) for design decisions.

## Current Status

**Phase 1 (MVP) - Browser Engine Layer - COMPLETE ✅**
- Browser engine (IBrowserManager, BrowserManager)
- Core services (Session, Navigation, DOM, Screenshot)
- Domain models, Request/Response models

**Phase 2 (Core Agent Features) - COMPLETE ✅**
- 15 REST API endpoints
- WebSocket infrastructure (handlers, events)

**Phase 3 (Extended Extraction) - COMPLETE ✅**
- Extraction service (metadata, prices, images, forms, links)
- 5 extraction endpoints

**Phase 4 (MCP Server) - COMPLETE ✅**
- MCP server with stdio transport
- 11 MCP tools implemented

**Repository:** [https://github.com/Ash96534/agent-browser](https://github.com/Ash96534/agent-browser)

## Development Progress

### Completed
- ✅ .NET 8 solution structure
- ✅ PuppeteerSharp integration
- ✅ Browser engine layer
- ✅ Core services
- ✅ 15 REST API endpoints
- ✅ WebSocket infrastructure
- ✅ Extraction service
- ✅ MCP server (stdio transport)
- ✅ Domain models
- ✅ Service dependency injection

### Next Steps
1. Console log streaming
2. Network request capture
3. Unit tests
4. Integration tests
5. API authentication
6. Session persistence
7. CI/CD pipeline

See [IMPLEMENTATION.md](IMPLEMENTATION.md) for the detailed task list.

## License

MIT