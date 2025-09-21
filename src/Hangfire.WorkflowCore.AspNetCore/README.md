# WorkflowCoreHangfire.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.AspNetCore.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire.AspNetCore)

**🚀 The complete ASP.NET Core integration** for combining [Hangfire](https://www.hangfire.io/) job scheduling with [WorkflowCore](https://github.com/danielgerlag/workflow-core) workflow orchestration.

## Features

✨ **One-line setup** - Automatic configuration with dashboard integration  
🌐 **HttpContext support** - Access HTTP request context in background workflows  
📊 **Rich dashboard** - Beautiful workflow visualization and monitoring  
🛡️ **Production ready** - Built on proven technologies  

## Installation

```bash
dotnet add package WorkflowCoreHangfire.AspNetCore
```

## Quick Start

### 1. Configure Services

```csharp
using Hangfire.WorkflowCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// One-line setup with automatic dashboard integration
builder.Services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseMemoryStorage());
    
// Includes:
// ✅ WorkflowCore integration
// ✅ Dashboard visualization  
// ✅ HttpContext support
// ✅ Background job processing

var app = builder.Build();
```

### 2. Add Dashboard

```csharp
// Workflow information included automatically
app.UseHangfireDashboard("/hangfire");

app.Run();
```

### 3. Create a Workflow

```csharp
public class MyWorkflow : IWorkflow<MyData>
{
    public string Id => "my-workflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<MyData> builder)
    {
        builder
            .StartWith<ProcessDataStep>()
                .Input(step => step.Data, data => data)
            .Then<SendNotificationStep>();
    }
}
```

### 4. Execute Workflows

```csharp
// Execute immediately
var jobId = BackgroundJobWorkflow.Instance
    .Enqueue<MyWorkflow, MyData>(new MyData { Value = "Hello" });

// Schedule for later
var scheduledJobId = BackgroundJobWorkflow.Instance
    .ScheduleWorkflow<MyWorkflow, MyData>(
        new MyData { Value = "Scheduled" }, 
        TimeSpan.FromMinutes(30));
```

## HttpContext Access

Access HTTP request context in your workflow steps:

```csharp
public class HttpAwareStep : StepBody
{
    private readonly IHttpContextSnapshotProvider _httpContext;
    
    public HttpAwareStep(IHttpContextSnapshotProvider httpContext)
    {
        _httpContext = httpContext;
    }
    
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var userAgent = _httpContext.UserAgent;
        var userId = _httpContext.UserId;
        
        // Use HTTP context data in background workflow
        return ExecutionResult.Next();
    }
}
```

## Dashboard Features

Navigate to `/hangfire` to see:

- 🔍 **Workflow Details**: Complete execution information
- 📈 **Step Progress**: Real-time step monitoring  
- 🎯 **Data Visualization**: Input/output data for each step
- 🚨 **Error Tracking**: Detailed error information

## Advanced Configuration

```csharp
services.AddHangfireWorkflowCoreAspNetCore(
    hangfire =>
    {
        hangfire.UseSqlServerStorage(connectionString);
        hangfire.UseColouredConsoleLogProvider();
    },
    workflow =>
    {
        workflow.UseSqlServer(connectionString, true, true);
        workflow.UseLockProvider(new SqlServerLockProvider(connectionString));
    });
```

## Package Relationships

- **WorkflowCoreHangfire** - Core integration library
- **WorkflowCoreHangfire.Dashboard** - Dashboard visualization  
- **WorkflowCoreHangfire.Abstractions** - Interfaces and contracts

This package includes all dependencies for a complete solution.

## Documentation

📖 **[Getting Started Guide](https://dominent.github.io/Hangfire.WorkflowCore/getting-started)**  
📖 **[ASP.NET Core Integration](https://dominent.github.io/Hangfire.WorkflowCore/aspnetcore-integration)**  
📖 **[Complete Documentation](https://dominent.github.io/Hangfire.WorkflowCore/)**

## Support

💖 **Support this project:** [Buy me a coffee](https://buymeacoffee.com/ppavlov)

## License

Licensed under **LGPL v3** - see [LICENSE](https://github.com/Dominent/Hangfire.WorkflowCore/blob/main/LICENSE) for details.