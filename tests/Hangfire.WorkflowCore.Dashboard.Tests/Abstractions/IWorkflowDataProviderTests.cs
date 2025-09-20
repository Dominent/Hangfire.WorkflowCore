using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using NSubstitute;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Abstractions;

public class IWorkflowDataProviderTests
{
    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_WorkflowDashboardData_When_JobExists()
    {
        // Arrange
        var provider = Substitute.For<IWorkflowDataProvider>();
        var jobId = "job-123";
        var expectedData = new WorkflowDashboardData
        {
            JobId = jobId,
            WorkflowInstanceId = "workflow-456",
            Status = WorkflowStatus.Runnable,
            CreatedAt = DateTime.UtcNow
        };

        provider.GetWorkflowDataAsync(jobId)
            .Returns(Task.FromResult<WorkflowDashboardData?>(expectedData));

        // Act
        var result = await provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.JobId.Should().Be(jobId);
        result.WorkflowInstanceId.Should().Be("workflow-456");
        result.Status.Should().Be(WorkflowStatus.Runnable);
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_Null_When_JobNotFound()
    {
        // Arrange
        var provider = Substitute.For<IWorkflowDataProvider>();
        var jobId = "nonexistent-job";

        provider.GetWorkflowDataAsync(jobId)
            .Returns(Task.FromResult<WorkflowDashboardData?>(null));

        // Act
        var result = await provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Handle_Exception_Gracefully()
    {
        // Arrange
        var provider = Substitute.For<IWorkflowDataProvider>();
        var jobId = "job-123";

        provider.GetWorkflowDataAsync(jobId)
            .Returns(Task.FromException<WorkflowDashboardData?>(new InvalidOperationException("Storage error")));

        // Act & Assert
        var act = async () => await provider.GetWorkflowDataAsync(jobId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Storage error");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetWorkflowDataAsync_Should_Handle_Invalid_JobId(string? invalidJobId)
    {
        // Arrange
        var provider = Substitute.For<IWorkflowDataProvider>();

        provider.GetWorkflowDataAsync(invalidJobId!)
            .Returns(Task.FromResult<WorkflowDashboardData?>(null));

        // Act
        var result = await provider.GetWorkflowDataAsync(invalidJobId!);

        // Assert
        result.Should().BeNull();
    }
}