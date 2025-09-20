namespace Hangfire.WorkflowCore.Abstractions;

/// <summary>
/// Provides workflow scheduling capabilities through Hangfire
/// </summary>
public interface IWorkflowScheduler
{
    /// <summary>
    /// Enqueues a workflow to run immediately
    /// </summary>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="queueName">Optional queue name</param>
    /// <returns>The Hangfire job ID</returns>
    string EnqueueWorkflow<TData>(string workflowId, TData data, string? queueName = null) where TData : class;

    /// <summary>
    /// Schedules a workflow to run after a delay
    /// </summary>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="delay">The delay before execution</param>
    /// <param name="queueName">Optional queue name</param>
    /// <returns>The Hangfire job ID</returns>
    string ScheduleWorkflow<TData>(string workflowId, TData data, TimeSpan delay, string? queueName = null) where TData : class;

    /// <summary>
    /// Schedules a workflow to run at a specific time
    /// </summary>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="enqueueAt">The time to enqueue the workflow</param>
    /// <param name="queueName">Optional queue name</param>
    /// <returns>The Hangfire job ID</returns>
    string ScheduleWorkflow<TData>(string workflowId, TData data, DateTimeOffset enqueueAt, string? queueName = null) where TData : class;

    /// <summary>
    /// Adds or updates a recurring workflow
    /// </summary>
    /// <param name="recurringJobId">The recurring job ID</param>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="cronExpression">The CRON expression</param>
    /// <param name="queueName">Optional queue name</param>
    void AddOrUpdateRecurringWorkflow<TData>(string recurringJobId, string workflowId, TData data, string cronExpression, string? queueName = null) where TData : class;

    /// <summary>
    /// Removes a recurring workflow
    /// </summary>
    /// <param name="recurringJobId">The recurring job ID</param>
    void RemoveRecurringWorkflow(string recurringJobId);

    /// <summary>
    /// Continues with a workflow after a job completes
    /// </summary>
    /// <param name="parentJobId">The parent job ID</param>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="queueName">Optional queue name</param>
    /// <returns>The continuation job ID</returns>
    string ContinueWorkflowWith<TData>(string parentJobId, string workflowId, TData data, string? queueName = null) where TData : class;

    /// <summary>
    /// Creates a batch of workflows
    /// </summary>
    /// <returns>A workflow batch builder</returns>
    IWorkflowBatch CreateBatch();

    /// <summary>
    /// Deletes a scheduled job
    /// </summary>
    /// <param name="jobId">The job ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    bool DeleteJob(string jobId);

    /// <summary>
    /// Requeues a failed job
    /// </summary>
    /// <param name="jobId">The job ID to requeue</param>
    /// <returns>True if requeued successfully</returns>
    bool RequeueJob(string jobId);
}

/// <summary>
/// Represents a batch of workflows
/// </summary>
public interface IWorkflowBatch
{
    /// <summary>
    /// Adds a workflow to the batch
    /// </summary>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="name">Optional name for the job</param>
    /// <returns>The batch for fluent chaining</returns>
    IWorkflowBatch Add<TData>(string workflowId, TData data, string? name = null) where TData : class;

    /// <summary>
    /// Adds a continuation workflow that runs after all batch jobs complete
    /// </summary>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="data">The workflow data</param>
    /// <returns>The batch for fluent chaining</returns>
    IWorkflowBatch ContinueWith<TData>(string workflowId, TData data) where TData : class;

    /// <summary>
    /// Enqueues the batch
    /// </summary>
    /// <returns>The batch ID</returns>
    Task<string> EnqueueAsync();

    /// <summary>
    /// Gets the number of jobs in the batch
    /// </summary>
    int Count { get; }
}