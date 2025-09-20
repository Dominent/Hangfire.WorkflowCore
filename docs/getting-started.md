---
layout: default
title: Getting Started
---

# Getting Started

Get up and running with Hangfire.WorkflowCore in minutes.

## Prerequisites

- .NET 6.0 or higher
- Understanding of [Hangfire](https://www.hangfire.io/) basics
- Basic knowledge of [WorkflowCore](https://github.com/danielgerlag/workflow-core) concepts

## Installation

Choose the packages you need:

```bash
# Core library (required)
dotnet add package Hangfire.WorkflowCore

# ASP.NET Core integration (recommended for web apps)
dotnet add package Hangfire.WorkflowCore.AspNetCore

# Standalone dashboard integration
dotnet add package Hangfire.WorkflowCore.Dashboard

# Abstractions only (for custom integrations)
dotnet add package Hangfire.WorkflowCore.Abstractions
```

## Quick Setup

### ASP.NET Core Applications

The simplest way to get started with automatic dashboard integration:

```csharp
using Hangfire.WorkflowCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire.WorkflowCore with automatic dashboard integration
builder.Services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseMemoryStorage());
    // Automatically includes:
    // - WorkflowCore integration
    // - Dashboard visualization
    // - HttpContext support

var app = builder.Build();

// Add Hangfire Dashboard (workflow info included automatically)
app.UseHangfireDashboard("/hangfire");

// Start WorkflowCore
var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();
workflowHost.RegisterWorkflow<YourWorkflow, YourData>();
await workflowHost.StartAsync(CancellationToken.None);

app.Run();
```

### Console/Worker Applications

For non-web applications:

```csharp
using Hangfire.WorkflowCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    // Add Hangfire.WorkflowCore
    services.AddHangfireWorkflowCore(
        hangfire => hangfire.UseMemoryStorage());
});

var host = builder.Build();

// Start services
await host.StartAsync();
var workflowHost = host.Services.GetRequiredService<IWorkflowHost>();
workflowHost.RegisterWorkflow<YourWorkflow, YourData>();
await workflowHost.StartAsync(CancellationToken.None);

// Your application logic here
await host.WaitForShutdownAsync();
```

## First Workflow

Create a simple workflow:

```csharp
using WorkflowCore.Interface;
using WorkflowCore.Models;

public class HelloWorldWorkflow : IWorkflow<string>
{
    public string Id => "hello-world";
    public int Version => 1;

    public void Build(IWorkflowBuilder<string> builder)
    {
        builder
            .StartWith<HelloStep>()
                .Input(step => step.Message, data => data)
            .Then<GoodbyeStep>()
                .Input(step => step.Message, data => "Goodbye from workflow!");
    }
}

public class HelloStep : StepBody
{
    public string Message { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        Console.WriteLine($"Hello: {Message}");
        return ExecutionResult.Next();
    }
}

public class GoodbyeStep : StepBody
{
    public string Message { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        Console.WriteLine($"Step: {Message}");
        return ExecutionResult.Next();
    }
}
```

## Execute Your First Workflow

```csharp
// Execute immediately
var jobId = BackgroundJobWorkflow.Instance
    .Enqueue<HelloWorldWorkflow, string>("Hello from Hangfire.WorkflowCore!");

// Schedule for later
var jobId = BackgroundJobWorkflow.Instance
    .ScheduleWorkflow<HelloWorldWorkflow, string>(
        "Scheduled message", 
        TimeSpan.FromMinutes(5));

// Create continuation workflows
var parentJobId = BackgroundJobWorkflow.Instance
    .Enqueue<HelloWorldWorkflow, string>("Parent workflow");

var childJobId = BackgroundJobWorkflow.Instance
    .ContinueWorkflowWith<HelloWorldWorkflow, string>(
        parentJobId, 
        "Child workflow runs after parent");
```

## View Dashboard

1. Navigate to `/hangfire` in your web application
2. Click on any workflow job to see detailed execution information
3. Monitor real-time progress, step details, and execution times

## Next Steps

- 📖 [Complete Guide](complete-guide) - Comprehensive documentation
- 🧪 [Examples](examples) - More complex workflow examples
- 🌐 [ASP.NET Core Integration](aspnetcore-integration) - HttpContext workflows
- 📊 [Dashboard Guide](dashboard) - Monitoring and visualization
- ⚙️ [Configuration](configuration) - Advanced setup options

## Common Issues

### Workflow Not Appearing in Dashboard

Make sure you're using the ASP.NET Core integration or have manually configured the dashboard:

```csharp
// ASP.NET Core (automatic)
services.AddHangfireWorkflowCoreAspNetCore(hangfire => hangfire.UseMemoryStorage());

// Manual configuration
services.AddHangfireWorkflowCore(hangfire => hangfire.UseMemoryStorage());
services.AddWorkflowDashboard();
GlobalConfiguration.Configuration.UseWorkflowJobDetailsRenderer(serviceProvider);
```

### Jobs Not Executing

Ensure you have started both [Hangfire](https://www.hangfire.io/) server and [WorkflowCore](https://github.com/danielgerlag/workflow-core) host:

```csharp
// ASP.NET Core applications
services.AddHangfireServer(); // Usually included in AddHangfireWorkflowCoreAspNetCore

// Console applications
var workflowHost = host.Services.GetRequiredService<IWorkflowHost>();
await workflowHost.StartAsync(CancellationToken.None);
```

---

Need help? Check the [FAQ](faq) or [open an issue](https://github.com/Dominent/Hangfire.WorkflowCore/issues).