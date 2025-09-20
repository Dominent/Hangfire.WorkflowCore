using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using NSubstitute;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Abstractions;

public class IWorkflowStatusCalculatorTests
{
    [Fact]
    public void CalculateStatus_Should_Return_WorkflowStatusInfo_For_CompletedWorkflow()
    {
        // Arrange
        var calculator = Substitute.For<IWorkflowStatusCalculator>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Complete,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow
        };

        var expectedStatusInfo = new WorkflowStatusInfo
        {
            Status = WorkflowStatus.Complete,
            ProgressPercentage = 100,
            Duration = TimeSpan.FromMinutes(10),
            IsCompleted = true,
            IsRunning = false,
            HasError = false
        };

        calculator.CalculateStatus(workflowData)
            .Returns(expectedStatusInfo);

        // Act
        var result = calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.ProgressPercentage.Should().Be(100);
        result.IsCompleted.Should().BeTrue();
        result.IsRunning.Should().BeFalse();
        result.HasError.Should().BeFalse();
    }

    [Fact]
    public void CalculateStatus_Should_Return_WorkflowStatusInfo_For_RunningWorkflow()
    {
        // Arrange
        var calculator = Substitute.For<IWorkflowStatusCalculator>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Steps = new List<WorkflowStepInfo>
            {
                new() { Status = WorkflowStatus.Complete, Order = 1 },
                new() { Status = WorkflowStatus.Runnable, Order = 2 },
                new() { Status = WorkflowStatus.Runnable, Order = 3 }
            }
        };

        var expectedStatusInfo = new WorkflowStatusInfo
        {
            Status = WorkflowStatus.Runnable,
            ProgressPercentage = 33,
            Duration = TimeSpan.FromMinutes(5),
            IsCompleted = false,
            IsRunning = true,
            HasError = false
        };

        calculator.CalculateStatus(workflowData)
            .Returns(expectedStatusInfo);

        // Act
        var result = calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Runnable);
        result.ProgressPercentage.Should().Be(33);
        result.IsRunning.Should().BeTrue();
        result.HasError.Should().BeFalse();
    }

    [Fact]
    public void CalculateStatus_Should_Return_WorkflowStatusInfo_For_FailedWorkflow()
    {
        // Arrange
        var calculator = Substitute.For<IWorkflowStatusCalculator>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Terminated,
            ErrorMessage = "Workflow failed due to invalid data",
            CreatedAt = DateTime.UtcNow.AddMinutes(-3),
            CompletedAt = DateTime.UtcNow
        };

        var expectedStatusInfo = new WorkflowStatusInfo
        {
            Status = WorkflowStatus.Terminated,
            ProgressPercentage = 0,
            Duration = TimeSpan.FromMinutes(3),
            IsCompleted = true,
            IsRunning = false,
            HasError = true,
            ErrorMessage = "Workflow failed due to invalid data"
        };

        calculator.CalculateStatus(workflowData)
            .Returns(expectedStatusInfo);

        // Act
        var result = calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Terminated);
        result.HasError.Should().BeTrue();
        result.ErrorMessage.Should().Be("Workflow failed due to invalid data");
    }

    [Fact]
    public void CalculateStatus_Should_Handle_EmptyWorkflowData()
    {
        // Arrange
        var calculator = Substitute.For<IWorkflowStatusCalculator>();
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            CreatedAt = DateTime.UtcNow
        };

        var expectedStatusInfo = new WorkflowStatusInfo
        {
            Status = WorkflowStatus.Runnable,
            ProgressPercentage = 0,
            Duration = TimeSpan.Zero,
            IsCompleted = false,
            IsRunning = true,
            HasError = false
        };

        calculator.CalculateStatus(workflowData)
            .Returns(expectedStatusInfo);

        // Act
        var result = calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.ProgressPercentage.Should().Be(0);
    }
}