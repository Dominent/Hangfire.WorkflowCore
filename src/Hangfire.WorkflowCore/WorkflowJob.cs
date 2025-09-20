using System.Text.Json;
using Hangfire.WorkflowCore.Abstractions;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore;

/// <summary>
/// Generic Hangfire job that executes a Workflow Core workflow
/// </summary>
/// <typeparam name="TWorkflow">The workflow type</typeparam>
/// <typeparam name="TData">The workflow data type</typeparam>
public class WorkflowJob<TWorkflow, TData> : IWorkflowJob
    where TWorkflow : IWorkflow<TData>, new()
    where TData : class, new()
{
    private readonly IWorkflowHost _workflowHost;
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _workflowInstanceProvider;
    private readonly ILogger<WorkflowJob<TWorkflow, TData>> _logger;
    
    private string? _workflowInstanceId;
    private string? _jobId;

    public WorkflowJob(
        IWorkflowHost workflowHost,
        IWorkflowStorageBridge storageBridge,
        IWorkflowInstanceProvider workflowInstanceProvider,
        ILogger<WorkflowJob<TWorkflow, TData>> logger)
    {
        _workflowHost = workflowHost ?? throw new ArgumentNullException(nameof(workflowHost));
        _storageBridge = storageBridge ?? throw new ArgumentNullException(nameof(storageBridge));
        _workflowInstanceProvider = workflowInstanceProvider ?? throw new ArgumentNullException(nameof(workflowInstanceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string? WorkflowInstanceId => _workflowInstanceId;

    /// <inheritdoc />
    public string? JobId => _jobId;

    /// <inheritdoc />
    public async Task<WorkflowExecutionResult> ExecuteAsync(string jobId, string data, CancellationToken cancellationToken = default)
    {
        _jobId = jobId;
        
        _logger.LogInformation("Starting workflow {WorkflowType} for job {JobId}", 
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

            // Start the workflow using the correct WorkflowCore API
            var workflowInstanceId = await _workflowHost.StartWorkflow(typeof(TWorkflow).Name, workflowData);
            _workflowInstanceId = workflowInstanceId;
            
            _logger.LogDebug("Workflow instance {WorkflowInstanceId} started for job {JobId}", 
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
            // For testing, we'll simulate getting the workflow instance
            // In a real implementation, we'd use a persistence provider or workflow controller
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