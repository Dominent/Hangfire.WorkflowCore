using Hangfire.WorkflowCore.Abstractions;

namespace Hangfire.WorkflowCore.Services;

/// <summary>
/// Null implementation for non-ASP.NET environments
/// </summary>
public class NullHttpContextSnapshotProvider : IHttpContextSnapshotProvider
{
    /// <inheritdoc />
    public HttpContextSnapshot? GetCurrentSnapshot() => null;
}