# Hangfire.WorkflowCore

[![NuGet](https://img.shields.io/nuget/v/Hangfire.WorkflowCore.svg)](https://www.nuget.org/packages/Hangfire.WorkflowCore)
[![Build](https://img.shields.io/github/workflow/status/username/Hangfire.WorkflowCore/Build)](https://github.com/username/Hangfire.WorkflowCore/actions)
[![License](https://img.shields.io/github/license/username/Hangfire.WorkflowCore)](LICENSE)

A powerful integration library that combines [Hangfire](https://www.hangfire.io/)'s robust job scheduling capabilities with [WorkflowCore](https://github.com/danielgerlag/workflow-core)'s advanced workflow orchestration engine.

## Features

- 🚀 **Seamless Integration** - Bridge Hangfire's job scheduling with WorkflowCore's workflow orchestration
- 📅 **Flexible Scheduling** - Immediate, delayed, recurring, and continuation workflows
- 🔄 **Workflow Orchestration** - Complex multi-step workflows with conditional logic and parallel execution
- 🛡️ **Reliable Execution** - Built on Hangfire's proven reliability and persistence
- 🏗️ **Clean Architecture** - Well-defined abstractions and interfaces for extensibility
- 📊 **Production Ready** - Comprehensive error handling, logging, and monitoring support

## Quick Start

### Installation

```bash
# Install the core package
dotnet add package Hangfire.WorkflowCore

# Install abstractions (if building custom integrations)
dotnet add package Hangfire.WorkflowCore.Abstractions
```

### Basic Usage

1. **Configure Services**

```csharp
services.AddHangfire(config => config.UseMemoryStorage());
services.AddHangfireServer();
services.AddWorkflow();

// Add your storage bridge implementation
services.AddSingleton<IWorkflowStorageBridge, YourStorageBridge>();
services.AddSingleton<IWorkflowInstanceProvider, YourInstanceProvider>();
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
// Execute immediately
var jobId = BackgroundJobWorkflow.Enqueue<VideoProcessingWorkflow, VideoData>(videoData);

// Execute with delay
var jobId = BackgroundJobWorkflow.ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(
    videoData, TimeSpan.FromMinutes(5));

// Execute at specific time
var jobId = BackgroundJobWorkflow.ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(
    videoData, DateTimeOffset.Now.AddHours(1));

// Create continuation workflow
var jobId = BackgroundJobWorkflow.ContinueWorkflowWith<VideoProcessingWorkflow, VideoData>(
    parentJobId, videoData);

// Recurring workflow
RecurringJobWorkflow.AddOrUpdateWorkflow<VideoProcessingWorkflow, VideoData>(
    "daily-processing", videoData, "0 2 * * *"); // Daily at 2 AM
```

## Architecture

### Core Components

- **WorkflowJob<TWorkflow, TData>** - Executes WorkflowCore workflows within Hangfire jobs
- **BackgroundJobWorkflow** - Extension methods for scheduling workflow jobs
- **RecurringJobWorkflow** - Extension methods for recurring workflow jobs
- **IWorkflowStorageBridge** - Abstraction for storing job-workflow mappings and results
- **IWorkflowInstanceProvider** - Abstraction for accessing workflow instances

### Data Flow

```
Hangfire Job → WorkflowJob → WorkflowCore Engine → Workflow Steps → Results Storage
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

- **VideoProcessing** - Demonstrates video processing workflows with analysis, conversion, upload, and cleanup steps

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

### v1.0.0
- Initial release
- Core workflow execution capabilities
- Background job scheduling extensions
- Recurring workflow support
- Comprehensive test suite
- Sample applications

## Support

- 📖 [Documentation](https://github.com/username/Hangfire.WorkflowCore/wiki)
- 🐛 [Issue Tracker](https://github.com/username/Hangfire.WorkflowCore/issues)
- 💬 [Discussions](https://github.com/username/Hangfire.WorkflowCore/discussions)

---

Built with ❤️ using Test-Driven Development and modern .NET practices.