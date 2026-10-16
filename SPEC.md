# AgentBrowser - Specification

> A fully exposed, agent-centric web browser for autonomous web agents

## 1. Project Overview

**Name:** AgentBrowser  
**Type:** Standalone browser daemon with full API exposure  
**Core Purpose:** Give AI agents complete, fine-grained control over browser behavior with capabilities beyond traditional automation tools — including access to hidden metadata, structured data extraction, and real-time DOM observation.

**Target Users:**
- Autonomous AI agents needing web interaction
- Developers prototyping agentic web behaviors
- QA/testing frameworks for complex web apps

---

## 2. Architecture

### 2.1 High-Level Design

```
┌─────────────────────────────────────────────────────────────┐
│                     AgentBrowser                            │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────┐   │
│  │ Agent API   │   │  WebSocket  │   │   MCP Server    │   │
│  │ (REST/HTTP) │   │  Real-time  │   │ (LLM Integration)│   │
│  └──────┬──────┘   └──────┬──────┘   └────────┬────────┘   │
│         │                 │                    │            │
│  ┌──────▼─────────────────▼────────────────────▼────────┐  │
│  │              Control Plane (Orchestrator)            │  │
│  │  - Session management    - Command routing           │  │
│  │  - State sync            - Event broadcasting        │  │
│  └──────────────────────────┬───────────────────────────┘  │
│                             │                               │
│  ┌──────────────────────────▼───────────────────────────┐  │
│  │              Browser Engine (Chromium via CDP)       │  │
│  │  - Rendering         - Network interception          │  │
│  │  - JavaScript exec   - DOM access                    │  │
│  │  - Storage           - Console capture               │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
         ▲
         │ HTTP/WebSocket/MCP
         │
    ┌────┴────┐
    │  Agents │
    │ (LLMs)  │
    └─────────┘
```

### 2.2 Technology Stack

| Layer | Technology | Rationale |
|-------|------------|-----------|
| Language | C# (.NET 8) | Compiled, native performance, 5-10x faster than Python |
| Browser Engine | Chromium via PuppeteerSharp | Mature CDP wrapper, standards-compliant |
| API Server | ASP.NET Core | High-performance HTTP server, built-in WebSocket |
| MCP Server | Custom MCP implementation | Native LLM agent integration |
| Storage | SQLite + in-memory | Lightweight, embedded, optional persistence |
| Containerization | Docker | Reproducible, isolated environments |

---

## 3. API Design

### 3.1 REST Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/session` | Create new browser session |
| `GET` | `/session/{id}` | Get session state |
| `DELETE` | `/session/{id}` | Close session |
| `POST` | `/session/{id}/navigate` | Navigate to URL |
| `GET` | `/session/{id}/html` | Get rendered HTML |
| `POST` | `/session/{id}/execute` | Execute JavaScript |
| `POST` | `/session/{id}/click` | Click element |
| `POST` | `/session/{id}/type` | Type text |
| `GET` | `/session/{id}/screenshot` | Capture screenshot |
| `GET` | `/session/{id}/console` | Get console logs |
| `GET` | `/session/{id}/network` | Get network requests/responses |
| `GET` | `/session/{id}/dom` | Full DOM tree |
| `POST` | `/session/{id}/extract` | Extract structured data |

### 3.2 WebSocket Events

**Server → Client:**
- `console` — Console log message
- `network` — Network request/response
- `dom-update` — DOM mutation detected
- `load-complete` — Page finished loading
- `error` — Browser error

**Client → Server:**
- `navigate` — Navigate command
- `execute` — JS execution
- `subscribe` — Subscribe to event streams

### 3.3 MCP Tools (for LLM Agents)

```
browse_navigate(url: string) → navigation_result
browse_click(selector: string) → click_result  
browse_type(selector: string, text: string) → type_result
browse_get_html() → html_content
browse_get_screenshot() → base64_image
browse_execute_js(code: string) → js_result
browse_extract_schema(schema: ExtractionSchema) → extracted_data
browse_get_hidden_metadata() → metadata
browse_search_dom(query: string) → matching_elements
browse_get_prices() → price_data
browse_get_images_with_context() → images_with_context
browse_get_forms() → form_data
browse_get_links() → link_data
browse_console_logs() → console_entries
browse_network_logs() → network_entries
```

---

## 4. Core Features

### 4.1 Basic Browser Operations
- [x] Headless and headed modes
- [x] Multi-tab support
- [x] Navigation (get, post, form submission)
- [x] History management (back, forward, reload)
- [x] Wait for element/load states
- [x] Screenshot capture (full page, viewport, element)
- [x] PDF generation

### 4.2 DOM Manipulation
- [x] Query by CSS selector, XPath, ID, class
- [x] Click, double-click, right-click
- [x] Type, select, clear input
- [x] Scroll, hover
- [x] Drag and drop
- [x] Execute arbitrary JavaScript
- [x] Evaluate JavaScript expressions

### 4.3 Network Interception
- [ ] Capture all HTTP/HTTPS requests
- [ ] Capture request headers, body
- [ ] Capture response headers, body
- [ ] Modify requests before sending
- [ ] Mock responses
- [ ] Block requests (ads, trackers)

### 4.4 Extended Data Extraction (The "Hidden" Stuff)

#### Hidden Metadata Extraction
- `<meta>` tags
- Open Graph tags (`og:*`)
- Twitter Card tags
- JSON-LD structured data
- Microdata (`itemscope`, `itemprop`)
- RDFa attributes

#### Price/Product Data
- `data-price` attributes
- `data-*` custom attributes
- Schema.org Price specifications
- Currency detection
- Sale/discount indicators
- Stock status

#### Image Context
- `alt` text
- `title` attribute
- `aria-label` descriptions
- Caption elements nearby
- Parent context (product card, article, etc.)
- Lazy-load data attributes

#### Semantic Extraction
- Article content extraction
- Table data parsing
- List item extraction
- Form field detection and mapping
- Link relationship mapping (`rel` attributes)

---

## 5. Data Models

### 5.1 Session

```csharp
public class Session
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public SessionStatus Status { get; set; }
    public List<Tab> Tabs { get; set; } = new();
    public Viewport Viewport { get; set; } = new();
    public string UserAgent { get; set; } = string.Empty;
    public List<Cookie> Cookies { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum SessionStatus
{
    Active,
    Idle,
    Closed
}
```

### 5.2 Extraction Schema

```csharp
public class ExtractionSchema
{
    public string Name { get; set; } = string.Empty;
    public List<ExtractionField> Fields { get; set; } = new();
}

public class ExtractionField
{
    public string Name { get; set; } = string.Empty;
    public string Selector { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // text, attribute, html, price, image, link
    public string Attribute { get; set; } = string.Empty;
    public bool Multiple { get; set; }
    public string? Transform { get; set; }
}
```

---

## 6. Implementation Phases

### Phase 1: MVP (COMPLETED) ✅
- [x] Chromium integration via PuppeteerSharp
- [x] Basic session management
- [x] Navigation and HTML retrieval
- [x] Screenshot capture
- [x] Basic DOM querying
- [x] REST API server
- [x] Hello world agent integration

**Completed:**
- ✅ Browser engine layer (IBrowserManager, BrowserManager)
- ✅ Core services (SessionService, NavigationService, DomService, ScreenshotService)
- ✅ Domain models (Session, Tab, Viewport, Cookie, etc.)
- ✅ Request/Response models (NavigateRequest, ClickRequest, etc.)
- ✅ Service dependency injection setup

### Phase 2: Core Agent Features (Next)
- [ ] WebSocket real-time communication
- [ ] Console log streaming
- [ ] Network request capture
- [ ] JavaScript execution
- [ ] Click/type/select operations
- [ ] Wait states

### Phase 3: Extended Extraction (Future)
- [ ] Metadata extraction (OG, Twitter, JSON-LD)
- [ ] Price/data attribute scanning
- [ ] Image context extraction
- [ ] Structured data extraction with schemas
- [ ] Form detection and mapping

### Phase 4: Agent Integration (Future)
- [ ] MCP server implementation
- [ ] Agent SDK libraries
- [ ] Authentication flows
- [ ] Session persistence

### Phase 5: Production Ready (Future)
- [ ] Error handling and recovery
- [ ] Rate limiting
- [ ] Resource management
- [ ] Scaling (multi-instance)
- [ ] Monitoring and logging

---

## 7. Project Structure

```
agent-browser/
├── src/
│   ├── AgentBrowser.Api/           # REST API (controllers)
│   │   ├── Controllers/
│   │   │   ├── SessionController.cs
│   │   │   ├── NavigationController.cs
│   │   │   ├── DomController.cs
│   │   │   └── ScreenshotController.cs
│   │   └── Program.cs
│   ├── AgentBrowser.Core/          # Business logic
│   │   ├── Services/
│   │   │   ├── ISessionService.cs
│   │   │   ├── SessionService.cs
│   │   │   ├── INavigationService.cs
│   │   │   ├── NavigationService.cs
│   │   │   ├── IDomService.cs
│   │   │   ├── DomService.cs
│   │   │   ├── IScreenshotService.cs
│   │   │   └── ScreenshotService.cs
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   ├── AgentBrowser.Browser/       # Browser engine
│   │   ├── Browser/
│   │   │   ├── IBrowserManager.cs
│   │   │   └── BrowserManager.cs
│   │   └── Extensions/
│   │       └── TabExtensions.cs
│   ├── AgentBrowser.MCP/           # MCP server
│   │   ├── Server/
│   │   │   ├── McpServer.cs
│   │   │   ├── McpTransport.cs
│   │   │   └── McpHandler.cs
│   │   └── Tools/
│   ├── AgentBrowser.Shared/        # Shared models
│   │   ├── Models/
│   │   │   ├── Session.cs
│   │   │   ├── SessionOptions.cs
│   │   │   ├── Tab.cs
│   │   │   ├── Viewport.cs
│   │   │   ├── Cookie.cs
│   │   │   ├── Requests/          # API request models
│   │   │   ├── Responses/         # API response models
│   │   │   └── Common/            # Shared types
│   └── models/
│       └── schemas.py
├── tests/
│   ├── AgentBrowser.Tests.Unit/
│   └── AgentBrowser.Tests.Integration/
├── docker/
│   └── Dockerfile
├── AgentBrowser.sln
└── README.md
```

---

## 8. Key Design Decisions

### 8.1 Why C# (.NET 8)?
- **Performance** — Compiled language, 5-10x faster than Python
- **Native async** — Excellent async/await support for concurrent operations
- **PuppeteerSharp** — Mature CDP wrapper with full browser control
- **Type safety** — Compile-time error checking
- **Production ready** — Strong tooling and debugging support

### 8.2 Why Chromium via PuppeteerSharp (CDP)?
- **Standards compliance** — Agents can test real-world behavior
- **Mature ecosystem** — Good debugging tools, documentation
- **CDP exposure** — Deep access to browser internals
- **Headless support** — Clean for automation

### 8.3 Why separate API server?
- **Language independence** — Agents can be in any language
- **Scalability** — Multiple agents can share browser instances
- **Security** — Browser runs in isolated process
- **Statefulness** — Sessions persist beyond single requests

### 8.4 Extended extraction philosophy
The "hidden" data lives in:
- Developer tooling/attributes (`data-*`, `aria-*`)
- Structured data (JSON-LD, Microdata)
- Meta tags (OG, Twitter Cards)
- Implicit context (nearby elements, parent relationships)

We expose these through a unified extraction API rather than just raw HTML access.

---

## 9. Next Steps

1. **Confirm architecture** — Does this alignment with your vision?
2. **Pick language** — C# (.NET 8) selected for performance
3. **Scope Phase 1** — MVP features: Session management, navigation, screenshot ✅ DONE
4. **Infrastructure** — Docker for the prototype?

---

## 10. Progress Tracker

| Phase | Status | Notes |
|-------|--------|-------|
| Phase 1 | ✅ Complete | Browser engine layer, core services |
| Phase 2 | ✅ Complete | API controllers, WebSocket infrastructure, 10 REST endpoints |
| Phase 3 | ✅ Complete | Extraction service, 5 extraction endpoints |
| Phase 4 | 🔄 In Progress | MCP server, agent integration |
| Phase 5 | 📅 Future | Production readiness |

---

**Last Updated:** 2026-10-17  
**Current Status:** Phase 2/3 Complete - 15 REST endpoints + WebSocket infrastructure