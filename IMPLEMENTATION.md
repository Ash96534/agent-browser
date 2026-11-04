# AgentBrowser Implementation Map

> Describes what is implemented and verified, not aspirations.

See [docs/VERIFICATION.md](docs/VERIFICATION.md) for the latest recorded verification run.

## Source layout

```text
src/
  AgentBrowser.Api/
    Controllers/       HTTP endpoints
    Middleware/        Exception handling, correlation, auth, size cap, API version
    Persistence/       Optional SQLite operation audit log
    WebSockets/        Connection and event forwarding
  AgentBrowser.Browser/
    Browser/           BrowserManager, engine seam, page operations
  AgentBrowser.Core/
    Extraction/        Scripts and typed extraction service
    Services/          Service adapters
    Extensions/        DI registration
  AgentBrowser.MCP/
    Tools/             MCP tools
  AgentBrowser.Shared/
    Exceptions/        Error hierarchy
    Models/            Requests, responses, models, extraction records
```

## What is implemented

- Browser lifecycle: create, get, list, close.
- Navigation, rendered HTML, body text, and Markdown.
- Click, type, wait, JavaScript execution.
- Screenshots with format, quality, and byte limits.
- Extraction of metadata, prices, images, forms, links, invoice.
- Clickable elements, label-based form fill, click confirmation.
- Console log and page error buffering.
- Session health and browser recycle endpoint.
- SQLite operation audit log (optional).

## Tests

- Unit tests: models, safety policy, services, controllers, MCP tools, telemetry payloads, operation store.
- Integration tests: API contract with faked browser manager, real browser extraction via local data URLs, WebSocket ping/pong, MCP stdio JSON-RPC.

## Known good commands

```bash
dotnet build AgentBrowser.sln --nologo -warnaserror
dotnet test AgentBrowser.sln --no-build --nologo
```

## Known gaps

- Browser pool and automatic recovery on crash.
- Production Docker release.
- License selection.
