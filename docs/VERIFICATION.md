# Verification

> This record tracks the checks required before the working MVP branch is considered verified.

## Checklist

- [x] `dotnet build AgentBrowser.sln --nologo -warnaserror` succeeds with 0 warnings and 0 errors.
- [x] `dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build` succeeds.
- [x] `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --filter "Category!=Browser&Category!=Mcp"` succeeds.
- [x] `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --filter "Category=Browser"` succeeds.
- [x] `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --filter "Category=Mcp"` succeeds.
- [x] Every documented HTTP route exists in `src/AgentBrowser.Api/Controllers`.
- [x] Every documented MCP tool exists in `src/AgentBrowser.MCP/Tools/BrowserTools.cs`.
- [x] Unsupported features are listed explicitly in README and SPEC.

## Latest run

Date: 2026-11-04

Results:

- `dotnet clean AgentBrowser.sln --nologo`: passed.
- `dotnet restore AgentBrowser.sln --nologo`: passed.
- `dotnet build AgentBrowser.sln --nologo -warnaserror`: 0 warnings, 0 errors.
- `dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build --nologo`: 43 passed.
- `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category!=Browser&Category!=Mcp"`: 19 passed.
- `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Browser"`: 5 passed.
- `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Mcp"`: 1 passed.
- API smoke: health `healthy`; session created; navigate title `Smoke`; price extraction returned 1 price; session closed.

Notes:

- Branch integration: `fix/working-mvp` through `ac968ca` was fast-forwarded into `master` and pushed directly, per the direct-master policy.
- Chromium is installed on first use into `.agentbrowser-cache` under the host application's output directory.
- Set `AGENTBROWSER_BROWSER_CACHE` to override the default cache path.
- The API smoke exposed a PuppeteerSharp path mismatch with a custom `BrowserFetcher` path. The engine now resolves the installed Chrome executable and passes `LaunchOptions.ExecutablePath`.
