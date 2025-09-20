using Hangfire.WorkflowCore.Dashboard.Models;

namespace Hangfire.WorkflowCore.Dashboard.Abstractions;

/// <summary>
/// Renders workflow data into display format
/// Single Responsibility Principle: Only responsible for rendering
/// Open/Closed Principle: Can be extended with different renderer implementations
/// </summary>
public interface IWorkflowRenderer
{
    /// <summary>
    /// Renders workflow data into HTML format for dashboard display
    /// </summary>
    /// <param name="workflowData">The workflow data to render</param>
    /// <returns>HTML representation of the workflow data</returns>
    string Render(WorkflowDashboardData? workflowData);
}