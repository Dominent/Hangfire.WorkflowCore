using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Microsoft.Extensions.Logging;

namespace Hangfire.WorkflowCore.Dashboard.Services;

/// <summary>
/// Main service for workflow dashboard operations
/// Implements Dependency Inversion Principle: Depends on abstractions, not concretions
/// Follows Single Responsibility Principle: Coordinates workflow dashboard functionality
/// Supports composition of multiple services following DIP
/// </summary>
public class WorkflowDashboardService : IWorkflowDashboardService
{
    private readonly IWorkflowDataProvider _dataProvider;
    private readonly IWorkflowStatusCalculator _statusCalculator;
    private readonly IWorkflowRenderer _renderer;
    private readonly ILogger<WorkflowDashboardService> _logger;

    public WorkflowDashboardService(
        IWorkflowDataProvider dataProvider,
        IWorkflowStatusCalculator statusCalculator,
        IWorkflowRenderer renderer,
        ILogger<WorkflowDashboardService> logger)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _statusCalculator = statusCalculator ?? throw new ArgumentNullException(nameof(statusCalculator));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> RenderWorkflowInfoAsync(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            _logger.LogWarning("Invalid job ID provided for workflow rendering: {JobId}", jobId);
            return RenderError("Invalid job ID provided");
        }

        try
        {
            _logger.LogDebug("Rendering workflow information for job {JobId}", jobId);

            // Get workflow data
            var workflowData = await _dataProvider.GetWorkflowDataAsync(jobId);

            if (workflowData == null)
            {
                _logger.LogDebug("No workflow data found for job {JobId}", jobId);
                return _renderer.Render(null);
            }

            // Calculate status information (this could be used for enhanced rendering in the future)
            var statusInfo = _statusCalculator.CalculateStatus(workflowData);
            _logger.LogDebug("Calculated workflow status for job {JobId}: {Status}, Progress: {Progress}%",
                jobId, statusInfo.Status, statusInfo.ProgressPercentage);

            // Render HTML
            var html = _renderer.Render(workflowData);

            _logger.LogDebug("Successfully rendered workflow information for job {JobId}", jobId);
            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering workflow information for job {JobId}", jobId);
            return RenderError("Unable to load workflow information");
        }
    }

    public async Task<WorkflowDashboardData?> GetWorkflowDataAsync(string jobId)
    {
        try
        {
            return await _dataProvider.GetWorkflowDataAsync(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow data for job {JobId}", jobId);
            return null;
        }
    }

    public WorkflowStatusInfo CalculateWorkflowStatus(WorkflowDashboardData workflowData)
    {
        ArgumentNullException.ThrowIfNull(workflowData);

        try
        {
            return _statusCalculator.CalculateStatus(workflowData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating workflow status for job {JobId}", workflowData.JobId);

            // Return a default status info in case of error
            return new WorkflowStatusInfo
            {
                Status = workflowData.Status,
                HasError = true,
                ErrorMessage = "Unable to calculate workflow status"
            };
        }
    }

    public string RenderWorkflowHtml(WorkflowDashboardData? workflowData)
    {
        try
        {
            return _renderer.Render(workflowData);
        }
        catch (Exception ex)
        {
            var jobId = workflowData?.JobId ?? "unknown";
            _logger.LogError(ex, "Error rendering workflow HTML for job {JobId}", jobId);
            return RenderError("Unable to render workflow information");
        }
    }

    private static string RenderError(string errorMessage)
    {
        return $"""
            <div class="workflow-section workflow-error">
                <div class="error-message">
                    <span class="error-icon">⚠️</span>
                    <span class="error-text">{HtmlEncode(errorMessage)}</span>
                </div>
            </div>
            """;
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