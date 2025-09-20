using Hangfire.WorkflowCore.Dashboard.Models;

namespace Hangfire.WorkflowCore.Dashboard.Abstractions;

/// <summary>
/// Provides workflow data for dashboard display
/// Single Responsibility Principle: Only responsible for data retrieval
/// </summary>
public interface IWorkflowDataProvider
{
    /// <summary>
    /// Retrieves workflow data for a given Hangfire job ID
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    /// <returns>Workflow dashboard data or null if not found</returns>
    Task<WorkflowDashboardData?> GetWorkflowDataAsync(string jobId);
}