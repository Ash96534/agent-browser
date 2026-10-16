# AgentBrowser - Detailed Implementation Plan

## Overview

This document provides a granular, step-by-step implementation plan for AgentBrowser. Each task is broken down into specific, actionable items that can be tracked and verified.

---

## Phase 1: MVP - Core Browser Operations (Week 1-2)

### 1.1 Project Setup & Infrastructure

#### 1.1.1 Solution Structure ✅ COMPLETED
- [x] Create .NET 8 solution file `AgentBrowser.sln`
- [x] Create source directory structure:
  ```
  src/
    AgentBrowser.Api/
    AgentBrowser.Core/
    AgentBrowser.Browser/
    AgentBrowser.MCP/
    AgentBrowser.Shared/
  tests/
    AgentBrowser.Tests.Unit/
    AgentBrowser.Tests.Integration/
  ```
- [ ] Create `Directory.Build.props` for centralized build configuration
- [ ] Create `Directory.Packages.props` for centralized package management (CPM)

#### 1.1.2 Project Files & Dependencies ✅ COMPLETED
- [x] Create `AgentBrowser.Shared` class library
  - Add NuGet: `System.Text.Json`, `Microsoft.Extensions.Logging.Abstractions`
- [x] Create `AgentBrowser.Browser` class library
  - Add NuGet: `PuppeteerSharp` (latest stable, ~18.x)
  - Add NuGet: `Microsoft.Extensions.Logging`
  - Reference: `AgentBrowser.Shared`
- [x] Create `AgentBrowser.Core` class library
  - Add NuGet: `Microsoft.Extensions.DependencyInjection.Abstractions`
  - Add NuGet: `Microsoft.Extensions.Logging`
  - Reference: `AgentBrowser.Shared`, `AgentBrowser.Browser`
- [x] Create `AgentBrowser.Api` web project (ASP.NET Core)
  - Reference: `AgentBrowser.Core`, `AgentBrowser.Shared`
- [x] Create `AgentBrowser.MCP` class library
  - Reference: `AgentBrowser.Core`, `AgentBrowser.Shared`
- [x] Create test projects
  - `AgentBrowser.Tests.Unit` (xUnit)
  - `AgentBrowser.Tests.Integration` (xUnit)
  - Add NuGet: `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`

#### 1.1.3 Docker Setup
- [ ] Create `docker/Dockerfile`
  - Base: `mcr.microsoft.com/dotnet/aspnet:8.0`
  - Build stage: `mcr.microsoft.com/dotnet/sdk:8.0`
  - Install Chromium dependencies in final image
- [ ] Create `docker/docker-compose.yml`
  - Service: `agent-browser`
  - Port mapping: `5000:8080`
  - Volume for Chromium cache
- [ ] Create `docker/.dockerignore`

#### 1.1.4 Configuration
- [ ] Create `appsettings.json`
  - Browser settings (headless, viewport, user agent)
  - API settings (port, cors)
  - Logging configuration
- [ ] Create `appsettings.Development.json`
- [ ] Create strongly-typed configuration classes:
  - `BrowserSettings.cs`
  - `ApiSettings.cs`
  - `SessionSettings.cs`

---

### 1.2 Core Domain Models

#### 1.2.1 Shared Models (`AgentBrowser.Shared`)

**Session Models:**
```
Models/
├── Session.cs
├── SessionOptions.cs
├── Tab.cs
├── Viewport.cs
├── Cookie.cs
└── StorageState.cs
```

- [ ] `Session.cs`
- [ ] `SessionOptions.cs`
- [ ] `Tab.cs`
- [ ] `Viewport.cs`
- [ ] `Cookie.cs`
- [ ] `StorageState.cs`

**API Models:**
```
Models/
├── Requests/
│   ├── NavigateRequest.cs
│   ├── ClickRequest.cs
│   ├── TypeRequest.cs
│   ├── ExecuteJsRequest.cs
│   └── ScreenshotRequest.cs
├── Responses/
│   ├── SessionResponse.cs
│   ├── NavigateResponse.cs
│   ├── HtmlResponse.cs
│   ├── ScreenshotResponse.cs
│   └── ErrorResponse.cs
└── Common/
    ├── SelectorOptions.cs
    └── WaitOptions.cs
```

- [ ] `NavigateRequest.cs`
- [ ] `ClickRequest.cs`
- [ ] `TypeRequest.cs`
- [ ] `ExecuteJsRequest.cs`
- [ ] `ScreenshotRequest.cs`
- [ ] `SessionResponse.cs`
- [ ] `ErrorResponse.cs`

---

### 1.3 Browser Engine Layer (`AgentBrowser.Browser`)

#### 1.3.1 Browser Manager
```
Browser/
├── IBrowserManager.cs
├── BrowserManager.cs
├── IBrowserSession.cs
├── BrowserSession.cs
├── IBrowserTab.cs
├── BrowserTab.cs
└── BrowserFactory.cs
```

- [ ] `IBrowserManager.cs`
- [ ] `BrowserManager.cs`
- [ ] `BrowserFactory.cs`
- [ ] `IBrowserSession.cs`
- [ ] `BrowserSession.cs`
- [ ] `IBrowserTab.cs`
- [ ] `BrowserTab.cs`

#### 1.3.2 Error Handling
```
Browser/
├── Exceptions/
│   ├── BrowserException.cs
│   ├── SessionNotFoundException.cs
│   ├── TabNotFoundException.cs
│   ├── NavigationException.cs
│   ├── SelectorException.cs
│   └── ScriptExecutionException.cs
└── ErrorMapper.cs
```

- [ ] Create custom exception hierarchy
- [ ] `ErrorMapper.cs` - Maps Puppeteer exceptions to domain exceptions

---

### 1.4 Core Business Logic (`AgentBrowser.Core`)

#### 1.4.1 Session Service
```
Core/
├── Services/
│   ├── ISessionService.cs
│   ├── SessionService.cs
│   ├── INavigationService.cs
│   ├── NavigationService.cs
│   ├── IDomService.cs
│   ├── DomService.cs
│   └── IScreenshotService.cs
│       ScreenshotService.cs
```

- [ ] `ISessionService.cs`
- [ ] `SessionService.cs`
- [ ] `INavigationService.cs`
- [ ] `NavigationService.cs`
- [ ] `IDomService.cs`
- [ ] `DomService.cs`
- [ ] `IScreenshotService.cs`
- [ ] `ScreenshotService.cs`

#### 1.4.2 Dependency Injection Setup
```
Core/
├── Extensions/
│   └── ServiceCollectionExtensions.cs
```

- [ ] `ServiceCollectionExtensions.cs`

---

### 1.5 API Layer (`AgentBrowser.Api`)

#### 1.5.1 Program.cs & Startup
- [ ] `Program.cs`
- [ ] `Startup.cs` (if using separate class)

#### 1.5.2 Controllers
```
Api/
├── Controllers/
│   ├── SessionController.cs
│   ├── NavigationController.cs
│   ├── DomController.cs
│   ├── ScreenshotController.cs
│   └── HealthController.cs
```

- [ ] `SessionController.cs`
- [ ] `NavigationController.cs`
- [ ] `DomController.cs`
- [ ] `ScreenshotController.cs`
- [ ] `HealthController.cs`

#### 1.5.3 Middleware
```
Api/
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── SessionValidationMiddleware.cs
```

- [ ] `ExceptionHandlingMiddleware.cs`
- [ ] `RequestLoggingMiddleware.cs`
- [ ] `SessionValidationMiddleware.cs`

#### 1.5.4 WebSocket Support
```
Api/
├── WebSockets/
│   ├── WebSocketHandler.cs
│   ├── WebSocketManager.cs
│   └── WebSocketMessage.cs
```

- [ ] `WebSocketHandler.cs`
- [ ] `WebSocketManager.cs`
- [ ] `WebSocketMessage.cs`

---

### 1.6 Testing

#### 1.6.1 Unit Tests
```
tests/AgentBrowser.Tests.Unit/
├── Browser/
│   ├── BrowserManagerTests.cs
│   ├── BrowserSessionTests.cs
│   └── BrowserTabTests.cs
├── Core/
│   ├── SessionServiceTests.cs
│   ├── NavigationServiceTests.cs
│   ├── DomServiceTests.cs
│   └── ScreenshotServiceTests.cs
└── Api/
    ├── SessionControllerTests.cs
    ├── NavigationControllerTests.cs
    └── DomControllerTests.cs
```

- [ ] `BrowserManagerTests.cs`
- [ ] `SessionServiceTests.cs`
- [ ] `NavigationServiceTests.cs`
- [ ] `DomServiceTests.cs`

#### 1.6.2 Integration Tests
```
tests/AgentBrowser.Tests.Integration/
├── SessionApiTests.cs
├── NavigationApiTests.cs
├── DomApiTests.cs
├── ScreenshotApiTests.cs
└── TestServer.cs
```

- [ ] `SessionApiTests.cs`
- [ ] `NavigationApiTests.cs`
- [ ] `DomApiTests.cs`
- [ ] `TestServer.cs`

---

## Phase 2: Core Agent Features (Week 3-4)

### 2.1 WebSocket Real-time Communication

#### 2.1.1 WebSocket Infrastructure
- [ ] Implement `WebSocketHandler` for real-time event streaming
- [ ] Add WebSocket endpoint `/ws/session/{id}`
- [ ] Implement connection management
  - Track active connections per session
  - Handle reconnection
  - Implement ping/pong heartbeat

#### 2.1.2 Event Streaming
```
Browser/
├── Events/
│   ├── BrowserEvent.cs
│   ├── ConsoleEvent.cs
│   ├── NetworkEvent.cs
│   ├── DomEvent.cs
│   └── IEventBroadcaster.cs
```

- [ ] `BrowserEvent.cs`
- [ ] `IEventBroadcaster.cs`

#### 2.1.3 Console Log Streaming
- [ ] Hook into Puppeteer console event
- [ ] Map console types (log, warn, error, info)
- [ ] Stream to connected WebSocket clients
- [ ] Store recent logs in memory (last 100)

#### 2.1.4 Network Event Streaming
- [ ] Enable request interception
- [ ] Capture request details (method, URL, headers, body)
- [ ] Capture response details (status, headers, body)
- [ ] Stream to WebSocket clients
- [ ] Store in session for later retrieval

---

### 2.2 JavaScript Execution & Evaluation

#### 2.2.1 Safe Execution
- [ ] Implement safe JavaScript execution wrapper
- [ ] Timeout handling
- [ ] Error capture and formatting
- [ ] Return value serialization

#### 2.2.2 Helper Scripts Library
```
Browser/Scripts/
├── DomHelpers.js
├── ExtractionHelpers.js
└── QueryHelpers.js
```

- [ ] `DomHelpers.js`
- [ ] `QueryHelpers.js`

---

### 2.3 Wait Strategies

#### 2.3.1 Wait Implementations
- [ ] `WaitForSelector` - Wait for element to appear
- [ ] `WaitForNavigation` - Wait for navigation to complete
- [ ] `WaitForFunction` - Wait for custom function to return true
- [ ] `WaitForTimeout` - Fixed timeout
- [ ] `WaitForRequest` - Wait for specific network request
- [ ] `WaitForResponse` - Wait for specific network response

---

### 2.4 Advanced DOM Operations

#### 2.4.1 Form Handling
- [ ] `SelectOptionAsync` - Select dropdown option
- [ ] `CheckAsync` / `UncheckAsync` - Checkbox handling
- [ ] `UploadFileAsync` - File input handling
- [ ] `GetFormValuesAsync` - Extract all form values
- [ ] `FillFormAsync` - Fill multiple form fields

#### 2.4.2 Advanced Interactions
- [ ] `HoverAsync` - Hover over element
- [ ] `FocusAsync` - Focus element
- [ ] `BlurAsync` - Blur element
- [ ] `DragAndDropAsync` - Drag and drop
- [ ] `ScrollAsync` - Scroll to element or position
- [ ] `DoubleClickAsync` - Double click
- [ ] `RightClickAsync` - Right click

---

## Phase 3: Extended Data Extraction (Week 5-6)

### 3.1 Metadata Extraction Layer

#### 3.1.1 Extractors
```
Extraction/
├── IExtractor.cs
├── MetadataExtractor.cs
├── PriceExtractor.cs
├── ImageExtractor.cs
├── FormExtractor.cs
├── LinkExtractor.cs
└── SchemaExtractor.cs
```

- [ ] `IExtractor.cs`
- [ ] `MetadataExtractor.cs`
- [ ] `PriceExtractor.cs`
- [ ] `ImageExtractor.cs`
- [ ] `FormExtractor.cs`
- [ ] `LinkExtractor.cs`
- [ ] `SchemaExtractor.cs`

#### 3.1.2 Metadata Extractor
- [ ] Extract all `<meta>` tags
- [ ] Extract Open Graph tags
- [ ] Extract Twitter Card tags
- [ ] Extract canonical URL
- [ ] Extract JSON-LD structured data
- [ ] Extract Microdata
- [ ] Extract RDFa attributes

#### 3.1.3 Price Extractor
- [ ] Scan for `data-price` attributes
- [ ] Scan for `data-*` price-related attributes
- [ ] Extract Schema.org price data
- [ ] Regex scan for currency patterns
- [ ] Detect sale/discount indicators
- [ ] Extract stock status

#### 3.1.4 Image Context Extractor
- [ ] Extract all images with:
  - `src`, `alt`, `title`
  - `aria-label`, `aria-describedby`
  - Width, height
  - Lazy-load data attributes
  - Nearby caption text
  - Parent context (product card, article, etc.)

#### 3.1.5 Form Extractor
- [ ] Extract all forms with:
  - Action URL
  - Method
  - All input fields (type, name, value, placeholder, required)
  - Labels associated with inputs
  - Validation attributes

#### 3.1.6 Link Extractor
- [ ] Extract all links with:
  - `href`, `text`, `title`
  - `rel` attribute
  - `target` attribute
  - Associated icon/image

#### 3.1.7 Schema-based Extraction
- [ ] Define extraction schema format
- [ ] Implement schema parser
- [ ] Implement schema executor
- [ ] Support field transformations

---

### 3.2 Extraction API Endpoints

#### 3.2.1 Controllers
```
Api/Controllers/
├── ExtractionController.cs
```

- [ ] `ExtractionController.cs`

---

## Phase 4: MCP Server & Agent Integration (Week 7)

### 4.1 MCP Server Implementation

#### 4.1.1 MCP Protocol Support
```
MCP/
├── Server/
│   ├── McpServer.cs
│   ├── McpTransport.cs
│   ├── McpHandler.cs
│   └── McpSchema.cs
├── Tools/
│   ├── BrowseNavigateTool.cs
│   ├── BrowseClickTool.cs
│   ├── BrowseTypeTool.cs
│   ├── BrowseGetHtmlTool.cs
│   ├── BrowseGetScreenshotTool.cs
│   ├── BrowseExecuteJsTool.cs
│   ├── BrowseExtractSchemaTool.cs
│   ├── BrowseGetMetadataTool.cs
│   ├── BrowseGetPricesTool.cs
│   ├── BrowseGetImagesTool.cs
│   └── BrowseGetFormsTool.cs
└── Resources/
    └── SessionResource.cs
```

#### 4.1.2 MCP Server Core
- [ ] Implement MCP protocol handler
- [ ] Implement stdio transport
- [ ] Implement HTTP transport (optional)

#### 4.1.3 Tool Definitions

- [ ] `BrowseNavigateTool`
- [ ] `BrowseClickTool`
- [ ] `BrowseTypeTool`
- [ ] `BrowseGetHtmlTool`
- [ ] `BrowseGetScreenshotTool`
- [ ] `BrowseExecuteJsTool`
- [ ] `BrowseExtractSchemaTool`
- [ ] `BrowseGetMetadataTool`
- [ ] `BrowseGetPricesTool`
- [ ] `BrowseGetImagesTool`
- [ ] `BrowseGetFormsTool`

---

### 4.2 Agent SDK (Optional)

#### 4.2.1 Python SDK
```
sdk/python/
├── agent_browser/
│   ├── __init__.py
│   ├── client.py
│   ├── models.py
│   └── exceptions.py
├── setup.py
└── README.md
```

- [ ] `client.py`

---

## Phase 5: Production Readiness (Week 8+)

### 5.1 Error Handling & Recovery

- [ ] Implement comprehensive error handling
- [ ] Implement retry logic with exponential backoff
- [ ] Implement circuit breaker for failing sessions

### 5.2 Resource Management

- [ ] Implement session limits
- [ ] Implement idle session cleanup
- [ ] Add resource monitoring endpoint

### 5.3 Security

- [ ] Implement API authentication (API key)
- [ ] Add CORS configuration
- [ ] Rate limiting per API key
- [ ] Input validation and sanitization
- [ ] HTTPS support

### 5.4 Logging & Monitoring

- [ ] Structured logging with Serilog
- [ ] Request/response logging
- [ ] Performance metrics
- [ ] Health check endpoints

### 5.5 Documentation

- [ ] OpenAPI/Swagger UI
- [ ] API documentation
- [ ] MCP tool documentation
- [ ] Architecture diagrams
- [ ] Usage examples

### 5.6 CI/CD

- [ ] GitHub Actions workflow
- [ ] Release automation
- [ ] Version management

---

## Development Workflow

### Daily Process
1. Pick task from implementation plan
2. Create feature branch: `feature/phase-X/task-name`
3. Implement with tests
4. Run tests locally
5. Create PR
6. Merge to `main`

### Branch Strategy
- `main` - Stable, deployable code
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `release/*` - Release preparation

### Commit Convention
- `feat:` - New feature
- `fix:` - Bug fix
- `refactor:` - Code refactoring
- `test:` - Test additions/changes
- `docs:` - Documentation
- `chore:` - Maintenance tasks

---

## Success Metrics

### Phase 1 Completion
- [ ] Can create browser session via API
- [ ] Can navigate to URL
- [ ] Can retrieve HTML
- [ ] Can take screenshot
- [ ] All tests passing
- [ ] Docker container builds and runs

### Phase 2 Completion
- [ ] WebSocket streaming working
- [ ] Console logs captured
- [ ] Network requests captured
- [ ] JavaScript execution working
- [ ] All DOM operations functional

### Phase 3 Completion
- [ ] Metadata extraction working
- [ ] Price extraction working
- [ ] Image context extraction working
- [ ] Schema-based extraction working
- [ ] All extraction endpoints functional

### Phase 4 Completion
- [ ] MCP server running
- [ ] All tools implemented
- [ ] Agent can connect and control browser
- [ ] End-to-end agent workflow working

### Phase 5 Completion
- [ ] Production-ready error handling
- [ ] Resource management working
- [ ] Security implemented
- [ ] Monitoring in place
- [ ] Documentation complete
- [ ] CI/CD pipeline operational

---

## Risk Mitigation

### Technical Risks
1. **Chromium binary size**
   - Mitigation: Download on first use, cache in Docker image

2. **Memory leaks in long-running sessions**
   - Mitigation: Implement session recycling, monitoring

3. **WebSocket connection drops**
   - Mitigation: Reconnection logic, heartbeat

4. **Network interception performance**
   - Mitigation: Make optional, limit body capture size

### Schedule Risks
1. **Underestimated complexity**
   - Mitigation: Prioritize MVP features, defer nice-to-have

2. **Learning curve with PuppeteerSharp**
   - Mitigation: Start with simple operations, build up

---

## Next Actions

1. **Week 1: Setup & Core Models**
   - Create solution structure ✅ DONE
   - Implement core domain models
   - Set up basic browser integration

2. **Week 2: API & Basic Operations**
   - Implement REST API
   - Session management
   - Navigation & screenshot

3. **Week 3: WebSocket & Events**
   - WebSocket infrastructure
   - Console & network streaming

4. **Week 4: Advanced DOM**
   - Form handling
   - Advanced interactions
   - Wait strategies

5. **Week 5-6: Extraction Layer**
   - Metadata extraction
   - Price extraction
   - Schema extraction

6. **Week 7: MCP Server**
   - MCP protocol implementation
   - Tool definitions
   - Agent integration testing

7. **Week 8+: Production**
   - Error handling
   - Security
   - Documentation
   - CI/CD