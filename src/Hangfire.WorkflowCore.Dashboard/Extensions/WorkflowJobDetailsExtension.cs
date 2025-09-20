using Hangfire;
using Hangfire.Dashboard;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hangfire.WorkflowCore.Dashboard.Extensions;

/// <summary>
/// Extension for integrating workflow information into Hangfire job details pages
/// Follows Dependency Inversion Principle: Depends on IWorkflowDashboardService abstraction
/// </summary>
public static class WorkflowJobDetailsExtension
{
    /// <summary>
    /// Registers the workflow job details renderer with Hangfire dashboard
    /// </summary>
    /// <param name="configuration">The Hangfire global configuration</param>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    /// <param name="order">The order in which this renderer should appear (lower numbers appear first)</param>
    /// <returns>The configuration for method chaining</returns>
    public static IGlobalConfiguration UseWorkflowJobDetailsRenderer(
        this IGlobalConfiguration configuration,
        IServiceProvider serviceProvider,
        int order = 10)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        configuration.UseJobDetailsRenderer(order, context =>
        {
            try
            {
                // Get the workflow dashboard service from DI
                var workflowService = serviceProvider.GetService<IWorkflowDashboardService>();
                if (workflowService == null)
                {
                    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                    var logger = loggerFactory?.CreateLogger("WorkflowJobDetailsExtension");
                    logger?.LogWarning("IWorkflowDashboardService not registered in DI container");
                    return new NonEscapedString(RenderServiceNotRegisteredMessage());
                }

                // Get the job ID from the context
                var jobId = context.JobId;
                if (string.IsNullOrEmpty(jobId))
                {
                    return new NonEscapedString(RenderNoJobIdMessage());
                }

                // Render workflow information asynchronously
                // Note: Since UseJobDetailsRenderer expects synchronous operation,
                // we need to use GetAwaiter().GetResult() here
                var workflowHtml = workflowService.RenderWorkflowInfoAsync(jobId).GetAwaiter().GetResult();

                return new NonEscapedString(workflowHtml);
            }
            catch (Exception ex)
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("WorkflowJobDetailsExtension");
                logger?.LogError(ex, "Error rendering workflow details for job {JobId}", context.JobId);
                return new NonEscapedString(RenderErrorMessage(ex.Message));
            }
        });

        return configuration;
    }

    private static string RenderServiceNotRegisteredMessage()
    {
        return """
            <div class="workflow-section workflow-warning">
                <h4>Workflow Information</h4>
                <div class="warning-message">
                    <span class="warning-icon">⚠️</span>
                    <span>Workflow dashboard service is not configured. Please register IWorkflowDashboardService in your DI container.</span>
                </div>
            </div>
            """;
    }

    private static string RenderNoJobIdMessage()
    {
        return """
            <div class="workflow-section workflow-warning">
                <h4>Workflow Information</h4>
                <div class="warning-message">
                    <span class="warning-icon">⚠️</span>
                    <span>No job ID available for workflow lookup.</span>
                </div>
            </div>
            """;
    }

    private static string RenderErrorMessage(string errorMessage)
    {
        var encodedError = HtmlEncode(errorMessage);
        return $"""
            <div class="workflow-section workflow-error">
                <h4>Workflow Information</h4>
                <div class="error-message">
                    <span class="error-icon">❌</span>
                    <span>Error loading workflow information: {encodedError}</span>
                </div>
            </div>
            """;
    }

    private static string HtmlEncode(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}