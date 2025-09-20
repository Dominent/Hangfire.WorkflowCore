using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Microsoft.Extensions.Logging;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Dashboard.Services;

/// <summary>
/// Provides workflow data for dashboard display
/// Implements Single Responsibility Principle: Only responsible for data retrieval
/// Follows Liskov Substitution Principle: Can be substituted with any IWorkflowDataProvider
/// </summary>
public class WorkflowDataProvider : IWorkflowDataProvider
{
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _instanceProvider;
    private readonly ILogger<WorkflowDataProvider> _logger;

    public WorkflowDataProvider(
        IWorkflowStorageBridge storageBridge,
        IWorkflowInstanceProvider instanceProvider,
        ILogger<WorkflowDataProvider> logger)
    {
        _storageBridge = storageBridge ?? throw new ArgumentNullException(nameof(storageBridge));
        _instanceProvider = instanceProvider ?? throw new ArgumentNullException(nameof(instanceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WorkflowDashboardData?> GetWorkflowDataAsync(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            _logger.LogWarning("Invalid job ID provided: {JobId}", jobId);
            return null;
        }

        try
        {
            // Get workflow instance ID from job mapping
            var workflowInstanceId = await _storageBridge.GetWorkflowInstanceIdAsync(jobId);
            if (string.IsNullOrEmpty(workflowInstanceId))
            {
                _logger.LogDebug("No workflow instance mapping found for job ID: {JobId}", jobId);
                return null;
            }

            // Get workflow instance
            var workflowInstance = await _instanceProvider.GetWorkflowInstanceAsync(workflowInstanceId);
            if (workflowInstance == null)
            {
                _logger.LogWarning("Workflow instance not found: {WorkflowInstanceId}", workflowInstanceId);
                return null;
            }

            // Get execution result if available
            var executionResult = await _storageBridge.GetWorkflowResultAsync(workflowInstanceId);

            // Convert to dashboard data
            var dashboardData = ConvertToWorkflowDashboardData(jobId, workflowInstance, executionResult);

            _logger.LogDebug("Successfully retrieved workflow data for job {JobId}", jobId);
            return dashboardData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow data for job {JobId}", jobId);
            return null;
        }
    }

    private static WorkflowDashboardData ConvertToWorkflowDashboardData(
        string jobId,
        WorkflowInstance workflowInstance,
        WorkflowExecutionResult? executionResult)
    {
        var dashboardData = new WorkflowDashboardData
        {
            JobId = jobId,
            WorkflowInstanceId = workflowInstance.Id,
            Status = workflowInstance.Status,
            Data = workflowInstance.Data,
            CreatedAt = workflowInstance.CreateTime,
            CompletedAt = workflowInstance.CompleteTime
        };

        // Add execution result information if available
        if (executionResult != null)
        {
            dashboardData.ErrorMessage = executionResult.ErrorMessage;
            if (executionResult.CompletedAt.HasValue)
            {
                dashboardData.CompletedAt = executionResult.CompletedAt;
            }
        }

        // Convert execution pointers to workflow steps
        if (workflowInstance.ExecutionPointers != null)
        {
            dashboardData.Steps = ConvertExecutionPointersToSteps(workflowInstance.ExecutionPointers);
        }

        return dashboardData;
    }

    private static List<WorkflowStepInfo> ConvertExecutionPointersToSteps(
        ExecutionPointerCollection executionPointers)
    {
        return executionPointers
            .Select((pointer, index) => new WorkflowStepInfo
            {
                Id = pointer.Id,
                Name = pointer.StepName,
                Status = ConvertPointerStatusToWorkflowStatus(pointer.Status),
                StartedAt = pointer.StartTime,
                CompletedAt = pointer.EndTime,
                Order = index + 1
            })
            .OrderBy(step => step.Order)
            .ToList();
    }

    private static WorkflowStatus ConvertPointerStatusToWorkflowStatus(PointerStatus pointerStatus)
    {
        return pointerStatus switch
        {
            PointerStatus.Legacy => WorkflowStatus.Runnable,
            PointerStatus.Pending => WorkflowStatus.Runnable,
            PointerStatus.Running => WorkflowStatus.Runnable,
            PointerStatus.Complete => WorkflowStatus.Complete,
            PointerStatus.Sleeping => WorkflowStatus.Suspended,
            PointerStatus.WaitingForEvent => WorkflowStatus.Suspended,
            PointerStatus.Failed => WorkflowStatus.Terminated,
            PointerStatus.Cancelled => WorkflowStatus.Terminated,
            _ => WorkflowStatus.Runnable
        };
    }
}