using Hangfire.WorkflowCore.Dashboard.Models;

namespace Hangfire.WorkflowCore.Dashboard.Abstractions;

/// <summary>
/// Main service for workflow dashboard operations
/// Single Responsibility Principle: Coordinates workflow dashboard functionality
/// Dependency Inversion Principle: Depends on abstractions, not concretions
/// </summary>
public interface IWorkflowDashboardService
{
    /// <summary>
    /// Renders complete workflow information as HTML for a given job ID
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    /// <returns>HTML representation of workflow information</returns>
    Task<string> RenderWorkflowInfoAsync(string jobId);

    /// <summary>
    /// Gets workflow data for a given job ID
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    /// <returns>Workflow dashboard data or null if not found</returns>
    Task<WorkflowDashboardData?> GetWorkflowDataAsync(string jobId);

    /// <summary>
    /// Calculates status information for the given workflow data
    /// </summary>
    /// <param name="workflowData">The workflow data to analyze</param>
    /// <returns>Calculated status information</returns>
    WorkflowStatusInfo CalculateWorkflowStatus(WorkflowDashboardData workflowData);

    /// <summary>
    /// Renders workflow data as HTML
    /// </summary>
    /// <param name="workflowData">The workflow data to render</param>
    /// <returns>HTML representation</returns>
    string RenderWorkflowHtml(WorkflowDashboardData? workflowData);
}