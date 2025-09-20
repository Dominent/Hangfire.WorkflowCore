using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hangfire.WorkflowCore.Dashboard.Extensions;

/// <summary>
/// Extension methods for registering workflow dashboard services
/// Follows Dependency Inversion Principle: Registers abstractions and implementations
/// Supports Open/Closed Principle: Allows custom implementations to be registered
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds workflow dashboard services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddWorkflowDashboard(this IServiceCollection services)
    {
        // Register core services
        services.TryAddScoped<IWorkflowDataProvider, WorkflowDataProvider>();
        services.TryAddScoped<IWorkflowStatusCalculator, WorkflowStatusCalculator>();
        services.TryAddScoped<IWorkflowRenderer, HtmlWorkflowRenderer>();
        services.TryAddScoped<IWorkflowDashboardService, WorkflowDashboardService>();

        return services;
    }

    /// <summary>
    /// Adds workflow dashboard services with custom implementations
    /// Supports Open/Closed Principle: Allows customization without modifying existing code
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for customizing service registrations</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddWorkflowDashboard(
        this IServiceCollection services,
        Action<WorkflowDashboardOptions> configure)
    {
        var options = new WorkflowDashboardOptions();
        configure(options);

        // Register services based on options
        if (options.DataProviderType != null)
        {
            services.AddScoped(typeof(IWorkflowDataProvider), options.DataProviderType);
        }
        else
        {
            services.TryAddScoped<IWorkflowDataProvider, WorkflowDataProvider>();
        }

        if (options.StatusCalculatorType != null)
        {
            services.AddScoped(typeof(IWorkflowStatusCalculator), options.StatusCalculatorType);
        }
        else
        {
            services.TryAddScoped<IWorkflowStatusCalculator, WorkflowStatusCalculator>();
        }

        if (options.RendererType != null)
        {
            services.AddScoped(typeof(IWorkflowRenderer), options.RendererType);
        }
        else
        {
            services.TryAddScoped<IWorkflowRenderer, HtmlWorkflowRenderer>();
        }

        // Always register the main dashboard service
        services.TryAddScoped<IWorkflowDashboardService, WorkflowDashboardService>();

        return services;
    }
}

/// <summary>
/// Configuration options for workflow dashboard services
/// Supports Open/Closed Principle: Allows extension without modification
/// </summary>
public class WorkflowDashboardOptions
{
    /// <summary>
    /// Custom implementation type for IWorkflowDataProvider
    /// </summary>
    public Type? DataProviderType { get; set; }

    /// <summary>
    /// Custom implementation type for IWorkflowStatusCalculator
    /// </summary>
    public Type? StatusCalculatorType { get; set; }

    /// <summary>
    /// Custom implementation type for IWorkflowRenderer
    /// </summary>
    public Type? RendererType { get; set; }

    /// <summary>
    /// Uses a custom data provider implementation
    /// </summary>
    /// <typeparam name="T">The custom data provider type</typeparam>
    /// <returns>The options for method chaining</returns>
    public WorkflowDashboardOptions UseDataProvider<T>() where T : class, IWorkflowDataProvider
    {
        DataProviderType = typeof(T);
        return this;
    }

    /// <summary>
    /// Uses a custom status calculator implementation
    /// </summary>
    /// <typeparam name="T">The custom status calculator type</typeparam>
    /// <returns>The options for method chaining</returns>
    public WorkflowDashboardOptions UseStatusCalculator<T>() where T : class, IWorkflowStatusCalculator
    {
        StatusCalculatorType = typeof(T);
        return this;
    }

    /// <summary>
    /// Uses a custom renderer implementation
    /// </summary>
    /// <typeparam name="T">The custom renderer type</typeparam>
    /// <returns>The options for method chaining</returns>
    public WorkflowDashboardOptions UseRenderer<T>() where T : class, IWorkflowRenderer
    {
        RendererType = typeof(T);
        return this;
    }
}