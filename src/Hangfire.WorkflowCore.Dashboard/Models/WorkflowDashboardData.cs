using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Dashboard.Models;

/// <summary>
/// Represents workflow data formatted for dashboard display
/// Single Responsibility: Contains only data needed for dashboard visualization
/// </summary>
public class WorkflowDashboardData
{
    /// <summary>
    /// Gets or sets the Hangfire job ID
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow instance ID
    /// </summary>
    public string WorkflowInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow status
    /// </summary>
    public WorkflowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the workflow data
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the workflow was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the workflow was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the workflow steps information
    /// </summary>
    public List<WorkflowStepInfo> Steps { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents information about a workflow step
/// </summary>
public class WorkflowStepInfo
{
    /// <summary>
    /// Gets or sets the step ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step status
    /// </summary>
    public WorkflowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the step started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the step completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets any error message for this step
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the step order
    /// </summary>
    public int Order { get; set; }
}