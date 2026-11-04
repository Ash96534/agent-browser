# Security

AgentBrowser is a local browser-control service. It is not a remote browser farm and must not be treated as a public-facing service.

## Threat model

### In scope

- Local automation by authorized agents and developers.
- A single local user or trusted local processes.
- Network clients capable of invoking your API.
- Machine or developer misuse: accidental navigation to private resources, file access, or external data leaks.

### Out of scope

- Multi-user SaaS deployments.
- Internet-exposed hosted APIs.
- CAPTCHA bypass, credential theft, account takeover, or any unauthorized access.
- Compliance with target site terms of service where you have no authorization.

## Operational guidance

- Keep AgentBrowser bound to localhost unless you have a concrete reason to expose it.
- Set `AGENTBROWSER_API_KEY` before sharing the API beyond your machine.
- Use `AGENTBROWSER_ALLOWED_HOSTS` and `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS=true` where appropriate.
- Treat browser sessions and their Chromium cache as sensitive user data.
- Cap concurrent and session counts. Close sessions you no longer use.

## Configuration

- `AGENTBROWSER_API_KEY`: require `X-Api-Key` or `Authorization: Bearer`.
- `AGENTBROWSER_ALLOWED_HOSTS`: comma-separated host allowlist. `*` allows all.
- `AGENTBROWSER_BLOCKED_SCHEMES`: comma-separated scheme blocklist. Defaults to `file,chrome,about`.
- `AGENTBROWSER_BLOCK_PRIVATE_NETWORKS`: block private IP addresses and `localhost` when `true`.
- `AGENTBROWSER_CORS_ORIGINS`: comma-separated CORS origins. CORS is disabled by default.
- `AGENTBROWSER_MAX_REQUEST_BYTES`: reject larger request bodies. Defaults to 1 MB.
- `AGENTBROWSER_PERSISTENCE=true`: enable SQLite operation audit log.
- `AGENTBROWSER_SQLITE_PATH`: override the operation database path.

## Known risks

- Browser sessions persist memory and cookies in memory only; they disappear when the process exits.
- A hostile prompt can instruct an agent to navigate anywhere the browser is allowed to go.
- API keys are shared over your transport. Prefer TLS when crossing machines.

## Reporting

If this repository is published, report suspected vulnerabilities to the repository owner privately. Do not file public issues containing exploitable details.

## Safety restrictions

AgentBrowser does not protect against every dangerous action. It is your responsibility to:

- Keep confirmation requirements for checkout, purchase, and payment screens.
- Avoid sensitive credentials and secrets unless necessary.
- Audit logs written by `AGENTBROWSER_PERSISTENCE` if you enable them.
