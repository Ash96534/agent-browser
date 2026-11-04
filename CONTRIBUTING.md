# Contributing

## Build and test

```bash
dotnet build AgentBrowser.sln --nologo -warnaserror
dotnet test AgentBrowser.sln --no-build --nologo
```

Browser tests use local `data:` URLs and require Chromium. They are marked with `Category=Browser`.

## What to contribute

Good first contributions:

- Extraction tests for new data shapes
- Documentation updates for behavior changes
- New MCP tools for an existing verified capability
- Clear, actionable error messages

## What not to do

- Do not add unverified features that call external services.
- Do not claim support for multi-tab, network interception, persistence, or authentication unless they are actually implemented.
- Do not commit Chromium binaries, API keys, cookies, `.agentbrowser-cache`, or SQLite operation logs.

## Style

- .NET 10 project targeting `net10.0`.
- Nullable reference types enabled.
- XML documentation for new public APIs.
- Small, focused classes and services.
- Machine-readable errors with `ErrorKind`, `TraceId`, and `Retryable`.

## Verification

- Update `docs/VERIFICATION.md` when behavior changes.
- Include the exact command and result count in the commit message.
- Example: `Verification: build 0 warnings, 0 errors; 48 unit and 27 integration tests passed.`

## Proposing changes

Open a pull request describing the verified behavior. If this repository has branch protection requiring review, a reviewer must approve before merge.

## License

This project does not yet have a `LICENSE` file. Do not publish copies until a license is selected.
