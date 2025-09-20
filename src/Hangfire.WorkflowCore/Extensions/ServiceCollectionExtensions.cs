using Hangfire;
using Hangfire.WorkflowCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hangfire.WorkflowCore.Extensions;

/// <summary>
/// Configuration options for WorkflowCore integration components
/// </summary>
public class WorkflowCoreOptions
{
    internal Type? CustomStorageBridgeType { get; set; }
    internal Type? CustomInstanceProviderType { get; set; }
    
    /// <summary>
    /// Use a custom implementation of IWorkflowStorageBridge
    /// </summary>
    /// <typeparam name="T">The custom storage bridge implementation</typeparam>
    public WorkflowCoreOptions UseStorageBridge<T>() where T : class, IWorkflowStorageBridge
    {
        CustomStorageBridgeType = typeof(T);
        return this;
    }
    
    /// <summary>
    /// Use a custom implementation of IWorkflowInstanceProvider
    /// </summary>
    /// <typeparam name="T">The custom instance provider implementation</typeparam>
    public WorkflowCoreOptions UseInstanceProvider<T>() where T : class, IWorkflowInstanceProvider
    {
        CustomInstanceProviderType = typeof(T);
        return this;
    }
}

/// <summary>
/// Extension methods for IServiceCollection to simplify Hangfire.WorkflowCore setup
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Hangfire and WorkflowCore services with integration components
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureHangfire">Hangfire configuration (storage, etc.)</param>
    /// <param name="configureWorkflowCore">WorkflowCore integration options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHangfireWorkflowCore(
        this IServiceCollection services,
        Action<IGlobalConfiguration> configureHangfire,
        Action<WorkflowCoreOptions> configureWorkflowCore)
    {
        // Configure Hangfire with user's configuration
        services.AddHangfire(configureHangfire);
        services.AddHangfireServer();
        
        // Configure WorkflowCore
        services.AddWorkflow();
        
        // Configure our integration components
        var workflowOptions = new WorkflowCoreOptions();
        configureWorkflowCore(workflowOptions);
        
        // Register storage bridge - REQUIRED
        if (workflowOptions.CustomStorageBridgeType == null)
        {
            throw new InvalidOperationException(
                "Workflow storage bridge is required. Please call UseStorageBridge<T>() to specify your storage implementation. " +
                "Example: workflowOptions.UseStorageBridge<MyWorkflowStorageBridge>()");
        }
        
        services.AddSingleton(typeof(IWorkflowStorageBridge), workflowOptions.CustomStorageBridgeType);
        
        // Register instance provider - REQUIRED
        if (workflowOptions.CustomInstanceProviderType == null)
        {
            throw new InvalidOperationException(
                "Workflow instance provider is required. Please call UseInstanceProvider<T>() to specify your provider implementation. " +
                "Example: workflowOptions.UseInstanceProvider<MyWorkflowInstanceProvider>()");
        }
        
        services.AddSingleton(typeof(IWorkflowInstanceProvider), workflowOptions.CustomInstanceProviderType);
        
        return services;
    }
}

