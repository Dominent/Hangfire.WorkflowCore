using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Abstractions;

/// <summary>
/// Represents a Hangfire job that executes a Workflow Core workflow
/// </summary>
public interface IWorkflowJob
{
    /// <summary>
    /// Gets the workflow instance identifier
    /// </summary>
    string? WorkflowInstanceId { get; }

    /// <summary>
    /// Gets the Hangfire job identifier
    /// </summary>
    string? JobId { get; }

    /// <summary>
    /// Executes the workflow with the provided data
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    /// <param name="data">The workflow data as JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the workflow execution result</returns>
    Task<WorkflowExecutionResult> ExecuteAsync(string jobId, string data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of workflow execution using WorkflowCore types
/// </summary>
public class WorkflowExecutionResult
{
    /// <summary>
    /// Gets or sets the workflow instance ID
    /// </summary>
    public string WorkflowInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow status using WorkflowCore enum
    /// </summary>
    public WorkflowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the result data
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets any error message if the workflow failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the completion time
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}