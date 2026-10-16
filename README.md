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

## License

MIT