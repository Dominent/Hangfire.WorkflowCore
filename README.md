# Hangfire.WorkflowCore

[![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire)
[![Build](https://img.shields.io/github/actions/workflow/status/Dominent/Hangfire.WorkflowCore/ci.yml?branch=main)](https://github.com/Dominent/Hangfire.WorkflowCore/actions)
[![License](https://img.shields.io/github/license/Dominent/Hangfire.WorkflowCore)](LICENSE)
[![Documentation](https://img.shields.io/badge/docs-github%20pages-blue)](https://dominent.github.io/Hangfire.WorkflowCore/)

A powerful integration library that combines [Hangfire](https://www.hangfire.io/)'s robust job scheduling capabilities with [WorkflowCore](https://github.com/danielgerlag/workflow-core)'s advanced workflow orchestration engine.

> **💖 Support this project:** If this library helps you, consider [buying me a coffee](https://buymeacoffee.com/ppavlov) to support continued development and maintenance!

## 📖 Documentation

**👉 [Complete Documentation Website](https://dominent.github.io/Hangfire.WorkflowCore/)**

- 🚀 [Getting Started Guide](https://dominent.github.io/Hangfire.WorkflowCore/getting-started)
- 📖 [Complete Documentation](https://dominent.github.io/Hangfire.WorkflowCore/complete-guide)
- 🧪 [Examples & Use Cases](https://dominent.github.io/Hangfire.WorkflowCore/examples)
- 🔧 [API Reference](https://dominent.github.io/Hangfire.WorkflowCore/api-reference)
- 📊 [Dashboard Integration](https://dominent.github.io/Hangfire.WorkflowCore/dashboard)

## Features

✨ **One-line setup** with automatic [Hangfire](https://www.hangfire.io/) dashboard integration  
🚀 **Seamless workflow execution** - Bridge job scheduling with [WorkflowCore](https://github.com/danielgerlag/workflow-core) orchestration  
🌐 **HttpContext support** - Access HTTP request context in background workflows  
📊 **Real-time monitoring** - Dashboard visualization with step-by-step progress tracking  
🛡️ **Production ready** - Built on [Hangfire](https://www.hangfire.io/)'s proven reliability and [WorkflowCore](https://github.com/danielgerlag/workflow-core)'s orchestration

## Quick Start

```bash
# Install for ASP.NET Core (recommended)
dotnet add package WorkflowCoreHangfire.AspNetCore
```

```csharp
// Configure services (ASP.NET Core)
services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseMemoryStorage());

// Execute workflows
var jobId = BackgroundJobWorkflow.Instance
    .Enqueue<YourWorkflow, YourData>(data);
```

👉 **[Get Started in 5 Minutes →](https://dominent.github.io/Hangfire.WorkflowCore/getting-started)**

## Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| **WorkflowCoreHangfire.AspNetCore** | ASP.NET Core integration with HttpContext support | [![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.AspNetCore.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire.AspNetCore) |
| **WorkflowCoreHangfire** | Core library for console/worker applications | [![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire) |
| **WorkflowCoreHangfire.Dashboard** | Standalone dashboard integration | [![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.Dashboard.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire.Dashboard) |
| **WorkflowCoreHangfire.Abstractions** | Interfaces and abstractions | [![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.Abstractions.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire.Abstractions) |

## License

Licensed under **LGPL v3** to ensure compatibility with [Hangfire](https://www.hangfire.io/)'s licensing. See [LICENSE](LICENSE) for details.

## Dependencies

- **[Hangfire](https://www.hangfire.io/)** (≥1.8.14) - Job scheduling and processing
- **[WorkflowCore](https://github.com/danielgerlag/workflow-core)** (≥3.10.0) - Workflow orchestration engine
- **[Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)** (≥8.0.0) - Dependency injection
- **[Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)** (≥8.0.0) - Logging abstractions