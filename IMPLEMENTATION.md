# AgentBrowser - Detailed Implementation Plan

## Overview

This document provides a granular, step-by-step implementation plan for AgentBrowser. Each task is broken down into specific, actionable items that can be tracked and verified.

---

## Phase 1: MVP - Core Browser Operations (Week 1-2)

### 1.1 Project Setup & Infrastructure

#### 1.1.1 Solution Structure
- [ ] Create .NET 8 solution file `AgentBrowser.sln`
- [ ] Create source directory structure:
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

#### 1.1.2 Project Files & Dependencies
- [ ] Create `AgentBrowser.Shared` class library
  - Add NuGet: `System.Text.Json`, `Microsoft.Extensions.Logging.Abstractions`
- [ ] Create `AgentBrowser.Browser` class library
  - Add NuGet: `PuppeteerSharp` (latest stable, ~18.x)
  - Add NuGet: `Microsoft.Extensions.Logging`
  - Reference: `AgentBrowser.Shared`
- [ ] Create `AgentBrowser.Core` class library
  - Add NuGet: `Microsoft.Extensions.DependencyInjection.Abstractions`
  - Add NuGet: `Microsoft.Extensions.Logging`
  - Reference: `AgentBrowser.Shared`, `AgentBrowser.Browser`
- [ ] Create `AgentBrowser.Api` web project (ASP.NET Core)
  - Reference: `AgentBrowser.Core`, `AgentBrowser.Shared`
- [ ] Create `AgentBrowser.MCP` class library
  - Reference: `AgentBrowser.Core`, `AgentBrowser.Shared`
- [ ] Create test projects
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
  ```csharp
  public class Session
  {
      public string Id { get; set; }           // UUID
      public DateTime CreatedAt { get; set; }
      public DateTime LastAccessedAt { get; set; }
      public SessionStatus Status { get; set; }  // Active, Idle, Closed
      public List<Tab> Tabs { get; set; }
      public Viewport Viewport { get; set; }
      public string UserAgent { get; set; }
      public List<Cookie> Cookies { get; set; }
      public Dictionary<string, object> Metadata { get; set; }
  }
  
  public enum SessionStatus
  {
      Active,
      Idle,
      Closed
  }
  ```

- [ ] `SessionOptions.cs`
  ```csharp
  public class SessionOptions
  {
      public bool Headless { get; set; } = true;
      public Viewport? Viewport { get; set; }
      public string? UserAgent { get; set; }
      public bool IgnoreHTTPSErrors { get; set; } = true;
      public bool DumpIO { get; set; } = false;
      public List<string>? Args { get; set; }
  }
  ```

- [ ] `Tab.cs`
  ```csharp
  public class Tab
  {
      public string Id { get; set; }
      public string? Url { get; set; }
      public string? Title { get; set; }
      public TabStatus Status { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  
  public enum TabStatus
  {
      Loading,
      Loaded,
      Error
  }
  ```

- [ ] `Viewport.cs`
  ```csharp
  public class Viewport
  {
      public int Width { get; set; } = 1280;
      public int Height { get; set; } = 720;
      public double DeviceScaleFactor { get; set; } = 1.0;
      public bool IsMobile { get; set; } = false;
      public bool HasTouch { get; set; } = false;
  }
  ```

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
  ```csharp
  public class NavigateRequest
  {
      public string Url { get; set; } = string.Empty;
      public string? Referer { get; set; }
      public int? Timeout { get; set; } = 30000;
      public WaitUntil? WaitUntil { get; set; } = WaitUntil.Load;
  }
  
  public enum WaitUntil
  {
      Load,
      DOMContentLoaded,
      NetworkIdle0,
      NetworkIdle2
  }
  ```

- [ ] `ClickRequest.cs`
  ```csharp
  public class ClickRequest
  {
      public string Selector { get; set; } = string.Empty;
      public ClickOptions? Options { get; set; }
  }
  
  public class ClickOptions
  {
      public int Delay { get; set; } = 0;
      public int Button { get; set; } = 0;  // 0=left, 1=middle, 2=right
      public int ClickCount { get; set; } = 1;
  }
  ```

- [ ] `TypeRequest.cs`
  ```csharp
  public class TypeRequest
  {
      public string Selector { get; set; } = string.Empty;
      public string Text { get; set; } = string.Empty;
      public int Delay { get; set; } = 0;
      public bool ClearFirst { get; set; } = true;
  }
  ```

- [ ] `ExecuteJsRequest.cs`
  ```csharp
  public class ExecuteJsRequest
  {
      public string Script { get; set; } = string.Empty;
      public object[]? Args { get; set; }
  }
  ```

- [ ] `ScreenshotRequest.cs`
  ```csharp
  public class ScreenshotRequest
  {
      public ScreenshotType Type { get; set; } = ScreenshotType.Viewport;
      public string? Selector { get; set; }
      public bool FullPage { get; set; } = false;
      public string Format { get; set; } = "png";
      public int? Quality { get; set; }
  }
  
  public enum ScreenshotType
  {
      Viewport,
      Element,
      FullPage
  }
  ```

- [ ] `SessionResponse.cs`
  ```csharp
  public class SessionResponse
  {
      public string Id { get; set; }
      public string Status { get; set; }
      public DateTime CreatedAt { get; set; }
      public List<TabResponse> Tabs { get; set; }
  }
  
  public class TabResponse
  {
      public string Id { get; set; }
      public string? Url { get; set; }
      public string? Title { get; set; }
  }
  ```

- [ ] `ErrorResponse.cs`
  ```csharp
  public class ErrorResponse
  {
      public string Error { get; set; }
      public string? Message { get; set; }
      public string? StackTrace { get; set; }  // Only in dev
  }
  ```

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
  - Interface for browser lifecycle management
  - Methods:
    - `Task<IBrowserSession> CreateSessionAsync(SessionOptions options)`
    - `Task<IBrowserSession?> GetSessionAsync(string sessionId)`
    - `Task CloseSessionAsync(string sessionId)`
    - `Task<IEnumerable<IBrowserSession>> GetAllSessionsAsync()`
    - `Task CleanupIdleSessionsAsync(TimeSpan maxIdleTime)`

- [ ] `BrowserManager.cs`
  - Implements `IBrowserManager`
  - Maintains dictionary of active sessions
  - Uses `ConcurrentDictionary<string, BrowserSession>` for thread safety
  - Implements background cleanup task for idle sessions
  - Dependencies: `ILogger<BrowserManager>`, `BrowserFactory`

- [ ] `BrowserFactory.cs`
  - Creates Puppeteer browser instances
  - Manages browser download/installation (first run)
  - Configures launch options from `SessionOptions`
  - Handles Chromium binary path resolution

- [ ] `IBrowserSession.cs`
  - Interface for a single browser session
  - Properties:
    - `string Id`
    - `DateTime CreatedAt`
    - `SessionStatus Status`
    - `IReadOnlyList<IBrowserTab> Tabs`
  - Methods:
    - `Task<IBrowserTab> CreateTabAsync()`
    - `Task<IBrowserTab?> GetTabAsync(string tabId)`
    - `Task CloseTabAsync(string tabId)`
    - `Task CloseAsync()`

- [ ] `BrowserSession.cs`
  - Implements `IBrowserSession`
  - Wraps Puppeteer `IBrowser` instance
  - Manages browser context (cookies, storage)
  - Tracks session metadata and timestamps
  - Dependencies: `IBrowser` (Puppeteer), `ILogger<BrowserSession>`

- [ ] `IBrowserTab.cs`
  - Interface for a browser tab
  - Properties:
    - `string Id`
    - `string? Url`
    - `string? Title`
    - `TabStatus Status`
  - Methods:
    - `Task NavigateAsync(string url, NavigateOptions? options = null)`
    - `Task<string> GetHtmlAsync()`
    - `Task<string> GetContentAsync()`
    - `Task<byte[]> ScreenshotAsync(ScreenshotOptions? options = null)`
    - `Task ClickAsync(string selector, ClickOptions? options = null)`
    - `Task TypeAsync(string selector, string text, int delay = 0)`
    - `Task<ElementHandle?> QuerySelectorAsync(string selector)`
    - `Task<IElementHandle[]> QuerySelectorAllAsync(string selector)`
    - `Task<T> EvaluateExpressionAsync<T>(string script)`
    - `Task WaitForSelectorAsync(string selector, WaitForOptions? options = null)`
    - `Task WaitForNavigationAsync(WaitUntil waitUntil = WaitUntil.Load)`
    - `Task CloseAsync()`

- [ ] `BrowserTab.cs`
  - Implements `IBrowserTab`
  - Wraps Puppeteer `IPage` instance
  - Implements all navigation and interaction methods
  - Handles error mapping from Puppeteer exceptions
  - Dependencies: `IPage` (Puppeteer), `ILogger<BrowserTab>`

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
  - Map `NavigationException` → `NavigationException`
  - Map `SelectorException` → `SelectorException`
  - Map `EvaluationFailedException` → `ScriptExecutionException`
  - Map `TargetClosedException` → `SessionNotFoundException`

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
  ```csharp
  public interface ISessionService
  {
      Task<SessionResponse> CreateSessionAsync(SessionOptions options);
      Task<SessionResponse?> GetSessionAsync(string sessionId);
      Task<bool> CloseSessionAsync(string sessionId);
      Task<IEnumerable<SessionResponse>> GetAllSessionsAsync();
  }
  ```

- [ ] `SessionService.cs`
  - Implements `ISessionService`
  - Maps between domain models and API responses
  - Validates session options
  - Logs session lifecycle events
  - Dependencies: `IBrowserManager`, `ILogger<SessionService>`

- [ ] `INavigationService.cs`
  ```csharp
  public interface INavigationService
  {
      Task<NavigateResponse> NavigateAsync(string sessionId, string tabId, NavigateRequest request);
      Task<string> GetHtmlAsync(string sessionId, string tabId);
      Task<string> GetContentAsync(string sessionId, string tabId);
  }
  ```

- [ ] `NavigationService.cs`
  - Implements `INavigationService`
  - Validates URLs
  - Handles navigation timeouts
  - Returns page content
  - Dependencies: `IBrowserManager`, `ILogger<NavigationService>`

- [ ] `IDomService.cs`
  ```csharp
  public interface IDomService
  {
      Task ClickAsync(string sessionId, string tabId, ClickRequest request);
      Task TypeAsync(string sessionId, string tabId, TypeRequest request);
      Task<T> ExecuteJsAsync<T>(string sessionId, string tabId, ExecuteJsRequest request);
      Task WaitForSelectorAsync(string sessionId, string tabId, string selector, int timeout = 30000);
  }
  ```

- [ ] `DomService.cs`
  - Implements `IDomService`
  - Handles DOM manipulation operations
  - Validates selectors
  - Executes JavaScript safely
  - Dependencies: `IBrowserManager`, `ILogger<DomService>`

- [ ] `IScreenshotService.cs`
  ```csharp
  public interface IScreenshotService
  {
      Task<byte[]> CaptureAsync(string sessionId, string tabId, ScreenshotRequest request);
      Task<string> CaptureBase64Async(string sessionId, string tabId, ScreenshotRequest request);
  }
  ```

- [ ] `ScreenshotService.cs`
  - Implements `IScreenshotService`
  - Handles screenshot capture options
  - Supports viewport, element, full-page screenshots
  - Returns base64 or byte array
  - Dependencies: `IBrowserManager`, `ILogger<ScreenshotService>`

#### 1.4.2 Dependency Injection Setup
```
Core/
├── Extensions/
│   └── ServiceCollectionExtensions.cs
```

- [ ] `ServiceCollectionExtensions.cs`
  ```csharp
  public static class ServiceCollectionExtensions
  {
      public static IServiceCollection AddAgentBrowserCore(
          this IServiceCollection services,
          Action<BrowserSettings>? configure = null)
      {
          // Register services
          services.AddSingleton<IBrowserManager, BrowserManager>();
          services.AddScoped<ISessionService, SessionService>();
          services.AddScoped<INavigationService, NavigationService>();
          services.AddScoped<IDomService, DomService>();
          services.AddScoped<IScreenshotService, ScreenshotService>();
          
          // Register browser factory
          services.AddSingleton<BrowserFactory>();
          
          return services;
      }
  }
  ```

---

### 1.5 API Layer (`AgentBrowser.Api`)

#### 1.5.1 Program.cs & Startup
- [ ] `Program.cs`
  - Configure Kestrel
  - Set up logging
  - Configure dependency injection
  - Set up middleware pipeline
  - Map controllers

- [ ] `Startup.cs` (if using separate class)
  - Configure services
  - Configure middleware

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
  ```
  POST   /api/session              - Create session
  GET    /api/session/{id}         - Get session info
  DELETE /api/session/{id}         - Close session
  GET    /api/sessions             - List all sessions
  POST   /api/session/{id}/tab     - Create new tab
  DELETE /api/session/{id}/tab/{tabId} - Close tab
  ```

- [ ] `NavigationController.cs`
  ```
  POST /api/session/{id}/tab/{tabId}/navigate  - Navigate to URL
  GET  /api/session/{id}/tab/{tabId}/html      - Get rendered HTML
  POST /api/session/{id}/tab/{tabId}/back      - Go back
  POST /api/session/{id}/tab/{tabId}/forward   - Go forward
  POST /api/session/{id}/tab/{tabId}/reload    - Reload page
  ```

- [ ] `DomController.cs`
  ```
  POST /api/session/{id}/tab/{tabId}/click       - Click element
  POST /api/session/{id}/tab/{tabId}/type        - Type text
  POST /api/session/{id}/tab/{tabId}/execute     - Execute JavaScript
  POST /api/session/{id}/tab/{tabId}/wait        - Wait for selector
  GET  /api/session/{id}/tab/{tabId}/query       - Query elements
  ```

- [ ] `ScreenshotController.cs`
  ```
  GET  /api/session/{id}/tab/{tabId}/screenshot  - Capture screenshot
  POST /api/session/{id}/tab/{tabId}/screenshot  - Capture with options
  ```

- [ ] `HealthController.cs`
  ```
  GET /health          - Health check
  GET /health/ready    - Readiness check
  GET /health/live     - Liveness check
  ```

#### 1.5.3 Middleware
```
Api/
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── SessionValidationMiddleware.cs
```

- [ ] `ExceptionHandlingMiddleware.cs`
  - Catches all unhandled exceptions
  - Maps to appropriate HTTP status codes
  - Returns `ErrorResponse` JSON
  - Logs exceptions

- [ ] `RequestLoggingMiddleware.cs`
  - Logs incoming requests
  - Logs response status
  - Correlation ID tracking

- [ ] `SessionValidationMiddleware.cs`
  - Validates session exists for session-scoped routes
  - Returns 404 if session not found
  - Injects session into HttpContext.Items

#### 1.5.4 WebSocket Support
```
Api/
├── WebSockets/
│   ├── WebSocketHandler.cs
│   ├── WebSocketManager.cs
│   └── WebSocketMessage.cs
```

- [ ] `WebSocketHandler.cs`
  - Handles WebSocket connections
  - Manages connection lifecycle
  - Subscribes to browser events

- [ ] `WebSocketManager.cs`
  - Tracks active WebSocket connections
  - Broadcasts events to clients
  - Handles client subscriptions

- [ ] `WebSocketMessage.cs`
  ```csharp
  public class WebSocketMessage
  {
      public string Type { get; set; }  // console, network, dom-update, etc.
      public string SessionId { get; set; }
      public string? TabId { get; set; }
      public object Payload { get; set; }
      public DateTime Timestamp { get; set; }
  }
  ```

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
  - Test session creation
  - Test session retrieval
  - Test session cleanup
  - Test thread safety

- [ ] `SessionServiceTests.cs`
  - Test session creation with options
  - Test session validation
  - Test error handling

- [ ] `NavigationServiceTests.cs`
  - Test navigation
  - Test URL validation
  - Test timeout handling
  - Test content retrieval

- [ ] `DomServiceTests.cs`
  - Test click
  - Test type
  - Test JavaScript execution
  - Test wait for selector

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
  - Test POST /api/session
  - Test GET /api/session/{id}
  - Test DELETE /api/session/{id}

- [ ] `NavigationApiTests.cs`
  - Test navigation flow
  - Test HTML retrieval
  - Test navigation errors

- [ ] `TestServer.cs`
  - Configure test server
  - Mock browser if needed
  - Setup test client

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
  ```csharp
  public abstract class BrowserEvent
  {
      public string Type { get; set; }
      public string SessionId { get; set; }
      public string? TabId { get; set; }
      public DateTime Timestamp { get; set; }
  }
  ```

- [ ] `IEventBroadcaster.cs`
  - Subscribe to browser events
  - Broadcast to WebSocket clients
  - Filter events by session

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
  - `getTextContent(selector)`
  - `getAttribute(selector, attr)`
  - `getStyles(selector)`
  - `getComputedStyles(selector)`

- [ ] `QueryHelpers.js`
  - `queryByText(text)`
  - `queryByRegex(pattern)`
  - `queryByPosition(x, y)`

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
  ```csharp
  public interface IExtractor
  {
      Task<ExtractionResult> ExtractAsync(IBrowserTab tab);
  }
  
  public class ExtractionResult
  {
      public bool Success { get; set; }
      public object Data { get; set; }
      public List<string> Errors { get; set; }
  }
  ```

#### 3.1.2 Metadata Extractor
- [ ] Extract all `<meta>` tags
  - Name, property, content
- [ ] Extract Open Graph tags
  - `og:title`, `og:description`, `og:image`, `og:url`, etc.
- [ ] Extract Twitter Card tags
  - `twitter:card`, `twitter:title`, `twitter:image`, etc.
- [ ] Extract canonical URL
- [ ] Extract JSON-LD structured data
  - Parse and validate
  - Support multiple JSON-LD blocks
- [ ] Extract Microdata
  - `itemscope`, `itemtype`, `itemprop`
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
  ```json
  {
    "name": "Product",
    "fields": [
      {
        "name": "title",
        "selector": "h1.product-title",
        "type": "text"
      },
      {
        "name": "price",
        "selector": ".price",
        "type": "price"
      },
      {
        "name": "images",
        "selector": ".product-image img",
        "type": "image",
        "multiple": true
      }
    ]
  }
  ```
- [ ] Implement schema parser
- [ ] Implement schema executor
- [ ] Support field transformations
  - `trim`, `lowercase`, `uppercase`
  - `regex` extraction
  - Custom JavaScript transform

---

### 3.2 Extraction API Endpoints

#### 3.2.1 Controllers
```
Api/Controllers/
├── ExtractionController.cs
```

- [ ] `ExtractionController.cs`
  ```
  GET  /api/session/{id}/tab/{tabId}/extract/metadata   - Extract all metadata
  GET  /api/session/{id}/tab/{tabId}/extract/prices     - Extract price data
  GET  /api/session/{id}/tab/{tabId}/extract/images     - Extract images with context
  GET  /api/session/{id}/tab/{tabId}/extract/forms      - Extract form data
  GET  /api/session/{id}/tab/{tabId}/extract/links      - Extract link data
  POST /api/session/{id}/tab/{tabId}/extract/schema     - Extract using schema
  ```

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
  - JSON-RPC 2.0 compliant
  - Support `initialize`, `tools/list`, `tools/call`
- [ ] Implement stdio transport
- [ ] Implement HTTP transport (optional)

#### 4.1.3 Tool Definitions

- [ ] `BrowseNavigateTool`
  - Input: `{ url: string, timeout?: number }`
  - Output: `{ success: boolean, title: string, url: string }`

- [ ] `BrowseClickTool`
  - Input: `{ selector: string }`
  - Output: `{ success: boolean }`

- [ ] `BrowseTypeTool`
  - Input: `{ selector: string, text: string }`
  - Output: `{ success: boolean }`

- [ ] `BrowseGetHtmlTool`
  - Input: `{}`
  - Output: `{ html: string }`

- [ ] `BrowseGetScreenshotTool`
  - Input: `{ fullPage?: boolean }`
  - Output: `{ image: string }` (base64)

- [ ] `BrowseExecuteJsTool`
  - Input: `{ script: string }`
  - Output: `{ result: any }`

- [ ] `BrowseExtractSchemaTool`
  - Input: `{ schema: ExtractionSchema }`
  - Output: `{ data: object }`

- [ ] `BrowseGetMetadataTool`
  - Input: `{}`
  - Output: `{ metadata: MetadataResult }`

- [ ] `BrowseGetPricesTool`
  - Input: `{}`
  - Output: `{ prices: PriceResult[] }`

- [ ] `BrowseGetImagesTool`
  - Input: `{}`
  - Output: `{ images: ImageResult[] }`

- [ ] `BrowseGetFormsTool`
  - Input: `{}`
  - Output: `{ forms: FormResult[] }`

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
  ```python
  class AgentBrowserClient:
      def __init__(self, base_url: str):
          pass
      
      async def create_session(self) -> Session:
          pass
      
      async def navigate(self, url: str) -> NavigationResult:
          pass
      
      async def get_html(self) -> str:
          pass
      
      async def screenshot(self) -> bytes:
          pass
      
      async def extract_metadata(self) -> Metadata:
          pass
  ```

---

## Phase 5: Production Readiness (Week 8+)

### 5.1 Error Handling & Recovery

- [ ] Implement comprehensive error handling
  - Browser crash recovery
  - Session auto-restart
  - Tab recovery
- [ ] Add retry logic with exponential backoff
- [ ] Implement circuit breaker for failing sessions

### 5.2 Resource Management

- [ ] Implement session limits
  - Max sessions per instance
  - Max tabs per session
  - Memory limits
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
  - File sink
  - Console sink
  - Optional: Elasticsearch/Application Insights
- [ ] Request/response logging
- [ ] Performance metrics
  - Request duration
  - Browser operation duration
  - Memory usage
- [ ] Health check endpoints

### 5.5 Documentation

- [ ] OpenAPI/Swagger UI
- [ ] API documentation
- [ ] MCP tool documentation
- [ ] Architecture diagrams
- [ ] Usage examples

### 5.6 CI/CD

- [ ] GitHub Actions workflow
  - Build
  - Test
  - Docker build
  - Docker push
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
   - Create solution structure
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

---

## File Checklist

### Phase 1 Files (126 total tasks)
- Solution & project files: 15 tasks
- Domain models: 20 tasks
- Browser engine: 18 tasks
- Core services: 15 tasks
- API layer: 25 tasks
- Middleware: 8 tasks
- WebSocket: 10 tasks
- Tests: 15 tasks

### Phase 2 Files (45 total tasks)
- WebSocket infrastructure: 15 tasks
- Event streaming: 10 tasks
- JavaScript execution: 10 tasks
- Advanced DOM: 10 tasks

### Phase 3 Files (50 total tasks)
- Metadata extraction: 15 tasks
- Price extraction: 10 tasks
- Image extraction: 10 tasks
- Schema extraction: 15 tasks

### Phase 4 Files (25 total tasks)
- MCP server: 15 tasks
- Tool definitions: 10 tasks

### Phase 5 Files (30 total tasks)
- Error handling: 5 tasks
- Resource management: 5 tasks
- Security: 5 tasks
- Logging: 5 tasks
- Documentation: 5 tasks
- CI/CD: 5 tasks

**Total: ~276 tasks across all phases**