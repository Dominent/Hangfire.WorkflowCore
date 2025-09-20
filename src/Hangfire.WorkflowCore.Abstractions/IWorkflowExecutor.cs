using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Abstractions;

/// <summary>
/// Extended workflow executor with Hangfire-specific functionality
/// </summary>
public interface IHangfireWorkflowExecutor
{
    /// <summary>
    /// Waits for a workflow to complete with a specific timeout
    /// </summary>
    /// <param name="instanceId">The workflow instance ID</param>
    /// <param name="timeout">Maximum time to wait</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow execution result</returns>
    Task<WorkflowExecutionResult> WaitForCompletionAsync(string instanceId, TimeSpan timeout, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets workflow execution result
    /// </summary>
    /// <param name="instanceId">The workflow instance ID</param>
    /// <returns>The execution result or null if not found</returns>
    Task<WorkflowExecutionResult?> GetExecutionResultAsync(string instanceId);
    
    /// <summary>
    /// Cancels a workflow and returns execution result
    /// </summary>
    /// <param name="instanceId">The workflow instance ID</param>
    /// <returns>The cancellation result</returns>
    Task<WorkflowExecutionResult> CancelWorkflowAsync(string instanceId);
}