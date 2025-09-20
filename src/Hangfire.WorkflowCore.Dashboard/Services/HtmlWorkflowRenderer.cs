using System.Text;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Dashboard.Services;

/// <summary>
/// Renders workflow data into HTML format for dashboard display
/// Implements Single Responsibility Principle: Only responsible for HTML rendering
/// Follows Liskov Substitution Principle: Can be substituted with any IWorkflowRenderer
/// Supports Open/Closed Principle: Can be extended with additional rendering features
/// </summary>
public class HtmlWorkflowRenderer : IWorkflowRenderer
{
    public string Render(WorkflowDashboardData? workflowData)
    {
        if (workflowData == null)
        {
            return RenderNoData();
        }

        var html = new StringBuilder();

        html.AppendLine("<div class=\"workflow-section\">");
        html.AppendLine("  <h4>Workflow Information</h4>");

        RenderStatus(html, workflowData);
        RenderProgress(html, workflowData);
        RenderTiming(html, workflowData);
        RenderSteps(html, workflowData);
        RenderErrorInformation(html, workflowData);

        html.AppendLine("</div>");

        return html.ToString();
    }

    private static string RenderNoData()
    {
        return """
            <div class="workflow-section">
                <p class="no-workflow-data">No workflow information available for this job.</p>
            </div>
            """;
    }

    private static void RenderStatus(StringBuilder html, WorkflowDashboardData workflowData)
    {
        var statusClass = GetStatusCssClass(workflowData.Status);
        var statusText = GetStatusDisplayText(workflowData.Status);

        html.AppendLine($"  <div class=\"workflow-status {statusClass}\">");
        html.AppendLine($"    <span class=\"status-badge\">{HtmlEncode(statusText)}</span>");
        html.AppendLine($"    <span class=\"workflow-id\">ID: {HtmlEncode(workflowData.WorkflowInstanceId)}</span>");
        html.AppendLine("  </div>");
    }

    private static void RenderProgress(StringBuilder html, WorkflowDashboardData workflowData)
    {
        var totalSteps = workflowData.Steps.Count;
        var completedSteps = workflowData.Steps.Count(s => s.Status == WorkflowStatus.Complete);

        var progressPercentage = totalSteps > 0 ? (int)Math.Round((double)completedSteps / totalSteps * 100) : 0;

        html.AppendLine("  <div class=\"workflow-progress\">");
        html.AppendLine("    <div class=\"progress-header\">");
        html.AppendLine($"      <span>Progress: {completedSteps} of {totalSteps} steps completed</span>");
        html.AppendLine($"      <span class=\"progress-percentage\">{progressPercentage}%</span>");
        html.AppendLine("    </div>");
        html.AppendLine("    <div class=\"progress-bar\">");
        html.AppendLine($"      <div class=\"progress-fill\" style=\"width: {progressPercentage}%\"></div>");
        html.AppendLine("    </div>");
        html.AppendLine("  </div>");
    }

    private static void RenderTiming(StringBuilder html, WorkflowDashboardData workflowData)
    {
        html.AppendLine("  <div class=\"workflow-timing\">");
        html.AppendLine("    <div class=\"timing-row\">");
        html.AppendLine($"      <span class=\"timing-label\">Created:</span>");
        html.AppendLine($"      <span class=\"timing-value\">{workflowData.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</span>");
        html.AppendLine("    </div>");

        if (workflowData.CompletedAt.HasValue)
        {
            html.AppendLine("    <div class=\"timing-row\">");
            html.AppendLine($"      <span class=\"timing-label\">Completed:</span>");
            html.AppendLine($"      <span class=\"timing-value\">{workflowData.CompletedAt.Value:yyyy-MM-dd HH:mm:ss} UTC</span>");
            html.AppendLine("    </div>");
        }

        var duration = (workflowData.CompletedAt ?? DateTime.UtcNow) - workflowData.CreatedAt;
        html.AppendLine("    <div class=\"timing-row\">");
        html.AppendLine($"      <span class=\"timing-label\">Duration:</span>");
        html.AppendLine($"      <span class=\"timing-value\">{FormatDuration(duration)}</span>");
        html.AppendLine("    </div>");
        html.AppendLine("  </div>");
    }

    private static void RenderSteps(StringBuilder html, WorkflowDashboardData workflowData)
    {
        if (!workflowData.Steps.Any())
        {
            return;
        }

        html.AppendLine("  <div class=\"workflow-steps\">");
        html.AppendLine("    <h5>Steps</h5>");
        html.AppendLine("    <div class=\"steps-list\">");

        var orderedSteps = workflowData.Steps.OrderBy(s => s.Order).ToList();

        foreach (var step in orderedSteps)
        {
            RenderStep(html, step);
        }

        html.AppendLine("    </div>");
        html.AppendLine("  </div>");
    }

    private static void RenderStep(StringBuilder html, WorkflowStepInfo step)
    {
        var stepStatusClass = GetStatusCssClass(step.Status);
        var stepStatusText = GetStatusDisplayText(step.Status);

        html.AppendLine($"    <div class=\"workflow-step step-{stepStatusClass}\">");
        html.AppendLine($"      <div class=\"step-header\">");
        html.AppendLine($"        <span class=\"step-name\">{HtmlEncode(step.Name)}</span>");
        html.AppendLine($"        <span class=\"step-status\">{HtmlEncode(stepStatusText)}</span>");
        html.AppendLine($"      </div>");

        if (step.StartedAt.HasValue || step.CompletedAt.HasValue)
        {
            html.AppendLine($"      <div class=\"step-timing\">");

            if (step.StartedAt.HasValue)
            {
                html.AppendLine($"        <span>Started: {step.StartedAt.Value:HH:mm:ss}</span>");
            }

            if (step.CompletedAt.HasValue)
            {
                html.AppendLine($"        <span>Completed: {step.CompletedAt.Value:HH:mm:ss}</span>");

                if (step.StartedAt.HasValue)
                {
                    var stepDuration = step.CompletedAt.Value - step.StartedAt.Value;
                    html.AppendLine($"        <span>Duration: {FormatDuration(stepDuration)}</span>");
                }
            }

            html.AppendLine($"      </div>");
        }

        if (!string.IsNullOrEmpty(step.ErrorMessage))
        {
            html.AppendLine($"      <div class=\"step-error\">");
            html.AppendLine($"        <span class=\"error-label\">Error:</span>");
            html.AppendLine($"        <span class=\"error-text\">{HtmlEncode(step.ErrorMessage)}</span>");
            html.AppendLine($"      </div>");
        }

        html.AppendLine($"    </div>");
    }

    private static void RenderErrorInformation(StringBuilder html, WorkflowDashboardData workflowData)
    {
        if (string.IsNullOrEmpty(workflowData.ErrorMessage))
        {
            return;
        }

        html.AppendLine("  <div class=\"workflow-error\">");
        html.AppendLine("    <h5>Error Information</h5>");
        html.AppendLine("    <div class=\"error-message\">");
        html.AppendLine($"      {HtmlEncode(workflowData.ErrorMessage)}");
        html.AppendLine("    </div>");
        html.AppendLine("  </div>");
    }

    private static string GetStatusCssClass(WorkflowStatus status)
    {
        return status switch
        {
            WorkflowStatus.Complete => "complete",
            WorkflowStatus.Runnable => "running",
            WorkflowStatus.Suspended => "suspended",
            WorkflowStatus.Terminated => "failed",
            _ => "unknown"
        };
    }

    private static string GetStatusDisplayText(WorkflowStatus status)
    {
        return status switch
        {
            WorkflowStatus.Complete => "Complete",
            WorkflowStatus.Runnable => "Running",
            WorkflowStatus.Suspended => "Suspended",
            WorkflowStatus.Terminated => "Failed",
            _ => "Unknown"
        };
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
        {
            return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
        }

        if (duration.TotalHours >= 1)
        {
            return $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
        }

        if (duration.TotalMinutes >= 1)
        {
            return $"{duration.Minutes}m {duration.Seconds}s";
        }

        return $"{duration.Seconds}s";
    }

    private static string HtmlEncode(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}