using FluentAssertions;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Hangfire.WorkflowCore.Dashboard.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Services;

public class WorkflowDashboardServiceTests
{
    private readonly IWorkflowDataProvider _dataProvider;
    private readonly IWorkflowStatusCalculator _statusCalculator;
    private readonly IWorkflowRenderer _renderer;
    private readonly ILogger<WorkflowDashboardService> _logger;
    private readonly WorkflowDashboardService _service;

    public WorkflowDashboardServiceTests()
    {
        _dataProvider = Substitute.For<IWorkflowDataProvider>();
        _statusCalculator = Substitute.For<IWorkflowStatusCalculator>();
        _renderer = Substitute.For<IWorkflowRenderer>();
        _logger = Substitute.For<ILogger<WorkflowDashboardService>>();
        _service = new WorkflowDashboardService(_dataProvider, _statusCalculator, _renderer, _logger);
    }

    [Fact]
    public async Task RenderWorkflowInfoAsync_Should_Return_HTML_When_WorkflowExists()
    {
        // Arrange
        var jobId = "job-123";
        var workflowData = new WorkflowDashboardData
        {
            JobId = jobId,
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Complete,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow
        };

        var statusInfo = new WorkflowStatusInfo
        {
            Status = WorkflowStatus.Complete,
            ProgressPercentage = 100,
            IsCompleted = true
        };

        var expectedHtml = "<div class=\"workflow-section\">Completed workflow</div>";

        _dataProvider.GetWorkflowDataAsync(jobId)
            .Returns(workflowData);

        _statusCalculator.CalculateStatus(workflowData)
            .Returns(statusInfo);

        _renderer.Render(workflowData)
            .Returns(expectedHtml);

        // Act
        var result = await _service.RenderWorkflowInfoAsync(jobId);

        // Assert
        result.Should().Be(expectedHtml);

        await _dataProvider.Received(1).GetWorkflowDataAsync(jobId);
        _statusCalculator.Received(1).CalculateStatus(workflowData);
        _renderer.Received(1).Render(workflowData);
    }

    [Fact]
    public async Task RenderWorkflowInfoAsync_Should_Return_NoDataMessage_When_WorkflowNotFound()
    {
        // Arrange
        var jobId = "nonexistent-job";
        var noDataHtml = "<div class=\"no-workflow-data\">No workflow data</div>";

        _dataProvider.GetWorkflowDataAsync(jobId)
            .Returns((WorkflowDashboardData?)null);

        _renderer.Render(null)
            .Returns(noDataHtml);

        // Act
        var result = await _service.RenderWorkflowInfoAsync(jobId);

        // Assert
        result.Should().Be(noDataHtml);

        await _dataProvider.Received(1).GetWorkflowDataAsync(jobId);
        _statusCalculator.DidNotReceive().CalculateStatus(Arg.Any<WorkflowDashboardData>());
        _renderer.Received(1).Render(null);
    }

    [Fact]
    public async Task RenderWorkflowInfoAsync_Should_HandleException_And_ReturnErrorMessage()
    {
        // Arrange
        var jobId = "job-123";

        _dataProvider.GetWorkflowDataAsync(jobId)
            .Returns(Task.FromException<WorkflowDashboardData?>(new InvalidOperationException("Database error")));

        // Act
        var result = await _service.RenderWorkflowInfoAsync(jobId);

        // Assert
        result.Should().Contain("error");
        result.Should().Contain("workflow information");
        result.Should().NotContain("Database error"); // Should not expose internal error details
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_WorkflowData_When_Exists()
    {
        // Arrange
        var jobId = "job-123";
        var expectedData = new WorkflowDashboardData
        {
            JobId = jobId,
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Runnable
        };

        _dataProvider.GetWorkflowDataAsync(jobId)
            .Returns(expectedData);

        // Act
        var result = await _service.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().Be(expectedData);
        await _dataProvider.Received(1).GetWorkflowDataAsync(jobId);
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var jobId = "nonexistent-job";

        _dataProvider.GetWorkflowDataAsync(jobId)
            .Returns((WorkflowDashboardData?)null);

        // Act
        var result = await _service.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().BeNull();
        await _dataProvider.Received(1).GetWorkflowDataAsync(jobId);
    }

    [Fact]
    public void CalculateWorkflowStatus_Should_Return_StatusInfo()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable
        };

        var expectedStatusInfo = new WorkflowStatusInfo
        {
            Status = WorkflowStatus.Runnable,
            ProgressPercentage = 50,
            IsRunning = true
        };

        _statusCalculator.CalculateStatus(workflowData)
            .Returns(expectedStatusInfo);

        // Act
        var result = _service.CalculateWorkflowStatus(workflowData);

        // Assert
        result.Should().Be(expectedStatusInfo);
        _statusCalculator.Received(1).CalculateStatus(workflowData);
    }

    [Fact]
    public void RenderWorkflowHtml_Should_Return_HTML()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Complete
        };

        var expectedHtml = "<div>Completed workflow</div>";

        _renderer.Render(workflowData)
            .Returns(expectedHtml);

        // Act
        var result = _service.RenderWorkflowHtml(workflowData);

        // Assert
        result.Should().Be(expectedHtml);
        _renderer.Received(1).Render(workflowData);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task RenderWorkflowInfoAsync_Should_Return_ErrorMessage_For_InvalidJobId(string? invalidJobId)
    {
        // Act
        var result = await _service.RenderWorkflowInfoAsync(invalidJobId!);

        // Assert
        result.Should().Contain("error");
        result.Should().Contain("Invalid job ID");
    }
}