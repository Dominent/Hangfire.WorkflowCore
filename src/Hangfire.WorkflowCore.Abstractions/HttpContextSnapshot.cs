namespace Hangfire.WorkflowCore.Abstractions;

/// <summary>
/// A serializable snapshot of HttpContext that can be passed to workflow jobs
/// </summary>
public class HttpContextSnapshot
{
    /// <summary>
    /// Gets or sets the request path
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the request headers
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Gets or sets the user claims
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = new();

    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the request ID for tracking
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the remote IP address
    /// </summary>
    public string? RemoteIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets when this snapshot was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a snapshot from an HttpContext
    /// </summary>
    /// <param name="httpContext">The HttpContext to snapshot, can be null</param>
    /// <returns>A serializable snapshot or null if httpContext is null</returns>
    public static HttpContextSnapshot? FromHttpContext(object? httpContext)
    {
        return httpContext == null ? null : new HttpContextSnapshot();
    }
}

/// <summary>
/// Wrapper that combines workflow data with HttpContext snapshot
/// </summary>
/// <typeparam name="TData">The workflow data type</typeparam>
public class WorkflowDataWithContext<TData> where TData : class
{
    /// <summary>
    /// Gets or sets the original workflow data
    /// </summary>
    public TData Data { get; set; } = default!;

    /// <summary>
    /// Gets or sets the HttpContext snapshot
    /// </summary>
    public HttpContextSnapshot? HttpContext { get; set; }

    /// <summary>
    /// Gets or sets when this was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Service for capturing HttpContext snapshots
/// </summary>
public interface IHttpContextSnapshotProvider
{
    /// <summary>
    /// Gets a snapshot of the current HttpContext if available
    /// </summary>
    /// <returns>HttpContext snapshot or null if not in ASP.NET environment</returns>
    HttpContextSnapshot? GetCurrentSnapshot();
}