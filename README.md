# Hangfire.WorkflowCore

[![NuGet](https://img.shields.io/nuget/v/Hangfire.WorkflowCore.svg)](https://www.nuget.org/packages/Hangfire.WorkflowCore)
[![Build](https://img.shields.io/github/workflow/status/username/Hangfire.WorkflowCore/Build)](https://github.com/username/Hangfire.WorkflowCore/actions)
[![License](https://img.shields.io/github/license/username/Hangfire.WorkflowCore)](LICENSE)

A powerful integration library that combines [Hangfire](https://www.hangfire.io/)'s robust job scheduling capabilities with [WorkflowCore](https://github.com/danielgerlag/workflow-core)'s advanced workflow orchestration engine.

## Features

- 🚀 **Seamless Integration** - Bridge Hangfire's job scheduling with WorkflowCore's workflow orchestration
- 🌐 **HttpContext Integration** - Access HTTP request context in background workflows with full serialization support
- 📊 **Automatic Dashboard** - Real-time workflow monitoring with automatic Hangfire dashboard integration
- 📅 **Flexible Scheduling** - Immediate, delayed, recurring, and continuation workflows
- 🔄 **Workflow Orchestration** - Complex multi-step workflows with conditional logic and parallel execution
- 🛡️ **Reliable Execution** - Built on Hangfire's proven reliability and persistence
- 🏗️ **Clean Architecture** - Well-defined abstractions and interfaces for extensibility
- 📈 **Production Ready** - Comprehensive error handling, logging, and monitoring support
- ⚡ **Simplified Setup** - One-line configuration with automatic dashboard and WorkflowCore integration
- 🎯 **Clean API** - Singleton pattern with intuitive extension methods
- 🧪 **Test-Driven** - Comprehensive test coverage (104+ tests) following TDD principles

## Quick Start

### Installation

```bash
# Install the core package
dotnet add package Hangfire.WorkflowCore

# Install ASP.NET Core integration (for HttpContext workflows + automatic dashboard)
dotnet add package Hangfire.WorkflowCore.AspNetCore

# Install dashboard (for standalone dashboard integration)
dotnet add package Hangfire.WorkflowCore.Dashboard

# Install abstractions (if building custom integrations)
dotnet add package Hangfire.WorkflowCore.Abstractions
```

### Basic Usage

1. **Configure Services**

```csharp
// Basic setup (Console/Worker applications)
services.AddHangfireWorkflowCore(
    // Configure Hangfire (storage, dashboard, etc.)
    hangfire => hangfire.UseMemoryStorage());
    // WorkflowCore integration uses sensible defaults:
    // - InMemoryWorkflowStorageBridge for storage
    // - WorkflowCoreInstanceProvider for real WorkflowCore integration

// ASP.NET Core setup with HttpContext integration + automatic dashboard (Web applications)
services.AddHangfireWorkflowCoreAspNetCore(
    // Configure Hangfire
    hangfire => hangfire.UseMemoryStorage());
    // Includes everything above PLUS:
    // - HttpContext capture and serialization
    // - Automatic dashboard services registration
    // - Automatic dashboard renderer configuration
    
// Optional: Custom implementations
services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseSqlServerStorage("connection-string"),
    workflow =>
    {
        workflow.UseStorageBridge<CustomStorageBridge>();
        workflow.UseInstanceProvider<CustomInstanceProvider>();
    });
```

2. **Define a Workflow**

```csharp
public class VideoProcessingWorkflow : IWorkflow<VideoData>
{
    public string Id => "VideoProcessingWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<VideoData> builder)
    {
        builder
            .StartWith<AnalyzeVideoStep>()
            .Then<ConvertVideoStep>()
            .Then<UploadVideoStep>()
            .Then<CleanupStep>();
    }
}

public class VideoData
{
    public string VideoId { get; set; }
    public string FilePath { get; set; }
    public VideoQuality Quality { get; set; }
}
```

3. **Schedule Workflows**

```csharp
// Basic workflows (without HttpContext)
var jobId = BackgroundJobWorkflow.Instance.Enqueue<VideoProcessingWorkflow, VideoData>(videoData);
var jobId = BackgroundJobWorkflow.Instance.ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(
    videoData, TimeSpan.FromMinutes(5));
var jobId = BackgroundJobWorkflow.Instance.ContinueWorkflowWith<VideoProcessingWorkflow, VideoData>(
    parentJobId, videoData);

// HttpContext-aware workflows (ASP.NET Core only)
var jobId = BackgroundJobWorkflow.Instance.EnqueueWithHttpContext<HttpContextVideoWorkflow, VideoData>(videoData);
var jobId = BackgroundJobWorkflow.Instance.ScheduleWithHttpContext<HttpContextVideoWorkflow, VideoData>(
    videoData, TimeSpan.FromMinutes(5));
var jobId = BackgroundJobWorkflow.Instance.ContinueWithHttpContext<HttpContextVideoWorkflow, VideoData>(
    parentJobId, videoData);

// Recurring workflows
RecurringJobWorkflow.Instance.AddOrUpdateWorkflow<VideoProcessingWorkflow, VideoData>(
    "daily-processing", videoData, "0 2 * * *"); // Daily at 2 AM
RecurringJobWorkflow.Instance.AddOrUpdateWithHttpContext<HttpContextVideoWorkflow, VideoData>(
    "daily-processing-with-context", videoData, "0 2 * * *");
```

## Workflow Dashboard Integration

### Automatic Dashboard Setup (ASP.NET Core)

When using `AddHangfireWorkflowCoreAspNetCore()`, the library automatically configures the Hangfire dashboard to show detailed workflow information for each job:

```csharp
// Single line setup - dashboard integration is automatic!
services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseMemoryStorage());
    
// Configure Hangfire dashboard (workflow renderer is automatically enabled)
app.UseHangfireDashboard("/hangfire");
```

### Dashboard Features

The integrated dashboard displays:

- **Real-time Workflow Status** - Running, Complete, Failed, Suspended
- **Step-by-Step Progress** - Each workflow step with timing and status
- **Execution Timing** - Start time, end time, and duration for each step
- **Progress Indicators** - Visual progress bars and completion percentages
- **Error Details** - Detailed error messages and stack traces when steps fail
- **Workflow Metadata** - Workflow ID, job ID, and execution context

### Manual Dashboard Setup (Console Applications)

For non-ASP.NET Core applications, you can manually add dashboard support:

```csharp
// Add dashboard services
services.AddWorkflowDashboard();

// Configure the dashboard renderer
GlobalConfiguration.Configuration.UseWorkflowJobDetailsRenderer(serviceProvider);
```

### Real WorkflowCore Integration

The dashboard shows **real workflow execution data** from WorkflowCore, including:

- Actual step names from your workflow definitions (`AnalyzeVideoStep`, `ConvertVideoStep`, etc.)
- Real execution timing and status from WorkflowCore's persistence layer
- Live workflow instance data with proper state management

## Architecture

### Core Components

- **WorkflowJob<TWorkflow, TData>** - Executes basic WorkflowCore workflows within Hangfire jobs
- **HttpContextWorkflowJob<TWorkflow, TData>** - Executes workflows with captured HttpContext data
- **BackgroundJobWorkflow** - Singleton with methods for scheduling workflow jobs
- **RecurringJobWorkflow** - Singleton with methods for recurring workflow jobs
- **IWorkflowStorageBridge** - Abstraction for storing job-workflow mappings and results
- **IWorkflowInstanceProvider** - Abstraction for accessing workflow instances (real WorkflowCore integration)
- **IHttpContextSnapshotProvider** - Abstraction for capturing and serializing HttpContext data

### Dashboard Components

- **IWorkflowDashboardService** - Main service for rendering workflow information in Hangfire dashboard
- **IWorkflowDataProvider** - Retrieves workflow execution data from WorkflowCore persistence layer
- **IWorkflowStatusCalculator** - Calculates workflow progress, timing, and completion percentages
- **IWorkflowRenderer** - Renders workflow information as HTML for dashboard display
- **WorkflowJobDetailsExtension** - Hangfire extension that integrates workflow info into job details pages

### Data Flow

```
Basic Workflow:
Hangfire Job → WorkflowJob → WorkflowCore Engine → Workflow Steps → Results Storage

HttpContext Workflow:
ASP.NET Core Request → HttpContext Capture → Hangfire Job → HttpContextWorkflowJob → 
WorkflowCore Engine → Workflow Steps (with HttpContext) → Results Storage
```

## HttpContext Integration

### HttpContext-Aware Workflows

Access HTTP request context data in your background workflows:

```csharp
public class HttpContextVideoProcessingWorkflow : IWorkflow<WorkflowDataWithContext<VideoData>>
{
    public string Id => "HttpContextVideoProcessingWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<WorkflowDataWithContext<VideoData>> builder)
    {
        builder
            .StartWith<AnalyzeVideoWithContextStep>()
            .Then<ConvertVideoStep>()
            .Then<UploadVideoStep>()
            .Then<CleanupStep>();
    }
}

public class AnalyzeVideoWithContextStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var dataWithContext = (WorkflowDataWithContext<VideoData>)context.Workflow.Data;
        var videoData = dataWithContext.Data;
        var httpContext = dataWithContext.HttpContext;

        // Access original HTTP request data
        var userAgent = httpContext?.UserAgent;
        var requestPath = httpContext?.RequestPath;
        var userId = httpContext?.Headers?.FirstOrDefault(h => h.Key == "X-User-Id")?.Value;

        // Your processing logic with HTTP context awareness
        Console.WriteLine($"Processing video {videoData.VideoId} from request {requestPath}");
        
        return ExecutionResult.Next();
    }
}
```

### HttpContext Data Structure

The captured HttpContext includes:

```csharp
public class HttpContextSnapshot
{
    public string? RequestPath { get; set; }
    public string? Method { get; set; }
    public string? UserAgent { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    public string? UserId { get; set; }  // From User.Identity.Name
    public DateTime CapturedAt { get; set; }
}
```

### ASP.NET Core Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    [HttpPost("process-with-context")]
    public ActionResult ProcessVideoWithContext([FromBody] VideoProcessRequest request)
    {
        var videoData = new VideoData
        {
            VideoId = request.VideoId,
            FilePath = request.FilePath,
            Quality = request.Quality
        };

        // HttpContext is automatically captured and passed to the workflow
        var jobId = BackgroundJobWorkflow.Instance.EnqueueWithHttpContext<HttpContextVideoProcessingWorkflow, VideoData>(videoData);

        return Ok(new { JobId = jobId, Status = "Enqueued with HttpContext" });
    }
}
```

## Advanced Usage

### Custom Storage Bridge

Implement `IWorkflowStorageBridge` for your persistence layer:

```csharp
public class PostgreSqlWorkflowStorageBridge : IWorkflowStorageBridge
{
    public async Task StoreJobWorkflowMappingAsync(string jobId, string workflowInstanceId)
    {
        // Store mapping in PostgreSQL
    }
    
    public async Task<WorkflowExecutionResult?> GetWorkflowResultAsync(string workflowInstanceId)
    {
        // Retrieve result from PostgreSQL
    }
    
    // Implement other interface methods...
}

// Register in your configuration
services.AddHangfireWorkflowCore(
    hangfire => hangfire.UseSqlServerStorage("connection-string"),
    workflow => workflow.UseStorageBridge<PostgreSqlWorkflowStorageBridge>());
```

### Error Handling

```csharp
public class RobustVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        try
        {
            // Your step logic
            return ExecutionResult.Next();
        }
        catch (Exception ex)
        {
            // Log error
            return ExecutionResult.Outcome(StepOutcome.Failure)
                .WithErrorMessage(ex.Message);
        }
    }
}
```

### Monitoring and Observability

The library provides comprehensive logging through Microsoft.Extensions.Logging:

```csharp
services.AddLogging(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Information));
```

## Sample Applications

Check out the `/samples` directory for complete examples:

### VideoProcessing.Console
Console application demonstrating basic workflow capabilities:
- Immediate workflow execution
- Scheduled workflows with delays  
- Workflow continuation (dependent jobs)
- Basic WorkflowJob integration

### VideoProcessing.Web
ASP.NET Core web application showcasing HttpContext integration and automatic dashboard:
- **Swagger UI** at `/swagger` - Interactive API documentation
- **Hangfire Dashboard** at `/hangfire` - Job monitoring with automatic workflow information display
- **Workflow Dashboard** - Real-time workflow step monitoring, progress tracking, and execution details
- **API Endpoints**:
  - `POST /api/videos/process` - Basic workflow execution
  - `POST /api/videos/process-with-context` - HttpContext-aware workflow
  - `POST /api/videos/schedule` - Scheduled workflow execution
  - `GET /api/videos/health` - Health check endpoint

### VideoProcessing.Core
Shared library containing:
- Workflow definitions (`VideoProcessingWorkflow`, `HttpContextVideoProcessingWorkflow`)
- Data models (`VideoData`, `VideoQuality`)
- Mock service implementations for development/testing

## Storage Providers

While the library is storage-agnostic, here are recommended approaches:

### Development/Testing
- **Hangfire.MemoryStorage** - In-memory storage for development and testing
- **Memory-based IWorkflowStorageBridge** - Simple dictionary-based implementation

### Production
- **Hangfire with SQL Server/PostgreSQL** - Reliable persistence for job storage
- **Dapper-based IWorkflowStorageBridge** - Efficient database access as suggested by user requirements
- **Redis/MongoDB** - For high-performance scenarios

## Performance Considerations

- **Workflow Complexity** - Break large workflows into smaller, composable parts
- **Data Size** - Keep workflow data lightweight; store large objects separately
- **Concurrency** - Configure Hangfire worker counts based on your workload
- **Monitoring** - Use Hangfire Dashboard and custom metrics for observability

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## Dependencies

- **Hangfire.Core** (≥1.8.14) - Job scheduling and processing
- **WorkflowCore** (≥3.10.0) - Workflow orchestration engine
- **Microsoft.Extensions.DependencyInjection** (≥8.0.0) - Dependency injection
- **Microsoft.Extensions.Logging** (≥8.0.0) - Logging abstractions

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

### v3.0.0 (Current)
- **🔥 Automatic Dashboard Integration** - Real-time workflow monitoring automatically configured with `AddHangfireWorkflowCoreAspNetCore()`
- **📊 Real WorkflowCore Integration** - Dashboard shows actual workflow execution data with real step names and timing
- **⚡ One-Line Setup** - Simplified configuration with automatic dashboard renderer registration
- **🧪 Enhanced Testing** - 104+ tests including TDD implementation of dashboard features
- **🏗️ New Dashboard Project**: `Hangfire.WorkflowCore.Dashboard` - Complete workflow visualization system
- **🚀 Production Defaults** - Library provides `WorkflowCoreInstanceProvider` and `InMemoryWorkflowStorageBridge` out-of-the-box
- **📈 Step-by-Step Monitoring** - Detailed progress tracking, timing, and status for each workflow step
- **Breaking Changes** - Enhanced `AddHangfireWorkflowCoreAspNetCore()` now includes automatic dashboard configuration

### v2.0.0
- **HttpContext Integration** - Complete ASP.NET Core integration with HttpContext capture and serialization
- **Clean API Design** - Singleton pattern replacing static classes (`BackgroundJobWorkflow.Instance`)
- **Enhanced Setup** - `AddHangfireWorkflowCoreAspNetCore()` for simplified ASP.NET Core configuration
- **New Projects**: 
  - `Hangfire.WorkflowCore.AspNetCore` - ASP.NET Core-specific extensions
  - `Hangfire.WorkflowCore.Abstractions` - Shared abstractions and interfaces
- **Comprehensive Testing** - 52+ tests with full TDD coverage
- **Sample Applications** - Console and Web samples demonstrating all features
- **Breaking Changes** - API changed from static to singleton pattern

### v1.0.0
- Initial release
- Core workflow execution capabilities
- Background job scheduling extensions
- Recurring workflow support
- Simplified one-line setup with `AddHangfireWorkflowCore()`
- Job ID bug fix using TDD approach with PerformContext injection
- Comprehensive test suite (30 tests)
- Sample applications

## Support

- 📖 [Documentation](https://github.com/username/Hangfire.WorkflowCore/wiki)
- 🐛 [Issue Tracker](https://github.com/username/Hangfire.WorkflowCore/issues)
- 💬 [Discussions](https://github.com/username/Hangfire.WorkflowCore/discussions)

---

Built with ❤️ using Test-Driven Development and modern .NET practices.