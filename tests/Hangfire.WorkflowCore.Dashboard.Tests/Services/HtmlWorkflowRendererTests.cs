using FluentAssertions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Hangfire.WorkflowCore.Dashboard.Services;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Services;

public class HtmlWorkflowRendererTests
{
    private readonly HtmlWorkflowRenderer _renderer = new();

    [Fact]
    public void Render_Should_Return_HTML_For_CompletedWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Complete,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow,
            Steps = new List<WorkflowStepInfo>
            {
                new() { Name = "Step 1", Status = WorkflowStatus.Complete, Order = 1 },
                new() { Name = "Step 2", Status = WorkflowStatus.Complete, Order = 2 }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-section");
        result.Should().Contain("Complete");
        result.Should().Contain("workflow-456");
        result.Should().Contain("Step 1");
        result.Should().Contain("Step 2");
        result.Should().Contain("complete");
        result.Should().NotContain("<script>"); // Should not contain any script tags
    }

    [Fact]
    public void Render_Should_Return_HTML_For_RunningWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Runnable,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Steps = new List<WorkflowStepInfo>
            {
                new() { Name = "Analyze Video", Status = WorkflowStatus.Complete, Order = 1 },
                new() { Name = "Convert Video", Status = WorkflowStatus.Runnable, Order = 2 },
                new() { Name = "Upload Video", Status = WorkflowStatus.Runnable, Order = 3 }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-section");
        result.Should().Contain("Running");
        result.Should().Contain("Analyze Video");
        result.Should().Contain("Convert Video");
        result.Should().Contain("Upload Video");
        result.Should().Contain("running");
        result.Should().Contain("step-complete");
        result.Should().Contain("step-running");
    }

    [Fact]
    public void Render_Should_Return_HTML_For_FailedWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Terminated,
            ErrorMessage = "Workflow failed due to timeout",
            CreatedAt = DateTime.UtcNow.AddMinutes(-3),
            CompletedAt = DateTime.UtcNow,
            Steps = new List<WorkflowStepInfo>
            {
                new() { Name = "Step 1", Status = WorkflowStatus.Complete, Order = 1 },
                new() { Name = "Step 2", Status = WorkflowStatus.Terminated, Order = 2, ErrorMessage = "Connection timeout" }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-section");
        result.Should().Contain("Failed");
        result.Should().Contain("failed");
        result.Should().Contain("Workflow failed due to timeout");
        result.Should().Contain("Connection timeout");
        result.Should().Contain("error-message");
    }

    [Fact]
    public void Render_Should_Return_HTML_For_SuspendedWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Suspended,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            Steps = new List<WorkflowStepInfo>
            {
                new() { Name = "Wait for User Input", Status = WorkflowStatus.Suspended, Order = 1 }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-section");
        result.Should().Contain("Suspended");
        result.Should().Contain("suspended");
        result.Should().Contain("Wait for User Input");
    }

    [Fact]
    public void Render_Should_Handle_Null_WorkflowData()
    {
        // Act
        var result = _renderer.Render(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("no-workflow-data");
        result.Should().Contain("No workflow information available");
    }

    [Fact]
    public void Render_Should_Escape_HTML_In_ErrorMessages()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Terminated,
            ErrorMessage = "<script>alert('xss')</script>Invalid data: <tag>",
            Steps = new List<WorkflowStepInfo>
            {
                new()
                {
                    Name = "<b>Malicious Step</b>",
                    Status = WorkflowStatus.Terminated,
                    ErrorMessage = "<img src=x onerror=alert('xss')>",
                    Order = 1
                }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain("<script>");
        result.Should().NotContain("<img src=x");
        result.Should().NotContain("<b>Malicious Step</b>");
        result.Should().Contain("&lt;script&gt;");
        result.Should().Contain("&lt;b&gt;Malicious Step&lt;/b&gt;");
        result.Should().Contain("&lt;img src=x");
    }

    [Fact]
    public void Render_Should_Include_Timing_Information()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddMinutes(-10);
        var completedAt = DateTime.UtcNow;
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Complete,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            Steps = new List<WorkflowStepInfo>
            {
                new()
                {
                    Name = "Process Data",
                    Status = WorkflowStatus.Complete,
                    StartedAt = createdAt.AddMinutes(1),
                    CompletedAt = createdAt.AddMinutes(5),
                    Order = 1
                }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-timing");
        result.Should().Contain("Duration:");
        result.Should().Contain("Created:");
        result.Should().Contain("Completed:");
    }

    [Fact]
    public void Render_Should_Include_Progress_Information()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            Steps = new List<WorkflowStepInfo>
            {
                new() { Name = "Step 1", Status = WorkflowStatus.Complete, Order = 1 },
                new() { Name = "Step 2", Status = WorkflowStatus.Complete, Order = 2 },
                new() { Name = "Step 3", Status = WorkflowStatus.Runnable, Order = 3 },
                new() { Name = "Step 4", Status = WorkflowStatus.Runnable, Order = 4 }
            }
        };

        // Act
        var result = _renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("progress-bar");
        result.Should().Contain("50%"); // 2 out of 4 steps completed
        result.Should().Contain("2 of 4 steps completed");
    }
}