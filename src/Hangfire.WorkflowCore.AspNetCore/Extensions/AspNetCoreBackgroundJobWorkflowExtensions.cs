using System.Text.Json;
using Hangfire.WorkflowCore.Abstractions;
using WorkflowCore.Interface;

namespace Hangfire.WorkflowCore.Extensions;

/// <summary>
/// ASP.NET Core extensions for BackgroundJobWorkflow - adds HttpContext support through extension methods
/// </summary>
public static class BackgroundJobWorkflowExtensions
{
    /// <summary>
    /// Enqueues a workflow to run immediately with HttpContext capture
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <returns>The Hangfire job ID</returns>
    public static string EnqueueWithHttpContext<TWorkflow, TData>(TData data)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.Enqueue<HttpContextWorkflowJob<TWorkflow, TData>>(
            job => job.ExecuteWithHttpContextAsync(null, jsonData, CancellationToken.None));
    }

    /// <summary>
    /// Schedules a workflow to run after a delay with HttpContext capture
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <param name="delay">The delay before execution</param>
    /// <returns>The Hangfire job ID</returns>
    public static string ScheduleWithHttpContext<TWorkflow, TData>(TData data, TimeSpan delay)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.Schedule<HttpContextWorkflowJob<TWorkflow, TData>>(
            job => job.ExecuteWithHttpContextAsync(null, jsonData, CancellationToken.None),
            delay);
    }

    /// <summary>
    /// Schedules a workflow to run at a specific time with HttpContext capture
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <param name="enqueueAt">The time to enqueue the workflow</param>
    /// <returns>The Hangfire job ID</returns>
    public static string ScheduleWithHttpContext<TWorkflow, TData>(TData data, DateTimeOffset enqueueAt)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.Schedule<HttpContextWorkflowJob<TWorkflow, TData>>(
            job => job.ExecuteWithHttpContextAsync(null, jsonData, CancellationToken.None),
            enqueueAt);
    }

    /// <summary>
    /// Creates a continuation workflow that runs after a parent job completes with HttpContext capture
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="parentJobId">The parent job ID</param>
    /// <param name="data">The workflow data</param>
    /// <returns>The continuation job ID</returns>
    public static string ContinueWithHttpContext<TWorkflow, TData>(string parentJobId, TData data)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        return Hangfire.BackgroundJob.ContinueJobWith<HttpContextWorkflowJob<TWorkflow, TData>>(
            parentJobId,
            job => job.ExecuteWithHttpContextAsync(null, jsonData, CancellationToken.None));
    }
}

/// <summary>
/// Extension methods that add the HttpContext functionality directly to BackgroundJobWorkflow singleton
/// This allows clean usage like: BackgroundJobWorkflow.Instance.EnqueueWithHttpContext()
/// </summary>
public static class BackgroundJobWorkflowHttpContextExtensions
{
    /// <summary>
    /// Extends BackgroundJobWorkflow with HttpContext-aware enqueue functionality
    /// </summary>
    /// <param name="backgroundJobWorkflow">The BackgroundJobWorkflow instance</param>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <returns>The Hangfire job ID</returns>
    public static string EnqueueWithHttpContext<TWorkflow, TData>(this BackgroundJobWorkflow backgroundJobWorkflow, TData data)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        return BackgroundJobWorkflowExtensions.EnqueueWithHttpContext<TWorkflow, TData>(data);
    }

    /// <summary>
    /// Extends BackgroundJobWorkflow with HttpContext-aware schedule functionality
    /// </summary>
    /// <param name="backgroundJobWorkflow">The BackgroundJobWorkflow instance</param>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="data">The workflow data</param>
    /// <param name="delay">The delay before execution</param>
    /// <returns>The Hangfire job ID</returns>
    public static string ScheduleWithHttpContext<TWorkflow, TData>(this BackgroundJobWorkflow backgroundJobWorkflow, TData data, TimeSpan delay)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        return BackgroundJobWorkflowExtensions.ScheduleWithHttpContext<TWorkflow, TData>(data, delay);
    }

    /// <summary>
    /// Extends BackgroundJobWorkflow with HttpContext-aware continuation functionality
    /// </summary>
    /// <param name="backgroundJobWorkflow">The BackgroundJobWorkflow instance</param>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="parentJobId">The parent job ID</param>
    /// <param name="data">The workflow data</param>
    /// <returns>The continuation job ID</returns>
    public static string ContinueWithHttpContext<TWorkflow, TData>(this BackgroundJobWorkflow backgroundJobWorkflow, string parentJobId, TData data)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        return BackgroundJobWorkflowExtensions.ContinueWithHttpContext<TWorkflow, TData>(parentJobId, data);
    }
}

/// <summary>
/// Extension methods that add HttpContext functionality to RecurringJobWorkflow singleton
/// This allows clean usage like: RecurringJobWorkflow.Instance.AddOrUpdateWithHttpContext()
/// </summary>
public static class RecurringJobWorkflowHttpContextExtensions
{
    /// <summary>
    /// Extends RecurringJobWorkflow with HttpContext-aware functionality
    /// </summary>
    /// <param name="recurringJobWorkflow">The RecurringJobWorkflow instance</param>
    /// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
    /// <typeparam name="TData">The original workflow data type</typeparam>
    /// <param name="recurringJobId">The recurring job ID</param>
    /// <param name="data">The workflow data</param>
    /// <param name="cronExpression">The CRON expression</param>
    /// <param name="options">Optional recurring job options</param>
    public static void AddOrUpdateWithHttpContext<TWorkflow, TData>(
        this RecurringJobWorkflow recurringJobWorkflow,
        string recurringJobId,
        TData data,
        string cronExpression,
        RecurringJobOptions? options = null)
        where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
        where TData : class, new()
    {
        var jsonData = JsonSerializer.Serialize(data);
        
        Hangfire.RecurringJob.AddOrUpdate<HttpContextWorkflowJob<TWorkflow, TData>>(
            recurringJobId,
            job => job.ExecuteWithHttpContextAsync(null, jsonData, CancellationToken.None),
            cronExpression,
            options ?? new RecurringJobOptions());
    }
}