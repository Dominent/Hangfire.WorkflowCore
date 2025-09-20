using Hangfire;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.AspNetCore;
using Hangfire.WorkflowCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hangfire.WorkflowCore.Extensions;

/// <summary>
/// Extension methods for adding ASP.NET Core integration to Hangfire.WorkflowCore
/// </summary>
public static class AspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds ASP.NET Core integration to Hangfire.WorkflowCore, enabling HttpContext capture
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAspNetCoreIntegration(this IServiceCollection services)
    {
        // Register HttpContextAccessor if not already registered
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        // Replace the null provider with ASP.NET Core implementation
        services.AddSingleton<IHttpContextSnapshotProvider, AspNetCoreHttpContextSnapshotProvider>();
        
        // Register HttpContextWorkflowJob as transient (resolved per job execution)
        services.TryAddTransient(typeof(HttpContextWorkflowJob<,>));
        
        return services;
    }

    /// <summary>
    /// Adds complete Hangfire.WorkflowCore setup with ASP.NET Core integration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureHangfire">Hangfire configuration action</param>
    /// <param name="configureWorkflowCore">WorkflowCore configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHangfireWorkflowCoreAspNetCore(
        this IServiceCollection services,
        Action<IGlobalConfiguration> configureHangfire,
        Action<WorkflowCoreOptions> configureWorkflowCore)
    {
        // Add the core Hangfire.WorkflowCore services
        services.AddHangfireWorkflowCore(configureHangfire, configureWorkflowCore);
        
        // Add ASP.NET Core specific integrations
        services.AddAspNetCoreIntegration();
        
        return services;
    }
}