using System.Text.Json;
using Hangfire.Server;
using Hangfire.WorkflowCore.Abstractions;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore;

/// <summary>
/// Specialized Hangfire job that executes a Workflow Core workflow with HttpContext support
/// </summary>
/// <typeparam name="TWorkflow">The workflow type that accepts WorkflowDataWithContext</typeparam>
/// <typeparam name="TData">The original workflow data type</typeparam>
public class HttpContextWorkflowJob<TWorkflow, TData> : IWorkflowJob
    where TWorkflow : IWorkflow<WorkflowDataWithContext<TData>>, new()
    where TData : class, new()
{
    private readonly IWorkflowHost _workflowHost;
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _workflowInstanceProvider;
    private readonly IHttpContextSnapshotProvider _httpContextProvider;
    private readonly ILogger<HttpContextWorkflowJob<TWorkflow, TData>> _logger;

    private string? _workflowInstanceId;
    private string? _jobId;

    public HttpContextWorkflowJob(
        IWorkflowHost workflowHost,
        IWorkflowStorageBridge storageBridge,
        IWorkflowInstanceProvider workflowInstanceProvider,
        IHttpContextSnapshotProvider httpContextProvider,
        ILogger<HttpContextWorkflowJob<TWorkflow, TData>> logger)
    {
        _workflowHost = workflowHost ?? throw new ArgumentNullException(nameof(workflowHost));
        _storageBridge = storageBridge ?? throw new ArgumentNullException(nameof(storageBridge));
        _workflowInstanceProvider = workflowInstanceProvider ?? throw new ArgumentNullException(nameof(workflowInstanceProvider));
        _httpContextProvider = httpContextProvider ?? throw new ArgumentNullException(nameof(httpContextProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string? WorkflowInstanceId => _workflowInstanceId;

    /// <inheritdoc />
    public string? JobId => _jobId;

    /// <inheritdoc />
    public async Task<WorkflowExecutionResult> ExecuteAsync(string jobId, string data, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithHttpContextInternalAsync(jobId, data, cancellationToken);
    }

    /// <summary>
    /// Executes the workflow with HttpContext integration
    /// </summary>
    /// <param name="context">The Hangfire perform context</param>
    /// <param name="data">The serialized workflow data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow execution result</returns>
    public async Task<WorkflowExecutionResult> ExecuteWithHttpContextAsync(PerformContext? context, string data, CancellationToken cancellationToken = default)
    {
        var jobId = context?.BackgroundJob?.Id ?? "unknown";
        return await ExecuteWithHttpContextInternalAsync(jobId, data, cancellationToken);
    }

    /// <summary>
    /// Internal method that performs workflow execution with HttpContext support
    /// </summary>
    private async Task<WorkflowExecutionResult> ExecuteWithHttpContextInternalAsync(string jobId, string data, CancellationToken cancellationToken = default)
    {
        _jobId = jobId;

        _logger.LogInformation("Starting workflow {WorkflowType} with HttpContext for job {JobId}",
            typeof(TWorkflow).Name, jobId);

        try
        {
            // Deserialize JSON data
            TData? workflowData;
            try
            {
                workflowData = JsonSerializer.Deserialize<TData>(data);
                if (workflowData == null)
                {
                    throw new InvalidOperationException("Deserialized data is null");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON data for job {JobId}", jobId);

                return new WorkflowExecutionResult
                {
                    WorkflowInstanceId = string.Empty,
                    Status = WorkflowStatus.Terminated,
                    ErrorMessage = $"Invalid JSON data: {ex.Message}",
                    CompletedAt = DateTime.UtcNow
                };
            }

            // Get HttpContext snapshot
            var httpContextSnapshot = _httpContextProvider.GetCurrentSnapshot();

            // Wrap data with HttpContext
            var dataWithContext = new WorkflowDataWithContext<TData>
            {
                Data = workflowData,
                HttpContext = httpContextSnapshot
            };

            // Start the workflow with enhanced data
            var workflowInstanceId = await _workflowHost.StartWorkflow(typeof(TWorkflow).Name, dataWithContext);
            _workflowInstanceId = workflowInstanceId;

            _logger.LogDebug("Workflow instance {WorkflowInstanceId} started with HttpContext for job {JobId}",
                workflowInstanceId, jobId);

            // Store the job-to-workflow mapping
            await _storageBridge.StoreJobWorkflowMappingAsync(jobId, workflowInstanceId);

            // Wait for workflow completion
            var result = await WaitForWorkflowCompletionAsync(workflowInstanceId, cancellationToken);

            // Store the result
            await _storageBridge.StoreWorkflowResultAsync(workflowInstanceId, result);

            _logger.LogInformation("Workflow {WorkflowInstanceId} completed with status {Status}",
                workflowInstanceId, result.Status);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Workflow execution cancelled for job {JobId}", jobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow for job {JobId}", jobId);

            var errorResult = new WorkflowExecutionResult
            {
                WorkflowInstanceId = _workflowInstanceId ?? string.Empty,
                Status = WorkflowStatus.Terminated,
                ErrorMessage = ex.Message,
                CompletedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(_workflowInstanceId))
            {
                await _storageBridge.StoreWorkflowResultAsync(_workflowInstanceId, errorResult);
            }

            return errorResult;
        }
    }

    private async Task<WorkflowExecutionResult> WaitForWorkflowCompletionAsync(
        string workflowInstanceId,
        CancellationToken cancellationToken)
    {
        // Poll for workflow completion
        while (!cancellationToken.IsCancellationRequested)
        {
            var instance = await _workflowInstanceProvider.GetWorkflowInstanceAsync(workflowInstanceId);

            if (instance == null)
            {
                return new WorkflowExecutionResult
                {
                    WorkflowInstanceId = workflowInstanceId,
                    Status = WorkflowStatus.Terminated,
                    ErrorMessage = "Workflow instance not found",
                    CompletedAt = DateTime.UtcNow
                };
            }

            // Check if workflow is complete
            if (instance.Status == WorkflowStatus.Complete)
            {
                return new WorkflowExecutionResult
                {
                    WorkflowInstanceId = workflowInstanceId,
                    Status = WorkflowStatus.Complete,
                    Data = instance.Data,
                    CompletedAt = instance.CompleteTime ?? DateTime.UtcNow
                };
            }

            // Check if workflow failed or was terminated
            if (instance.Status == WorkflowStatus.Terminated)
            {
                return new WorkflowExecutionResult
                {
                    WorkflowInstanceId = workflowInstanceId,
                    Status = WorkflowStatus.Terminated,
                    ErrorMessage = "Workflow was terminated",
                    CompletedAt = instance.CompleteTime ?? DateTime.UtcNow
                };
            }

            // Wait a bit before checking again
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        // If we get here, cancellation was requested
        throw new OperationCanceledException("Workflow execution was cancelled", cancellationToken);
    }
}