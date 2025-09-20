using System.Text.Json;
using WorkflowCore.Interface;

namespace Hangfire.WorkflowCore.Extensions;

/// <summary>
/// Workflow-specific methods for BackgroundJob
/// </summary>
public static class BackgroundJobWorkflow
{
    /// <summary>
    /// Enqueues a workflow to run immediately
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type</typeparam>
    /// <typeparam name="TData">The workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <returns>The Hangfire job ID</returns>
    public static string Enqueue<TWorkflow, TData>(TData data)
        where TWorkflow : IWorkflow<TData>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.Enqueue<WorkflowJob<TWorkflow, TData>>(
            job => job.ExecuteAsync(null!, jsonData, CancellationToken.None));
    }

    /// <summary>
    /// Schedules a workflow to run after a delay
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type</typeparam>
    /// <typeparam name="TData">The workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <param name="delay">The delay before execution</param>
    /// <returns>The Hangfire job ID</returns>
    public static string ScheduleWorkflow<TWorkflow, TData>(TData data, TimeSpan delay)
        where TWorkflow : IWorkflow<TData>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.Schedule<WorkflowJob<TWorkflow, TData>>(
            job => job.ExecuteAsync(null!, jsonData, CancellationToken.None),
            delay);
    }

    /// <summary>
    /// Schedules a workflow to run at a specific time
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type</typeparam>
    /// <typeparam name="TData">The workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <param name="enqueueAt">The time to enqueue the workflow</param>
    /// <returns>The Hangfire job ID</returns>
    public static string ScheduleWorkflow<TWorkflow, TData>(TData data, DateTimeOffset enqueueAt)
        where TWorkflow : IWorkflow<TData>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.Schedule<WorkflowJob<TWorkflow, TData>>(
            job => job.ExecuteAsync(null!, jsonData, CancellationToken.None),
            enqueueAt);
    }

    /// <summary>
    /// Creates a continuation workflow that runs after a parent job completes
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type</typeparam>
    /// <typeparam name="TData">The workflow data type</typeparam>
    /// <param name="parentJobId">The parent job ID</param>
    /// <param name="data">The workflow data</param>
    /// <returns>The continuation job ID</returns>
    public static string ContinueWorkflowWith<TWorkflow, TData>(string parentJobId, TData data)
        where TWorkflow : IWorkflow<TData>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.ContinueJobWith<WorkflowJob<TWorkflow, TData>>(
            parentJobId,
            job => job.ExecuteAsync(null!, jsonData, CancellationToken.None));
    }
}

/// <summary>
/// Workflow-specific methods for RecurringJob
/// </summary>
public static class RecurringJobWorkflow
{
    /// <summary>
    /// Adds or updates a recurring workflow
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type</typeparam>
    /// <typeparam name="TData">The workflow data type</typeparam>
    /// <param name="recurringJobId">The recurring job ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="cronExpression">The CRON expression</param>
    /// <param name="options">Optional recurring job options</param>
    public static void AddOrUpdateWorkflow<TWorkflow, TData>(
        string recurringJobId,
        TData data,
        string cronExpression,
        RecurringJobOptions? options = null)
        where TWorkflow : IWorkflow<TData>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        Hangfire.RecurringJob.AddOrUpdate<WorkflowJob<TWorkflow, TData>>(
            recurringJobId,
            job => job.ExecuteAsync(null!, jsonData, CancellationToken.None),
            cronExpression,
            options ?? new RecurringJobOptions());
    }

    /// <summary>
    /// Removes a recurring workflow
    /// </summary>
    /// <param name="recurringJobId">The recurring job ID</param>
    public static void RemoveWorkflow(string recurringJobId)
    {
        Hangfire.RecurringJob.RemoveIfExists(recurringJobId);
    }

    /// <summary>
    /// Triggers a recurring workflow immediately
    /// </summary>
    /// <param name="recurringJobId">The recurring job ID</param>
    public static void TriggerWorkflow(string recurringJobId)
    {
        Hangfire.RecurringJob.TriggerJob(recurringJobId);
    }
}