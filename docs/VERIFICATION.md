# Verification

> This record tracks the checks required before the MVP is considered verified.

## API verification checklist

- [x] Every route in `src/AgentBrowser.Api/Controllers` is covered by an integration test or deliberately verified manually.
- [x] Every documented MCP tool exists in `src/AgentBrowser.MCP/Tools/BrowserTools.cs`.
- [x] Chromium operates end-to-end: navigate, type, click, wait, screenshot, extract.
- [x] Browser tests avoid the public network and use local `data:` URLs.
- [x] `dotnet build AgentBrowser.sln --nologo -warnaserror` succeeds with 0 warnings and 0 errors.
- [x] `dotnet test AgentBrowser.sln --no-build --nologo` passes.

## Latest run

Date: 2026-11-04

- `dotnet restore AgentBrowser.sln --nologo`: passed.
- `dotnet build AgentBrowser.sln --nologo -warnaserror`: 0 warnings, 0 errors.
- `dotnet test AgentBrowser.sln --no-build --nologo`:
  - Unit: 49 passed
  - Integration: 27 passed
- API smoke: health endpoint returned `healthy`; session created; page navigated; extraction succeeded; session closed.
- Health/restart: `GET /session/{id}/health` and `POST /session/{id}/restart` compile and register; unit and integration suites pass.

## Notes

- Chromium is installed on first use into `.agentbrowser-cache` under the host application's output directory.
- Set `AGENTBROWSER_BROWSER_CACHE` to override the default cache path.
- Optional SQLite operation auditing is disabled by default. Enable with `AGENTBROWSER_PERSISTENCE=true`.
- CORS is denied by default. Set `AGENTBROWSER_CORS_ORIGINS` to allow specific origins.
- API key authentication is disabled by default. Set `AGENTBROWSER_API_KEY` to enable it.
