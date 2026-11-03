# AgentBrowser

> A local browser-control daemon for AI agents, backed by Chromium and PuppeteerSharp.

AgentBrowser exposes a controlled Chromium instance to agents through a REST API, a WebSocket event stream, and an MCP server over stdio. It is a single-tab MVP: one Chromium browser per session and one active page per session.

## Current status

| Area | Status |
|------|--------|
| Session lifecycle | Implemented and verified |
| Navigation and page content | Implemented and verified |
| DOM click, type, wait, JavaScript | Implemented and verified |
| Screenshots (viewport, full page, element) | Implemented and verified |
| Hidden metadata, prices, images, forms, links | Implemented and verified |
| WebSocket console, load-complete, page-error | Implemented and verified |
| MCP over stdio | Implemented and verified |

## What is supported

- One Chromium browser per session; exactly one active page per session.
- Session create, get, list, and close.
- Navigate, get rendered HTML, and get visible body text.
- Click, type, execute JavaScript, and wait for a selector.
- Capture viewport, full-page, or element screenshots as PNG or JPEG.
- Extract metadata, prices, images, forms, and links.
- Receive console, page load, and page error events on `/ws/session/{sessionId}`.
- Control the browser from an MCP client using standard initialize, tools/list, and tools/call.

## What is not supported yet

- Multiple tabs per session.
- Network interception, request modification, mocking, or blocking.
- DOM mutation events.
- Session persistence, authentication, authorization, or rate limiting.
- Multi-instance scaling and distributed state.
- Docker, PDF generation, and image caching beyond PuppeteerSharp.

## Requirements

- .NET 10 SDK 10.0.301 or later.
- Windows is the verified platform for this branch.
- Chromium, downloaded automatically on first use.
- Set `AGENTBROWSER_BROWSER_CACHE` to override the Chromium cache location.

## Quick start

```bash
git clone https://github.com/Ash96534/agent-browser.git
cd agent-browser

dotnet restore AgentBrowser.sln
dotnet build AgentBrowser.sln --nologo -warnaserror
dotnet test AgentBrowser.sln --no-build --nologo

dotnet run --project src/AgentBrowser.Api
```

The API listens on the URL configured by ASP.NET Core. Health check:

```bash
curl http://localhost:5000/health
```

## HTTP endpoints

### Sessions

```text
POST   /session
GET    /session
GET    /session/{id}
DELETE /session/{id}
```

### Navigation

```text
POST   /session/{sessionId}/navigate
GET    /session/{sessionId}/html
GET    /session/{sessionId}/content
```

### DOM and JavaScript

```text
POST   /session/{sessionId}/dom/click
POST   /session/{sessionId}/dom/type
POST   /session/{sessionId}/dom/execute
GET    /session/{sessionId}/dom/wait?selector=...&timeout=30000
```

### Screenshots

```text
GET    /session/{sessionId}/screenshot?type=viewport|fullpage|element&format=png|jpeg&quality=80
GET    /session/{sessionId}/screenshot/base64?type=viewport
```

### Extraction

```text
GET    /session/{sessionId}/extract/metadata
GET    /session/{sessionId}/extract/prices
GET    /session/{sessionId}/extract/images
GET    /session/{sessionId}/extract/forms
GET    /session/{sessionId}/extract/links
```

### Health

```text
GET    /health
```

### WebSocket

```text
GET    /ws/session/{sessionId}
```

WebSocket messages use a `type` field and a `sessionId`. Supported event types are `console`, `load-complete`, and `error`.

## MCP tools

The MCP server uses stdio and standard MCP initialize, tools/list, and tools/call.

```text
browse_create_session
browse_close_session
browse_navigate
browse_click
browse_type
browse_get_html
browse_get_screenshot
browse_execute_js
browse_get_metadata
browse_get_prices
browse_get_images
browse_get_forms
browse_get_links
```

Run it with:

```bash
dotnet run --project src/AgentBrowser.MCP
```

MCP logs are written to stderr. Stdout contains only MCP protocol frames.

## Architecture

```text
Agent / application
        |
        | HTTP, WebSocket, or MCP
        v
ASP.NET Core API / MCP server
        |
        | Core services
        v
Browser layer
        |
        | PuppeteerSharp / CDP
        v
Chromium
```

The layers are:

- `AgentBrowser.Shared`: models, responses, requests, and exceptions.
- `AgentBrowser.Browser`: Chromium startup and page operations.
- `AgentBrowser.Core`: service adapters and extraction logic.
- `AgentBrowser.Api`: REST controllers, middleware, and WebSocket streaming.
- `AgentBrowser.MCP`: MCP server based on the official C# SDK.

## Project structure

```text
AgentBrowser.sln
global.json
src/
  AgentBrowser.Api/
    Controllers/
    Middleware/
    WebSockets/
  AgentBrowser.Browser/
    Browser/
  AgentBrowser.Core/
    Extraction/
    Services/
    Extensions/
  AgentBrowser.MCP/
    Tools/
  AgentBrowser.Shared/
    Exceptions/
    Models/
tests/
  AgentBrowser.Tests.Unit/
  AgentBrowser.Tests.Integration/
docs/
  VERIFICATION.md
  superpowers/plans/
```

## Development

```bash
dotnet build AgentBrowser.sln --nologo -warnaserror
dotnet test AgentBrowser.sln --no-build --nologo
dotnet test tests/AgentBrowser.Tests.Integration --no-build --filter "Category=Browser"
dotnet test tests/AgentBrowser.Tests.Integration --no-build --filter "Category=Mcp"
```

Browser tests use local `data:` URLs and do not access the public network. They do need a downloaded Chromium binary; PuppeteerSharp downloads it on first use.

## Development workflow

No merge PR is required for verified work in this repository.

- Run the build and tests before pushing.
- Push directly to `master` after verification.
- Commit messages must describe the intent, key changes, and verification results.
- Documentation must be updated in the same commit as behavior changes.
- See [AGENTS.md](AGENTS.md) for the project rules.

## Documentation

- [SPEC.md](SPEC.md): current supported specification and known limitations.
- [IMPLEMENTATION.md](IMPLEMENTATION.md): implementation map and verification status.
- [ROADMAP.md](ROADMAP.md): current and planned work.
- [DECISIONS.md](DECISIONS.md): architecture decisions.
- [docs/VERIFICATION.md](docs/VERIFICATION.md): recorded verification runs.

## License

License: undecided. This repository does not currently contain a `LICENSE` file.
