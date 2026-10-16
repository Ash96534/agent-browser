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
- [ ] Headless and headed modes
- [ ] Multi-tab support
- [ ] Navigation (get, post, form submission)
- [ ] History management (back, forward, reload)
- [ ] Wait for element/load states
- [ ] Screenshot capture (full page, viewport, element)
- [ ] PDF generation

### 4.2 DOM Manipulation
- [ ] Query by CSS selector, XPath, ID, class
- [ ] Click, double-click, right-click
- [ ] Type, select, clear input
- [ ] Scroll, hover
- [ ] Drag and drop
- [ ] Execute arbitrary JavaScript
- [ ] Evaluate JavaScript expressions

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

```python
class Session:
    id: str                      # UUID
    created_at: datetime
    browser_context: BrowserContext
    viewport: Viewport
    user_agent: str
    cookies: list[Cookie]
    storage: StorageState
    tabs: list[Tab]
    interceptors: list[Interceptor]
```

### 5.2 Extraction Schema

```python
class ExtractionSchema:
    name: str
    fields: list[ExtractionField]
    
class ExtractionField:
    name: str
    selector: str
    type: str                    # text, attribute, html, price, image, link
    attribute: str               # for attribute type
    multiple: bool               # extract multiple matches
    transform: str               # optional transformation
```

---

## 6. Implementation Phases

### Phase 1: MVP (2-3 weeks)
- [ ] Chromium integration via CDP/Puppeteer
- [ ] Basic session management
- [ ] Navigation and HTML retrieval
- [ ] Screenshot capture
- [ ] Basic DOM querying
- [ ] REST API server
- [ ] Hello world agent integration

### Phase 2: Core Agent Features (2-3 weeks)
- [ ] WebSocket real-time communication
- [ ] Console log streaming
- [ ] Network request capture
- [ ] JavaScript execution
- [ ] Click/type/select operations
- [ ] Wait states

### Phase 3: Extended Extraction (2-3 weeks)
- [ ] Metadata extraction (OG, Twitter, JSON-LD)
- [ ] Price/data attribute scanning
- [ ] Image context extraction
- [ ] Structured data extraction with schemas
- [ ] Form detection and mapping

### Phase 4: Agent Integration (1-2 weeks)
- [ ] MCP server implementation
- [ ] Agent SDK libraries
- [ ] Authentication flows
- [ ] Session persistence

### Phase 5: Production Ready (ongoing)
- [ ] Error handling and recovery
- [ ] Rate limiting
- [ ] Resource management
- [ ] Scaling (multi-instance)
- [ ] Monitoring and logging

---

## 7. File Structure

```
agent-browser/
├── src/
│   ├── api/
│   │   ├── routes/
│   │   │   ├── session.py
│   │   │   ├── navigate.py
│   │   │   ├── extract.py
│   │   │   └── websocket.py
│   │   └── server.py
│   ├── browser/
│   │   ├── engine.py          # CDP wrapper
│   │   ├── session.py
│   │   ├── tab.py
│   │   └── interceptor.py
│   ├── extraction/
│   │   ├── metadata.py
│   │   ├── prices.py
│   │   ├── images.py
│   │   └── schema.py
│   ├── mcp/
│   │   ├── server.py
│   │   └── tools.py
│   └── models/
│       └── schemas.py
├── tests/
│   ├── unit/
│   └── integration/
├── docker/
│   └── Dockerfile
├── pyproject.toml
└── README.md
```

---

## 8. Key Design Decisions

### 8.1 Why Chromium via CDP?
- **Standards compliance** — Agents can test real-world behavior
- **Mature ecosystem** — Good debugging tools, documentation
- **CDP exposure** — Deep access to browser internals
- **Headless support** — Clean for automation

### 8.2 Why separate API server?
- **Language independence** — Agents can be in any language
- **Scalability** — Multiple agents can share browser instances
- **Security** — Browser runs in isolated process
- **Statefulness** — Sessions persist beyond single requests

### 8.3 Extended extraction philosophy
The "hidden" data lives in:
- Developer tooling/attributes (`data-*`, `aria-*`)
- Structured data (JSON-LD, Microdata)
- Meta tags (OG, Twitter Cards)
- Implicit context (nearby elements, parent relationships)

We expose these through a unified extraction API rather than just raw HTML access.

---

## 9. Next Steps

1. **Confirm architecture** — Does this alignment with your vision?
2. **Pick language** — Python (FastAPI) or Node.js (Express) for API layer?
3. **Scope Phase 1** — What MVP features are essential vs nice-to-have?
4. **Infrastructure** — Docker for the prototype?