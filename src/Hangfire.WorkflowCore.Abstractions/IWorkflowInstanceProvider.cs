using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Abstractions;

/// <summary>
/// Provides access to workflow instances
/// </summary>
public interface IWorkflowInstanceProvider
{
    /// <summary>
    /// Gets a workflow instance by ID
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance ID</param>
    /// <returns>The workflow instance or null if not found</returns>
    Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId);
}