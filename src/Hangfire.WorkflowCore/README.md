# WorkflowCoreHangfire

[![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire)

Core library for integrating [Hangfire](https://www.hangfire.io/) job scheduling with [WorkflowCore](https://github.com/danielgerlag/workflow-core) workflow orchestration.

## Features

🚀 **Seamless workflow execution** - Execute WorkflowCore workflows as Hangfire background jobs  
⚙️ **Flexible configuration** - Works with any Hangfire storage provider  
🛡️ **Production ready** - Built on Hangfire's proven reliability  
📊 **Monitoring support** - Basic workflow tracking and monitoring  

## Installation

```bash
# Core library
dotnet add package WorkflowCoreHangfire

# For ASP.NET Core applications (recommended)
dotnet add package WorkflowCoreHangfire.AspNetCore
```

## Quick Start

### Console/Worker Applications

```csharp
using Hangfire.WorkflowCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    services.AddHangfireWorkflowCore(
        hangfire => hangfire.UseMemoryStorage());
});

var host = builder.Build();
await host.StartAsync();

// Execute workflows as background jobs
var jobId = BackgroundJobWorkflow.Instance
    .Enqueue<YourWorkflow, YourData>(workflowData);
```

### ASP.NET Core Applications

For web applications, use **[WorkflowCoreHangfire.AspNetCore](https://www.nuget.org/packages/WorkflowCoreHangfire.AspNetCore)** instead for automatic dashboard integration and HttpContext support.

## Documentation

📖 **[Getting Started Guide](https://dominent.github.io/Hangfire.WorkflowCore/getting-started)**  
📖 **[Complete Documentation](https://dominent.github.io/Hangfire.WorkflowCore/)**

## Support

💖 **Support this project:** [Buy me a coffee](https://buymeacoffee.com/ppavlov)

## License

Licensed under **LGPL v3** - see [LICENSE](https://github.com/Dominent/Hangfire.WorkflowCore/blob/main/LICENSE) for details.