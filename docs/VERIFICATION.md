# Verification

> This record tracks the checks required before the working MVP branch is considered verified.

## Checklist

- [ ] `dotnet build AgentBrowser.sln -warnaserror` succeeds with 0 warnings and 0 errors.
- [ ] `dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build` succeeds.
- [ ] `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --filter "Category!=Browser&Category!=Mcp"` succeeds.
- [ ] `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --filter "Category=Browser"` succeeds.
- [ ] `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --filter "Category=Mcp"` succeeds.
- [ ] Every documented HTTP route exists in `src/AgentBrowser.Api/Controllers`.
- [ ] Every documented MCP tool exists in `src/AgentBrowser.MCP/Tools/BrowserTools.cs`.
- [ ] Unsupported features are listed explicitly in README and SPEC.

## Latest run

Date: 2026-11-04

Results:

- `dotnet build AgentBrowser.sln --no-restore --nologo -warnaserror`: 0 warnings, 0 errors.
- `dotnet test tests/AgentBrowser.Tests.Unit/AgentBrowser.Tests.Unit.csproj --no-build --nologo`: 43 passed.
- `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category!=Browser&Category!=Mcp"`: 19 passed.
- `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Browser"`: 5 passed.
- `dotnet test tests/AgentBrowser.Tests.Integration/AgentBrowser.Tests.Integration.csproj --no-build --nologo --filter "Category=Mcp"`: 1 passed.

