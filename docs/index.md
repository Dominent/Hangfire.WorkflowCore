# Hangfire.WorkflowCore Documentation

A powerful integration library that combines [Hangfire](https://www.hangfire.io/)'s robust job scheduling capabilities with [WorkflowCore](https://github.com/danielgerlag/workflow-core)'s advanced workflow orchestration engine.

> **💖 Support this project:** If this library helps you, consider [buying me a coffee](https://buymeacoffee.com/ppavlov) to support continued development and maintenance!

## Quick Links

- 🚀 [**Getting Started**](getting-started.md) - Installation and basic setup
- 📖 [**Complete Guide**](complete-guide.md) - Comprehensive documentation
- 🧪 [**Examples**](examples.md) - Sample code and use cases
- 🔧 [**API Reference**](api-reference.md) - Detailed API documentation
- 📈 [**Dashboard**](dashboard.md) - Workflow monitoring and visualization
- 🌐 [**ASP.NET Core Integration**](aspnetcore-integration.md) - HttpContext workflows
- ⚙️ [**Configuration**](configuration.md) - Advanced configuration options
- 🔄 [**Migration Guide**](migration-guide.md) - Upgrading from older versions
- ❓ [**FAQ**](faq.md) - Frequently asked questions
- 📋 [**Changelog**](changelog.md) - Version history and updates

## Features Overview

- 🚀 **Seamless Integration** - Bridge [Hangfire](https://www.hangfire.io/)'s job scheduling with [WorkflowCore](https://github.com/danielgerlag/workflow-core)'s workflow orchestration
- 🌐 **HttpContext Integration** - Access HTTP request context in background workflows with full serialization support
- 📊 **Automatic Dashboard** - Real-time workflow monitoring with automatic [Hangfire](https://www.hangfire.io/) dashboard integration
- 📅 **Flexible Scheduling** - Immediate, delayed, recurring, and continuation workflows
- 🔄 **Workflow Orchestration** - Complex multi-step workflows with conditional logic and parallel execution
- 🛡️ **Reliable Execution** - Built on [Hangfire](https://www.hangfire.io/)'s proven reliability and persistence
- 🏗️ **Clean Architecture** - Well-defined abstractions and interfaces for extensibility
- 📈 **Production Ready** - Comprehensive error handling, logging, and monitoring support
- ⚡ **Simplified Setup** - One-line configuration with automatic dashboard and [WorkflowCore](https://github.com/danielgerlag/workflow-core) integration
- 🎯 **Clean API** - Singleton pattern with intuitive extension methods
- 🧪 **Test-Driven** - Comprehensive test coverage (104+ tests) following TDD principles

## Installation

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

## Quick Example

```csharp
// ASP.NET Core setup with automatic dashboard integration
services.AddHangfireWorkflowCoreAspNetCore(
    hangfire => hangfire.UseMemoryStorage());

// Execute a workflow immediately
var jobId = BackgroundJobWorkflow.Instance
    .Enqueue<VideoProcessingWorkflow, VideoData>(videoData);

// Schedule a workflow with delay
var jobId = BackgroundJobWorkflow.Instance
    .ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(
        videoData, TimeSpan.FromMinutes(5));
```

## License

This project is licensed under **LGPL v3** to ensure compatibility with [Hangfire](https://www.hangfire.io/)'s licensing requirements.

## External Dependencies

- **[Hangfire](https://www.hangfire.io/)** (≥1.8.14) - Job scheduling and processing
- **[WorkflowCore](https://github.com/danielgerlag/workflow-core)** (≥3.10.0) - Workflow orchestration engine
- **[Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)** (≥8.0.0) - Dependency injection
- **[Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)** (≥8.0.0) - Logging abstractions

---

📖 **Get started with the [Getting Started Guide](getting-started.md)** or explore the [Complete Documentation](complete-guide.md)!