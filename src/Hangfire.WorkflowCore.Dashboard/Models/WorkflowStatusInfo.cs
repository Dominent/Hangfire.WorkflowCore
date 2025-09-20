using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Dashboard.Models;

/// <summary>
/// Represents calculated status information for a workflow
/// Single Responsibility: Contains only status-related calculations
/// </summary>
public class WorkflowStatusInfo
{
    /// <summary>
    /// Gets or sets the workflow status
    /// </summary>
    public WorkflowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the duration of the workflow
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets whether the workflow is completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets whether the workflow is currently running
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets whether the workflow has an error
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Gets or sets the error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; set; }

    /// <summary>
    /// Gets or sets the workflow performance metrics
    /// </summary>
    public WorkflowPerformanceMetrics Performance { get; set; } = new();
}

/// <summary>
/// Represents performance metrics for a workflow
/// </summary>
public class WorkflowPerformanceMetrics
{
    /// <summary>
    /// Gets or sets the average step duration
    /// </summary>
    public TimeSpan AverageStepDuration { get; set; }

    /// <summary>
    /// Gets or sets the total processing time
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets the total wait time
    /// </summary>
    public TimeSpan TotalWaitTime { get; set; }

    /// <summary>
    /// Gets or sets the number of completed steps
    /// </summary>
    public int CompletedSteps { get; set; }

    /// <summary>
    /// Gets or sets the total number of steps
    /// </summary>
    public int TotalSteps { get; set; }
}