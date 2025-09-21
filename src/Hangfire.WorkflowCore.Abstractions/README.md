# WorkflowCoreHangfire.Abstractions

[![NuGet](https://img.shields.io/nuget/v/WorkflowCoreHangfire.Abstractions.svg)](https://www.nuget.org/packages/WorkflowCoreHangfire.Abstractions)

Abstractions and interfaces for integrating [Hangfire](https://www.hangfire.io/) with [WorkflowCore](https://github.com/danielgerlag/workflow-core).

## What's Included

- Core interfaces for workflow-job integration
- Base abstractions for storage bridges
- HTTP context provider interfaces
- Workflow instance provider contracts

## Installation

```bash
dotnet add package WorkflowCoreHangfire.Abstractions
```

## Usage

This package contains only interfaces and abstractions. For complete functionality, install one of the implementation packages:

- **[WorkflowCoreHangfire.AspNetCore](https://www.nuget.org/packages/WorkflowCoreHangfire.AspNetCore)** - ASP.NET Core integration (recommended)
- **[WorkflowCoreHangfire](https://www.nuget.org/packages/WorkflowCoreHangfire)** - Core library for console applications

## Documentation

📖 **[Complete Documentation](https://dominent.github.io/Hangfire.WorkflowCore/)**

## Support

💖 **Support this project:** [Buy me a coffee](https://buymeacoffee.com/ppavlov)

## License

Licensed under **LGPL v3** - see [LICENSE](https://github.com/Dominent/Hangfire.WorkflowCore/blob/main/LICENSE) for details.