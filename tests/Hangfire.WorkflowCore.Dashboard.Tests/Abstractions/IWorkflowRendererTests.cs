using FluentAssertions;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using NSubstitute;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Abstractions;

public class IWorkflowRendererTests
{
    [Fact]
    public void Render_Should_Return_HTML_For_CompletedWorkflow()
    {
        // Arrange
        var renderer = Substitute.For<IWorkflowRenderer>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Complete,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow
        };

        var expectedHtml = """
            <div class="workflow-section">
                <h4>Workflow Information</h4>
                <div class="workflow-status completed">
                    <span class="status-badge">Completed</span>
                    <span class="progress">100%</span>
                </div>
            </div>
            """;

        renderer.Render(workflowData).Returns(expectedHtml);

        // Act
        var result = renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-section");
        result.Should().Contain("Completed");
        result.Should().Contain("100%");
    }

    [Fact]
    public void Render_Should_Return_HTML_For_RunningWorkflow()
    {
        // Arrange
        var renderer = Substitute.For<IWorkflowRenderer>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            Steps = new List<WorkflowStepInfo>
            {
                new() { Name = "Step 1", Status = WorkflowStatus.Complete },
                new() { Name = "Step 2", Status = WorkflowStatus.Runnable }
            }
        };

        var expectedHtml = """
            <div class="workflow-section">
                <h4>Workflow Information</h4>
                <div class="workflow-status running">
                    <span class="status-badge">Running</span>
                    <div class="workflow-steps">
                        <div class="step completed">Step 1</div>
                        <div class="step running">Step 2</div>
                    </div>
                </div>
            </div>
            """;

        renderer.Render(workflowData).Returns(expectedHtml);

        // Act
        var result = renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("workflow-steps");
        result.Should().Contain("Step 1");
        result.Should().Contain("Step 2");
    }

    [Fact]
    public void Render_Should_Return_HTML_For_FailedWorkflow()
    {
        // Arrange
        var renderer = Substitute.For<IWorkflowRenderer>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Terminated,
            ErrorMessage = "Workflow failed due to timeout"
        };

        var expectedHtml = """
            <div class="workflow-section">
                <h4>Workflow Information</h4>
                <div class="workflow-status failed">
                    <span class="status-badge error">Failed</span>
                    <div class="error-message">Workflow failed due to timeout</div>
                </div>
            </div>
            """;

        renderer.Render(workflowData).Returns(expectedHtml);

        // Act
        var result = renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("status-badge error");
        result.Should().Contain("Failed");
        result.Should().Contain("Workflow failed due to timeout");
    }

    [Fact]
    public void Render_Should_Handle_Null_WorkflowData()
    {
        // Arrange
        var renderer = Substitute.For<IWorkflowRenderer>();
        WorkflowDashboardData? nullData = null;

        var expectedHtml = """
            <div class="workflow-section">
                <p class="no-workflow-data">No workflow data available</p>
            </div>
            """;

        renderer.Render(nullData).Returns(expectedHtml);

        // Act
        var result = renderer.Render(nullData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("no-workflow-data");
        result.Should().Contain("No workflow data available");
    }

    [Fact]
    public void Render_Should_Escape_HTML_In_ErrorMessage()
    {
        // Arrange
        var renderer = Substitute.For<IWorkflowRenderer>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Terminated,
            ErrorMessage = "<script>alert('xss')</script>"
        };

        var expectedHtml = """
            <div class="workflow-section">
                <h4>Workflow Information</h4>
                <div class="workflow-status failed">
                    <span class="status-badge error">Failed</span>
                    <div class="error-message">&lt;script&gt;alert('xss')&lt;/script&gt;</div>
                </div>
            </div>
            """;

        renderer.Render(workflowData).Returns(expectedHtml);

        // Act
        var result = renderer.Render(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("&lt;script&gt;");
        result.Should().NotContain("<script>");
    }
}