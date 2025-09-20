using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Dashboard.Services;

/// <summary>
/// Calculates status information for workflows
/// Implements Single Responsibility Principle: Only responsible for status calculations
/// Follows Liskov Substitution Principle: Can be substituted with any IWorkflowStatusCalculator
/// </summary>
public class WorkflowStatusCalculator : IWorkflowStatusCalculator
{
    public WorkflowStatusInfo CalculateStatus(WorkflowDashboardData workflowData)
    {
        ArgumentNullException.ThrowIfNull(workflowData);

        var statusInfo = new WorkflowStatusInfo
        {
            Status = workflowData.Status,
            ErrorMessage = workflowData.ErrorMessage
        };

        // Calculate duration
        var endTime = workflowData.CompletedAt ?? DateTime.UtcNow;
        statusInfo.Duration = endTime - workflowData.CreatedAt;

        // Determine completion and running state
        statusInfo.IsCompleted = IsWorkflowCompleted(workflowData.Status);
        statusInfo.IsRunning = IsWorkflowRunning(workflowData.Status);
        statusInfo.HasError = HasWorkflowError(workflowData.Status, workflowData.ErrorMessage);

        // Calculate progress and performance metrics
        CalculateProgressAndPerformance(workflowData, statusInfo);

        // Estimate completion time for running workflows
        if (statusInfo.IsRunning && !statusInfo.IsCompleted)
        {
            EstimateCompletionTime(workflowData, statusInfo);
        }

        return statusInfo;
    }

    private static bool IsWorkflowCompleted(WorkflowStatus status)
    {
        return status == WorkflowStatus.Complete || status == WorkflowStatus.Terminated;
    }

    private static bool IsWorkflowRunning(WorkflowStatus status)
    {
        return status == WorkflowStatus.Runnable;
    }

    private static bool HasWorkflowError(WorkflowStatus status, string? errorMessage)
    {
        return status == WorkflowStatus.Terminated || !string.IsNullOrEmpty(errorMessage);
    }

    private static void CalculateProgressAndPerformance(
        WorkflowDashboardData workflowData,
        WorkflowStatusInfo statusInfo)
    {
        var steps = workflowData.Steps;
        var totalSteps = steps.Count;
        var completedSteps = steps.Count(s => s.Status == WorkflowStatus.Complete);

        statusInfo.Performance.TotalSteps = totalSteps;
        statusInfo.Performance.CompletedSteps = completedSteps;

        // Calculate progress percentage
        if (totalSteps > 0)
        {
            statusInfo.ProgressPercentage = (int)Math.Round((double)completedSteps / totalSteps * 100);
        }
        else if (statusInfo.IsCompleted)
        {
            statusInfo.ProgressPercentage = 100;
        }
        else
        {
            statusInfo.ProgressPercentage = 0;
        }

        // Calculate performance metrics for completed steps
        var completedStepsWithTiming = steps
            .Where(s => s.Status == WorkflowStatus.Complete &&
                       s.StartedAt.HasValue &&
                       s.CompletedAt.HasValue)
            .ToList();

        if (completedStepsWithTiming.Any())
        {
            var stepDurations = completedStepsWithTiming
                .Select(s => s.CompletedAt!.Value - s.StartedAt!.Value)
                .ToList();

            statusInfo.Performance.TotalProcessingTime = TimeSpan.FromTicks(
                stepDurations.Sum(d => d.Ticks));

            statusInfo.Performance.AverageStepDuration = TimeSpan.FromTicks(
                (long)stepDurations.Average(d => d.Ticks));
        }

        // Calculate total wait time (time between step completions)
        CalculateWaitTime(steps, statusInfo);
    }

    private static void CalculateWaitTime(
        List<WorkflowStepInfo> steps,
        WorkflowStatusInfo statusInfo)
    {
        var orderedSteps = steps.OrderBy(s => s.Order).ToList();
        var totalWaitTime = TimeSpan.Zero;

        for (int i = 1; i < orderedSteps.Count; i++)
        {
            var previousStep = orderedSteps[i - 1];
            var currentStep = orderedSteps[i];

            if (previousStep.CompletedAt.HasValue && currentStep.StartedAt.HasValue)
            {
                var waitTime = currentStep.StartedAt.Value - previousStep.CompletedAt.Value;
                if (waitTime > TimeSpan.Zero)
                {
                    totalWaitTime += waitTime;
                }
            }
        }

        statusInfo.Performance.TotalWaitTime = totalWaitTime;
    }

    private static void EstimateCompletionTime(
        WorkflowDashboardData workflowData,
        WorkflowStatusInfo statusInfo)
    {
        var remainingSteps = workflowData.Steps.Count(s =>
            s.Status != WorkflowStatus.Complete &&
            s.Status != WorkflowStatus.Terminated);

        if (remainingSteps == 0)
        {
            statusInfo.EstimatedCompletionTime = DateTime.UtcNow;
            return;
        }

        // Use average step duration if available
        if (statusInfo.Performance.AverageStepDuration > TimeSpan.Zero)
        {
            var estimatedRemainingTime = TimeSpan.FromTicks(
                statusInfo.Performance.AverageStepDuration.Ticks * remainingSteps);

            statusInfo.EstimatedCompletionTime = DateTime.UtcNow.Add(estimatedRemainingTime);
        }
        else
        {
            // Fallback: estimate based on workflow age and progress
            var workflowAge = DateTime.UtcNow - workflowData.CreatedAt;
            if (statusInfo.ProgressPercentage > 0)
            {
                var estimatedTotalTime = TimeSpan.FromTicks(
                    (long)(workflowAge.Ticks * (100.0 / statusInfo.ProgressPercentage)));

                var estimatedRemainingTime = estimatedTotalTime - workflowAge;
                statusInfo.EstimatedCompletionTime = DateTime.UtcNow.Add(estimatedRemainingTime);
            }
        }
    }
}