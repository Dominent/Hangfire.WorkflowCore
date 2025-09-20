using Hangfire.WorkflowCore.Dashboard.Models;

namespace Hangfire.WorkflowCore.Dashboard.Abstractions;

/// <summary>
/// Calculates status information for workflows
/// Single Responsibility Principle: Only responsible for status calculations
/// </summary>
public interface IWorkflowStatusCalculator
{
    /// <summary>
    /// Calculates status information for the given workflow data
    /// </summary>
    /// <param name="workflowData">The workflow data to analyze</param>
    /// <returns>Calculated status information</returns>
    WorkflowStatusInfo CalculateStatus(WorkflowDashboardData workflowData);
}