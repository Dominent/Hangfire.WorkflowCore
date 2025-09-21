# WorkflowCoreHangfire.Dashboard

[![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.Dashboard.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire.Dashboard)

Dashboard integration for visualizing [WorkflowCore](https://github.com/danielgerlag/workflow-core) workflows in the [Hangfire](https://www.hangfire.io/) dashboard.

## Features

📊 **Rich visualization** - See workflow steps, execution times, and progress  
🔍 **Detailed monitoring** - Step-by-step workflow execution tracking  
🎨 **Beautiful UI** - Clean, intuitive dashboard integration  
⚡ **Real-time updates** - Live progress monitoring  

## Installation

```bash
# Standalone dashboard integration
dotnet add package WorkflowCoreHangfire.Dashboard

# Or use the complete ASP.NET Core package (recommended)
dotnet add package WorkflowCoreHangfire.AspNetCore
```

## Usage

### Manual Configuration

```csharp
using Hangfire.WorkflowCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.Services.AddHangfireWorkflowCore(
    hangfire => hangfire.UseMemoryStorage());

// Add dashboard integration
builder.Services.AddWorkflowDashboard();

var app = builder.Build();

// Configure dashboard with workflow renderer
GlobalConfiguration.Configuration
    .UseWorkflowJobDetailsRenderer(app.Services);

app.UseHangfireDashboard("/hangfire");
```

### Automatic Configuration (Recommended)

Use **[WorkflowCoreHangfire.AspNetCore](https://www.nuget.org/packages/WorkflowCoreHangfire.AspNetCore)** for automatic setup:

```csharp
// One-line setup with automatic dashboard integration
builder.Services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseMemoryStorage());
```

## What You'll See

- 🔍 **Job Details**: Complete workflow execution information
- 📈 **Step Progress**: Individual step status and execution times  
- 🎯 **Data Flow**: Input/output data for each workflow step
- 🚨 **Error Tracking**: Detailed error information and stack traces

## Documentation

📖 **[Dashboard Guide](https://dominent.github.io/Hangfire.WorkflowCore/dashboard)**  
📖 **[Complete Documentation](https://dominent.github.io/Hangfire.WorkflowCore/)**

## Support

💖 **Support this project:** [Buy me a coffee](https://buymeacoffee.com/ppavlov)

## License

Licensed under **LGPL v3** - see [LICENSE](https://github.com/Dominent/Hangfire.WorkflowCore/blob/main/LICENSE) for details.