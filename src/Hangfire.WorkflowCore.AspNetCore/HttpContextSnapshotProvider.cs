using Hangfire.WorkflowCore.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Hangfire.WorkflowCore.AspNetCore;

/// <summary>
/// ASP.NET Core implementation of HttpContext snapshot provider
/// </summary>
public class AspNetCoreHttpContextSnapshotProvider : IHttpContextSnapshotProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AspNetCoreHttpContextSnapshotProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public HttpContextSnapshot? GetCurrentSnapshot()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        return CreateSnapshot(httpContext);
    }

    /// <summary>
    /// Creates a snapshot from an HttpContext
    /// </summary>
    /// <param name="httpContext">The HttpContext to snapshot</param>
    /// <returns>A serializable snapshot</returns>
    public static HttpContextSnapshot CreateSnapshot(HttpContext httpContext)
    {
        var snapshot = new HttpContextSnapshot
        {
            RequestPath = httpContext.Request.Path.Value,
            Method = httpContext.Request.Method,
            RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault(),
            RequestId = httpContext.TraceIdentifier
        };

        // Copy selected headers (avoid sensitive data)
        var safeHeaders = new[] { "Authorization", "Content-Type", "Accept", "Accept-Language" };
        foreach (var headerName in safeHeaders)
        {
            if (httpContext.Request.Headers.TryGetValue(headerName, out var values))
            {
                snapshot.Headers[headerName] = values.FirstOrDefault() ?? string.Empty;
            }
        }

        // Copy user claims if authenticated
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            snapshot.UserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Copy important claims
            var importantClaims = new[] { ClaimTypes.Name, ClaimTypes.Email, ClaimTypes.Role, "sub" };
            foreach (var claimType in importantClaims)
            {
                var claim = httpContext.User.FindFirst(claimType);
                if (claim != null)
                {
                    snapshot.Claims[claimType] = claim.Value;
                }
            }
        }

        return snapshot;
    }
}